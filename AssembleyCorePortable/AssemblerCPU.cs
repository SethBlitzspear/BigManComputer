using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{

    public enum CPUStatus : UInt32
    {
        AssToRAM = 1,
        AddBus = 2,
        DatBus = 4,
        PCToMAR = 8,
        PCToPC = 16,
        MBRToCIR = 32,
        CIRToALU = 64,
        CIRToREG = 128,
        ALUToReg = 256,
        PC = 512,
        MBR = 1024,
        RAM = 2048,
        CIR = 4096,
        ALU = 8192,
        MAR = 16384,
        CIRToCU = 32768,
        REGToREG = 65536,
        R1 = 131072,
        R2 = 262144,
        R3 = 524288,
        R4 = 1048576,
        R5 = 2097152,
        R6 = 4194304,
        R7 = 8388608,
        R8 = 16777216,
        R9 = 33554432,
        R10 = 67108864,
        R11 = 134217728,
        R0 = 268435456,
        REGToALU = 536870912,
        CIRToMAR = 1073741824,
        MBRToReg = 2147483648




    }
    public class AssemblerCPU
    {
        //Memory
        MemoryBank RAM;

        public byte regMax = 12;

        //Current OpCode
        Action opCodeRunTime;


        private bool steppingMode = false;
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
        private UInt32 ALUAnswer = 0;

        private UInt32 status;
        private string stepSpeed;

        bool haltCondition = false;
        bool safetyBrake = true;
        bool safetyBrakeEngaged = false;
        bool breakPointHit = false;
        int cycleNumber = 0;

        private string cpuStatus = "";  // Verbose Description of what has happened
        private string cpuCondition = ""; // Idle / Stepping/Running/Halted
        private string cpuCycle = ""; // Fetch/Decode/Execute/null

        Int32 compare1 = 0;
        Int32 compare2 = 0;

        UInt32 value1;
        UInt32 value2;


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

        public bool SteppingMode
        {
            get
            {
                return steppingMode;
            }

            set
            {
                if (!value)
                {
                    ResetProgress();
                }
                steppingMode = value;
            }
        }

        public bool HaltCondition
        {
            get
            {
                return haltCondition;
            }

            set
            {
                haltCondition = value;
            }
        }

        public string CpuStatus
        {
            get
            {
                return cpuStatus;
            }

            set
            {
                cpuStatus = value;
            }
        }

        public uint Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }

        public string CpuCondition
        {
            get
            {
                return cpuCondition;
            }

            set
            {
                cpuCondition = value;
            }
        }

        public string CpuCycle
        {
            get
            {
                return cpuCycle;
            }

            set
            {
                cpuCycle = value;
            }
        }

        public bool CycleComplete
        {
            get
            {
                return cycleComplete;
            }

            set
            {
                cycleComplete = value;
            }
        }

        public string StepSpeed
        {
            get
            {
                return stepSpeed;
            }

            set
            {
                stepSpeed = value;
            }
        }

        public bool SafetyBrake
        {
            get
            {
                return safetyBrake;
            }

            set
            {
                safetyBrake = value;
            }
        }

        public int CycleNumber
        {
            get
            {
                return cycleNumber;
            }

            set
            {
                cycleNumber = value;
            }
        }

        public bool SafetyBrakeEngaged
        {
            get
            {
                return safetyBrakeEngaged;
            }

            set
            {
                safetyBrakeEngaged = value;
            }
        }

        public bool BreakPointHit
        {
            get
            {
                return breakPointHit;
            }

            set
            {
                breakPointHit = value;
            }
        }

        public AssemblyCommand.OP OpCode
        {
            get
            {
                return opCode;
            }

            set
            {
                opCode = value;
            }
        }

        public AssemblyCommand.COND Condition
        {
            get
            {
                return condition;
            }

            set
            {
                condition = value;
            }
        }

        public byte Rn
        {
            get
            {
                return rn;
            }

            set
            {
                rn = value;
            }
        }

        public byte Rd
        {
            get
            {
                return rd;
            }

            set
            {
                rd = value;
            }
        }

        public byte Rm
        {
            get
            {
                return rm;
            }

            set
            {
                rm = value;
            }
        }

        public short Operand
        {
            get
            {
                return operand;
            }

            set
            {
                operand = value;
            }
        }

        public bool Immediate
        {
            get
            {
                return immediate;
            }

            set
            {
                immediate = value;
            }
        }

        public bool SetCondition
        {
            get
            {
                return setCondition;
            }

            set
            {
                setCondition = value;
            }
        }

        public AssemblyCommand.COND CurrentCondition
        {
            get
            {
                return currentCondition;
            }

            set
            {
                currentCondition = value;
            }
        }

        public uint Value1
        {
            get
            {
                return value1;
            }

            set
            {
                value1 = value;
            }
        }

        public uint Value2
        {
            get
            {
                return value2;
            }

            set
            {
                value2 = value;
            }
        }

        public uint ALUAnswer1
        {
            get
            {
                return ALUAnswer;
            }

            set
            {
                ALUAnswer = value;
            }
        }

        public void ResetStatus()
        {
            Status = 0;
        }

        public void AddStatus (CPUStatus newStatus)
        {
            Status = Status | (UInt32)newStatus;
        }

        public void AddRegisterStatus(int reg)
        {
            switch(reg)
            {
                case 1:
                    AddStatus(CPUStatus.R1);
                    break;
                case 2:
                    AddStatus(CPUStatus.R2);
                    break;
                case 3:
                    AddStatus(CPUStatus.R3);
                    break;
                case 4:
                    AddStatus(CPUStatus.R4);
                    break;
                case 5:
                    AddStatus(CPUStatus.R5);
                    break;
                case 6:
                    AddStatus(CPUStatus.R6);
                    break;
                case 7:
                    AddStatus(CPUStatus.R7);
                    break;
                case 8:
                    AddStatus(CPUStatus.R8);
                    break;
                case 9:
                    AddStatus(CPUStatus.R9);
                    break;
                case 10:
                    AddStatus(CPUStatus.R10);
                    break;
                case 11:
                    AddStatus(CPUStatus.R11);
                    break;
                case 0:
                    AddStatus(CPUStatus.R0);
                    break;
            }

        }

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


        public AssemblerCPU()
        {
            RAM = new MemoryBank();
            Reset();
        }

        public void Reset()
        {
            registers = new UInt32[regMax];
            ProgramCounter = 0;
            MemoryAddressReguister = 0;
            MemoryDataRegister = 0;
            CurrentInstructionRegister = 0;
            HaltCondition = false;
            CycleNumber = 1;
            CurrentCondition = AssemblyCommand.COND.AL;
            ResetProgress();
            ResetStatus();
        }


        private int fetchProgress;
        private int cycleProgress;
        private int decodeProgress;
        private int executeProgress;
        private bool cycleComplete;

        private void ResetProgress()
        {
            fetchProgress = 0;
            cycleProgress = 0;
            decodeProgress = 0;
            executeProgress = -1; //So it's not the same as decode progress
        }
        public void Cycle()
        {
            if (CycleNumber++ % 100 == 0 && SafetyBrake)
            {
                SafetyBrakeEngaged = true;
                HaltCondition = true;
                CpuCondition = "Safety Brake";
            }
            else
            {
                CycleComplete = false;
                ResetStatus();
                if (!SteppingMode)
                {
                    CpuCondition = "Running";
                }
                else
                {
                    CpuCondition = "Stepping";
                }
                bool stepDone = false;
                if (!HaltCondition)
                {
                    if (!SteppingMode || cycleProgress == 0 && !stepDone)
                    {
                        stepDone = true;

                        if (fetch())
                        {
                            cycleProgress++;
                        }
                    }
                    if (!SteppingMode || cycleProgress == 1 && !stepDone)
                    {
                        stepDone = true;


                        if (decode())
                        {
                            cycleProgress = 0;
                            CycleComplete = true;
                        }
                    }

                }
            }
        }

        private bool fetch()
        {
            CpuCycle = "Fetch";
            bool stepDone = false;
            if (!SteppingMode || fetchProgress == 0 && !stepDone)
            {
                stepDone = true;
                fetchProgress++;
                CpuStatus = "moving the PC to the MAR";
                AddStatus(CPUStatus.PCToMAR | CPUStatus.MAR);
                MemoryAddressReguister = ProgramCounter;
            }
            if (!SteppingMode || fetchProgress == 1 && !stepDone)
            {
                stepDone = true;
                fetchProgress++;
                CpuStatus = "Moving Memory referenced by MAR into the MBR, incrementing PC";
                AddStatus(CPUStatus.PCToPC | CPUStatus.PC | CPUStatus.DatBus | CPUStatus.MBR | CPUStatus.AddBus);
                MemoryDataRegister = RAM.Read((short)MemoryAddressReguister);
                ProgramCounter++;
            }
            if (!SteppingMode || fetchProgress == 2 && !stepDone)
            {
                stepDone = true;
                fetchProgress++;
                CpuStatus = "moving the MBR to the CIR";
                AddStatus(CPUStatus.MBRToCIR | CPUStatus.CIR);
                CurrentInstructionRegister = MemoryDataRegister;
            }

            if (fetchProgress == 3 || !SteppingMode)
            {
                fetchProgress = 0;
                return true;                
            }
            else
            {
                return false;
            }
        }

        private bool decode()
        {
            if(RAM.isBreakPoint((short)MemoryAddressReguister))
            {
                SteppingMode = true;
                HaltCondition = true;
                BreakPointHit = true;
                CpuCondition = "Breakpoint";
            }
            bool stepDone = false;
            if (!SteppingMode)
            {
                executeProgress = 0;
            }
            if (!SteppingMode || decodeProgress == 0 && !stepDone)
            {
                CpuCycle = "Decode";
                CpuStatus = "Decoding and Running Instruction";
                AddStatus(CPUStatus.CIRToCU);
                AssemblyCommand thisCommand = new AssemblyCommand(CurrentInstructionRegister);
                OpCode = (AssemblyCommand.OP)thisCommand.Opcode;
                Condition = thisCommand.Condition;
                Rn = thisCommand.FirstRegister;
                Rd = thisCommand.DestinationRegister;
                Rm = thisCommand.SecondRegister;
                Immediate = thisCommand.Immediate;
                SetCondition = thisCommand.SetCondition;
                Operand = thisCommand.OperandTwo;
                stepDone = true;
                decodeProgress++;

            }
            if (!SteppingMode || decodeProgress >= 1 && !stepDone)
            {
                CpuCycle = "Exucute";
                switch (OpCode) // Execute
                {
                    case AssemblyCommand.OP.ADD:
                        ALU("Contents in ALU added and result copied to register " + Rd, (x, y) => x + y);
                        break;
                    case AssemblyCommand.OP.AND:
                        ALU("Contents in ALU apllied to bitwise AND and result copied to register " + Rd, (x, y) => x & y);
                        break;

                    case AssemblyCommand.OP.B:
                        executeProgress = 1;
                        Branch();  
                        break;

                    case AssemblyCommand.OP.CMP:
                        CMP();
                        break;

                    case AssemblyCommand.OP.EOR:
                        ALU("Contents in ALU apllied to bitwise Exclusive OR and result copied to register " + Rd, (x, y) => x ^ y);
                        break;

                    case AssemblyCommand.OP.HALT:
                        CpuCondition = "Halted";
                        CpuStatus = "Halting";
                        executeProgress = 1;
                        HaltCondition = true;
                        break;

                    case AssemblyCommand.OP.LDR:
                        executeProgress = 1;
                        LDR();
                        break;

                    case AssemblyCommand.OP.LSL:
                        ALU("First operand in ALU shifted left by the second operand and result copied to register " + Rd, (x, y) => x << y);
                        break;

                    case AssemblyCommand.OP.LSR:
                        ALU("First operand in ALU shifted right by the second operand and result copied to register " + Rd, (x, y) => x >> y);
                        break;

                    case AssemblyCommand.OP.MOV: 
                        executeProgress = 1;
                        MOV();
                        break;

                    case AssemblyCommand.OP.MVN: 
                        ALU("Contents in ALU apllied to bitwise NOT and result copied to register " + Rd);
                        break;

                    case AssemblyCommand.OP.ORR:
                        ALU("Contents in ALU apllied to bitwise OR and result copied to register " + Rd, (x, y) => x | y);
                        break;

                    case AssemblyCommand.OP.STR:
                        STR();
                        break;

                    case AssemblyCommand.OP.SUB:
                        ALU("Contents in ALU subtracted and result copied to register " +Rd, (x, y) => x - y);
                        break;

                    default:
                        break;
                }
            }
            if(decodeProgress == executeProgress || !SteppingMode)
            {
                CpuCycle = "";
                decodeProgress = 0;
                executeProgress = -1;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void STR()
        {
            executeProgress = 4;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {

                AddStatus(CPUStatus.CIRToMAR);
                AddStatus(CPUStatus.MAR);
                CpuStatus = "operand copied to MAR";
                MemoryAddressReguister = (UInt32)Operand;

                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                AddStatus(CPUStatus.MBRToReg);
                AddStatus(CPUStatus.MBR);
                CpuStatus = "register " + Rd + " copied to MBR";
                MemoryDataRegister = GetRegister(Rd);
                decodeProgress++;
                stepDone = true;
            }

            if (!SteppingMode || decodeProgress == 3 && !stepDone)
            {
                AddStatus(CPUStatus.DatBus);
                AddStatus(CPUStatus.AddBus);
                CpuStatus = "MBR copied to memory location specified by MAR";

                RAM.Write((short)MemoryAddressReguister, MemoryDataRegister);
                decodeProgress++;
                stepDone = true;
            }

        }

        private void LDR()
        {
            executeProgress = 4;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {
                
                AddStatus(CPUStatus.CIRToMAR);
                AddStatus(CPUStatus.MAR);
                CpuStatus = "operand copied to MAR";
                MemoryAddressReguister = (UInt32)Operand;
                
                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                AddStatus(CPUStatus.DatBus);
                AddStatus(CPUStatus.AddBus);
                AddStatus(CPUStatus.MBR);
                CpuStatus = "memory location specified by MAR copied to MBR";
                MemoryDataRegister = RAM.Read((short)MemoryAddressReguister);
                decodeProgress++;
                stepDone = true;
            }

            if (!SteppingMode || decodeProgress == 3 && !stepDone)
            {
                AddStatus(CPUStatus.MBRToReg);
                AddRegisterStatus(Rd);
                CpuStatus = "MBR copied to Register " + Rd;
                
                SetRegister(Rd, MemoryDataRegister);
                decodeProgress++;   
                stepDone = true;
            }

        }


        private void CMP()
        {
            executeProgress = 4;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {
                compare1 = (Int32)GetRegister(Rn);
                AddStatus(CPUStatus.REGToALU);
                CpuStatus = "Register " + Rn + " copied to ALU";
                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                if (Immediate)
                {
                    AddStatus(CPUStatus.CIRToALU | CPUStatus.ALU);
                    CpuStatus = "CIR[Operand]  copied to ALU";
                    compare2 =  (Int32)Operand;
                }
                else
                {
                    AddStatus(CPUStatus.REGToALU | CPUStatus.ALU);
                    CpuStatus = "Register " + Rm + " copied to ALU";
                    compare2 = (Int32)GetRegister(Rm);
                }
                decodeProgress++;
                stepDone = true;

            }
            if (!SteppingMode || decodeProgress == 3 && !stepDone)
            {
                String cmpResult = "";
                if (compare1 == compare2)
                {
                    CurrentCondition = AssemblyCommand.COND.EQ;
                    cmpResult = "EQ";
                }
                else if (compare1 < compare2)
                {
                    CurrentCondition = AssemblyCommand.COND.LT;
                    cmpResult = "LT";
                }
                else
                {
                    CurrentCondition = AssemblyCommand.COND.GT;
                    cmpResult = "GT";
                }
                AddStatus(CPUStatus.ALU);
                CpuStatus = "Values in ALU compared and compare status set to " + cmpResult;
                decodeProgress++;
                stepDone = true;
            }
        }

        private void Branch()
        {
            bool doBranch = false;
            if (SetCondition)
            {
                switch (Condition)
                {
                    case AssemblyCommand.COND.AL:
                        ProgramCounter = (UInt32)Operand;
                        doBranch = true;
                        break;

                    case AssemblyCommand.COND.NE:
                        if (CurrentCondition == AssemblyCommand.COND.LT || CurrentCondition == AssemblyCommand.COND.GT)
                        {
                            ProgramCounter = (UInt32)Operand;
                            doBranch = true;
                        }
                        break;

                    case AssemblyCommand.COND.EQ:
                        if (CurrentCondition == AssemblyCommand.COND.EQ)
                        {
                            ProgramCounter = (UInt32)Operand;
                            doBranch = true;
                        }
                        break;

                    case AssemblyCommand.COND.GT:
                        if (CurrentCondition == AssemblyCommand.COND.GT)
                        {
                            ProgramCounter = (UInt32)Operand;
                            doBranch = true;
                        }
                        break;

                    case AssemblyCommand.COND.LT:
                        if (CurrentCondition == AssemblyCommand.COND.LT)
                        {
                            ProgramCounter = (UInt32)Operand;
                            doBranch = true;
                        }
                        break;
                }
            }
            else
            {
                ProgramCounter = (UInt32)Operand;
            }

            if (doBranch)
            {
                ProgramCounter = (UInt32)Operand;
                AddStatus(CPUStatus.PC);
                //AddStatus(CPUStatus.CIRToPC); // Oops.. out of space
                CpuStatus = "CIR[Operand] copied to Program counter ";
            }
            else
            {
                CpuStatus = "No Branch to do!";
            }
        }

        private void MOV()
        {
            if (Immediate)
            {
                AddStatus(CPUStatus.CIRToREG);
                AddRegisterStatus(Rd);
                CpuStatus = "CIR[Operand] copied to Register " + Rd;
                SetRegister(Rd, (UInt32)Operand);
            }
            else
            {
                AddStatus(CPUStatus.REGToREG);
                AddRegisterStatus(Rd);
                CpuStatus = "Register " + Rm + " copied to Register " + Rd;
                SetRegister(Rd, GetRegister(Rm));
            }
        }

       


        private void ALU(string statusText)  //ALU Overload for MVN
        {
            executeProgress = 3;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {
                if (Immediate)
                {
                    AddStatus(CPUStatus.CIRToALU);
                    CpuStatus = "CIR[Operand]  copied to ALU";
                    Value1 = (UInt32)Operand;
                }
                else
                {
                    AddStatus(CPUStatus.REGToALU);
                    CpuStatus = "Register " + Rm + " copied to ALU";
                    Value1 = GetRegister(Rm);
                }
                ALUAnswer1 = ~Value1;
                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                ALUResult(statusText);
            }
        }


        private void ALU(string statusText, Func<UInt32, int, UInt32> op)  //ALU Overload for LSL, LSR
        {
            executeProgress = 3;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {
                Value1 = GetRegister(Rn);
                if (Immediate)
                {
                    AddStatus(CPUStatus.CIRToALU | CPUStatus.REGToALU);
                    CpuStatus = "CIR[Operand] and Register " + Rn + " copied to ALU";
                    Value2 = (UInt32)Operand;
                }
                else
                {
                    AddStatus(CPUStatus.REGToALU);
                    CpuStatus = "Register " + Rm + " and Register " + Rn + " copied to ALU";
                    Value2 = GetRegister(Rm);
                }
                ALUAnswer1 = op(Value1,  (int)Value2);
                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                ALUResult(statusText);
            }
        }
        
        private void ALU(string statusText, Func<UInt32, UInt32, UInt32> op)  //ALU Overload for AND, SUB, ORR, AND, EOR
        {
            executeProgress = 3;
            bool stepDone = false;
            if (!SteppingMode || decodeProgress == 1 && !stepDone)
            {
                Value1 = GetRegister(Rn);
                if (Immediate)
                {
                    AddStatus(CPUStatus.CIRToALU | CPUStatus.REGToALU);
                    CpuStatus = "CIR[Operand] and Register " + Rn + " copied to ALU";
                    Value2 = (UInt32)Operand;
                }
                else
                {
                    AddStatus(CPUStatus.REGToALU);
                    CpuStatus = "Register " + Rm + " and Register " + Rn + " copied to ALU";
                    Value2 =  GetRegister(Rm);
                }
                ALUAnswer1 = op(Value1, Value2);
                decodeProgress++;
                stepDone = true;
            }
            if (!SteppingMode || decodeProgress == 2 && !stepDone)
            {
                ALUResult(statusText);
            }
        }

        private void ALUResult(string statusText)
        {
            SetRegister(Rd, ALUAnswer1);
            CpuStatus = statusText;
            AddStatus(CPUStatus.ALUToReg);
            AddRegisterStatus(Rd);
            decodeProgress++;
        }
    }
}
