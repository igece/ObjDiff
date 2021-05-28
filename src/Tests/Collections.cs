using System.Collections.Generic;
using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
  internal class CollectionsClass
  {
    public IList<int> Int32List { get; set; }

    public IList<BasicTypesClass> ComplexTypeList { get; set; }
  }


  public class Collections
  {
    [Fact(DisplayName = "No differences")]
    public void NoDifferences()
    {
      var left = new CollectionsClass { Int32List = new List<int>() { 3, 25, 8 } };
      var right = new CollectionsClass { Int32List = new List<int>() { 3, 25, 8 } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "One difference")]
    public void Test4()
    {
      var left = new CollectionsClass { Int32List = new List<int>() { 7, 56, 3 } };
      var right = new CollectionsClass { Int32List = new List<int>() { 7, 14, 3 } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }
  }
}
