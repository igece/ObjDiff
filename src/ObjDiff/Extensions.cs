using System.Collections.Generic;


namespace ObjDiff
{
  public static class Extensions
  {
    public static IEnumerable<Difference> Diff<T>(this T left, T right, CompareOptions compareOptions = null) where T : class
    {
      return ObjDiff.Diff(left, right, compareOptions);
    }


    public static void Patch<T>(this T target, IEnumerable<Difference> differences) where T : class
    {
      ObjDiff.Patch(target, differences);
    }
  }
}