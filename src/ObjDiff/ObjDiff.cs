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
    public static IEnumerable<Difference> Diff<T>(T object1, T object2, CompareOptions compareOptions = null) where T : class
    {
      return Diff(object1, object2, compareOptions, null, null, 1);
    }


    public static void Patch<T>(T obj, IEnumerable<Difference> differences) where T : class
    {
      foreach (var difference in differences)
      {
        var singleProperties = difference.Path.Split('.');
        object objAux = obj;
        int i = 0;

        while (i < singleProperties.Length - 1)
        {
          var currentProperty = singleProperties[i++];
          var match = Regex.Match(currentProperty, @"([a-zA-z_0-9]+)\[([0-9]+)\]");

          if (match.Success)
          {
            currentProperty = match.Groups[1].Value;
            var index = int.Parse(match.Groups[2].Value);

            var collection = objAux.GetType().GetProperty(currentProperty)?.GetValue(objAux);

            var indexerName = ((DefaultMemberAttribute)collection.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;
            objAux = collection.GetType().GetProperty(indexerName).GetValue(collection, new object[] { index });
          }
          else
            objAux = objAux.GetType().GetProperty(currentProperty)?.GetValue(objAux);

          if (objAux == null)
            throw new TargetException("Invalid path");
        }

        var property = objAux.GetType().GetProperty(singleProperties[i]);

        if (property == null)
          throw new TargetException("Invalid path");

        if (IsCollection(property))
        {
          var collection = property.GetValue(objAux);

          if (collection is IList list)
          {
            if (Equals(difference.LeftValue, ItemStatus.NotExist))
              list.Add(difference.RightValue);

            else if (Equals(difference.RightValue, ItemStatus.NotExist))
              list.Remove(difference.LeftValue);
          }

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


    private static IEnumerable<Difference> Diff<T>(T object1, T object2, CompareOptions compareOptions, string path, int? arrayIndex, uint currentDepth) where T : class
    {
      if (compareOptions == null)
        compareOptions = new CompareOptions();

      var differences = new List<Difference>();

      /*
      if (IsSimpleType(typeof(T)))
      {
          if (!Equals(object1, object2))
              diffs.Add(new Difference())
      }
      */

      var applicableProperties = object1.GetType().GetProperties().Where(p => !compareOptions.IgnoredProperties.Contains(p.Name) &&
          !p.GetCustomAttributes(false).Any(x => compareOptions.IgnoredAttributes.Contains(x.GetType().Name)));

      foreach (var property in applicableProperties)
      {
        var value1 = property.GetValue(object1);
        var value2 = property.GetValue(object2);

        if (string.IsNullOrEmpty(path) && (arrayIndex != null))
          throw new Exception("Array index specified while in root path");

        var arrayIndexStr = arrayIndex != null ? $"[{arrayIndex}]" : string.Empty;
        var propertyPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}{arrayIndexStr}.{property.Name}";

        if (IsCollection(property))
        {
          var collection1 = (IEnumerable<object>)value1;
          var collection2 = (IEnumerable<object>)value2;

          var collection1Count = collection1.Count();
          var collection2Count = collection2.Count();

          /*
          if (!compareOptions.CollectionsSameOrder)
          {
              collection1 = collection1.OrderBy(item => item);
              collection2 = collection2.OrderBy(item => item);
          }
          */

          if (collection1Count == collection2Count)
          {
            if (IsSimpleType(property.PropertyType) || (currentDepth == compareOptions.MaxDepth))
            {
              if (compareOptions.CollectionsSameOrder)
              {
                for (int i = 0; i < collection1Count; i++)
                {
                  var collection1Value = collection1.ElementAt(i);
                  var collection2Value = collection2.ElementAt(i);

                  if (!Equals(collection1Value, collection2Value))
                    differences.Add(new Difference(propertyPath, collection1Value, collection2Value));
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
            // Esto vale para el caso CollectionsSameOrder = true. Si no se va a tener en cuenta el orden, y aunque se hayan ordenado las listas,
            // por cada elemento habría que comprobar si la otra lista lo contiene (método Contains).

            var minItems = Math.Min(collection1Count, collection2Count);
            var maxItems = Math.Max(collection1Count, collection2Count);

            if (IsSimpleType(property.PropertyType) || (currentDepth == compareOptions.MaxDepth))
            {
              if (compareOptions.CollectionsSameOrder)
              {
                for (int i = 0; i < minItems; i++)
                {
                  var collection1Value = collection1.ElementAt(i);
                  var collection2Value = collection2.ElementAt(i);

                  if (!Equals(collection1Value, collection2Value))
                    differences.Add(new Difference(propertyPath, collection1Value, collection2Value));
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
                if (maxItems == collection1Count)
                {
                  var notInCollection2 = collection1.Except(collection2).ToList();

                  /*
                  foreach (var item in notInCollection2)
                  {
                      differences.Add(new Difference( ))
                  }
                  */
                }
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