using System;

namespace ObjDiff
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreDifferencesAttribute : Attribute
    {
    }
}
