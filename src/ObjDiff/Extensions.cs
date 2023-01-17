using System.Collections.Generic;


namespace ObjDiff
{
    public static class Extensions
    {
        public static IEnumerable<Difference<T>> Diff<T>(this T left, T right, CompareOptions compareOptions = null) where T : class
        {
            return ObjDiff.Diff(left, right, compareOptions);
        }


        public static void Patch<T>(this T target, IEnumerable<Difference<T>> differences) where T : class
        {
            ObjDiff.Patch(target, differences);
        }


        public static bool IsEqualTo<T>(this T left, T right, CompareOptions compareOptions = null) where T : class
        {
            return ObjDiff.AreEqual(left, right, compareOptions);
        }


        public static void MakeEqualTo<T>(this T target, T source, CompareOptions compareOptions = null) where T : class
        {
            ObjDiff.MakeEqual(target, source, compareOptions);
        }
    }
}