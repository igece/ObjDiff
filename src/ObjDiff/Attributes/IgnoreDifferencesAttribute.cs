using System;

namespace ObjDiff.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreDifferencesAttribute : Attribute
    {
    }
}
