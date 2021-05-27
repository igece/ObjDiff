using System.Collections.Generic;


namespace ObjDiff
{
  public static class Extensions
  {
    public static IEnumerable<Difference> Diff<T>(this T obj, T other, CompareOptions compareOptions = null) where T : class
    {
      return ObjDiff.Diff(obj, other, compareOptions);
    }


    public static void Patch<T>(this T obj, IEnumerable<Difference> differences) where T : class
    {
      ObjDiff.Patch(obj, differences);
    }
  }
}