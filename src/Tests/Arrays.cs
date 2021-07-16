using System;
using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
  internal class ArraysClass : IEquatable<ArraysClass>
  {
    public int[] Int32Array { get; set; }

    public BasicTypesClass[] ComplexTypeArray { get; set; }

    public ArraysClass()
    {
      Int32Array = Array.Empty<int>();
      ComplexTypeArray = Array.Empty<BasicTypesClass>();
    }

    public bool Equals(ArraysClass other)
    {
      if (other == null)
        return false;

      return Enumerable.SequenceEqual(Int32Array, other.Int32Array) &&
        Enumerable.SequenceEqual(ComplexTypeArray, other.ComplexTypeArray);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as ArraysClass);
    }
  }


  public class Arrays
  {
    [Fact(DisplayName = "int[] - No differences")]
    public void ArrayInt32NoDifferences()
    {
      var left = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue } };
      var right = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "int[] - One difference")]
    public void ArrayInt32OneDifference()
    {
      var left = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue } };
      var right = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue - 1 } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "int[] - Patching difference")]
    public void ArrayInt32PatchingDifference()
    {
      var left = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue } };
      var right = new ArraysClass { Int32Array = new int[] { int.MinValue, int.MaxValue - 1 } };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }
  }
}
