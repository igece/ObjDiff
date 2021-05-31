using System;


namespace ObjDiff
{
  public class CompareOptions
  {
    /// When comparing arrays/collections, items must be in the same order. Default value is true.
    public bool CollectionsSameOrder { get; set; }

    /// Ignore properties marked with any of the specified attribute names. None set by default.
    public string[] IgnoredAttributes { get; set; }

    /// Ignore properties named with any of the specified values. None set by default.
    public string[] IgnoredProperties { get; set; }

    /// Maximum number of children levels to dive into. Default value is 10.
    public uint MaxDepth { get; set; }


    public CompareOptions()
    {      
      CollectionsSameOrder = true;
      IgnoredAttributes = Array.Empty<string>();
      IgnoredProperties = Array.Empty<string>();
      MaxDepth = 10;
    }
  }
}