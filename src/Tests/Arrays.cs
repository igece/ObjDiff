using ObjDiff;

using Xunit;


namespace Tests
{
  internal class ArraysClass
  {
    public int[] Int32Array { get; set; }
  }


  public class Arrays
  {
    [Fact(DisplayName = "No differences")]
    public void NoDifferences()
    {
      var left = new ArraysClass { Int32Array = new int[] { 1, 3 } };
      var right = new ArraysClass { Int32Array = new int[] { 1, 3 } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "One difference")]
    public void OneDifference()
    {
      var left = new ArraysClass { Int32Array = new int[] { 1, 3 } };
      var right = new ArraysClass { Int32Array = new int[] { 1, 5 } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }
  }
}
