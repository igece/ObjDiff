namespace ObjDiff
{
  public class Difference
  {
    public string Path { get; private set; }

    public object LeftValue { get; private set; }

    public object RightValue { get; private set; }


    public Difference(string path, object leftValue, object rightValue)
    {
      Path = path;
      LeftValue = leftValue;
      RightValue = rightValue;
    }
  }
}