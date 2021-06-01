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
            var indexerName = ((DefaultMemberAttribute)collection.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;

            objAux = collection.GetType().GetProperty(indexerName).GetValue(collection, new object[] { index });
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
          var indexerName = ((DefaultMemberAttribute)collection.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;

          collection.GetType().GetProperty(indexerName).SetValue(collection, difference.RightValue, new object[] { index2 });
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


    private static IEnumerable<Difference> Diff<T>(T left, T right, CompareOptions compareOptions, string path, int? arrayIndex, uint currentDepth) where T : class
    {
      var differences = new List<Difference>();

      if ((left == null) && (right == null))
        return differences;

      if (compareOptions == null)
        compareOptions = new CompareOptions();

      var applicableProperties = left.GetType().GetProperties().Where(p => !compareOptions.IgnoredProperties.Contains(p.Name) &&
          !p.GetCustomAttributes(false).Any(x => compareOptions.IgnoredAttributes.Contains(x.GetType().Name)));

      foreach (var property in applicableProperties)
      {
        var value1 = property.GetValue(left);
        var value2 = property.GetValue(right);

        if ((value1 == null) && (value2 == null))
          continue;

        if (string.IsNullOrEmpty(path) && (arrayIndex != null))
          throw new Exception("Array index specified while in root path");

        var arrayIndexStr = arrayIndex != null ? $"[{arrayIndex}]" : string.Empty;
        var propertyPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}{arrayIndexStr}.{property.Name}";

        if (IsCollection(property))
        {
          var collectionType = property.PropertyType.IsArray ? property.PropertyType : property.PropertyType.GetGenericArguments()[0];

          var collection1 = (value1 as IEnumerable)?.OfType<object>() ?? new List<object>();
          var collection2 = (value2 as IEnumerable)?.OfType<object>() ?? new List<object>();

          var collection1Count = collection1.Count();
          var collection2Count = collection2.Count();
        
          if (collection1Count == collection2Count)
          {
            if (IsSimpleType(collectionType) || collectionType.IsArray || (currentDepth == compareOptions.MaxDepth))
            {
              if (compareOptions.CollectionsSameOrder)
              {
                for (int i = 0; i < collection1Count; i++)
                {
                  var collection1Value = collection1.ElementAt(i);
                  var collection2Value = collection2.ElementAt(i);

                  if (!Equals(collection1Value, collection2Value))
                    differences.Add(new Difference($"{propertyPath}[{i}]", collection1Value, collection2Value));
                }
              }

              else
              {
                var collection1Exclusive = collection1.Except(collection2).ToList();
                var collection2Exclusive = collection2.Except(collection1).ToList();

                foreach (var item in collection1Exclusive)
                  differences.Add(new Difference(propertyPath, item, ItemStatus.NotExist));

                foreach (var item in collection2Exclusive)
                  differences.Add(new Difference(propertyPath, ItemStatus.NotExist, item));
              }
            }

            else
            {
              if (compareOptions.CollectionsSameOrder)
              {
                for (int i = 0; i < collection1Count; i++)
                  differences.AddRange(Diff(collection1.ElementAt(i), collection2.ElementAt(i), compareOptions, propertyPath, i, currentDepth + 1));
              }

              else
              {
                var collection1Exclusive = collection1.Except(collection2).ToList();
                var collection2Exclusive = collection2.Except(collection1).ToList();

                foreach (var item in collection1Exclusive)
                  differences.Add(new Difference(propertyPath, item, ItemStatus.NotExist));

                foreach (var item in collection2Exclusive)
                  differences.Add(new Difference(propertyPath, ItemStatus.NotExist, item));
              }
            }
          }

          else
          {
            var minItems = Math.Min(collection1Count, collection2Count);
            var maxItems = Math.Max(collection1Count, collection2Count);

            if (IsSimpleType(collectionType) || collectionType.IsArray || (currentDepth == compareOptions.MaxDepth))
            {
              if (compareOptions.CollectionsSameOrder)
              {
                for (int i = 0; i < minItems; i++)
                {
                  var collection1Value = collection1.ElementAt(i);
                  var collection2Value = collection2.ElementAt(i);

                  if (!Equals(collection1Value, collection2Value))
                    differences.Add(new Difference($"{propertyPath}[{i}]", collection1Value, collection2Value));
                }

                if (minItems == collection1Count)
                {
                  for (int i = minItems; i < collection2Count; i++)
                  {
                    var collection2Value = collection2.ElementAt(i);
                    differences.Add(new Difference(propertyPath, ItemStatus.NotExist, collection2Value));
                  }
                }

                else
                {
                  for (int i = minItems; i < collection1Count; i++)
                  {
                    var collection1Value = collection1.ElementAt(i);
                    differences.Add(new Difference(propertyPath, collection1Value, ItemStatus.NotExist));
                  }
                }
              }

              else
              {
                throw new NotImplementedException();
              }
            }

            else
            {
              for (int i = 0; i < minItems; i++)
                differences.AddRange(Diff(collection1.ElementAt(i), collection2.ElementAt(i), compareOptions, propertyPath, i, currentDepth + 1));

              if (minItems == collection1Count)
              {
                for (int i = minItems; i < collection2Count; i++)
                {
                  var collection2Value = collection2.ElementAt(i);
                  differences.Add(new Difference(propertyPath, ItemStatus.NotExist, collection2Value));
                }
              }

              else
              {
                for (int i = minItems; i < collection1Count; i++)
                {
                  var collection1Value = collection1.ElementAt(i);
                  differences.Add(new Difference(propertyPath, collection1Value, ItemStatus.NotExist));
                }
              }
            }
          }
        }


        else if (IsDictionary(property))
        {
          // TODO: Dictionaries not yet supported.
          continue;
        }


        else
        {
          if (IsSimpleType(property.PropertyType) || (currentDepth == compareOptions.MaxDepth) || value1 == null || value2 == null)
          {
            if (!Equals(value1, value2))
              differences.Add(new Difference(propertyPath, value1, value2));
          }

          else
            differences.AddRange(Diff(value1, value2, compareOptions, propertyPath, arrayIndex, currentDepth + 1));
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
      IsNullableSimpleType(type));

      bool IsNullableSimpleType(Type t)
      {
        var underlyingType = Nullable.GetUnderlyingType(t);
        return underlyingType != null && IsSimpleType(underlyingType);
      }
    }


    private static bool IsCollection(PropertyInfo propertyInfo)
    {
      return propertyInfo.PropertyType != typeof(string) &&
             propertyInfo.PropertyType.GetInterface(nameof(IEnumerable)) != null;
    }


    private static bool IsDictionary(PropertyInfo propertyInfo)
    {
      return propertyInfo.PropertyType.GetInterface(nameof(IDictionary)) != null;
    }
  }
}