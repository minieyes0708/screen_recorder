using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace screen_recorder
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        private string btnRecordText = "Record Screen";
        public string BtnRecordText
        {
            get { return btnRecordText; }
            set { btnRecordText = value; Notify("BtnRecordText"); }
        }
        private string btnConvertGifText = "Convert Gif";
        public string BtnConvertGifText
        {
            get { return btnConvertGifText; }
            set { btnConvertGifText = value; Notify("BtnConvertGifText"); }
        }

        Process process = new Process();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private async void btnSelectRegion_Click(object sender, RoutedEventArgs e)
        {
            await App.selectRegion.Start();
        }
        private void btnRecordScreen_Click(object sender, RoutedEventArgs e)
        {
            if (BtnRecordText == "Stop")
            {
                BtnRecordText = "Record Screen";
                process.StandardInput.Write("q");
                process.WaitForExit();
                return;
            }

            BtnRecordText = "Stop";
            Rect rect = App.selectRegion.GetSelectedRegion();
            process.StartInfo = new ProcessStartInfo("ffmpeg",
                string.Join(" ", new string[]
                {
                    "-hide_banner",
                    "-loglevel error",
                    "-stats",
                    "-f dshow",
                    "-i audio=\"麥克風排列 (w500)\"",
                    "-f gdigrab",
                    "-framerate 60",
                    $"-offset_x {(int)rect.Left}",
                    $"-offset_y {(int)rect.Top}",
                    $"-video_size {(int)rect.Width + ((int)rect.Width % 2)}x{(int)rect.Height + ((int)rect.Height % 2)}",
                    "-draw_mouse 1",
                    "-i desktop",
                    "-c:v libx264",
                    "-r 60",
                    "-preset ultrafast",
                    "-pix_fmt yuv420p",
                    "-y screen_record.mp4"
                }))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
            };
            process.Start();
        }
        private void btnShowVideo_Click(object sender, RoutedEventArgs e)
        {
            _ = new Process()
            {
                StartInfo = new ProcessStartInfo("screen_record.mp4"),
            }.Start();
        }
        private void btnConvertGif_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            BtnConvertGifText = "Converting...";

            if (File.Exists("screen_record.gif"))
            {
                File.Delete("screen_record.gif");
            }

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo("ffmpeg",
                    string.Join(" ", new string[] {
                        "-i screen_record.mp4",
                        "-ss 0",
                        "-filter_complex \"[0:v]fps=15,scale=-1:256,split[a][b];[a]palettegen[p];[b][p]paletteuse\"",
                        "screen_record.gif" }))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                },
            };
            process.Start();
            process.WaitForExit();

            BtnConvertGifText = "Convert Gif";
            MessageBox.Show("Done");
            IsEnabled = true;
        }
        private void btnShowGif_Click(object sender, RoutedEventArgs e)
        {
            _ = new Process()
            {
                StartInfo = new ProcessStartInfo("screen_record.gif"),
            }.Start();
        }
    }
}
