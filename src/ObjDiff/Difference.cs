using System;


namespace ObjDiff
{
    /// <summary>
    /// The use of generics ensures that a set of differences obtained from two instances
    /// of type A cannot be applied to an instance of type B (the compiler will complain).
    /// </summary>
    /// <typeparam name="T">Class of the compared objects this difference relates to.</typeparam>
    public class Difference<T>
    {
        /// <summary>
        /// Type of the compared objects this difference relates to.
        /// </summary>
        public Type RelatedType { get; private set; }

        /// Full path of the element were values differ between compared objects.
        public string Path { get; private set; }

        /// Value of the object being compared.
        public object LeftValue { get; private set; }

        /// Value of the object against LeftValue is being compared.
        public object RightValue { get; private set; }


        public Difference(string path, object leftValue, object rightValue)
        {
            RelatedType = typeof(T);
            Path = path;
            LeftValue = leftValue;
            RightValue = rightValue;
        }
    }
}