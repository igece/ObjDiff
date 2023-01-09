using System;


namespace ObjDiff
{
    public class CompareOptions
    {
        /// <summary>
        /// When comparing arrays/collections, items must be in the same order. Default value is true.
        /// </summary>
        public bool CollectionsSameOrder { get; set; }

        /// <summary>
        /// Ignore properties marked with any of the specified attribute names. None set by default.
        /// </summary>
        public string[] IgnoredAttributes { get; set; }

        /// <summary>
        /// Ignore properties named with any of the specified values. None set by default.
        /// </summary>
        public string[] IgnoredProperties { get; set; }

        /// <summary>
        /// Don't dive inside any property named with any of the specified values. None set by default.
        /// </summary>
        public string[] DontDiveProperties { get; set; }

        /// <summary>
        /// Maximum number of children levels to dive into. Default value is 10.
        /// <summary>
        public uint MaxDepth { get; set; }


        public CompareOptions()
        {
            CollectionsSameOrder = true;
            IgnoredAttributes = Array.Empty<string>();
            IgnoredProperties = Array.Empty<string>();
            DontDiveProperties = Array.Empty<string>();
            MaxDepth = 10;
        }
    }
}