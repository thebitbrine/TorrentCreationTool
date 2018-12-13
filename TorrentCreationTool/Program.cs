using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCreationTool
{
    class Program
    {
        string EmergencyPath = "";
        static void Main(string[] args)
        {
            Program p = new Program();
            bool Headless = false;
            try { Console.Title = "TorrentCreationTool v1.0"; } catch { Headless = true; p.PrintLine("Running in headless mode."); }
            try { p.Run(args); p.PrintLine("INFO: Torrent created successfully. Exiting..."); }
            catch (Exception ex) { p.PrintLine($"ERROR: {ex.Message.Replace("\r\n","")}"); p.Print(!string.IsNullOrWhiteSpace(p.EmergencyPath) ? p.Tag($"INFO: Files are saved in \"{p.EmergencyPath}\"") : "", false); }
            if (Headless) System.Threading.Thread.Sleep(5000); else Console.ReadKey();
        }

        public void Run(string[] args)
        {
            string ASCII = ReturnResource("TorrentCreationTool.ASCII.txt");
            Console.WriteLine(ASCII);
            //args = new string[] { @"New Text Document.txt", @"New Text Document.torrent" };
            string Trackers = (" -t " + MergeArguments(ReadData(Rooter("Configs/Trackers.csv")), " -t "));
            Dictionary<string, string> Configs = GetConfigs(Rooter("Configs/Configs.cfg"));

            ExtractResource("TorrentCreationTool.Transmission.cygwin1.dll", "cygwin1.dll");
            string TransmissionPath = ExtractResource("TorrentCreationTool.Transmission.transmission-create.exe", "transmission-create.exe");
            string WorkingDir = TransmissionPath.Replace(TransmissionPath.Replace("/", "\\").Split('\\').Last(),"");
            EmergencyPath = WorkingDir;

            bool Private = (Configs["PRIVATE"].ToLower().Contains('t') || Configs["PRIVATE"].ToLower().Contains('1') || Configs["PRIVATE"].ToLower().Contains('y') ? true : false);
            string Comment = Configs["COMMENT"];
            string PieceSize = Configs["PIECESIZE"];

            string InputFile = ((args.Length > 0 ? args[0] : "").StartsWith("\"") ? args[0] : $"\"{args[0]}\"");
            string OutputFile  = ((args.Length > 1 ? args[1] : "").StartsWith("\"") ? args[1] : $"\"{args[1]}\"");
            string TempOutputPath = $"\"{OutputFile.Replace("/", "\\").Split('\\').Last().Replace("\"","")}\"";
            Execute(TransmissionPath, $"{InputFile} -s {PieceSize}{(string.IsNullOrWhiteSpace(Comment) ? "" : $"-c {Comment} ")}{(Private ? "-private " : "")}{Trackers} -o {TempOutputPath}", true);

            System.IO.File.Move(System.IO.Path.Combine(WorkingDir, TempOutputPath.Replace("\"","")), OutputFile.Replace("\"",""));
        
            CleanWorkingDir(WorkingDir);
        }

        public void CleanWorkingDir(string Path)
        {
            string[] Files = System.IO.Directory.GetFiles(Path);
            foreach (var File in Files)
            {
                System.IO.File.Delete(File);
            }

            System.IO.Directory.Delete(Path);
        }

        public Dictionary<string, string> GetConfigs(string ConfigFilePath)
        {
            try
            {
                string RawConfigs = System.IO.File.ReadAllText(ConfigFilePath);
                string[] RawConfigsArray = RawConfigs.Replace("\r", "").Split('\n');
                List<string> MediumRareConfigs = new List<string>();
                Dictionary<string, string> CookedConfigs = new Dictionary<string, string>();
                foreach (var Line in RawConfigsArray)
                {
                    if (Line.Contains('=') == true && CookedConfigs.ContainsKey(Line.Split('=')[0]) == false && !Line.StartsWith("//"))
                        CookedConfigs.Add(Line.Split('=')[0], Line.Substring(Line.IndexOf('=') + 1));
                }
                return CookedConfigs;
            }
            catch { }
            return null;
        }

        public string ExtractResource(string ResourcePath, string Filename = null)
        {
            string TempPath = System.IO.Path.GetTempPath();
            string AppName = ResourcePath.Split('.').First();
            string WorkingDirectory = System.IO.Path.Combine(TempPath, AppName);
            string DestinationPath = System.IO.Path.Combine(WorkingDirectory, Filename);
            if (Filename == null) Filename = ResourcePath.Split('.').Last();

            if (!System.IO.Directory.Exists(System.IO.Path.Combine(TempPath, AppName)))
                System.IO.Directory.CreateDirectory(WorkingDirectory);

            var Assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (System.IO.Stream ResourceStream = Assembly.GetManifestResourceStream(ResourcePath))
            using (System.IO.Stream FileStream = System.IO.File.Create(DestinationPath))
            {
                byte[] Buffer = new byte[8 * 1024];
                int Length;
                while ((Length = ResourceStream.Read(Buffer, 0, Buffer.Length)) > 0)
                    FileStream.Write(Buffer, 0, Length);
            }
            return DestinationPath;
        }

        public dynamic ReturnResource(string ResourcePath)
        {
            dynamic Resource;
            var Assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (System.IO.Stream ResourceStream = Assembly.GetManifestResourceStream(ResourcePath))
            using (System.IO.StreamReader Reader = new System.IO.StreamReader(ResourceStream))
            {
                Resource = Reader.ReadToEnd();
            }
            return Resource;

        }

        public string MergeArguments(string[] args, string MergerString = " ")
        {
            if (args != null && args.Length > 0 && MergerString != null)
                return string.Join(MergerString, args);
            return null;
        }

        #region Essentials
        public string LogPath = @"Data\Logs.txt";
        public bool NoConsolePrint = false;
        public bool NoFilePrint = false;
        public void Print(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "")));
            if (!NoConsolePrint) Console.Write(Tag(String));
        }
        public void Print(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", ""))); if (!NoConsolePrint) Console.Write(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "")); if (!NoConsolePrint) Console.Write(String); }
        }
        public void PrintLine(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
            if (!NoConsolePrint) Console.WriteLine(Tag(String));
        }
        public void PrintLine(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
        }
        public void PrintLine()
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Environment.NewLine);
            if (!NoConsolePrint) Console.WriteLine();
        }
        public void PrintLines(string[] StringArray)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
                if (!NoConsolePrint) Console.WriteLine(Tag(String));
            }
        }
        public void PrintLines(string[] StringArray, bool DoTag)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
                else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
            }
        }
        public void Check()
        {
            if (!NoFilePrint && !System.IO.File.Exists(LogPath)) Touch(LogPath);
        }
        private bool WriteLock = false;
        public void WaitWrite(string Path, string Data)
        {
            while (WriteLock) { System.Threading.Thread.Sleep(20); }
            WriteLock = true;
            System.IO.File.AppendAllText(Path, Data);
            WriteLock = false;
        }
        public string[] ReadData(string DataDir)
        {
            if (System.IO.File.Exists(DataDir))
            {
                List<string> Data = System.IO.File.ReadAllLines(DataDir).ToList<string>();
                foreach (var Line in Data)
                {
                    if (Line == "\n" || Line == "\r" || Line == "\t" || string.IsNullOrWhiteSpace(Line))
                        Data.Remove(Line);
                }
                return Data.ToArray();
            }
            else
                return null;
        }
        public string ReadText(string TextDir)
        {
            if (System.IO.File.Exists(TextDir))
            {
                return System.IO.File.ReadAllText(TextDir);
            }
            return null;
        }
        public string SafeJoin(string[] Array)
        {
            if (Array != null && Array.Length != 0)
                return string.Join("\r\n", Array);
            else return "";
        }
        public void CleanLine()
        {
            Console.Write("\r");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Console.Write("\r");
        }
        public void CleanLastLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            CleanLine();
        }
        public string Rooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public static string StaticRooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public string Tag(string Text)
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + Text;
        }
        public string Tag()
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] ";
        }
        public bool Touch(string Path)
        {
            try
            {
                System.Text.StringBuilder PathCheck = new System.Text.StringBuilder();
                string[] Direcories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (var Directory in Direcories)
                {
                    PathCheck.Append(Directory);
                    string InnerPath = PathCheck.ToString();
                    if (System.IO.Path.HasExtension(InnerPath) == false)
                    {
                        PathCheck.Append("\\");
                        if (System.IO.Directory.Exists(InnerPath) == false) System.IO.Directory.CreateDirectory(InnerPath);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(InnerPath, "");
                    }
                }
                if (IsDirectory(Path) && System.IO.Directory.Exists(PathCheck.ToString())) { return true; }
                if (!IsDirectory(Path) && System.IO.File.Exists(PathCheck.ToString())) { return true; }
            }
            catch (Exception ex) { PrintLine("ERROR: Failed touching \"" + Path + "\". " + ex.Message, true); }
            return false;
        }
        public bool IsDirectory(string Path)
        {
            try
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(Path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                if (System.IO.Path.HasExtension(Path)) return true;
                else return false;
            }
        }
        #endregion
        #region Execute
        public bool ExeLogToFile = true;
        public bool ExeLogToConsole = true;
        public string ExeLogPath = StaticRooter("Data/ExternalLogs.txt");
        public void Execute(string Executable, string Arguments, bool WaitForExit)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Executable;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = Executable.Replace("/", "\\").Remove(Executable.LastIndexOf('\\'));
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ExeOutputHandler);
            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ExeOutputHandler);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (WaitForExit) process.WaitForExit();
        }
        public string SingleExecute(string Executable, string Arguments, bool WaitForExit, bool RunInCMD = false)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Executable;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = Executable.Replace("/", "\\").Remove(Executable.LastIndexOf('\\'));
            process.StartInfo.WindowStyle = (RunInCMD ? System.Diagnostics.ProcessWindowStyle.Normal : System.Diagnostics.ProcessWindowStyle.Hidden);
            process.StartInfo.UseShellExecute = RunInCMD;
            process.StartInfo.UseShellExecute = RunInCMD;
            process.StartInfo.RedirectStandardOutput = !RunInCMD;
            process.StartInfo.RedirectStandardError = !RunInCMD;
            process.Start();
            if (WaitForExit) process.WaitForExit();
            if (RunInCMD) return "";
            string std = process.StandardOutput.ReadToEnd();
            string etd = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(std)) return std;
            if (!string.IsNullOrWhiteSpace(etd)) return etd;
            return "";
        }

        public void ExeOutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
        {
            string Output = outLine.Data;
            if (string.IsNullOrWhiteSpace(Output) == false)
            {
                if (ExeLogToFile) System.IO.File.AppendAllText(ExeLogPath, Tag(Output + Environment.NewLine));
                if (ExeLogToConsole) Console.WriteLine(Tag(Output));
            }
        }
        #endregion
    }
}
