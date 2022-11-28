using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{

    public enum dataType
    {
        Decimal = 0,
        Hex = 1,
        Binary = 2
    }
    public class MemoryCell : INotifyPropertyChanged
    {
        private short address;
        private UInt32 internalValue;
        private bool breakPoint;
        private MemoryBank myBank;
       

        public event PropertyChangedEventHandler PropertyChanged;

        public string DefaultAddress
        {
            get
            {
                switch (MyBank.DefaultDataType)
                {
                    case dataType.Decimal:
                        return Convert.ToString(Address).PadLeft(4, '0');
                        
                    case dataType.Hex:
                        return HexAddress;

                    default:
                        return null;
                       
                }
                
            }
        }

        public string DefaultValue
        {
            get
            {
                switch (MyBank.DefaultDataType)
                {
                    case dataType.Decimal:
                        return Convert.ToString(Value).PadLeft(8, '0');

                    case dataType.Hex:
                        return HexValue;

                    default:
                        return null;

                }

            }

            set
            {
                switch (MyBank.DefaultDataType)
                {
                    case dataType.Decimal:
                        Value = Convert.ToUInt32(value);
                        break;

                    case dataType.Hex:
                        HexValue = value;
                        break;


                }
               
                OnPropertyChanged("DefaultValue");
            }
        }

        public string HexAddress
        {
            get
            {
                return address.ToString("X4");
            }
        }

        public string HexValue
        {
            get
            {
                return internalValue.ToString("X8");
            }
            set
            {
                try
                {
                    internalValue = (UInt32)Convert.ToInt32(value, 16);
                    OnPropertyChanged("HexValue");
                }
                catch (Exception ex)
                {
                    internalValue = 0;

                }
            }
        }

        public short Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
                OnPropertyChanged("Address");
            }
        }

        public uint Value
        {
            get
            {
                return internalValue;
            }

            set
            {
                this.internalValue = value;
                OnPropertyChanged("Value");
            }
        }

        public bool BreakPoint
        {
            get
            {
                return breakPoint;
            }

            set
            {
                breakPoint = value;
            }
        }

        public MemoryBank MyBank { get => myBank; set => myBank = value; }

        private void OnPropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }
    }
    public class MemoryBank
    {
       
        private List<MemoryCell> memory;
        private short memoryCapacity;
        private dataType defaultDataType = dataType.Hex;
        public dataType DefaultDataType { get => defaultDataType; set => defaultDataType = value; }

        public List<MemoryCell> Memory
        {
            get
            {
                return memory;
            }

            set
            {
                memory = value;
            }
        }

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
            GenerateMemoryBank();
        }

        public void GenerateMemoryBank()
        {
            Memory = new List<MemoryCell>();

            for (short cellCount = 0; cellCount < memoryCapacity; cellCount++)
            {
                MemoryCell theCell = new MemoryCell();
                theCell.Address = cellCount;
                theCell.Value = 0x00000000;
                theCell.BreakPoint = false;
                theCell.MyBank = this;
                Memory.Add(theCell);
            }
        }

        public bool isBreakPoint(short location)
        {
            if (location >= memoryCapacity)
            {
                return false;
            }
            return Memory[location].BreakPoint;
        }

        public UInt32 Read (short location) 
        {
            if (location >= memoryCapacity)
            {
                throw new MemoryOutOfBoundsException("Unable to read from " + location  + " as only " + memoryCapacity + " locations available, " + location + " out of bounds");
            }
            return Memory[location].Value;
        }

        public void Write(short location, UInt32 data)
        {
            if (location >= memoryCapacity)
            {
                throw new MemoryOutOfBoundsException("Unable to wrtie to " + location + " as only " + memoryCapacity + " locations available, " + location + " out of bounds");
            }
            int locationINdex = Memory.FindIndex(x => x.Address == location);
            MemoryCell tempCell =  Memory[locationINdex];
            tempCell.Value = data;
            Memory[locationINdex] = tempCell;
        }
    }
}
