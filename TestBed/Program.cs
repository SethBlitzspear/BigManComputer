/*using AssemblerCore;*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{
    class Program
    {
        static void Main(string[] args)
        {

          /*  MemoryBank RAM = new MemoryBank();
            Assembler myAssembler = new Assembler();

            String testSource = "MOV R1, #200\nADD R2, R1, #64\nSTR R2, 8";

            myAssembler.RawSource = testSource;
            myAssembler.Ram = RAM;

            AssemblerCPU theCPU = new AssemblerCPU(RAM);

            myAssembler.Assemble();
            for (short ramCount = 0; ramCount < 10; ramCount++)
            {
                Console.WriteLine("Memory location " + ramCount + " is: " + RAM.Read(ramCount));
            }
            Console.ReadLine();

            theCPU.Cycle();
            theCPU.Cycle();
            theCPU.Cycle();
            for (short ramCount = 0; ramCount < 10; ramCount++)
            {
                Console.WriteLine("Memory location " + ramCount + " is: " + RAM.Read(ramCount));
            }
            Console.ReadLine();

            //AssemblyCommand test = new AssemblyCommand(0x0196C00A);

            /* AssemblyCommand test = new AssemblyCommand();

             test.Condition = 10;
             test.Immediate = true;
             test.Opcode = AssemblyCommand.OP.EOR;
             test.OperandTwo = 14;
             test.FirstRegister = 13;
             test.DestinationRegister = 7;

             AssemblyCommand.OP myOp = test.Opcode;
             short myCond = test.Condition;
             bool myImmediate = test.Immediate;
             bool mySetCond = test.SetCondition;
             short myR1 = test.FirstRegister;
             short MyRd = test.DestinationRegister;
             short MyR2 = test.SecondRegister;

             Console.WriteLine("Op is " + myOp);
             Console.WriteLine("Condtion is is " + myCond);
             Console.WriteLine("Immed is " + (myImmediate?"1":"0"));
             Console.WriteLine("Set Cond is " + (mySetCond ? "1" : "0"));
             Console.WriteLine("Rn is " + myR1);
             Console.WriteLine("Rd is " + MyRd);
             Console.WriteLine("R2 is " + MyR2);

            Console.ReadLine();*/
        }
    }
}