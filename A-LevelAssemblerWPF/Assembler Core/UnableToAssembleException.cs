using System;
using System.Runtime.Serialization;

namespace AssemblerCore
{
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

    }
}