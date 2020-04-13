using System;
using System.IO;
using System.Security.Permissions;

namespace Synchronizer
{
    public class Watcher
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

        public Watcher(string source, string dest, bool makeLog) 
        {
            Source = source;
            Dest = dest;
            MakeLog = makeLog;
        }

        static string Source { get; set; }
        static string Dest { get; set; }
        public static bool MakeLog { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void MonitorDirectory(string source, FileSystemWatcher fileSystemWatcher)
        {
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Path = source;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Renamed += OnRenamed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Log("++ Created", e.Name, e.FullPath, DateTime.Now.ToString(), MakeLog);
            Sync();
        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Log("-- Deleted", e.Name, e.FullPath, DateTime.Now.ToString(), MakeLog);
            Delete(e.FullPath);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Log("?? ChangeType", e.Name, e.ChangeType.ToString(), DateTime.Now.ToString(), MakeLog);
            Log("?? Moved", e.Name, e.FullPath, DateTime.Now.ToString(), MakeLog);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Log("+- OldFile", e.OldName, e.OldFullPath, DateTime.Now.ToString(), MakeLog);
            Log("+- Renamed", e.Name, e.FullPath, DateTime.Now.ToString(), MakeLog);
            Delete(e.OldFullPath);
            Sync();
        }

        static void Sync() 
        {
            Synchronize sh = new Synchronize();
            sh.SyncAllFiles(Source, Dest, true);
        }

        static void Delete(string fullPath) 
        {
            Synchronize sh = new Synchronize();
            sh.SyncDelete(Source, fullPath, Dest);
        }

        static void Log(string type, string name, string path, string time, bool makeLog) 
        {
            Log log = new Log(type, name, path, time, makeLog);
            log.LogEntry();
        }
    }

    class Log 
    {
        public Log(string type, string name, string path, string time, bool makeLog)
        {
            Type = type;
            Name = name;
            Path = path;
            Time = time;
            MakeLog = makeLog;
        }

        string Type { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        string Time { get; set; }
        bool MakeLog { get; set; }

        public void LogEntry() 
        {
            if (MakeLog == true)
            {
                if (!File.Exists("log.dat"))
                {
                    File.Create("log.dat");
                }

                string logToWrite = $"\n{this.Type} : {this.Name} : {this.Path} : {this.Time}";

                File.AppendAllText("log.dat", logToWrite);
            }
        }

        public override string ToString()
        {
            return $"Log entry: {this.Type} : {this.Name} : {this.Path} : {this.Time}";
        }
    }
}
