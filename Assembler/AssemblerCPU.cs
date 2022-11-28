using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{
    public class AssemblerCPU
    {
        //Memory
        MemoryBank RAM;

        public byte regMax = 12;

        //Current OpCode
        Action opCodeRunTime;

        //Decoder
        ushort operation;
        AssemblyCommand.OP opCode;
        AssemblyCommand.COND condition;
        byte rn;
        byte rd;
        byte rm;
        short operand;
        bool immediate;
        bool setCondition;
        AssemblyCommand.COND currentCondition = AssemblyCommand.COND.AL;

        bool haltCondition = false;

        // Special Purpose Registers
        UInt32 accumulator;
        UInt32 memoryAddressReguister;
        UInt32 memoryDataRegister;
        UInt32 currentInstructionRegister;
        UInt32 programCounter;

        //general purpose registers
        UInt32[] registers;
       
        public uint Accumulator
        {
            get
            {
                return accumulator;
            }

            set
            {
                accumulator = value;
            }
        }

        public uint MemoryAddressReguister
        {
            get
            {
                return memoryAddressReguister;
            }

            set
            {
                memoryAddressReguister = value;
            }
        }

        public uint MemoryDataRegister
        {
            get
            {
                return memoryDataRegister;
            }

            set
            {
                memoryDataRegister = value;
            }
        }

        public uint CurrentInstructionRegister
        {
            get
            {
                return currentInstructionRegister;
            }

            set
            {
                currentInstructionRegister = value;
            }
        }

        public uint ProgramCounter
        {
            get
            {
                return programCounter;
            }

            set
            {
                programCounter = value;
            }
        }

/*        public uint R1
        {
            get
            {
                return registers[0];
            }

            set
            {
                r1 = value;
            }
        }

        public uint R2
        {
            get
            {
                return r2;
            }

            set
            {
                r2 = value;
            }
        }

        public uint R3
        {
            get
            {
                return r3;
            }

            set
            {
                r3 = value;
            }
        }

        public uint R4
        {
            get
            {
                return r4;
            }

            set
            {
                r4 = value;
            }
        }

        public uint R5
        {
            get
            {
                return r5;
            }

            set
            {
                r5 = value;
            }
        }

        public uint R6
        {
            get
            {
                return r6;
            }

            set
            {
                r6 = value;
            }
        }

        public uint R7
        {
            get
            {
                return r7;
            }

            set
            {
                r7 = value;
            }
        }

        public uint R8
        {
            get
            {
                return r8;
            }

            set
            {
                r8 = value;
            }
        }

        public uint R9
        {
            get
            {
                return r9;
            }

            set
            {
                r9 = value;
            }
        }

        public uint R10
        {
            get
            {
                return r10;
            }

            set
            {
                r10 = value;
            }
        }

        public uint R11
        {
            get
            {
                return r11;
            }

            set
            {
                r11 = value;
            }
        }

        public uint R12
        {
            get
            {
                return r12;
            }

            set
            {
                r12 = value;
            }
        }*/

         public UInt32 GetRegister(byte theReg)
        {
            return registers[theReg];
        }

        public void SetRegister(byte theReg, UInt32 theValue)
        {
            registers[theReg] = theValue;
        }

        public AssemblerCPU(MemoryBank newRAM)
        {
            RAM = newRAM;
            Reset();
        }


        public AssemblerCPU ()
        {
            RAM = new MemoryBank();
            Reset();
        }

        private void Reset()
        {
            registers = new UInt32[regMax];
            ProgramCounter = 0;
            haltCondition = false;
        }

        public void Cycle()
        {
            if (!haltCondition)
            {
                fetch();
                decode();
            }
        }
        private void fetch()
        {
            MemoryAddressReguister = ProgramCounter++;
            MemoryDataRegister = RAM.Read((short)MemoryAddressReguister);
            CurrentInstructionRegister = MemoryDataRegister;
        }

        private void decode()
        {
            AssemblyCommand thisCommand = new AssemblyCommand(CurrentInstructionRegister);
            opCode = (AssemblyCommand.OP)thisCommand.Opcode;
            condition = thisCommand.Condition;
            rn = thisCommand.FirstRegister;
            rd = thisCommand.DestinationRegister;
            rm = thisCommand.SecondRegister;
            immediate = thisCommand.Immediate;
            setCondition = thisCommand.SetCondition;
            operand = thisCommand.OperandTwo;
            
            switch (opCode) // Execute
            {
                case AssemblyCommand.OP.ADD:
                    ADD();
                    break;
                case AssemblyCommand.OP.AND:
                    AND();
                    break;

                case AssemblyCommand.OP.B:
                    Branch();
                    break;

                case AssemblyCommand.OP.CMP:
                    CMP();
                    break;

                case AssemblyCommand.OP.EOR:
                    EOR();
                    break;

                case AssemblyCommand.OP.HALT:
                    haltCondition = true;
                    break;

                case AssemblyCommand.OP.LDR:
                    SetRegister(rd, RAM.Read(operand));
                    break;

                case AssemblyCommand.OP.LSL:
                    LSL();
                    break;

                case AssemblyCommand.OP.LSR:
                    LSR();
                    break;

                case AssemblyCommand.OP.MOV:
                    MOV();
                    break;

                case AssemblyCommand.OP.MVN:
                    MVN();
                    break;

                case AssemblyCommand.OP.ORR:
                    ORR();
                    break;

                case AssemblyCommand.OP.STR:
                    RAM.Write(operand, GetRegister(rd));
                    break;

                case AssemblyCommand.OP.SUB:
                    SUB();
                    break;




                default:
                    break;
            }
        }

        private void CMP()
        {
            UInt32 compare1 = (UInt32)rn;
            UInt32 compare2 = 0;
            if (immediate)
            {
                compare2 = GetRegister(rn) & (UInt32)operand;
            }
            else
            {
                compare2 = GetRegister(rn) & GetRegister(rm);
            }

            if(compare1 == compare2)
            {
                currentCondition = AssemblyCommand.COND.EQ;
            }
            else if (compare1 < compare2)
            {
                currentCondition = AssemblyCommand.COND.LT;
            }
            else
            {
                currentCondition = AssemblyCommand.COND.GT;
            }
        }

        private void Branch()
        {
            if (setCondition)
            {
                switch (currentCondition)
                {
                    case AssemblyCommand.COND.AL:
                        ProgramCounter = (UInt32)operand;
                        break;

                    case AssemblyCommand.COND.NE:
                        if (condition == AssemblyCommand.COND.LT || condition == AssemblyCommand.COND.GT)
                        {
                            ProgramCounter = (UInt32)operand;
                        }
                        break;

                    case AssemblyCommand.COND.EQ:
                        if (condition == AssemblyCommand.COND.EQ)
                        {
                            ProgramCounter = (UInt32)operand;
                        }
                        break;

                    case AssemblyCommand.COND.GT:
                        if (condition == AssemblyCommand.COND.GT)
                        {
                            ProgramCounter = (UInt32)operand;
                        }
                        break;

                    case AssemblyCommand.COND.LT:
                        if (condition == AssemblyCommand.COND.LT)
                        {
                            ProgramCounter = (UInt32)operand;
                        }
                        break;
                }
            }
            else
            {
                ProgramCounter = (UInt32)operand;
            }
        }

        private void MOV()
        {
            if (immediate)
            {
                SetRegister(rd, (UInt32)operand);
            }
            else
            {
                SetRegister(rd, GetRegister(rm));
            }
        }

        private void AND()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) & (UInt32)operand;
            }
            else
            {
                Answer = GetRegister(rn) & GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
        private void ORR()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) | (UInt32)operand;
            }
            else
            {
                Answer = GetRegister(rn) | GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
        private void MVN()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = ~ (UInt32)operand;
            }
            else
            {
                Answer = ~ GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }

        private void LSL()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) << (int)operand;
            }
            else
            {
                Answer = GetRegister(rn) << (int)GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
        private void LSR()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) >> (int)operand;
            }
            else
            {
                Answer = GetRegister(rn) >> (int)GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
       
        private void EOR()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) ^ (UInt32)operand;
            }
            else
            {
                Answer = GetRegister(rn) ^ GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }

        private void ADD()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) + (UInt32)operand;
            }
            else
            {
                Answer = GetRegister(rn) + GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
        private void SUB()
        {
            UInt32 Answer = 0;
            if (immediate)
            {
                Answer = GetRegister(rn) - (UInt32)operand;
            }
            else
            {
                Answer = GetRegister(rn) - GetRegister(rm);
            }
            SetRegister(rd, Answer);
        }
    }
}
