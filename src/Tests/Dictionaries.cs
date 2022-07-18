using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
    internal class DictionaryBasedType : Dictionary<string, string>, IEquatable<DictionaryBasedType>
    {
        public bool Equals(DictionaryBasedType other)
        {
            if (other == null)
                return false;

            return Enumerable.SequenceEqual(this, other);
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as DictionariesClass);
        }
    }


    internal class DictionariesClass : IEquatable<DictionariesClass>
    {
        public IDictionary<int, string> Int32StringDict { get; set; }

        public IDictionary<string, BasicTypesClass> StringComplexTypeDict { get; set; }

        public DictionaryBasedType DictionaryBasedType { get; set; }



        public DictionariesClass()
        {
            Int32StringDict = new Dictionary<int, string>();
            StringComplexTypeDict = new Dictionary<string, BasicTypesClass>();
            DictionaryBasedType = new DictionaryBasedType();
        }

        public bool Equals(DictionariesClass other)
        {
            if (other == null)
                return false;

            return Enumerable.SequenceEqual(Int32StringDict, other.Int32StringDict) &&
              Enumerable.SequenceEqual(StringComplexTypeDict, other.StringComplexTypeDict) &&
              Enumerable.SequenceEqual(DictionaryBasedType, other.DictionaryBasedType);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DictionariesClass);
        }
    }


    public class Dictionaries
    {
        [Fact(DisplayName = "Dictionary<int, string> - No differences")]
        public void DictInt32StringNoDifferences()
        {
            var left = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MinValue] = "Left Value" } };
            var right = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MinValue] = "Left Value" } };

            var differences = left.Diff(right);

            Assert.Empty(differences);
        }


        [Fact(DisplayName = "Dictionary<int, string> - One difference")]
        public void DictInt32StringOneDifference()
        {
            var left = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MinValue] = "Left Value" } };
            var right = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MaxValue] = "Right Value" } };

            var differences = left.Diff(right);

            Assert.Single(differences);
        }


        [Fact(DisplayName = "Dictionary<int, string> - Patching difference")]
        public void DictInt32StringPatchingDifference()
        {
            var left = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MinValue] = "Left Value" } };
            var right = new DictionariesClass { Int32StringDict = new Dictionary<int, string> { [int.MaxValue] = "Right Value" } };

            var differences = left.Diff(right);
            left.Patch(differences);

            Assert.Equal(left, right);
        }


        [Fact(DisplayName = "Dictionary<int, string> - Trying to patch readonly dictionary")]
        public void DictInt32StringTryingPatchReadOnly()
        {
            var left = new DictionariesClass { Int32StringDict = new ReadOnlyDictionary<int, string>(new Dictionary<int, string> { [int.MinValue] = "Left Value" }) };
            var right = new DictionariesClass { Int32StringDict = new ReadOnlyDictionary<int, string>(new Dictionary<int, string> { [int.MaxValue] = "Right Value" }) };

            var differences = left.Diff(right);

            Assert.Throws<NotSupportedException>(() => left.Patch(differences));
        }



        [Fact(DisplayName = "Dictionary<string, ComplexType> - No differences")]
        public void DictStringComplexTypeNoDifferences()
        {
            var left = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MinValue } } };
            var right = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MinValue } } };

            var differences = left.Diff(right);

            Assert.Empty(differences);
        }


        [Fact(DisplayName = "Dictionary<string, ComplexType> - One difference")]
        public void DictStringComplexTypeOneDifference()
        {
            var left = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MinValue } } };
            var right = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MaxValue } } };

            var differences = left.Diff(right);

            Assert.Single(differences);
        }


        [Fact(DisplayName = "Dictionary<string, ComplexType> - Patching difference")]
        public void DictStringComplexTypePatchingDifference()
        {
            var left = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MinValue } } };
            var right = new DictionariesClass { StringComplexTypeDict = new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MaxValue } } };

            var differences = left.Diff(right);
            left.Patch(differences);

            Assert.Equal(left, right);
        }


        [Fact(DisplayName = "Dictionary<string, ComplexType> - Trying to patch readonly dictionary")]
        public void DictStringComplexTypeTryingPatchReadOnly()
        {
            var left = new DictionariesClass {
                StringComplexTypeDict = new ReadOnlyDictionary<string, BasicTypesClass>(new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MinValue } })
            };

            var right = new DictionariesClass {
                StringComplexTypeDict = new ReadOnlyDictionary<string, BasicTypesClass>(new Dictionary<string, BasicTypesClass> { ["Left Value"] = new BasicTypesClass { Int32Property = int.MaxValue } })
            };

            var differences = left.Diff(right);

            Assert.Throws<NotSupportedException>(() => left.Patch(differences));
        }


        [Fact(DisplayName = "Dictionary<string, string> specialization - No differences")]
        public void DictBasedClassNoDifferences()
        {
            var left = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item1" }
            };

            var right = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item1" }
            };

            var differences = left.Diff(right);

            Assert.Empty(differences);
        }


        [Fact(DisplayName = "Dictionary<string, string> specialization - One difference (different keys)")]
        public void DictBasedClassOneDifferenceKey()
        {
            var left = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key1"] = "Item1" }
            };

            var right = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key2"] = "Item1" }
            };

            var differences = left.Diff(right);

            Assert.Single(differences);
        }


        [Fact(DisplayName = "Dictionary<string, string> specialization - One difference (different values for same key)")]
        public void DictBasedClassOneDifferenceValue()
        {
            var left = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item1" }
            };

            var right = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item2" }
            };

            var differences = left.Diff(right);

            Assert.Single(differences);
        }


        [Fact(DisplayName = "Dictionary<string, string> specialization - Patching difference (different values for same key)")]
        public void DictBasedClassPatchingDifferenceOnKey()
        {
            var left = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item1" }
            };

            var right = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key"] = "Item2" }
            };

            var differences = left.Diff(right);
            left.Patch(differences);

            Assert.Equal(left, right);
        }


        [Fact(DisplayName = "Dictionary<string, string> specialization - Patching difference (different values and keys)")]
        public void DictBasedClassPatchingDifferenceOnKeyAndValue()
        {
            var left = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key1"] = "Item1" }
            };

            var right = new DictionariesClass {
                DictionaryBasedType = new DictionaryBasedType { ["Key2"] = "Item2" }
            };

            var differences = left.Diff(right);
            left.Patch(differences);

            Assert.Equal(left, right);
        }
    }
}
