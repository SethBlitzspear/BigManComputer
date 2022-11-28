using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{
    public class AssemblyCommand
    {


        private UInt32 instruction;


        private UInt32 opCodeMask = 0x01E00000;
        private UInt32 conditionCodeMask = 0xF0000000;
        private UInt32 firstRegisterCodeMask = 0x000F0000;
        private UInt32 destinationRegisterCodeMask = 0x0000F000;
        private UInt32 secondRegisterCodeMask = 0x0000000F;
        private UInt32 operandCodeMask = 0x00000FFF;
        private UInt32 setCondtionCodeMask = 0x00100000;
        private UInt32 immediateCodeMask = 0x02000000;

        private UInt32 clearOpCodeMask = 0xFE1FFFFF;
        private UInt32 clearConditionCodeMask = 0x0FFFFFFF;
        private UInt32 clearFirstRegisterCodeMask = 0xFFF0FFFF;
        private UInt32 clearDestinationRegisterCodeMask = 0xFFFF0FFF;
        private UInt32 clearSecondRegisterCodeMask = 0xFFFFFFF0;
        private UInt32 clearOperandCodeMask = 0xFFFFF000;
        private UInt32 clearSetCondtionCodeMask = 0xFFEFFFFF;
        private UInt32 clearImmediateCodeMask = 0xFDFFFFFF;


        public enum COND : byte // use ARM codes for future proofing
        {
            EQ = 0x00,
            NE = 0x01,
            LT = 0x0B,
            GT = 0x0C,
            AL = 0x0E
        }


        public enum OP : byte
        {
            HALT = 0x00,
            LDR = 0x01,
            STR = 0x02,
            ADD = 0x03,
            SUB = 0x04,
            MOV = 0x05,
            CMP = 0x06,
            B = 0x07,
            AND = 0x08,
            ORR = 0x09,
            EOR = 0x0A,
            MVN = 0x0B,
            LSL = 0x0C,
            LSR = 0x0D,

        }
        public static OP GetOpCode(string OpName)
        {
            switch (OpName)
            {
                case "ADD":
                    return OP.ADD;
                case "LDR":
                    return OP.LDR;
                case "STR":
                    return OP.STR;
                case "SUB":
                    return OP.SUB;
                case "AND":
                    return OP.AND;
                case "ORR":
                    return OP.ORR;
                case "EOR":
                    return OP.EOR;
                case "LSL":
                    return OP.LSL;
                case "LSR":
                    return OP.LSR;
                case "MOV":
                    return OP.MOV;
                case "MVN":
                    return OP.MVN;
                case "CMP":
                    return OP.CMP;
                case "B":
                case "BNE":
                case "BGT":
                case "BLT":
                case "BEQ":
                    return OP.B;
                case "HALT":
                default:
                    return OP.HALT;
            }
        }
        public static string GetOpName(OP OpCode)
        {
            switch (OpCode)
            {
                case OP.ADD:
                    return "ADD";
                case OP.LDR:
                    return "LDR";
                case OP.STR:
                    return "STR";
                case OP.SUB:
                    return "SUB";
                case OP.AND:
                    return "AND";
                case OP.ORR:
                    return "ORR";
                case OP.EOR:
                    return "EOR";
                case OP.LSL:
                    return "LSL";
                case OP.LSR:
                    return "LSR";
                case OP.MOV:
                    return "MOV";
                case OP.MVN:
                    return "MVN";
                case OP.CMP:
                    return "CMP";
                case OP.B:
                    return "B";
                case OP.HALT:
                default:
                    return "HALT";
            }
        }

        public OP Opcode
        {
            get
            {
                return (OP)((Instruction & opCodeMask) >> 21);
            }
            set
            {
                Instruction = Instruction & clearOpCodeMask;
                Instruction = Instruction | (UInt32)((byte)(value) << 21);
            }
        }

        public COND Condition
        {
            get
            {
                return (COND)((Instruction & conditionCodeMask) >> 28);
            }
            set
            {
                Instruction = Instruction & clearConditionCodeMask;
                Instruction = Instruction | (UInt32)((byte)(value) << 28);
            }
        }

        public byte FirstRegister
        {
            get
            {
                return (byte)((Instruction & firstRegisterCodeMask) >> 16);
            }
            set
            {
                Instruction = Instruction & clearFirstRegisterCodeMask;
                Instruction = Instruction | (UInt32)(value << 16);
            }
        }
        public byte SecondRegister
        {
            get
            {
                return (byte)((Instruction & secondRegisterCodeMask));
            }
            set
            {
                Instruction = Instruction & clearSecondRegisterCodeMask;
                Instruction = Instruction | (UInt32)(value);
            }
        }
        public byte DestinationRegister
        {
            get
            {
                return (byte)((Instruction & destinationRegisterCodeMask) >> 12);
            }
            set
            {
                Instruction = Instruction & clearDestinationRegisterCodeMask;
                Instruction = Instruction | (UInt32)(value << 12);
            }
        }

        public short OperandTwo
        {
            get
            {
                return (short)((Instruction & operandCodeMask));
            }
            set
            {
                Instruction = Instruction & clearOperandCodeMask;
                Instruction = Instruction | (UInt32)(value);
            }
        }
        public bool SetCondition
        {
            get
            {
                if ((Instruction & setCondtionCodeMask) == 0x00000000)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                Instruction = Instruction & clearSetCondtionCodeMask;
                if (value)
                {
                    Instruction = Instruction | setCondtionCodeMask;
                }
            }
        }
        public bool Immediate
        {
            get
            {
                if ((Instruction & immediateCodeMask) == 0x00000000)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                Instruction = Instruction & clearImmediateCodeMask;
                if (value)
                {
                    Instruction = Instruction | immediateCodeMask;
                }
            }
        }
        public AssemblyCommand()
        {
        }
        public AssemblyCommand(UInt32 newCommand)
        {
            Instruction = newCommand;
        }
        public uint Instruction
        {
            get
            {
                return instruction;
            }

            set
            {
                instruction = value;
            }
        }
    }
}
