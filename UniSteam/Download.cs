using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace UniSteam
{
    public partial class Download : Form
    {
        private Game game;
        public Download(Game game)
        {
            this.game = game;
            InitializeComponent();
        }

        private void Download_Load(object sender, EventArgs e)
        {
            label1.Text = "Click Start to begin downloading " + game.name + ".";
            label1.AutoSize = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void zipDownloaded(object sender, AsyncCompletedEventArgs e)
        {
            label1.Text = ("Installing game ...");
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;
            progressBar1.Value = 1;
            //progressBar1.Style = ProgressBarStyle.Marquee;


            ZipFile.ExtractToDirectory("" + Form1.root + "\\games\\temp.zip", "" + Form1.root + "\\games" + "\\" + game.id);

            //progressBar1.Style = ProgressBarStyle.Continuous;
            button2.Enabled = true;
            button2.Text = "Done";
            label1.Text = (game.name + " successfully installed!");

            Close();
            MessageBox.Show("" + game.name + " successfully installed!");

        }

        long last = 0;
        private async void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (button2.Text == "Done")
            {
                Close();
            }
            else
            {
#if DEBUG
                Console.WriteLine("http://" + Globals.DOMAIN + "/" + game.id + "/current.zip");
                Console.WriteLine(Form1.root + "\\games\\temp.zip");
#else
                label1.Text = "Downloading " + game.name + " ...";
#endif

                WebClient client = new WebClient();


                client.DownloadProgressChanged += delegate(object Object, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
                {
                    
                    //yeah inefficient as anything but like i gots to know
                    if (totalBytes == -1) totalBytes = downloadProgressChangedEventArgs.TotalBytesToReceive;
                    if(downloadProgressChangedEventArgs.BytesReceived > last + (1024*1024)) {
                        Console.WriteLine(downloadProgressChangedEventArgs.BytesReceived);
                        last = downloadProgressChangedEventArgs.BytesReceived;
                    }
                    if (downloadProgressChangedEventArgs.BytesReceived == downloadProgressChangedEventArgs.TotalBytesToReceive) last = downloadProgressChangedEventArgs.TotalBytesToReceive;
                    //we send it the number of bytes we done got so far
                    if(backgroundWorker1.IsBusy) backgroundWorker1.ReportProgress((int)(downloadProgressChangedEventArgs.BytesReceived));
                    
                };
                client.DownloadFileCompleted += delegate(object Object, AsyncCompletedEventArgs asyncCompletedEventArgs)
                {
                    Console.WriteLine("wii dun he'");
                };
                await client.DownloadFileTaskAsync(
                    new Uri("http://" + Globals.DOMAIN + "/" + game.id + "/current.zip"),
                    Form1.root + "\\games\\temp.zip");
                Console.WriteLine("i appear to have returned...");
                

            }
        }

        private long totalBytes = -1;



        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = (int)(totalBytes / 1024);
            progressBar1.Value = e.ProgressPercentage;

        }

    }
}