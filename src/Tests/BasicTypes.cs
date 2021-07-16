using System;
using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
  internal class BasicTypesClass : IEquatable<BasicTypesClass>
  {
    public int Int32Property { get; set; }

    public long Int64Property { get; set; }

    public float SingleProperty { get; set; }

    public double DoubleProperty { get; set; }

    public decimal DecimalProperty { get; set; }

    public string StringProperty { get; set; }

    public DateTime DateTimeProperty { get; set; }

    public DateTimeOffset DateTimeOffsetProperty { get; set; }

    public TimeSpan TimeSpanProperty { get; set; }

    public Guid GuidProperty { get; set; }


    public bool Equals(BasicTypesClass other)
    {
      return Equals(StringProperty, other.StringProperty) &&
        Equals(Int32Property, other.Int32Property) &&
        Equals(Int64Property, other.Int64Property) &&
        Equals(SingleProperty, other.SingleProperty) &&
        Equals(DoubleProperty, other.DoubleProperty) &&
        Equals(DecimalProperty, other.DecimalProperty) &&
        DateTime.Equals(DateTimeProperty, other.DateTimeProperty) &&
        DateTimeOffset.Equals(DateTimeOffsetProperty, other.DateTimeOffsetProperty) &&
        Equals(GuidProperty, other.GuidProperty);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as BasicTypesClass);
    }
  }


  public class BasicTypes
  {
    [Fact(DisplayName = "Int32 - No differences")]
    public void Int32NoDifferences()
    {
      var left = new BasicTypesClass { Int32Property = int.MinValue };
      var right = new BasicTypesClass { Int32Property = int.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Int32 - One difference")]
    public void Int32OneDifference()
    {
      var left = new BasicTypesClass { Int32Property = int.MinValue };
      var right = new BasicTypesClass { Int32Property = int.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Int32 - Patching difference")]
    public void Int32PatchingDifference()
    {
      var left = new BasicTypesClass { Int32Property = int.MinValue };
      var right = new BasicTypesClass { Int32Property = int.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Int64 - No differences")]
    public void Int64NoDifferences()
    {
      var left = new BasicTypesClass { Int64Property = long.MinValue };
      var right = new BasicTypesClass { Int64Property = long.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Int64 - One difference")]
    public void Int64OneDifference()
    {
      var left = new BasicTypesClass { Int64Property = long.MinValue };
      var right = new BasicTypesClass { Int64Property = long.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Int64 - Patching difference")]
    public void Int64PatchingDifference()
    {
      var left = new BasicTypesClass { Int64Property = long.MinValue };
      var right = new BasicTypesClass { Int64Property = long.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Float - No differences")]
    public void FloatNoDifferences()
    {
      var left = new BasicTypesClass { SingleProperty = float.MinValue };
      var right = new BasicTypesClass { SingleProperty = float.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Float - One difference")]
    public void FloatOneDifference()
    {
      var left = new BasicTypesClass { SingleProperty = float.MinValue };
      var right = new BasicTypesClass { SingleProperty = float.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Float - Patching difference")]
    public void FloatPatchingDifference()
    {
      var left = new BasicTypesClass { SingleProperty = float.MinValue };
      var right = new BasicTypesClass { SingleProperty = float.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Double - No differences")]
    public void DoubleNoDifferences()
    {
      var left = new BasicTypesClass { SingleProperty = float.MinValue };
      var right = new BasicTypesClass { SingleProperty = float.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Double - One difference")]
    public void DoubleOneDifference()
    {
      var left = new BasicTypesClass { DoubleProperty = double.MinValue };
      var right = new BasicTypesClass { DoubleProperty = double.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Double - Patching difference")]
    public void DoublePatchingDifference()
    {
      var left = new BasicTypesClass { DoubleProperty = double.MinValue };
      var right = new BasicTypesClass { DoubleProperty = double.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Decimal - No differences")]
    public void DecimalNoDifferences()
    {
      var left = new BasicTypesClass { DecimalProperty = decimal.MinValue };
      var right = new BasicTypesClass { DecimalProperty = decimal.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Decimal - One difference")]
    public void DecimalOneDifference()
    {
      var left = new BasicTypesClass { DecimalProperty = decimal.MinValue };
      var right = new BasicTypesClass { DecimalProperty = decimal.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Decimal - Patching difference")]
    public void DecimalPatchingDifference()
    {
      var left = new BasicTypesClass { DecimalProperty = decimal.MinValue };
      var right = new BasicTypesClass { DecimalProperty = decimal.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "String - No differences")]
    public void StringNoDifferences()
    {
      var left = new BasicTypesClass { StringProperty = "Value" };
      var right = new BasicTypesClass { StringProperty = "Value" };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "String - One difference")]
    public void StringOneDifference()
    {
      var left = new BasicTypesClass { StringProperty = "Left Value" };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "String - Patching difference")]
    public void StringPatchingDifference()
    {
      var left = new BasicTypesClass { StringProperty = "Left Value" };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "DateTime - No differences")]
    public void DateTimeNoDifferences()
    {
      var left = new BasicTypesClass { DateTimeProperty = DateTime.MinValue };
      var right = new BasicTypesClass { DateTimeProperty = DateTime.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "DateTime - One difference")]
    public void DateTimeOneDifference()
    {
      var left = new BasicTypesClass { DateTimeProperty = DateTime.MinValue };
      var right = new BasicTypesClass { DateTimeProperty = DateTime.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "DateTime - Patching difference")]
    public void DateTimePatchingDifference()
    {
      var left = new BasicTypesClass { DateTimeProperty = DateTime.MinValue };
      var right = new BasicTypesClass { DateTimeProperty = DateTime.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "DateTimeOffset - No differences")]
    public void DateTimeOffsetNoDifferences()
    {
      var left = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MinValue };
      var right = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "DateTimeOffset - One difference")]
    public void DateTimeOffsetOneDifference()
    {
      var left = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MinValue };
      var right = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "DateTimeOffset - Patching difference")]
    public void DateTimeOffsetPatchingDifference()
    {
      var left = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MinValue };
      var right = new BasicTypesClass { DateTimeOffsetProperty = DateTimeOffset.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "TimeSpan - No differences")]
    public void TimeSpanNoDifferences()
    {
      var left = new BasicTypesClass { TimeSpanProperty = TimeSpan.MinValue };
      var right = new BasicTypesClass { TimeSpanProperty = TimeSpan.MinValue };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "TimeSpan - One difference")]
    public void TimeSpanOneDifference()
    {
      var left = new BasicTypesClass { TimeSpanProperty = TimeSpan.MinValue };
      var right = new BasicTypesClass { TimeSpanProperty = TimeSpan.MaxValue };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "TimeSpan - Patching difference")]
    public void TimeSpanPatchingDifference()
    {
      var left = new BasicTypesClass { TimeSpanProperty = TimeSpan.MinValue };
      var right = new BasicTypesClass { TimeSpanProperty = TimeSpan.MaxValue };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Guid - No differences")]
    public void GuidNoDifferences()
    {
      var left = new BasicTypesClass { GuidProperty = Guid.Empty };
      var right = new BasicTypesClass { GuidProperty = Guid.Empty };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Guid - One difference")]
    public void GuidOneDifference()
    {
      var left = new BasicTypesClass { GuidProperty = Guid.NewGuid() };
      var right = new BasicTypesClass { GuidProperty = Guid.NewGuid() };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Guid - Patching difference")]
    public void GuidPatchingDifference()
    {
      var left = new BasicTypesClass { GuidProperty = Guid.NewGuid() };
      var right = new BasicTypesClass { GuidProperty = Guid.NewGuid() };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Multiple differences")]
    public void MultipleDifferences()
    {
      var left = new BasicTypesClass { Int32Property = int.MinValue };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      var differences = left.Diff(right);

      Assert.True(differences.Count() == 2);
    }


    [Fact(DisplayName = "Patching multiple differences")]
    public void PatchingMultipleDifferences()
    {
      var left = new BasicTypesClass { Int32Property = int.MinValue };
      var right = new BasicTypesClass { StringProperty = "Right Value" };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }
  }
}
