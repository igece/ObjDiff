namespace ObjDiff
{
  public class Difference
  {
    /// Full path of the element were values differ between compared objects.
    public string Path { get; private set; }

    /// Value of the object being compared.
    public object LeftValue { get; private set; }

    /// Value of the object against LeftValue is being compared.
    public object RightValue { get; private set; }


    public Difference(string path, object leftValue, object rightValue)
    {
      Path = path;
      LeftValue = leftValue;
      RightValue = rightValue;
    }
  }
}