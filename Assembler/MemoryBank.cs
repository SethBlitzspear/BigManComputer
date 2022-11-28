using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{
    public class MemoryBank
    {

        private UInt32[] memory;
        private short memoryCapacity;

        public MemoryBank(short capacity)
        {
            GenerateMemoryBank(capacity);
        }

        public MemoryBank()
        {
            GenerateMemoryBank(4096);
        }

        private void GenerateMemoryBank(short capacity)
        {
            memoryCapacity = capacity;
            memory = new UInt32[memoryCapacity];
            for (int cellCount = 0; cellCount < memoryCapacity; cellCount++)
            {
                memory[cellCount] = 0x00000000; 
            }
        }

        public UInt32 Read (short location) 
        {
            if (location >= memoryCapacity)
            {
                throw new MemoryOutOfBoundsException("Unable to read from " + location  + " as only " + memoryCapacity + " locations available, " + location + " out of bounds");
            }
            return memory[location];
        }

        public void Write(short location, UInt32 data)
        {
            if (location >= memoryCapacity)
            {
                throw new MemoryOutOfBoundsException("Unable to wrtie to " + location + " as only " + memoryCapacity + " locations available, " + location + " out of bounds");
            }
            memory[location] = data;
        }
    }
}
