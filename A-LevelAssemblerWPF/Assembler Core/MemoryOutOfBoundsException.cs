using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore 
{
    class MemoryOutOfBoundsException : Exception
    {

        public MemoryOutOfBoundsException() : base()
        {
        }
        public MemoryOutOfBoundsException(string message) : base(message)
        {
        }
        public MemoryOutOfBoundsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
