using AssemblerCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace A_LevelAssemblerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AssemblerCPU theCPU;
        MemoryBank RAM;
        Assembler theAssembler;
        ObservableCollection<AssemblerError> ErrorList;
        ObservableCollection<MemoryCell> MemoryList;
        DispatcherTimer resetTimer = new DispatcherTimer();
        private string fileName = "";
        DispatcherTimer stepTimer = new DispatcherTimer();
        DispatcherTimer formatTimer = new DispatcherTimer();
        bool dontUpdateSave = false;


        public MainWindow()
        {
            this.InitializeComponent();
            RAM = new MemoryBank(4096);
            theCPU = new AssemblerCPU(RAM);
            theAssembler = new Assembler();
            theAssembler.Ram = RAM;
            ErrorList = new ObservableCollection<AssemblerError>();
            MemoryList = new ObservableCollection<MemoryCell>(RAM.Memory);
            MemoryListBox.ItemsSource = MemoryList;
            ErrorListView.ItemsSource = ErrorList;
            stepTimer.Tick += new EventHandler(dispatcherTimer_StepTick);
            resetTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            formatTimer.Tick += new EventHandler(dispatcherTimer_FormatTick);
            formatTimer.Interval = new TimeSpan(0, 0, 1);

            //  foreach (MemoryCell mem in RAM.Memory)
            // {
            //    MemoryList.Add(mem);
            //}

        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            TrySave();
            TextRange myRange = new TextRange(SourceTextBox.Document.ContentStart, SourceTextBox.Document.ContentEnd);
            theAssembler.RawSource = myRange.Text;
            theAssembler.TryParse();
            ErrorList.Clear();

            foreach (AssemblerError error in theAssembler.Errors)
            {
                ErrorList.Add(error);
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            SourceTextBox.Document.Blocks.Clear();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();


            dlg.DefaultExt = ".asm";
            dlg.Filter = "Assembler Files (*.asm)|*.asm";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                fileName = dlg.FileName;
                string[] lines = File.ReadAllLines(fileName);
                SourceTextBox.Document.Blocks.Clear();
                foreach (string line in lines)
                {
                    SourceTextBox.Document.Blocks.Add(new Paragraph(new Run(line)));
                }
            }
            SaveButton.IsEnabled = false;

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            TrySave();
        }

        private void TrySave()
        {
            if (fileName == "")
            {
                SaveAs();
            }
            else
            {
                Save();
            }
        }

        private void SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Assembler"; // Default file name
            dlg.DefaultExt = ".asm"; // Default file extension
            dlg.Filter = "Text documents (.asm)|*.asm"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                fileName = dlg.FileName;
                Save();
            }
        }
        private void Save()
        {
            TextRange myRange = new TextRange(SourceTextBox.Document.ContentStart, SourceTextBox.Document.ContentEnd);
            File.WriteAllText(fileName, myRange.Text);
            SaveButton.IsEnabled = false;
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void AssembleButton_Click(object sender, RoutedEventArgs e)
        {
            TrySave();
            try
            {
                TextRange myRange = new TextRange(SourceTextBox.Document.ContentStart, SourceTextBox.Document.ContentEnd);
                theAssembler.RawSource = myRange.Text;
                theCPU.Reset();
                theAssembler.Assemble();
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message, "Error Assembling", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            ErrorList.Clear();
            if (theAssembler.Errors.Count == 0)
            {
                AssembleyBus.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                foreach (AssemblerError error in theAssembler.Errors)
                {
                    ErrorList.Add(error);
                }
            }

            updateScreen();
            SetReset();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            bool haltCOnditionRestore = theCPU.HaltCondition;
            theCPU.HaltCondition = false;
            theCPU.SteppingMode = false;
            while (!theCPU.HaltCondition)
            {
                theCPU.Cycle();
                updateScreen();
            }
            if (theCPU.SafetyBrakeEngaged)
            {
                MessageBoxResult result = MessageBox.Show("Safety Brake engaged as 100 Fetch Execute cycles completed", "Safety Brake", MessageBoxButton.OK, MessageBoxImage.Error);
                theCPU.SafetyBrakeEngaged = false;
                theCPU.HaltCondition = haltCOnditionRestore;
            }
            if (theCPU.BreakPointHit)
            {
                MessageBoxResult result = MessageBox.Show("Hit a Breakpoint at memory location " + theCPU.MemoryAddressReguister, "BreakPoint", MessageBoxButton.OK, MessageBoxImage.Error);
                theCPU.BreakPointHit = false;
                theCPU.HaltCondition = haltCOnditionRestore;
            }
        }

        private void CycleButton_Click(object sender, RoutedEventArgs e)
        {
            bool reapplySafetyBrake = false;
            if (theCPU.SafetyBrake)
            {
                theCPU.SafetyBrake = false;
                reapplySafetyBrake = true;
            }
            if (theCPU.SteppingMode)
            {
                while (!theCPU.CycleComplete)
                {
                    theCPU.Cycle();
                }
                theCPU.SteppingMode = false;
            }
            else
            {
                theCPU.Cycle();
            }
            theCPU.CpuCondition = "Paused";
            if (reapplySafetyBrake)
            {
                theCPU.SafetyBrake = true;
            }
            if (theCPU.BreakPointHit)
            {
                MessageBoxResult result = MessageBox.Show("Hit a Breakpoint at memory location " + theCPU.MemoryAddressReguister, "BreakPoint", MessageBoxButton.OK, MessageBoxImage.Error);
                theCPU.BreakPointHit = false;

            }
            updateScreen();

        }

        private void dispatcherTimer_StepTick(object sender, object e)
        {
            if (!theCPU.HaltCondition)
            {
                ResetAnimation();
                theCPU.SteppingMode = true;
                theCPU.Cycle();
                FlashStatus();
                updateScreen();

                setNextStep();
            }
        }

        private void setNextStep()
        {
            if (StepSpeedSlider.Value == 0)
            {
                SetReset();
                stepTimer.Stop();
                return;
            }
            else if (StepSpeedSlider.Value == 1)
            {
                stepTimer.Interval = new TimeSpan(0, 0, 5);
            }
            else if (StepSpeedSlider.Value == 2)
            {
                stepTimer.Interval = new TimeSpan(0, 0, 3);
            }
            else if (StepSpeedSlider.Value == 3)
            {
                stepTimer.Interval = new TimeSpan(0, 0, 1);
            }
            else if (StepSpeedSlider.Value == 4)
            {
                stepTimer.Interval = new TimeSpan(0, 0, 0, 500);
            }
            else
            {
                stepTimer.Interval = new TimeSpan(0, 0, 0, 250);
            }

            stepTimer.Start();
        }


        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!theCPU.HaltCondition)
            {
                theCPU.SteppingMode = true;
                theCPU.Cycle();
                FlashStatus();
                updateScreen();
                if (StepSpeedSlider.Value == 0)
                {
                    SetReset();
                }
                else
                {
                    setNextStep();
                    RunButton.IsEnabled = false;
                    CycleButton.IsEnabled = false;
                    StepButton.IsEnabled = false;
                }
            }

           
        }

        private void updateScreen()
        {


            MachineStateTextBlock.Text = theCPU.CpuStatus;
            MachineConditionTextBlock.Text = theCPU.CpuCondition;
            MachineCycleTextBlock.Text = theCPU.CpuCycle;

            if ((string)((ComboBoxItem)DisplayModeComboBox.SelectedItem).Content == "Decimal")
            {
                R0TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)0)).PadLeft(8, '0');
                R1TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)1)).PadLeft(8, '0');
                R2TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)2)).PadLeft(8, '0');
                R3TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)3)).PadLeft(8, '0');
                R4TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)4)).PadLeft(8, '0');
                R5TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)5)).PadLeft(8, '0');
                R6TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)6)).PadLeft(8, '0');
                R7TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)7)).PadLeft(8, '0');
                R8TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)8)).PadLeft(8, '0');
                R9TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)9)).PadLeft(8, '0');
                R10TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)10)).PadLeft(8, '0');
                R11TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)11)).PadLeft(8, '0');

                MARTextBlock.Text = Convert.ToString(theCPU.MemoryAddressReguister).PadLeft(8, '0');
                MBRTextBlock.Text = Convert.ToString(theCPU.MemoryDataRegister).PadLeft(8, '0');
                CIRTextBlock.Text = Convert.ToString(theCPU.CurrentInstructionRegister).PadLeft(8, '0');
                PCTextBlock.Text = Convert.ToString(theCPU.ProgramCounter).PadLeft(8, '0');

                MemoryList.Clear();
                foreach (MemoryCell mem in RAM.Memory)
                {
                    MemoryList.Add(mem);
                }
            }
            else if ((string)((ComboBoxItem)DisplayModeComboBox.SelectedItem).Content == "Hex")
            {
                R0TextBlock.Text = theCPU.GetRegister((byte)0).ToString("X8");
                R1TextBlock.Text = theCPU.GetRegister((byte)1).ToString("X8");
                R2TextBlock.Text = theCPU.GetRegister((byte)2).ToString("X8");
                R3TextBlock.Text = theCPU.GetRegister((byte)3).ToString("X8");
                R4TextBlock.Text = theCPU.GetRegister((byte)4).ToString("X8");
                R5TextBlock.Text = theCPU.GetRegister((byte)5).ToString("X8");
                R6TextBlock.Text = theCPU.GetRegister((byte)6).ToString("X8");
                R7TextBlock.Text = theCPU.GetRegister((byte)7).ToString("X8");
                R8TextBlock.Text = theCPU.GetRegister((byte)8).ToString("X8");
                R9TextBlock.Text = theCPU.GetRegister((byte)9).ToString("X8");
                R10TextBlock.Text = theCPU.GetRegister((byte)10).ToString("X8");
                R11TextBlock.Text = theCPU.GetRegister((byte)11).ToString("X8");

                MARTextBlock.Text = theCPU.MemoryAddressReguister.ToString("X8");
                MBRTextBlock.Text = theCPU.MemoryDataRegister.ToString("X8");
                CIRTextBlock.Text = theCPU.CurrentInstructionRegister.ToString("X8");
                PCTextBlock.Text = theCPU.ProgramCounter.ToString("X8");

                MemoryList.Clear();

                foreach (MemoryCell mem in RAM.Memory)
                {
                    MemoryList.Add(mem);
                }
            }

            Val1Val.Text = "";
            Val2Val.Text = "";
            AnsVal.Text = "";


            Val1Label.Foreground = new SolidColorBrush(Colors.Gray);
            Val2Label.Foreground = new SolidColorBrush(Colors.Gray);
            AnsLabel.Foreground = new SolidColorBrush(Colors.Gray);


            RdLabel.Foreground = new SolidColorBrush(Colors.Gray);
            RnLabel.Foreground = new SolidColorBrush(Colors.Gray);
            RmLabel.Foreground = new SolidColorBrush(Colors.Gray);
            Op2Label.Foreground = new SolidColorBrush(Colors.Gray);
            AddressingLabel.Foreground = new SolidColorBrush(Colors.Gray);
            EQCondLAbel.Foreground = new SolidColorBrush(Colors.Gray);
            NECondLAbel.Foreground = new SolidColorBrush(Colors.Gray);
            LTCondLAbel.Foreground = new SolidColorBrush(Colors.Gray);
            GTCondLAbel.Foreground = new SolidColorBrush(Colors.Gray);

            RdValue.Text = "";
            RnValue.Text = "";
            RmValue.Text = "";
            Op2Value.Text = "";
            AddressingValue.Text = "";

            switch (theCPU.OpCode)
            {
                case AssemblyCommand.OP.ADD:
                case AssemblyCommand.OP.SUB:
                case AssemblyCommand.OP.AND:
                case AssemblyCommand.OP.ORR:
                case AssemblyCommand.OP.EOR:
                case AssemblyCommand.OP.LSL:
                case AssemblyCommand.OP.LSR:
                case AssemblyCommand.OP.MVN:
                    Val1Label.Foreground = new SolidColorBrush(Colors.Black);
                     AnsLabel.Foreground = new SolidColorBrush(Colors.Black);

                    Val1Val.Text = Convert.ToString(theCPU.Value1);
                    if(theCPU.OpCode != AssemblyCommand.OP.MVN)
                    {
                        Val2Label.Foreground = new SolidColorBrush(Colors.Black);
                        Val2Val.Text = Convert.ToString(theCPU.Value1);
                    }
                    AnsVal.Text = Convert.ToString(theCPU.ALUAnswer1);
                    break;
            }

            if (theCPU.CurrentCondition == AssemblyCommand.COND.EQ)
            {
                EQCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (theCPU.CurrentCondition == AssemblyCommand.COND.LT)
            {
                NECondLAbel.Foreground = new SolidColorBrush(Colors.Black);
                LTCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (theCPU.CurrentCondition == AssemblyCommand.COND.GT)
            {
                NECondLAbel.Foreground = new SolidColorBrush(Colors.Black);
                GTCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }

            OpCodeValue.Text = AssemblyCommand.GetOpName(theCPU.OpCode);
            if (theCPU.OpCode != AssemblyCommand.OP.HALT)
            {
                if (theCPU.OpCode != AssemblyCommand.OP.CMP && theCPU.OpCode != AssemblyCommand.OP.B)
                {
                    RdLabel.Foreground = new SolidColorBrush(Colors.Black);
                    RdValue.Text = Convert.ToString(theCPU.Rd);
                    if(theCPU.Immediate)
                    {
                        Op2Label.Foreground = new SolidColorBrush(Colors.Black);
                        AddressingLabel.Foreground = new SolidColorBrush(Colors.Black);
                        Op2Value.Text = Convert.ToString(theCPU.Operand);
                        AddressingValue.Text = "Immediate";
                    }
                    else
                    {
                        if (theCPU.OpCode == AssemblyCommand.OP.LDR || theCPU.OpCode == AssemblyCommand.OP.STR)
                        {
                            Op2Label.Foreground = new SolidColorBrush(Colors.Black);
                            AddressingLabel.Foreground = new SolidColorBrush(Colors.Black);
                            Op2Value.Text = Convert.ToString(theCPU.Operand);
                            AddressingValue.Text = "Direct";
                        }
                        else
                        { 
                            RmLabel.Foreground = new SolidColorBrush(Colors.Black);
                            AddressingLabel.Foreground = new SolidColorBrush(Colors.Black);
                            RmValue.Text = Convert.ToString(theCPU.Rm);
                            AddressingValue.Text = "Direct";
                        }
                    }
                }
                if (theCPU.OpCode == AssemblyCommand.OP.ADD || theCPU.OpCode == AssemblyCommand.OP.SUB || theCPU.OpCode == AssemblyCommand.OP.AND || theCPU.OpCode == AssemblyCommand.OP.ORR || theCPU.OpCode == AssemblyCommand.OP.EOR || theCPU.OpCode == AssemblyCommand.OP.LSL || theCPU.OpCode == AssemblyCommand.OP.LSR)
                {
                    RnValue.Text = Convert.ToString(theCPU.Rn);
                    RnLabel.Foreground = new SolidColorBrush(Colors.Black);
                }
               
            }


        }

        private void DisplayModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           

            if (theCPU != null) // Don't call this pre screen et up
            {
                if ((string)((ComboBoxItem)DisplayModeComboBox.SelectedItem).Content == "Decimal")
                {
                    RAM.DefaultDataType = dataType.Decimal;
                }
                else if ((string)((ComboBoxItem)DisplayModeComboBox.SelectedItem).Content == "Hex")
                {
                    RAM.DefaultDataType = dataType.Hex;
                }
                updateScreen();
            }

        }

        private void FlashStatus()
        {
            resetTimer.Stop();
            ResetAnimation();
            if ((theCPU.Status & (UInt32)CPUStatus.PCToMAR) == (UInt32)CPUStatus.PCToMAR)
            {
                PCToBusGrid.Background = new SolidColorBrush(Colors.Red);
                MARToBusGrid.Background = new SolidColorBrush(Colors.Red);
                UpperHub.Background = new SolidColorBrush(Colors.Red);

            }
            if ((theCPU.Status & (UInt32)CPUStatus.ALUToReg) == (UInt32)CPUStatus.ALUToReg)
            {
                ALUToBUS.Background = new SolidColorBrush(Colors.Red);
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                MiddleHub.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.MBRToCIR) == (UInt32)CPUStatus.MBRToCIR)
            {
                MBRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToREG) == (UInt32)CPUStatus.CIRToREG)
            {
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToALU) == (UInt32)CPUStatus.CIRToALU)
            {
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                ALUToBUS.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                ALULabel.Foreground = new SolidColorBrush(Colors.Red);
                MiddleHub.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
                Val1Val.Foreground = new SolidColorBrush(Colors.Red);
                Val2Val.Foreground = new SolidColorBrush(Colors.Red);
                AnsVal.Foreground = new SolidColorBrush(Colors.Red);

            }
            if ((theCPU.Status & (UInt32)CPUStatus.REGToALU) == (UInt32)CPUStatus.REGToALU)
            {
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                ALUToBUS.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                ALULabel.Foreground = new SolidColorBrush(Colors.Red);
                MiddleHub.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
                Val1Val.Foreground = new SolidColorBrush(Colors.Red);
                Val2Val.Foreground = new SolidColorBrush(Colors.Red);
                AnsVal.Foreground = new SolidColorBrush(Colors.Red);

            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToCU) == (UInt32)CPUStatus.CIRToCU)
            {
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                CUToBUS.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                OpCodeValue.Foreground = new SolidColorBrush(Colors.Red);
                RdValue.Foreground = new SolidColorBrush(Colors.Red);
                RnValue.Foreground = new SolidColorBrush(Colors.Red);
                RmValue.Foreground = new SolidColorBrush(Colors.Red);
                Op2Value.Foreground = new SolidColorBrush(Colors.Red);
                AddressingValue.Foreground = new SolidColorBrush(Colors.Red);

                MiddleHub.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToMAR) == (UInt32)CPUStatus.CIRToMAR)
            {
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                BUSUpper.Background = new SolidColorBrush(Colors.Red);
                MARToBusGrid.Background = new SolidColorBrush(Colors.Red);
                MiddleHub.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
                UpperHub.Background = new SolidColorBrush(Colors.Red);

            }

            if ((theCPU.Status & (UInt32)CPUStatus.MBRToReg) == (UInt32)CPUStatus.MBRToReg)
            {
                MBRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                LowerHub.Background = new SolidColorBrush(Colors.Red);
            }

            if ((theCPU.Status & (UInt32)CPUStatus.PCToPC) == (UInt32)CPUStatus.PCToPC)
            {
                // NEED TO DO THIS
            }


            if ((theCPU.Status & (UInt32)CPUStatus.AssToRAM) == (UInt32)CPUStatus.AssToRAM)
            {
                AssembleyBus.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.AddBus) == (UInt32)CPUStatus.AddBus)
            {
                AddressBus.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.DatBus) == (UInt32)CPUStatus.DatBus)
            {
                DataBus.Background = new SolidColorBrush(Colors.Red);
            }


            if ((theCPU.Status & (UInt32)CPUStatus.PC) == (UInt32)CPUStatus.PC)
            {
                PCTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.MAR) == (UInt32)CPUStatus.MAR)
            {
                MARTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.MBR) == (UInt32)CPUStatus.MBR)
            {
                MBRTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIR) == (UInt32)CPUStatus.CIR)
            {
                CIRTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.ALU) == (UInt32)CPUStatus.ALU)
            {
                ALULabel.Foreground = new SolidColorBrush(Colors.Red);
                if (theCPU.CurrentCondition == AssemblyCommand.COND.EQ)
                {
                    EQCondLAbel.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (theCPU.CurrentCondition == AssemblyCommand.COND.LT)
                {
                    NECondLAbel.Foreground = new SolidColorBrush(Colors.Red);
                    LTCondLAbel.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (theCPU.CurrentCondition == AssemblyCommand.COND.GT)
                {
                    NECondLAbel.Foreground = new SolidColorBrush(Colors.Red);
                    GTCondLAbel.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            if ((theCPU.Status & (UInt32)CPUStatus.RAM) == (UInt32)CPUStatus.RAM)
            {
                //Not sure what to do here!
            }

            if ((theCPU.Status & (UInt32)CPUStatus.R0) == (UInt32)CPUStatus.R0)
            {
                R0TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R1) == (UInt32)CPUStatus.R1)
            {
                R1TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R2) == (UInt32)CPUStatus.R2)
            {
                R2TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R3) == (UInt32)CPUStatus.R3)
            {
                R3TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R4) == (UInt32)CPUStatus.R4)
            {
                R4TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R5) == (UInt32)CPUStatus.R5)
            {
                R5TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R6) == (UInt32)CPUStatus.R6)
            {
                R6TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R7) == (UInt32)CPUStatus.R7)
            {
                R7TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R8) == (UInt32)CPUStatus.R8)
            {
                R8TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R9) == (UInt32)CPUStatus.R9)
            {
                R9TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R10) == (UInt32)CPUStatus.R10)
            {
                R10TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.R11) == (UInt32)CPUStatus.R11)
            {
                R11TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void SetReset()
        {
            

            resetTimer.Interval = new TimeSpan(0, 0, 1);
            resetTimer.Start();
        }

        private void ResetAnimation()
        {
            SolidColorBrush back = new SolidColorBrush(Colors.Blue);
            AddressBus.Background = back;
            DataBus.Background = back;
            AssembleyBus.Background = back;
            PCToBusGrid.Background = back;
            CIRToBusGrid.Background = back;
            BUSLower.Background = back;
            BUSUpper.Background = back;
            MARToBusGrid.Background = back;
            MBRToBusGrid.Background = back;
            GeneralToBus.Background = back;
            ALUToBUS.Background = back;
            CUToBUS.Background = back;

            UpperHub.Background = back;
            MiddleHub.Background = back;
            LowerHub.Background = back;

            PCTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            MARTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            MBRTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            CIRTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            ALULabel.Foreground = new SolidColorBrush(Colors.Black);
            CULabel.Foreground = new SolidColorBrush(Colors.Black);
            OpCodeValue.Foreground = new SolidColorBrush(Colors.Black);
            RdValue.Foreground = new SolidColorBrush(Colors.Black);
            RnValue.Foreground = new SolidColorBrush(Colors.Black);
            RmValue.Foreground = new SolidColorBrush(Colors.Black);
            Op2Value.Foreground = new SolidColorBrush(Colors.Black);
            AddressingValue.Foreground = new SolidColorBrush(Colors.Black);
            Val1Val.Foreground = new SolidColorBrush(Colors.Black);
            Val2Val.Foreground = new SolidColorBrush(Colors.Black);
            AnsVal.Foreground = new SolidColorBrush(Colors.Black);

            R0TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R1TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R2TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R3TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R4TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R5TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R6TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R7TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R8TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R9TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R10TextBlock.Foreground = new SolidColorBrush(Colors.Black);
            R11TextBlock.Foreground = new SolidColorBrush(Colors.Black);


            if (theCPU.CurrentCondition == AssemblyCommand.COND.EQ)
            {
                EQCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (theCPU.CurrentCondition == AssemblyCommand.COND.LT)
            {
                NECondLAbel.Foreground = new SolidColorBrush(Colors.Black);
                LTCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (theCPU.CurrentCondition == AssemblyCommand.COND.GT)
            {
                NECondLAbel.Foreground = new SolidColorBrush(Colors.Black);
                GTCondLAbel.Foreground = new SolidColorBrush(Colors.Black);
            }

        }

        private void dispatcherTimer_Tick(object sender, object e)
        {
            ResetAnimation();
            resetTimer.Stop();
        }
        private void dispatcherTimer_FormatTick(object sender, object e)
        {
            dontUpdateSave = true;
            TextPointer pointer = SourceTextBox.Document.ContentStart;
            TextPointer endPointer = SourceTextBox.Document.ContentEnd;
            
            if (pointer != null && endPointer != null)
            {
                ClearSourceFormatting(pointer, endPointer);
                FormatSource(pointer);
            }
            dontUpdateSave = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            theCPU.Reset();
            updateScreen();
        }

        private bool dontParse = false;
        private void ErrorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (ErrorListView.SelectedIndex > -1)
            {
               // ClearSourceFormatting();
                AssemblerError theError =  (AssemblerError)ErrorListView.Items[ErrorListView.SelectedIndex];
                Block theBlock = SourceTextBox.Document.Blocks.ElementAt(theError.Line);
                TextRange r = new TextRange(theBlock.ContentStart, theBlock.ContentEnd);
                if (r.Text.Trim().Equals("") == false)
                {
                    dontParse = true;
                    r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Red);
                    r.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    dontParse = false;
                }
            }

        }

        public delegate void highlighter(TextPointer start, TextPointer end);

        public void highlightOperation(TextPointer start, TextPointer end)
        {
            TextRange highlight = new TextRange(start, end);
            highlight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
            highlight.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            SourceTextBox.CaretPosition = SourceTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            if (end.GetNextInsertionPosition(LogicalDirection.Forward) == null)
            {
                new Run("", end);
            }
        }
        public void highlightRegister(TextPointer start, TextPointer end)
        {
            // end.GetAdjacentElement(LogicalDirection.Backward);
            TextRange highlight = new TextRange(start, end);
            highlight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Orange);
        }
        public void highlightAbsolouteValue(TextPointer start, TextPointer end)
        {
            // end.GetAdjacentElement(LogicalDirection.Backward);
            TextRange highlight = new TextRange(start, end);
            highlight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }
        public void highlightAbsolouteHexValue(TextPointer start, TextPointer end)
        {
            // end.GetAdjacentElement(LogicalDirection.Backward);
            TextRange highlight = new TextRange(start, end);
            highlight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Purple);
        }

        public void highlightLabel(TextPointer start, TextPointer end)
        {
            TextRange highlight = new TextRange(start, end);
            highlight.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            highlight.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            SourceTextBox.CaretPosition = SourceTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
            if (end.GetNextInsertionPosition(LogicalDirection.Forward) == null)
            {
                new Run("", end);
            }
        }

        public void formatText(TextPointer pointer, string pattern, highlighter myHighlighter)
        {
            while (pointer != null)
            {

                if (pointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementEnd)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        
                        dontParse = true;
                        myHighlighter(start, end);
                        dontParse = false;
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        private void SourceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            formatTimer.Stop();
            if (!dontUpdateSave)
            {
                SaveButton.IsEnabled = true;
            }
            if (!dontParse)
            {
                TextPointer pointer = SourceTextBox.CaretPosition.GetLineStartPosition(0);
                TextPointer endPointer = SourceTextBox.CaretPosition.GetLineStartPosition(1);
                if (endPointer == null)
                {
                    endPointer = SourceTextBox.Document.ContentEnd;
                }
                else
                {
                    endPointer = endPointer.GetInsertionPosition(LogicalDirection.Backward);
                }
                if (pointer != null && endPointer != null)
                {
                    ClearSourceFormatting(pointer, endPointer);
                    FormatSource(pointer);
                }



            }
            formatTimer.Start();

        }


        private void FormatSource(TextPointer pointer)
        {
            string pattern = "(MOV|CMP|LDR|STR|ADD|SUB|AND|ORR|EOR|LSL|LSR|MVN|BEQ|BNE|BLT|BGT|B|HALT)(\\s+|\\Z)";
            formatText(pointer, pattern, highlightOperation);
            pattern = "R\\d+(\\s*|\\Z|,)";
            formatText(pointer, pattern, highlightRegister);
            pattern = "#0x(\\d|A|B|C|D|E|F)+(\\s*|\\Z|,)";
            formatText(pointer, pattern, highlightAbsolouteHexValue);
            pattern = "#\\d+(?!x)(\\s*|\\Z|,)";
            formatText(pointer, pattern, highlightAbsolouteValue);
            pattern = "\\w+:(\\s+|\\Z)";
            formatText(pointer, pattern, highlightLabel);


        }



        private void ClearSourceFormatting()
        {
            ClearSourceFormatting(SourceTextBox.Document.ContentStart, SourceTextBox.Document.ContentEnd);
        }



        private void ClearSourceFormatting(TextPointer start, TextPointer end)
        {
            TextRange r = new TextRange(start, end);
            if (r.Text.Trim().Equals("") == false)
            {
                dontParse = true;
                r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
                r.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                dontParse = false;
            }
        }

        private void SourceTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
           // ClearSourceFormatting();
        }


        private void MemoryValueTextBlock_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            // your event handler here
            e.Handled = true;
            MessageBox.Show("Enter pressed");
        }

        private void MemoryValueTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox memoryLocation = (TextBox)sender;
                  memoryLocation.IsEnabled = true;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (theCPU != null)
            {
                if (StepSpeedSlider.Value == 0)
                {
                    theCPU.StepSpeed = "Step Only";
                    RunButton.IsEnabled = true;
                    CycleButton.IsEnabled = true;
                    StepButton.IsEnabled = true;
                    
                }
                else if (StepSpeedSlider.Value == 1)
                {
                    theCPU.StepSpeed = "Slow";
                }
                else if (StepSpeedSlider.Value == 2)
                {
                    theCPU.StepSpeed = "Medium";
                }
                else if (StepSpeedSlider.Value == 3)
                {
                    theCPU.StepSpeed = "Fast";
                }
                else if (StepSpeedSlider.Value == 4)
                {
                    theCPU.StepSpeed = "Very Fast";
                }
                else
                {
                    theCPU.StepSpeed = "Lightning";
                }
                StepSpeedValue.Text = theCPU.StepSpeed;
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (theCPU != null)
            {
                theCPU.SafetyBrake = true;
            }
           
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (theCPU != null)
            {
                theCPU.SafetyBrake = false;
            }
        }

        private void SourceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
           /* if (e. && e.KeyCode == Keys.V)
            {
                Clipboard.SetText((string)Clipboard.GetData("Text"), TextDataFormat.Text);
            }*/

        }
    }
}
