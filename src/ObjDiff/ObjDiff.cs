using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


namespace ObjDiff
{
    public class ObjDiff
    {
        public static IEnumerable<Difference> Diff<T>(T left, T right, CompareOptions compareOptions = null) where T : class
        {
            return Diff(left, right, compareOptions, null, null, 1);
        }


        public static void Patch<T>(T target, IEnumerable<Difference> differences) where T : class
        {
            foreach (var difference in differences)
            {
                var singleProperties = difference.Path.Split('.');
                object objAux = target;
                var i = 0;

                while (i < singleProperties.Length - 1)
                {
                    if (IsCollectionItem(singleProperties[i++], out string currentProperty, out int? index))
                    {
                        var collection = objAux.GetType().GetProperty(currentProperty)?.GetValue(objAux);
                        var indexerName = string.Empty;

                        if (collection is Array array)
                            objAux = array.GetValue(index.Value);
                        else
                        {
                            indexerName = ((DefaultMemberAttribute)collection.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;
                            objAux = collection.GetType().GetProperty(indexerName).GetValue(collection, new object[] { index.Value });
                        }
                    }
                    else
                        objAux = objAux.GetType().GetProperty(currentProperty)?.GetValue(objAux);

                    if (objAux == null)
                        throw new TargetException("Invalid path");
                }

                // Special case: If last property in the path contains an index it means that property is a collection of
                // simple types AND the difference is a value modification (no addition or removal) so directly apply
                // the new value and continue to the next difference.
                if (IsCollectionItem(singleProperties.Last(), out string lastProperty, out int? index2))
                {
                    var collection = objAux.GetType().GetProperty(lastProperty)?.GetValue(objAux);

                    if (collection is Array array)
                    {
                        array.SetValue(difference.RightValue, index2.Value);
                    }

                    else if (collection is IDictionary dict)
                    {
                        var leftKey = difference.LeftValue.GetType().GetProperty("Key").GetValue(difference.LeftValue);
                        var leftValue = difference.LeftValue.GetType().GetProperty("Value").GetValue(difference.LeftValue);

                        var rightKey = difference.RightValue.GetType().GetProperty("Key").GetValue(difference.RightValue);
                        var rightValue = difference.RightValue.GetType().GetProperty("Value").GetValue(difference.RightValue);

                        if (!Equals(leftKey, rightKey))
                        {
                            dict.Remove(leftKey);
                            dict[rightKey] = rightValue;
                        }

                        else
                            dict[leftKey] = rightValue;
                    }

                    else
                    {
                        var indexerName = ((DefaultMemberAttribute)collection.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;
                        collection.GetType().GetProperty(indexerName).SetValue(collection, difference.RightValue, new object[] { index2.Value });
                    }

                    continue;
                }

                var property = objAux.GetType().GetProperty(lastProperty);

                if (property == null)
                    throw new TargetException("Invalid path");

                if (IsCollection(property))
                {
                    var collection = property.GetValue(objAux);

                    // For those collections implementing IList, missing/extra items can be directly added/deleted.

                    if (collection is IList list)
                    {
                        if (Equals(difference.LeftValue, ItemStatus.NotExist))
                            list.Add(difference.RightValue);

                        else if (Equals(difference.RightValue, ItemStatus.NotExist))
                            list.Remove(difference.LeftValue);
                    }

                    // If not, the collection will be replaced with a new one with the affected items already added/removed.
                    else
                    {
                        var mutableList = (property.GetValue(objAux) as IEnumerable<object>).ToList();

                        if (Equals(difference.LeftValue, ItemStatus.NotExist))
                            mutableList.Add(difference.RightValue);

                        else if (Equals(difference.RightValue, ItemStatus.NotExist))
                            mutableList.Remove(difference.LeftValue);

                        property.SetValue(objAux, mutableList.AsEnumerable());
                    }
                }

                else
                    property.SetValue(objAux, difference.RightValue);
            }
        }


        public static void MakeEqual<T>(T target, T source, CompareOptions compareOptions = null) where T : class
        {
            Patch(target, Diff(target, source, compareOptions));
        }


        private static IEnumerable<Difference> Diff<T>(T left, T right, CompareOptions compareOptions, string path, int? arrayIndex, uint currentDepth) where T : class
        {
            var differences = new List<Difference>();

            if ((left == null) && (right == null))
                return differences;

            if (compareOptions == null)
                compareOptions = new CompareOptions();

            var applicableProperties = left.GetType().GetProperties().Where(p => !compareOptions.IgnoredProperties.Contains(p.Name) &&
              !p.GetCustomAttributes(false).Any(x => x.GetType() == typeof(IgnoreDifferencesAttribute) || compareOptions.IgnoredAttributes.Contains(x.GetType().Name)));

            foreach (var property in applicableProperties)
            {
                var leftValue = property.GetValue(left);
                var rightValue = property.GetValue(right);

                if ((leftValue == null) && (rightValue == null))
                    continue;

                if (string.IsNullOrEmpty(path) && (arrayIndex != null))
                    throw new Exception("Array index specified while in root path");

                var arrayIndexStr = arrayIndex != null ? $"[{arrayIndex}]" : string.Empty;
                var propertyPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}{arrayIndexStr}.{property.Name}";

                if (IsCollection(property))
                {
                    Type collectionType = null;

                    if (property.PropertyType.IsArray)
                        collectionType = property.PropertyType.GetElementType();

                    else
                    {
                        var genericArguments = property.PropertyType.GetGenericArguments();

                        while (genericArguments != null && genericArguments.Length == 0)
                            genericArguments = property.PropertyType.BaseType?.GetGenericArguments();

                        if (genericArguments == null)
                            throw new Exception("Unable to obtain generic arguments of a collection property");

                        collectionType = genericArguments[0];
                    }

                    if (!HasEqualityDefined(collectionType))
                        continue;

                    var leftCollection = (leftValue as IEnumerable)?.OfType<object>() ?? new List<object>();
                    var rightCollection = (rightValue as IEnumerable)?.OfType<object>() ?? new List<object>();
                    var leftCollectionCount = leftCollection.Count();
                    var rightCollectionCount = rightCollection.Count();
                    var minItems = Math.Min(leftCollection.Count(), rightCollection.Count());

                    if (IsSimpleType(collectionType) || (currentDepth == compareOptions.MaxDepth))
                    {
                        if (compareOptions.CollectionsSameOrder)
                        {
                            for (int i = 0; i < minItems; i++)
                            {
                                var leftCollectionValue = leftCollection.ElementAt(i);
                                var rightCollectionValue = rightCollection.ElementAt(i);

                                if (!Equals(leftCollectionValue, rightCollectionValue))
                                    differences.Add(new Difference($"{propertyPath}[{i}]", leftCollectionValue, rightCollectionValue));
                            }
                        }
                    }

                    else
                    {
                        if (compareOptions.CollectionsSameOrder)
                        {
                            for (int i = 0; i < minItems; i++)
                                differences.AddRange(Diff(leftCollection.ElementAt(i), rightCollection.ElementAt(i), compareOptions, propertyPath, i, currentDepth + 1));
                        }
                    }

                    // Add non-common items to differences.

                    if (compareOptions.CollectionsSameOrder)
                    {
                        if (leftCollectionCount != rightCollectionCount)
                        {
                            if (minItems == leftCollectionCount)
                            {
                                for (int i = minItems; i < rightCollectionCount; i++)
                                {
                                    var rightCollectionValue = rightCollection.ElementAt(i);
                                    differences.Add(new Difference(propertyPath, ItemStatus.NotExist, rightCollectionValue));
                                }
                            }

                            else
                            {
                                for (int i = minItems; i < leftCollectionCount; i++)
                                {
                                    var leftCollectionValue = leftCollection.ElementAt(i);
                                    differences.Add(new Difference(propertyPath, leftCollectionValue, ItemStatus.NotExist));
                                }
                            }
                        }
                    }

                    else
                    {
                        var leftCollectionExclusive = leftCollection.Except(rightCollection).ToList();
                        var rightCollectionExclusive = rightCollection.Except(leftCollection).ToList();

                        foreach (var item in leftCollectionExclusive)
                            differences.Add(new Difference(propertyPath, item, ItemStatus.NotExist));

                        foreach (var item in rightCollectionExclusive)
                            differences.Add(new Difference(propertyPath, ItemStatus.NotExist, item));
                    }
                }

                else
                {
                    if (!HasEqualityDefined(property.PropertyType))
                        continue;

                    if (IsSimpleType(property.PropertyType) || (currentDepth == compareOptions.MaxDepth) || leftValue == null || rightValue == null)
                    {
                        if (!Equals(leftValue, rightValue))
                            differences.Add(new Difference(propertyPath, leftValue, rightValue));
                    }

                    else
                        differences.AddRange(Diff(leftValue, rightValue, compareOptions, propertyPath, arrayIndex, currentDepth + 1));
                }
            }

            return differences;
        }


        private static bool IsCollectionItem(string indexedProperty, out string propertyName, out int? index)
        {
            var match = Regex.Match(indexedProperty, @"([a-zA-z_0-9]+)\[([0-9]+)\]");

            if (match.Success)
            {
                propertyName = match.Groups[1].Value;
                index = int.Parse(match.Groups[2].Value);

                return true;
            }

            propertyName = indexedProperty;
            index = null;

            return false;
        }


        private static bool HasEqualityDefined(Type type)
        {
            var filter = new TypeFilter((c, o) => c.ToString() == o.ToString());

            if (type.FindInterfaces(filter, typeof(IEquatable<>)).Length > 0)
                return true;

            return type.GetMethod("Equals", new Type[] { typeof(object) }).DeclaringType != typeof(object);
        }


        private static readonly ConcurrentDictionary<Type, bool> IsSimpleTypeCache = new ConcurrentDictionary<Type, bool>();


        private static bool IsSimpleType(Type type)
        {
            return IsSimpleTypeCache.GetOrAdd(type, t =>
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
#if NET6_0_OR_GREATER
                type == typeof(DateOnly) ||
                type == typeof(TimeOnly) ||
#endif
                IsNullableSimpleType(type));

            bool IsNullableSimpleType(Type t)
            {
                var underlyingType = Nullable.GetUnderlyingType(t);
                return underlyingType != null && IsSimpleType(underlyingType);
            }
        }


        private static bool IsDictionary(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }


        private static bool IsCollection(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType != typeof(string) &&
                   propertyInfo.PropertyType.GetInterface(nameof(IEnumerable)) != null;
        }
    }
}