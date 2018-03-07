using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

namespace MultiThreading
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double fibbonacciValue, fibonacciResult;
        public MainWindow()
        {
            InitializeComponent();
        }

        //Zadanie 1a
        private void buttonTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int n = int.Parse(textBoxNValue.Text);
                int k = int.Parse(textBoxKValue.Text);
                if (n >= k)
                {
                    Task<int> taskLicznik = Task.Factory.StartNew(() =>
                    {
                        int result = n;
                        for (int i = n - 1; i >= n - k + 1; i--)
                        {
                            result *= i;
                        }
                        return result;
                    });
                    Task<int> taskMianownik = Task.Factory.StartNew(() =>
                    {
                        int result = 1;
                        for (int i = 1; i <= k; i++)
                        {
                            result *= i;
                        }
                        return result;
                    });
                    labelTasksResult.Content = taskLicznik.Result / taskMianownik.Result;
                } else
                {
                    MessageBox.Show("n musi być >= k");
                }
            }
            catch
            {
                MessageBox.Show("Zły format wejściowy");
            }
        }

        //Zadanie 1b
        private void buttonDelegates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int n = int.Parse(textBoxNValue.Text);
                int k = int.Parse(textBoxKValue.Text);
                if (n >= k)
                {
                    Func<int, int, int> newtonLicznik = licznik;
                    Func<int, int> newtonMianownik = mianownik;
                    IAsyncResult asyncLicznik;
                    IAsyncResult asyncMianownik;
                    asyncLicznik = newtonLicznik.BeginInvoke(n, k, null, null);
                    asyncMianownik = newtonMianownik.BeginInvoke(k, null, null);
                    labelDelegatesResult.Content = newtonLicznik.EndInvoke(asyncLicznik) / newtonMianownik.EndInvoke(asyncMianownik);
                }
                else
                {
                    MessageBox.Show("n musi być >= k");
                }
            }
            catch
            {
                MessageBox.Show("Zły format wejściowy");
            }

        }

        private int licznik(int n, int k)
        {
            int result = n;
            for (int i = n - 1; i >= n - k + 1; i--)
            {
                result *= i;
            }
            return result;
        }

        private int mianownik(int k)
        {
            int result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= i;
            }
            return result;
        }

        //Zadanie 1c
        private async void buttonAsyncAwait_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int n = int.Parse(textBoxNValue.Text);
                int k = int.Parse(textBoxKValue.Text);
                if (n >= k)
                {
                    int licznik = await licznikAsyncAwait(n, k);
                    int mianownik = await mianownikAsyncAwait(k);
                    labelAsuncAwaitResult.Content = licznik / mianownik;
                }
                else
                {
                    MessageBox.Show("n musi być >= k");
                }
            }
            catch
            {
                MessageBox.Show("Zły format wejściowy");
            }
        }

        private async Task<int> licznikAsyncAwait(int n, int k)
        {
            int result = n;
            for (int i = n - 1; i >= n - k + 1; i--)
            {
                result *= i;
            }
            return result;
        }

        private async Task<int> mianownikAsyncAwait(int k)
        {
            int result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= i;
            }
            return result;
        }

        //Zadanie 2
        private void buttonFibonacciGet_Click(object sender, RoutedEventArgs e)
        {
            fibbonacciValue = int.Parse(textBoxiValue.Text);
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;         
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            double a = 0;
            double b = 1;
            for (int i = 1; i < fibbonacciValue; i++)
            {
                b += a;
                a = b - a;
                ((BackgroundWorker)sender).ReportProgress((int)((100 / (double)fibbonacciValue) * (i + 1)));
                Thread.Sleep(20);


            }
            fibonacciResult = b;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarFibonacci.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelFibonacciResult.Content = fibonacciResult.ToString();
        }

        //Zadanie 3 Kompresja
        private void buttonCompress_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Wybierz folder do kompresji"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo directorySelected = new DirectoryInfo(dlg.SelectedPath);
                Parallel.ForEach(directorySelected.GetFiles().ToList(), fileToCompress => {
                    Compress(fileToCompress);
                });
            }
        }

        //https://msdn.microsoft.com/en-us/library/ms404280(v=vs.110).aspx
        public static void Compress(FileInfo fileToCompress)
        {
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) &
                   FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                           CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);

                        }
                    }
                    FileInfo info = new FileInfo(fileToCompress.DirectoryName + "\\" + fileToCompress.Name + ".gz");
                    Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                    fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                }

            }
        }

        //Zadanie 3 Dekompresja
        private void buttonDecompress_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Wybierz folder do dekompresji"
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo directorySelected = new DirectoryInfo(dlg.SelectedPath);
                Parallel.ForEach(directorySelected.GetFiles("*.gz").ToList(), fileToDecompress => {
                    Decompress(fileToDecompress);
                });
            }


        }

        //https://msdn.microsoft.com/en-us/library/ms404280(v=vs.110).aspx
        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }

        //Zadanie 4
        string wynikDNS = "";
        private void buttonDNSResolve_Click(object sender, RoutedEventArgs e)
        {
            textBoxDNSResult.Text = "";

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();



                

        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBoxDNSResult.Text = wynikDNS.ToString();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] hostNames = { "www.microsoft.com", "www.apple.com", "www.google.com", "www.ibm.com", "cisco.netacad.net", "www.oracle.com", "www.nokia.com", "www.hp.com", "www.dell.com", "www.samsung.com", "www.toshiba.com", "www.siemens.com", "www.amazon.com", "www.sony.com", "www.canon.com", "www.alcatel-lucent.com", "www.acer.com", "www.motorola.com" };
            
            hostNames.AsParallel().ForAll((hostname) => {
                string tmp = "";
                tmp += (hostname + " =>\n");
                IPAddress[] ips;
                ips = Dns.GetHostAddresses(hostname);
                foreach (IPAddress ip in ips)
                {
                    tmp += (ip.ToString() + "\n");
                }
                wynikDNS += tmp;

            });
            Thread.Sleep(20);
        }
    }
}
