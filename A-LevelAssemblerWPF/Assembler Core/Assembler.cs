using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerCore
{

    public struct AssemblerError
    {
        private Assembler source;
        private int line;
        private string error;

        public string ErrorReport
        {
            get
            {
                return "Error at line " + (line + 1) + ": " + error;
            }
        }
 
        public Assembler Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
            }
        }

        public int Line
        {
            get
            {
                return line;
            }

            set
            {
                line = value;
            }
        }

        public string Error
        {
            get
            {
                return error;
            }

            set
            {
                error = value;
            }
        }

        public Assembler Assembler
        {
            get => default(Assembler);
            set
            {
            }
        }
    }

    public struct AssemblerLabel
    {
        private string labelName;
        private short lineNumber;

        public string LabelName
        {
            get
            {
                return labelName;
            }

            set
            {
                labelName = value;
            }
        }

        public short LineNumber
        {
            get
            {
                return lineNumber;
            }

            set
            {
                lineNumber = value;
            }
        }

    }

    public class Assembler
    {

        private int regMin = 0;
        private int regMax = 11;  //currently set to 12 as opposed to 15 as AQA spec states 12 max
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
                return Errors.Count;
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
                source = RawSource.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
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

        public List<AssemblerError> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        public List<AssemblerLabel> Labels
        {
            get
            {
                return labels;
            }

            set
            {
                labels = value;
            }
        }

        public List<AssemblyCommand> Commands
        {
            get
            {
                return commands;
            }

            set
            {
                commands = value;
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
                newError.Error = "Register " + registerName + " reference must start with an 'R'";
                Errors.Add(newError);
            }
            else
            {
               
                try
                {
                    registerNumber = Convert.ToByte(regToken.Substring(1, regToken.Length - 1));
                    if (registerNumber < regMin || registerNumber > regMax)
                    {
                        newError.Error = "Register " + registerName + " reference must be between " + regMin + " and " + regMax;
                        Errors.Add(newError);
                    }
                }
                catch
                {
                    newError.Error = "Register " + registerName + " reference must start with an 'R' and be followed by a number";
                    Errors.Add(newError);
                }            
            }
            return registerNumber;

        }
        private short ParseMemoryRef(string regToken, AssemblerError newError)  //To Do.. Add support for hex here
        {
            short memoryReference = 0;
           
            try
            {
                memoryReference = Convert.ToInt16(regToken);
                if (memoryReference < memoryMin || memoryReference > memoryMax)
                {
                    newError.Error = "Memory reference must be between " + memoryMin + " and " + memoryMax;
                    Errors.Add(newError);
                }
            }
            catch
            {
                newError.Error = "Memory reference must be a number";
                Errors.Add(newError);
            }
            return memoryReference;
        }

        private AssemblyCommand ParseOperand2(AssemblyCommand newCommand, string operand, AssemblerError newError)
        {
            
            if (operand.Length < 2)
            {
                newError.Error = "Operand 2 should start with a R or # followed by a number representing the register or absoloute value";
                Errors.Add(newError);
            }
            else {
                char typeIdentifier = operand[0];
                string reference = operand.Substring(1, operand.Length - 1);
                switch (typeIdentifier)
                {
                    case 'R':
                        newCommand.SecondRegister = ParseRegister("<Rm>", operand.Trim(), newError);
                        newCommand.Immediate = false;
                        break;
                        
                    case '#':
                        if (reference.Length > 2 && reference.Substring(0, 2) == "0x")
                        {
                            if (reference.Length == 2)
                            {
                                newError.Error = "Hex operand absoloute value must must have value after 0x";
                                Errors.Add(newError);
                            }
                            else
                            {
                                try
                                {
                                    string hexReference = reference.Substring(2, reference.Length - 2);
                                    short op2Value = Convert.ToInt16(hexReference, 16);
                                    parseAbsValue(newCommand, op2Value, newError);
                                }
                                catch (Exception ex)
                                {
                                    newError.Error = "Unable to parse hex value";
                                    Errors.Add(newError);

                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                short op2Value = Convert.ToInt16(reference); // We don't do a range check as no guidance on limits. default to 32 bits
                                parseAbsValue(newCommand, op2Value, newError);
                            }
                            catch
                            {
                                newError.Error = "Operand absoloute value must be a 32 bit number";
                                Errors.Add(newError);
                            }
                        }
                        break;

                    default:
                        newError.Error = "Operand 2 should start with a R or # followed by a number representing the register or absoloute value";
                        Errors.Add(newError);
                        break;
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseAbsValue(AssemblyCommand newCommand, short op2Val, AssemblerError newError)
        {
           
            if (op2Val < 0 || op2Val > 4095)
            {
                newError.Error = "Absoloute value must be between 0 and 4095";
                Errors.Add(newError);
            }
            else
            {
                newCommand.OperandTwo = op2Val;
                newCommand.Immediate = true;
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenRegisterMemoryRef(AssemblyCommand newCommand, string instructionName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.Error = instructionName + " instruction with no memory reference or destination";
                Errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 2)
                {
                    newError.Error = instructionName + " instruction must have two operands; <Rd> desintation register and <memory ref> location of memory to read from";
                    Errors.Add(newError);
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
                newError.Error = instructionName + " instruction with no source/destination register or operand";
                Errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 3)
                {
                    newError.Error = instructionName + " instruction must have three operands; <Rd> desintation register, <Rn> source register and <operand2> value";
                    Errors.Add(newError);
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
                newError.Error = instructionName + " instruction with no label";
                Errors.Add(newError);
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
                        newError.Error = tokens[1] + " is invalid label";
                        Errors.Add(newError);
                    }
                }
                bool foundLabel = false;
                foreach (AssemblerLabel label in Labels)
                {
                    if (label.LabelName == tokens[1])
                    {
                        foundLabel = true;
                        newCommand.OperandTwo = label.LineNumber;
                    }
                }
                if(!foundLabel)
                {
                    newError.Error = tokens[1] + " is invalid label";
                    Errors.Add(newError);
                }
            }
            return newCommand;
        }

        private AssemblyCommand parseTokenRegisterOperand(AssemblyCommand newCommand, string instructionName, string registerName, string[] tokens, AssemblerError newError)
        {
            if (tokens.Length == 1)
            {
                newError.Error = instructionName + " instruction with no source/destination register or operand";
                Errors.Add(newError);
            }
            else
            {
                string[] operandTokens = tokens[1].Split(",".ToCharArray());
                if (operandTokens.Length != 2)
                {
                    newError.Error = instructionName + " instruction must have two operands; <Rd> desintation register and <operand2> value";
                    Errors.Add(newError);
                }
                else
                {
                    if (registerName == "<Rd>")
                    {
                        newCommand.DestinationRegister = ParseRegister(registerName, operandTokens[0].Trim(), newError);
                    }
                    else
                    {
                        newCommand.FirstRegister = ParseRegister(registerName, operandTokens[0].Trim(), newError);
                    }
                    newCommand = ParseOperand2(newCommand, operandTokens[1].Trim(), newError);
                }
            }
            return newCommand;
        }

        public bool TryParse()
        {
            Errors.Clear();
            Labels.Clear();
            Commands.Clear();
            short commandCOunt = 0;
            //
            // First parse through and look for any labels
            //
            for (short lineCount = 0; lineCount < SourceLength; lineCount++)
            {
                
                string line = (source[lineCount].Split(new char[] { ';' })[0]).Trim();
                string[] tokens = line.Split(null, 2);
                if (tokens[0] != "")
                {
                    if (tokens[0][tokens[0].Length - 1] == ':' && tokens.Length == 1)
                    {
                        AssemblerLabel newLabel = new AssemblerLabel();
                        newLabel.LabelName = tokens[0].Substring(0, tokens[0].Length - 1);
                        newLabel.LineNumber = commandCOunt;
                        Labels.Add(newLabel);
                        /* This is clunky way of stripping the label out of the source, but I am tired.
                        string[] newSource = new string[SourceLength - 1];
                        for (int sourceCount = 0; sourceCount < lineCount; sourceCount++)
                        {
                            newSource[sourceCount] = source[sourceCount];
                        }
                        for (int sourceCount = lineCount + 1; sourceCount < SourceLength; sourceCount++)
                        {
                            newSource[sourceCount - 1] = source[sourceCount];
                        }
                        source = newSource;*/

                    }
                    else
                    {
                        commandCOunt++;
                    }
                }
                else
                {
                    commandCOunt++;
                }
            }

            for (short lineCount = 0; lineCount < SourceLength; lineCount++)
            {
                bool addCommand = true;
                AssemblerError newError = new AssemblerError();
                AssemblyCommand newCommand = new AssemblyCommand();
                newError.Source = this;
                newError.Line = lineCount;
                string line = source[lineCount].Split(new char[] { ';' })[0].Trim();
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
                            newError.Error = "HALT instruction must have no other token`    s";
                            Errors.Add(newError);
                        }
                        break;

                    default:
                        if (line == "")
                        {
                            addCommand = false; //Do nothing?
                        }
                        else if (tokens[0][tokens[0].Length - 1] != ':')
                        {
                            newError.Error = "Unrecognised token " + tokens[0] + " at line " + lineCount;
                            Errors.Add(newError);
                        }
                        else if (tokens.Length != 1)
                        {
                            newError.Error = "Unrecognised token(s) after label at line " + lineCount;
                            Errors.Add(newError);
                        }
                        else
                        {
                            addCommand = false; //It's a label
                        }
                        break;

                }
                if (addCommand)
                {
                    Commands.Add(newCommand);
                }
            }
            return (Errors.Count == 0)? true :false;
        }

        public void Assemble() 
        {
            if (TryParse())
            {
                Ram.GenerateMemoryBank();
                if (Commands.Count > 0 && Ram != null)
                {
                    for (short commandCount = 0; commandCount < Commands.Count; commandCount++)
                    {
                        Ram.Write(commandCount, Commands[commandCount].Instruction);
                    }
                }
                else
                {
                    if (Commands.Count == 0)
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
