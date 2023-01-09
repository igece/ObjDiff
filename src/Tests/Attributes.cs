using System;

using ObjDiff;
using ObjDiff.Attributes;

using Xunit;


namespace Tests
{
    internal class SampleInnerClass : IEquatable<SampleInnerClass>
    {
        public int InnerProperty1 { get; }

        public int InnerProperty2 { get; }


        public SampleInnerClass(int innerProperty1, int innerProperty2)
        {
            InnerProperty1 = innerProperty1;
            InnerProperty2 = innerProperty2;
        }


        public bool Equals(SampleInnerClass other)
        {
            return InnerProperty1.Equals(other.InnerProperty1) &&
                InnerProperty2.Equals(other.InnerProperty2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SampleInnerClass);
        }
    }

    internal class SampleClass : IEquatable<SampleClass>
    {
        public int Property1 { get; set; }

        [IgnoreDifferences]
        public int Property2 { get; set; }

        public SampleInnerClass Property3 { get; set; }

        public bool Equals(SampleClass other)
        {
            return Property1.Equals(other.Property1) &&
                Property2.Equals(other.Property2) &&
                Property3.Equals(other.Property3);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SampleClass);
        }
    }


    public class Attributes
    {
        [Fact(DisplayName = "Ignore properties marked with the IgnoreDifferences attribute")]
        public void IgnoreDifferencesAttribute()
        {
            var left = new SampleClass { Property1 = 1, Property2 = 2, Property3 = new SampleInnerClass(1, 2) };
            var right = new SampleClass { Property1 = 1, Property2 = 999, Property3 = new SampleInnerClass(1, 2) };

            var differences = left.Diff(right);

            Assert.Empty(differences);
        }
    }
}
