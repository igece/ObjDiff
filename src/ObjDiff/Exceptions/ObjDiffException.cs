using System;
using System.Runtime.Serialization;


namespace ObjDiff.Exceptions
{
    [Serializable]
    public class ObjDiffException : Exception
    {
        public ObjDiffException()
        {
        }

        public ObjDiffException(string message) : base(message)
        {
        }

        public ObjDiffException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ObjDiffException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}