using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
  internal class BasicTypesClass
  {
    public string StringProperty { get; set; }

    public int Int32Property { get; set; }

    public long Int64Property { get; set; }

    public float SingleProperty { get; set; }

    public double DoubleProperty { get; set; }

    public decimal DecimalProperty { get; set; }
  }


  public class BasicTypes
  {
    [Fact(DisplayName = "No differences")]
    public void NoDifferences()
    {
      var left = new BasicTypesClass { Int32Property = 12 };
      var right = new BasicTypesClass { Int32Property = 12 };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "One difference")]
    public void Test2()
    {
      var left = new BasicTypesClass { StringProperty = "Left Value" };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Multiple differences")]
    public void Test3()
    {
      var left = new BasicTypesClass { Int32Property = 5 };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      var differences = left.Diff(right);

      Assert.True(differences.Count() == 2);
    }
  }
}
