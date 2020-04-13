using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Synchronizer;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

namespace FolderSync
{
    public partial class Form1 : Form
    {

        /*MIT License

        Copyright(c) 2020 Sipos Attila

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files(the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.*/

        public Form1()
        {
            InitializeComponent();
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;

            if (!File.Exists("LICENSE.txt"))
            {
                DialogResult result = MessageBox.Show("The LICENSE.txt has been deleted!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    Application.Exit();
                }
            }

            if (!File.Exists("config.conf"))
            {
                var create = File.Create("config.conf");

                create.Close();

                List<string> conf = new List<string>();
                conf.Add("AlwaysRememberLast=true");
                conf.Add("AlwaysOverride=true");
                conf.Add("AlwaysMakeLogEntry=true");
                conf.Add("AlwaysStartWithWindows=true");

                Override = true;
                LogEntry = true;
                Remember = true;

                foreach (var item in conf)
                {
                    File.AppendAllText("config.conf", $"{item}\n");
                }
                MessageBox.Show("This is just information: This program is under development and this program and me Sipos Attila will not guarantee that your files will not go corrupt or that your files gets deleted of the program's faliure!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                foreach (var item in File.ReadAllLines("config.conf"))
                {
                    string boolean = "";
                    string conf = item;
                    boolean = conf.Remove(0, conf.IndexOf("=") + 1);
                    conf = conf.Remove(conf.IndexOf("="), boolean.Length + 1);

                    switch (conf)
                    {
                        case "AlwaysRememberLast":
                            if (boolean == "true")
                            {
                                Remember = true;
                            }
                            else
                            {
                                Remember = false;
                            }
                            break;
                        case "AlwaysOverride":
                            if (boolean == "true")
                            {
                                Override = true;
                            }
                            else 
                            {
                                Override = false;
                            }
                            break;
                        case "AlwaysMakeLogEntry":
                            if (boolean == "true")
                            {
                                LogEntry = true;
                            }
                            else 
                            {
                                LogEntry = false;
                            }
                            break;
                    }
                }

                InitData();
            }
        }

        static string Source { get; set; }
        static string Dest { get; set; }
        static bool Override { get; set; }
        static bool LogEntry { get; set; }
        static bool Remember { get; set; }
        static bool StartWithWindows { get; set; }

        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        CancellationTokenSource cWtSourceToken = new CancellationTokenSource();
        CancellationToken cWtToken = new CancellationToken();


        // TODO:WINDOWS STARTUP
        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("My Program", "\"" + Application.ExecutablePath + "\"");
            }
        }

        public static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("My Program", false);
            }
        }

        private void InitData()
        {
            if (Remember == true)
            {
                label8.Text = Properties.Settings.Default.SourcePath;
                label9.Text = Properties.Settings.Default.DestPath;
                Source = Properties.Settings.Default.SourcePath;
                Dest = Properties.Settings.Default.DestPath;
            }
            else
            {
                label8.Text = "Source Path";
                label9.Text = "Destination Path";
            }
        }

        private void SaveData()
        {
            if (Remember == true)
            {
                Properties.Settings.Default.SourcePath = Source;
                Properties.Settings.Default.DestPath = Dest;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.SourcePath = "Source Path";
                Properties.Settings.Default.DestPath = "Destination Path";
                Properties.Settings.Default.Save();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {

                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var selectedFolder = dialog.SelectedPath;
                    label8.Text = selectedFolder;
                    Source = label8.Text;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {

                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var selectedFolder = dialog.SelectedPath;
                    label9.Text = selectedFolder;
                    Dest = label9.Text;
                }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (Source == "What you want to sync" || Dest == "Destination")
            {
                MessageBox.Show("Please select a valid path for both \"What you want to sync\" and \"Destination\"!", "NotValidPath", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Synchronize sh = new Synchronize();

                Cursor = Cursors.WaitCursor;
                pictureBox2.Visible = true;

                SaveData();
                await Task.Run(() => sh.SyncAllFiles(Source, Dest, Override));

                Cursor = Cursors.Default;
                pictureBox2.Visible = false;

                MessageBox.Show("The directory is synchronized!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (Source == "What you want to sync" || Dest == "Destination")
            {
                MessageBox.Show("Please select a valid path for both \"What you want to sync\" and \"Destination\"!", "NotValidPath", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                pictureBox3.Visible = true;

                SaveData();

                Watcher wt = new Watcher(Source, Dest, LogEntry);
                await Task.Factory.StartNew(() => Watcher.MonitorDirectory(Source, fileSystemWatcher), cWtToken);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox3.Visible = false;

            fileSystemWatcher.Dispose();

            cWtSourceToken.Dispose();

            MessageBox.Show("The realtime sync has been canceled by user!", "EndOfTask", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Restart();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "config.conf");
        }
    }
}
