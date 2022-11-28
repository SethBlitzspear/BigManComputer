using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{

    public struct AssemblerError
    {
        public Assembler source;
        public int line;
        public string error;
    }

    public struct AssemblerLabel
    {
        public string labelName;
        public short lineNumber;
    }

    public class Assembler
    {

        private int regMin = 0;
        private int regMax = 12;  //currently set to 12 as opposed to 15 as AQA spec states 12 max
        private int memoryMin = 0;
        private int memoryMax = 4095;  //currently set to 4095 as no guidance from AQA as to memory size, so will use full ARM operand 2 size.

        private string rawSource;
        private string[] source;
        private List<AssemblerError> errors = new List<AssemblerError>();
        private List<AssemblerLabel> labels = new List<AssemblerLabel>();
        private List<AssemblyCommand> commands = new List<AssemblyCommand>();

        private MemoryBank ram;
        
        public int SourceLength
        {
            get
            {
                return source.Length;
            }
        }

        public int ErrorCount
        {
            get
            {
                return errors.Count;
            }
        }

        public string RawSource
        {
            get
            {
                return rawSource;
            }

            set
            {
                rawSource = value;
                source = RawSource.Split(Environment.NewLine.ToCharArray());
            }
        }

        public MemoryBank Ram
        {
            get
            {
                return ram;
            }

            set
            {
                ram = value;
            }
        }

        public Assembler()
        {
        }

        public Assembler(string newSource)
        {
            RawSource = newSource;
        }
        private byte ParseRegister(string registerName, string regToken, AssemblerError newError)
        {
            byte registerNumber = 0;
            if (regToken[0] != 'R')
            {
                newError.error = "Register " + registerName + " reference must start with an 'R'";
                errors.Add(newError);
            }
            else
            {
               
                try
                {
                    registerNumber = Convert.ToByte(regToken.Substring(1, regToken.Length - 1));
                    if (registerNumber < regMin || registerNumber > regMax)
                    {
                        newError.error = "Register " + registerName + " reference must be between " + regMin + " and " + regMax;
                        errors.Add(newError);
                    }
                }
                catch
                {
                    newError.error = "Register " + registerName + " reference must start with an 'R' and be followed by a number";
                    errors.Add(newError);
                }            
            }
            return registerNumber;

        }
        private short ParseMemoryRef(string regToken, AssemblerError newError)
        {
            short memoryReference = 0;
           
            try
            {
                memoryReference = Convert.ToInt16(regToken);
                if (memoryReference < memoryMin || memoryReference > memoryMax)
                {
                    newError.error = "Memory reference must be between " + memoryMin + " and " + memoryMax;
                    errors.Add(newError);
                }
            }
            catch
            {
                newError.error = "Memory reference must be a number";
                errors.Add(newError);
            }
            return memoryReference;
        }

        private AssemblyCommand ParseOperand2(AssemblyCommand newCommand, string operand, AssemblerError newError)
        {
            
            if (operand.Length < 2)
            {
                newError.error = "Operand 2 should start with a R or # followed by a number representing the register or absoloute value";
                errors.Add(newError);
            }
            else {
                char typeIdentifier = operand[0];
                string reference = operand.Substring(1, operand.Length - 1);
                switch (typeIdentifier)
                {
                    case 'R':
                        newCommand.SecondRegister = ParseRegister("<Rm>", reference, newError);
                        newCommand.Immediate = false;
                        break;
                        
                    case '#':
                        try
                        {
                            newCommand.OperandTwo = Convert.ToInt16(reference); // We don't do a range check as no guidance on limits. default to 32 bits
                            newCommand.Immediate = true;
                        }
                        catch
                        {
                            newError.error = "Operand absoloute value must be a 32 bit number";
                            errors.Add(newError);
                        }
                        break;

                    default:
                        newError.error = "Operand 2 should start with a R or # followed by a number representing the register or absoloute value";
                        errors.Add(newError);
                        break;
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenRegisterMemoryRef(AssemblyCommand newCommand, string instructionName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.error = instructionName + " instruction with no memory reference or destination";
                errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 2)
                {
                    newError.error = instructionName + " instruction must have two operands; <Rd> desintation register and <memory ref> location of memory to read from";
                    errors.Add(newError);
                }
                else
                {
                    newCommand.DestinationRegister = ParseRegister("<Rd>", operandTokens[0].Trim(), newError);
                    newCommand.OperandTwo = ParseMemoryRef(operandTokens[1].Trim(), newError);
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenRegisterRegisterOperand(AssemblyCommand newCommand, string instructionName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.error = instructionName + " instruction with no source/destination register or operand";
                errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 3)
                {
                    newError.error = instructionName + " instruction must have three operands; <Rd> desintation register, <Rn> source register and <operand2> value";
                    errors.Add(newError);
                }
                else
                {
                    newCommand.DestinationRegister =  ParseRegister("<Rd>", operandTokens[0].Trim(), newError);
                    newCommand.FirstRegister = ParseRegister("<Rn>", operandTokens[1].Trim(), newError);
                    newCommand = ParseOperand2(newCommand, operandTokens[2].Trim(), newError);
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenLabel(AssemblyCommand newCommand, string instructionName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.error = instructionName + " instruction with no label";
                errors.Add(newError);
            }
            else
            {
                if (tokens[0].Length != 1)
                {
                    if (tokens[0].Length == 3)
                    {
                        newCommand.SetCondition = true;
                        string cond = tokens[0].Substring(1, 2);
                        switch (cond) // Use ARM values for future proofing
                        {
                            case "EQ":
                                newCommand.Condition = AssemblyCommand.COND.EQ;
                                break;
                            case "NE":
                                newCommand.Condition = AssemblyCommand.COND.NE;
                                break;
                            case "GT":
                                newCommand.Condition = AssemblyCommand.COND.GT;
                                break;
                            case "LT":
                                newCommand.Condition = AssemblyCommand.COND.LT;
                                break;

                        }
                    }
                    else
                    {
                        newError.error = tokens[1] + " is invalid label";
                        errors.Add(newError);
                    }
                }
                bool foundLabel = false;
                foreach (AssemblerLabel label in labels)
                {
                    if (label.labelName == tokens[1])
                    {
                        foundLabel = true;
                        newCommand.OperandTwo = label.lineNumber;
                    }
                }
                if(!foundLabel)
                {
                    newError.error = tokens[1] + " is invalid label";
                    errors.Add(newError);
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenRegisterOperand(AssemblyCommand newCommand, string instructionName, string registerName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.error = instructionName + " instruction with no source/destination register or operand";
                errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 2)
                {
                    newError.error = instructionName + " instruction must have two operands; <Rd> desintation register and <operand2> value";
                    errors.Add(newError);
                }
                else
                {
                    newCommand.DestinationRegister = ParseRegister(registerName, operandTokens[0].Trim(), newError);
                    newCommand = ParseOperand2(newCommand, operandTokens[1].Trim(), newError);
                }
            }
            return newCommand;
        }

        public bool TryParse()
        {
            errors.Clear();
            labels.Clear();
            commands.Clear();

            //
            // First parse through and look for any labels
            //
            for (short lineCount = 0; lineCount < SourceLength; lineCount++)
            {
                string line = source[lineCount];
                string[] tokens = line.Split(null, 2);
                if (tokens[0][tokens[0].Length - 1] == ':' && tokens.Length == 1)
                {
                    AssemblerLabel newLabel = new AssemblerLabel();
                    newLabel.labelName = tokens[0].Substring(0, tokens[0].Length - 1);
                    newLabel.lineNumber = lineCount;
                    labels.Add(newLabel);
                }
            }

            for (short lineCount = 0; lineCount < SourceLength; lineCount++)
            {
                AssemblerError newError = new AssemblerError();
                AssemblyCommand newCommand = new AssemblyCommand();
                newError.source = this;
                newError.line = lineCount;
                string line = source[lineCount];
                string[] tokens = line.Split(null, 2);
                switch (tokens[0])
                {
                    case "LDR":
                    case "STR":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        newCommand = parseTokenRegisterMemoryRef(newCommand, tokens[0], tokens, newError);
                        break;

                    case "ADD":
                    case "SUB":
                    case "AND":
                    case "ORR":
                    case "EOR":
                    case "LSL":
                    case "LSR":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        newCommand = parseTokenRegisterRegisterOperand(newCommand, tokens[0], tokens, newError);
                        break;

                    case "MOV":
                    case "MVN":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        newCommand = parseTokenRegisterOperand(newCommand, tokens[0], "<Rd>", tokens, newError);
                        break;

                    case "CMP":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        newCommand = parseTokenRegisterOperand(newCommand, "CMP", "<Rn>", tokens, newError);
                        break;

                    case "B":
                    case "BEQ":
                    case "BNE":
                    case "BGT":
                    case "BLT":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        newCommand = parseTokenLabel(newCommand, tokens[0], tokens, newError);
                        break;

                    case "HALT":
                        newCommand.Opcode = AssemblyCommand.GetOpCode(tokens[0]);
                        if (tokens.Length != 1)
                        {
                            newError.error = "HALT instruction must have no other tokens";
                            errors.Add(newError);
                        }
                        break;

                    default:
                        if (tokens[0][tokens[0].Length - 1] != ':')
                        {
                            newError.error = "Unrecognised token " + tokens[0] + " at line " + lineCount;
                            errors.Add(newError);
                        }
                        else if (tokens.Length != 1)
                        {
                            newError.error = "Unrecognised token(s) after label at line " + lineCount;
                            errors.Add(newError);
                        }
                        break;

                }
                commands.Add(newCommand);
            }
            return (errors.Count == 0)? true :false;
        }

        public void Assemble() 
        {
            if (TryParse())
            {
                if (commands.Count > 0 && Ram != null)
                {
                    for (short commandCount = 0; commandCount < commands.Count; commandCount++)
                    {
                        Ram.Write(commandCount, commands[commandCount].Instruction);
                    }
                }
                else
                {
                    if (commands.Count == 0)
                    {
                        throw new UnableToAssembleException("Cannot Assemble due to no code to be assembled");
                    }
                    else
                    {
                        throw new UnableToAssembleException("Cannot Assemble as no RAM attached");
                    }
                }
            }
            else
            {
                throw new UnableToAssembleException("Cannot Assemble due to errors in the Assmbly code");
            }
        }
         

    }
}
