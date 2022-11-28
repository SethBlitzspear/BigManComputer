using AssemblerCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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


        public MainWindow()
        {
            this.InitializeComponent();
            RAM = new MemoryBank(4096);
            theCPU = new AssemblerCPU(RAM);
            theAssembler = new Assembler();
            theAssembler.Ram = RAM;
            ErrorList = new ObservableCollection<AssemblerError>();
            MemoryList = new ObservableCollection<MemoryCell>();
            MemoryListBox.ItemsSource = MemoryList;
            ErrorListView.ItemsSource = ErrorList;
            foreach (MemoryCell mem in RAM.Memory)
            {
                MemoryList.Add(mem);
            }

        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            theAssembler.RawSource = SourceTextBox.Text;
            theAssembler.TryParse();
            ErrorList.Clear();

            foreach (AssemblerError error in theAssembler.Errors)
            {
                ErrorList.Add(error);
            }


        }

        private void AssembleButton_Click(object sender, RoutedEventArgs e)
        {
            theAssembler.RawSource = SourceTextBox.Text;
            theCPU.Reset();
            try
            {
                theAssembler.Assemble();
            }
            catch (Exception ex)
            {
                //
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
            if (!theCPU.HaltCondition)
            {
                theCPU.SteppingMode = false;
                theCPU.Cycle();
                updateScreen();
            }
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (!theCPU.HaltCondition)
            {
                theCPU.SteppingMode = true;
                theCPU.Cycle();
                FlashStatus();
                updateScreen();

                SetReset();
            }
        }

        private void updateScreen()
        {


            MachineStateTextBlock.Text = theCPU.CpuStatus;
            MachineConditionTextBlock.Text = theCPU.CpuCondition;
            MachineCycleTextBlock.Text = theCPU.CpuCycle;

            if ((string)((ComboBoxItem)DisplayModeComboBox.SelectedItem).Content == "Decimal")
            {
                R0TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)0));
                R1TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)1));
                R2TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)2));
                R3TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)3));
                R4TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)4));
                R5TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)5));
                R6TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)6));
                R7TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)7));
                R8TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)8));
                R9TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)9));
                R10TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)10));
                R11TextBlock.Text = Convert.ToString(theCPU.GetRegister((byte)11));

                MARTextBlock.Text = Convert.ToString(theCPU.MemoryAddressReguister);
                MBRTextBlock.Text = Convert.ToString(theCPU.MemoryDataRegister);
                CIRTextBlock.Text = Convert.ToString(theCPU.CurrentInstructionRegister);
                PCTextBlock.Text = Convert.ToString(theCPU.ProgramCounter);

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

        }

        private void DisplayModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (theCPU != null) // Don't call this pre screen et up
            {
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
            }
            if ((theCPU.Status & (UInt32)CPUStatus.ALUToReg) == (UInt32)CPUStatus.ALUToReg)
            {
                ALUToBUS.Background = new SolidColorBrush(Colors.Red);
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.MBRToCIR) == (UInt32)CPUStatus.MBRToCIR)
            {
                MBRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToREG) == (UInt32)CPUStatus.CIRToREG)
            {
                GeneralToBus.Background = new SolidColorBrush(Colors.Red);
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToALU) == (UInt32)CPUStatus.CIRToALU)
            {
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                ALUToBUS.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                ALULabel.Foreground = new SolidColorBrush(Colors.Red);
            }
            if ((theCPU.Status & (UInt32)CPUStatus.CIRToCU) == (UInt32)CPUStatus.CIRToCU)
            {
                CIRToBusGrid.Background = new SolidColorBrush(Colors.Red);
                CUToBUS.Background = new SolidColorBrush(Colors.Red);
                BUSLower.Background = new SolidColorBrush(Colors.Red);
                CULabel.Foreground = new SolidColorBrush(Colors.Red);
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
            resetTimer.Tick += new EventHandler(dispatcherTimer_Tick);

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

            PCTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            MARTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            MBRTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            CIRTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            ALULabel.Foreground = new SolidColorBrush(Colors.Black);
            CULabel.Foreground = new SolidColorBrush(Colors.Black);

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

        }

        private void dispatcherTimer_Tick(object sender, object e)
        {
            ResetAnimation();
            resetTimer.Stop();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            theCPU.Reset();
        }
    }
}
