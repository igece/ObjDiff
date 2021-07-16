using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ObjDiff;

using Xunit;


namespace Tests
{
  internal class CollectionsClass : IEquatable<CollectionsClass>
  {
    public IList<int> Int32List { get; set; }

    public ICollection<int> Int32Collection { get; set; }

    public IList<BasicTypesClass> ComplexTypeList { get; set; }

    public CollectionsClass()
    {
      Int32List = new List<int>();
      Int32Collection = new Collection<int>();
      ComplexTypeList = new List<BasicTypesClass>();
    }

    public bool Equals(CollectionsClass other)
    {
      if (other == null)
        return false;

      return Enumerable.SequenceEqual(Int32List, other.Int32List) &&
        Enumerable.SequenceEqual(Int32Collection, other.Int32Collection) &&
        Enumerable.SequenceEqual(ComplexTypeList, other.ComplexTypeList);
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as CollectionsClass);
    }
  }


  public class Collections
  {
    [Fact(DisplayName = "List<int> - No differences")]
    public void ListInt32NoDifferences()
    {
      var left = new CollectionsClass { Int32List = new List<int>() { 3, 25, 8 } };
      var right = new CollectionsClass { Int32List = new List<int>() { 3, 25, 8 } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "List<int> - One difference")]
    public void ListInt32OneDifference()
    {
      var left = new CollectionsClass { Int32List = new List<int>() { 7, 56, 3 } };
      var right = new CollectionsClass { Int32List = new List<int>() { 7, 14, 3 } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "List<int> - Patching difference")]
    public void ListInt32PatchingDifference()
    {
      var left = new CollectionsClass { Int32List = new List<int>() { 7, 56, 3 } };
      var right = new CollectionsClass { Int32List = new List<int>() { 7, 14, 3 } };

      left.MakeEqualTo(right);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Collection<int> - No differences")]
    public void CollectionInt32NoDifferences()
    {
      var left = new CollectionsClass { Int32Collection = new Collection<int>() { 9, 6, 13 } };
      var right = new CollectionsClass { Int32Collection = new Collection<int>() { 9, 6, 13 } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Collection<int> - One difference")]
    public void CollectionInt32OneDifference()
    {
      var left = new CollectionsClass { Int32Collection = new Collection<int>() { 9, 6, 13 } };
      var right = new CollectionsClass { Int32Collection = new Collection<int>() { -4, 6, 13 } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Collection<int> - Patching difference")]
    public void CollectionInt32PatchingDifference()
    {
      var left = new CollectionsClass { Int32Collection = new Collection<int>() { 9, 6, 13 } };
      var right = new CollectionsClass { Int32Collection = new Collection<int>() { -4, 6, 13 } };

      var differences = left.Diff(right);
      left.Patch(differences);

      Assert.Equal(left, right);
    }


    [Fact(DisplayName = "Collection<ComplexObject> - No differences")]
    public void CollectionComplexObjectNoDifferences()
    {
      var left = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass> { new BasicTypesClass { Int32Property = int.MinValue } } };
      var right = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass> { new BasicTypesClass { Int32Property = int.MinValue } } };

      var differences = left.Diff(right);

      Assert.Empty(differences);
    }


    [Fact(DisplayName = "Collection<ComplexObject> - One difference")]
    public void CollectionComplexObjectOneDifference()
    {
      var left = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass> { new BasicTypesClass { Int32Property = int.MinValue } } };
      var right = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass> { new BasicTypesClass { Int32Property = int.MaxValue } } };

      var differences = left.Diff(right);

      Assert.Single(differences);
    }


    [Fact(DisplayName = "Collection<ComplexObject> - Patching difference")]
    public void CollectionComplexObjectPatchingDifference()
    {
      var left = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass>() { new BasicTypesClass { Int32Property = int.MinValue } } };
      var right = new CollectionsClass { ComplexTypeList = new Collection<BasicTypesClass>() { new BasicTypesClass { Int32Property = int.MaxValue } } };

      var differences = left.Diff(right);
      left.Patch(differences);

      Assert.Equal(left, right);
    }
  }
}
