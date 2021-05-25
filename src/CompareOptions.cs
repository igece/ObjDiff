using System;


namespace ObjDiff
{
  public class CompareOptions
  {
    public uint MaxDepth { get; set; }

    public string[] IgnoredProperties { get; set; }

    public string[] IgnoredAttributes { get; set; }

    public bool CollectionsSameOrder { get; set; }


    public CompareOptions()
    {
      MaxDepth = 10;
      IgnoredProperties = Array.Empty<string>();
      IgnoredAttributes = Array.Empty<string>();
      CollectionsSameOrder = true;
    }
  }
}