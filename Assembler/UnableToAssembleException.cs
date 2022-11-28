using System;
using System.Runtime.Serialization;

namespace AssemblerCore
{
    [Serializable]
    internal class UnableToAssembleException : Exception
    {
        public UnableToAssembleException()
        {
        }

        public UnableToAssembleException(string message) : base(message)
        {
        }

        public UnableToAssembleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnableToAssembleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}