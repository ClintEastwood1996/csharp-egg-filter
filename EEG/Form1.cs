using System;
using System.IO;
using System.Windows.Forms;


namespace EEG
{
    public partial class Form1 : Form
    {
        #region Переменные
        public double[,] EEG = new double[1000, 3];
        public double[,] EEG_F = new double[1000, 3];
        public double[,] EEG_Alpha = new double[1000, 3];
        public double[,] EEG_Beta = new double[1000, 3];
        public double[,] EEG_Delta = new double[1000, 3];
        public double[,] EEG_Teta = new double[1000, 3];

        public string filename;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Button_Get_File_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[][] Eeg_string = new string[1000][];
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                for (int i = 0; i < 1000; i++)
                {
                    Eeg_string[i] = sr.ReadLine().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                }


                for (int i = 0; i < 1000; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        EEG[i, j] = Double.Parse(Eeg_string[i][j]);
                    }
                }
            }

            ShowSignal(EEG);
            GetFilts();
            Calculate(EEG_F, comboBox_Signals.SelectedIndex, "All");
            Calculate(EEG_Alpha, comboBox_Signals.SelectedIndex, "Alpha");
            Calculate(EEG_Beta, comboBox_Signals.SelectedIndex, "Beta");
            Calculate(EEG_Delta, comboBox_Signals.SelectedIndex, "Delta");
            Calculate(EEG_Teta, comboBox_Signals.SelectedIndex, "Teta");
            SaveFile(EEG_F, "EEG_F");
            SaveFile(EEG_Alpha, "EEG_Alpha");
            SaveFile(EEG_Beta, "EEG_Beta");
            SaveFile(EEG_Delta, "EEG_Delta");
            SaveFile(EEG_Teta, "EEG_Teta");

            groupBox_Filtering.Enabled = true;
            groupBox_Params.Enabled = true;
        }

        private void ComboBox_Signals_SelectedIndexChanged(object sender, EventArgs e)
        {
            Calculate(EEG_F, comboBox_Signals.SelectedIndex, "All");
            Calculate(EEG_Alpha, comboBox_Signals.SelectedIndex, "Alpha");
            Calculate(EEG_Beta, comboBox_Signals.SelectedIndex, "Beta");
            Calculate(EEG_Delta, comboBox_Signals.SelectedIndex, "Delta");
            Calculate(EEG_Teta, comboBox_Signals.SelectedIndex, "Teta");
        }

        #region РадиоБАТОНЫ
        private void RadioButton_With_Filter_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_With_Filter.Checked == true)
            {
                ShowSignal(EEG_F);
                groupBox_Rhythms.Enabled = true;
            }
            else
            {
                ShowSignal(EEG);
                groupBox_Rhythms.Enabled = false;
                radioButton_All.Checked = true;
            }
        }


        private void RadioButton_All_CheckedChanged(object sender, EventArgs e)
        {
            ShowSignal(EEG_F);
        }

        private void RadioButton_Alpha_CheckedChanged(object sender, EventArgs e)
        {
            ShowSignal(EEG_Alpha);
        }

        private void RadioButton_Beta_CheckedChanged(object sender, EventArgs e)
        {
            ShowSignal(EEG_Beta);
        }

        private void RadioButton_Delta_CheckedChanged(object sender, EventArgs e)
        {
            ShowSignal(EEG_Delta);
        }

        private void RadioButton_Teta_CheckedChanged(object sender, EventArgs e)
        {
            ShowSignal(EEG_Teta);
        }
        #endregion

        public void GetFilts()
        {
            //Режекторный фильтр 50 ГЦ
            double[] b = new double[5] { 0.81299926660162602, 0.20653078859909993, 0.81299926660162602, 0, 0 };
            double[] a = new double[5] { 1, 0.20653078859909993, 0.62599853320325205, 0, 0 };

            //Фильтр высоких частот
            double[] b0 = new double[5] { 0.059560596234149672, 0.23824238493659869, 0.35736357740489805, 0.23824238493659869, 0.059560596234149672 };
            double[] a0 = new double[5] { 1, -0.52804595798372822, 0.57428885374252248, -0.11645534850163169, 0.023181992489232316 };

            double[,] EEG_F50 = new double[1000, 3];

            Filter(a, b, EEG, EEG_F50);
            Filter(a0, b0, EEG_F50, EEG_F);

            //Альфа-ритм
            double[] b1 = new double[5] { 0.0064243272708383583, 0, -0.012848654541676717, 0, 0.0064243272708383583 };
            double[] a1 = new double[5] { 1, -3.536940547162883, 4.898377413936986, -3.1353761314893918, 0.78650924073317463 };

            Filter(a1, b1, EEG_F, EEG_Alpha);

            //Бета-ритм
            double[] b2 = new double[5] { 0.11808244068350608, 0, -0.23616488136701216, 0, 0.11808244068350608 };
            double[] a2 = new double[5] { 1, -1.8977367021353782, 1.7778721911155602, -0.94700425523135534, 0.29335754459191271 };

            Filter(a2, b2, EEG_F, EEG_Beta);

            //Дельта-ритм
            double[] b3 = new double[5] { 0.0016993510112648601, 0, -0.0033987020225297202, 0, 0.0016993510112648601 };
            double[] a3 = new double[5] { 1, -3.876695812206167, 5.6404417820010009, -3.6505946195466077, 0.88685147161922362 };

            Filter(a3, b3, EEG_F, EEG_Delta);

            //Тета-ритм
            double[] b4 = new double[5] { 0.0011002475046724876, 0, -0.0022004950093449751, 0, 0.0011002475046724876 };
            double[] a4 = new double[5] { 1, -3.8500711759386448, 5.6118082110652443, -3.6694168177956703, 0.9084074363157848 };

            Filter(a4, b4, EEG_F, EEG_Teta);
        }

        public void ShowSignal(double[,] EEGin)
        {
            //Первый сигнал
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series[0].Points.Clear();
            int y1 = 0;
            double x1 = 0;
            while (y1 < 1000) // /////////////////
            {
                chart1.Series[0].Points.AddXY(x1, EEGin[y1, 0]);
                x1 = x1 + 0.004;//250 Гц
                y1++;
            }

            //Второй сигнал
            chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series[0].Points.Clear();
            int y2 = 0;
            double x2 = 0;
            while (y2 < 1000) ////////
            {
                chart2.Series[0].Points.AddXY(x2, EEGin[y2, 1]);
                x2 = x2 + 0.004;
                y2++;
            }

            //Третий сигнал
            chart3.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart3.Series[0].Points.Clear();
            int y3 = 0;
            double x3 = 0;
            while (y3 < 1000) /////////////
            {
                chart3.Series[0].Points.AddXY(x3, EEGin[y3, 2]);
                x3 = x3 + 0.004;
                y3++;
            }
        }

        public void Filter(double[] a, double[] b, double[,] EEGin, double[,] EEGOut)
        {
            double[,] b_signal = new double[EEGin.GetLength(0), EEGin.GetLength(1)];
            double[,] a_signal = new double[EEGin.GetLength(0), EEGin.GetLength(1)];
            for (int i = b.Length; i < EEGin.GetLength(0); i++)
                for (int j = 0; j < EEGin.GetLength(1); j++)
                {
                    for (int k = 0; k < b.Length; k++)
                    {
                        b_signal[i, j] = b_signal[i, j] + b[k] * EEGin[i - k, j];
                    }
                    for (int k = 1; k < b.Length; k++)
                    {
                        a_signal[i, j] = a_signal[i, j] + a[k] * EEGOut[i - k, j];
                    }
                    EEGOut[i, j] = b_signal[i, j] - a_signal[i, j];
                }
        }

        public void Calculate(double[,] EEGin, int SignalNumber, string Rhythm)
        {
            int N = EEGin.GetLength(0);
            double MD0 = 0;
            double MD = 0;
            double MaxD = 0;
            double SD0 = 0;
            double SD = 0;
            double Var = 0;

            double summ = 0;
            for (int i = 0; i < N; i++)
                summ = summ + EEGin[i, SignalNumber];
            double mean = summ / N; //среднее значение


            //Срденее отклонение
            for (int i = 0; i < N; i++)
            {
                MD0 = MD0 + Math.Abs(EEGin[i, SignalNumber] - mean);
            }

            MD = Math.Round(MD0 / N, 4);

            //Максимальное отклонение
            for (int i = 0; i < N; i++)
            {
                if (MaxD < EEGin[i, SignalNumber])
                {
                    MaxD = Math.Round(EEGin[i, SignalNumber], 4);
                }
            }

            //СКО
            for (int i = 0; i < N; i++)
            {
                SD0 = SD0 + (Math.Pow((EEGin[i, SignalNumber] - mean), 2));
            }

            SD = Math.Round(Math.Sqrt(SD0 / N), 4);

            //Дисперсия
            Var = Math.Round(Math.Pow(SD, 2), 4);

            //устанавливаем для всех ритмов
            if (Rhythm == "All")
            {
                textBox_All_MD.Text = MD.ToString();
                textBox_All_MaxD.Text = MaxD.ToString();
                textBox_All_SD.Text = SD.ToString();
                textBox_All_Var.Text = Var.ToString();
            }


            //устанавливаем для всех Alpha
            if (Rhythm == "Alpha")
            {
                textBox_Alpha_MD.Text = MD.ToString();
                textBox_Alpha_MaxD.Text = MaxD.ToString();
                textBox_Alpha_SD.Text = SD.ToString();
                textBox_Alpha_Var.Text = Var.ToString();
            }

            //устанавливаем для всех Beta
            if (Rhythm == "Beta")
            {
                textBox_Beta_MD.Text = MD.ToString();
                textBox_Beta_MaxD.Text = MaxD.ToString();
                textBox_Beta_SD.Text = SD.ToString();
                textBox_Beta_Var.Text = Var.ToString();
            }

            //устанавливаем для всех Delta
            if (Rhythm == "Delta")
            {
                textBox_Delta_MD.Text = MD.ToString();
                textBox_Delta_MaxD.Text = MaxD.ToString();
                textBox_Delta_SD.Text = SD.ToString();
                textBox_Delta_Var.Text = Var.ToString();
            }

            //устанавливаем для всех Teta
            if (Rhythm == "Delta")
            {
                textBox_Teta_MD.Text = MD.ToString();
                textBox_Teta_MaxD.Text = MaxD.ToString();
                textBox_Teta_SD.Text = SD.ToString();
                textBox_Teta_Var.Text = Var.ToString();
            }
        }

        public void SaveFile(double[,] EEGin, string name)
        {
            string[] EEGStr = new string[1000];

            for (int i = 0; i < EEGStr.Length; i++)
            {
                EEGStr[i] = Math.Round(EEGin[i, 0], 2).ToString() + "  " + Math.Round(EEGin[i, 1], 2).ToString() + "  "
                    + Math.Round(EEGin[i, 2], 2).ToString();
            }

            string FilePath = Directory.GetCurrentDirectory().ToString() + @"\" + name + ".txt";
            File.WriteAllLines(FilePath, EEGStr);
        }
    }


}
