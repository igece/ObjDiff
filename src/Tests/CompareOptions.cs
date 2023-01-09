using ObjDiff;

using Xunit;


namespace Tests
{
    public class CompareOptions
    {
        [Fact(DisplayName = "Don't dive on selected property, compare property values instead")]
        public void DontDeepOnProperty()
        {
            var left = new SampleClass { Property1 = 1, Property2 = 2, Property3 = new SampleInnerClass(1, 2) };
            var right = new SampleClass { Property1 = 1, Property2 = 999, Property3 = new SampleInnerClass(3, 4) };
            var compareOptions = new ObjDiff.CompareOptions { DontDiveProperties = new string[] { "Property3" } };

            left.MakeEqualTo(right, compareOptions);

            Assert.True(left.IsEqualTo(right, compareOptions));
        }
    }
}
