using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UniSteam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            
        }

        //File Transfer Successful

        private void Button1_Click(object sender, EventArgs e)
        {
            new Upload().ShowDialog();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadStore();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private Game[] games;

        private void loadStore()
        {
            //get a list of games we have.
            games = getGames();
        }

        private Game[] getGames()
        {
            backgroundWorker1.ReportProgress(-1);
            //create an empty list of game objects.
            List<Game> games = new List<Game>();

            string pythonresponse = getPythonResponse("search", "");

            //L L L L L L L L L L L L L LLOOOOP DA LINES
            string[] lines = pythonresponse.Split('\n');
            int i = 0;
            foreach (string id in lines)
            {
                i++;
                Game game = getGame(id);

                games.Add(game);
                backgroundWorker1.ReportProgress((int)((i/(double)lines.Length)*100d));

            }

            return games.ToArray<Game>();
        }

        public static string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

        public static string exeFolder = exePath.Substring(0, exePath.LastIndexOf("\\"));

        private static string getPythonResponse(string script, string args)
        {
            string _return = null;
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = script + ".exe";
            startInfo.WorkingDirectory = exeFolder;
            startInfo.Arguments = " " + args;
            startInfo.CreateNoWindow = true;

            int pid = new Random().Next(100, 999);

            StringBuilder builder = new StringBuilder();

            startInfo.RedirectStandardOutput = true;
            process.ErrorDataReceived += delegate(object o, DataReceivedEventArgs e)
            {
                Console.WriteLine("[" + pid + " Python err] " + e.Data);
            };


            startInfo.RedirectStandardError = true;
            process.OutputDataReceived += delegate(object o, DataReceivedEventArgs e)
            {

                Console.WriteLine("[" + pid + " Python out] " + e.Data);
                builder.Append(e.Data + "\n");
                if (e.Data == "end")
                {
                    _return = builder.ToString();

                }

            };

            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
            return _return.Substring(0, _return.Length - 5);
        }

        private static Game getGame(string id)
        {
            return JsonConvert.DeserializeObject<Game>(getPythonResponse("getGame", id));
        }

        public static string root;

        private void addGameListing(Game game)
        {
            bool downloaded = File.Exists(root + "\\games\\" + game.id + "\\" + game.executableName);

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.FlowDirection = FlowDirection.TopDown;
            panel.Size = new Size(400, 100);
            Button download = new Button();
            Console.WriteLine(root);
            download.Text = downloaded ? "Play Now!" : "Click here to download.";
            
            if (!downloaded)
            {
                download.Click += delegate(object sender, EventArgs e)
                {
                    Download downloadWindow = new Download(game);
                    downloadWindow.ShowDialog();

                };
            }
            else
            {
                download.Click += delegate(object sender, EventArgs e)
                {
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = root + "\\games\\" + game.id + "\\" + game.executableName;
                    process.Start();

                };
            }
            download.AutoSize = true;

            Label title = new Label();
            title.Font = new Font("Arial", 20);
            title.Text = game.name.Replace("&", "&&");
            title.AutoSize = true;
            panel.Controls.Add(title);

            Label versionLabel = new Label();
            versionLabel.Text = "v" + game.version;

            panel.Controls.Add(versionLabel);
            panel.Controls.Add(download);

            flowLayoutPanel1.Controls.Add(panel);



        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case -1:
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.MarqueeAnimationSpeed = 20;
                    break;
                default:
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = e.ProgressPercentage;
                    break;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //TODO
            flowLayoutPanel1.Controls.Clear();

            foreach (Game game in games) addGameListing(game);

        }

    }

    public class Game
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "executableName")]
        public string executableName { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string version { get; set; }
    }

}
