using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Text;
using System.IO;
using System;
using System.Linq;

namespace DeleteOldFiles
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            IniFile ini = new IniFile();
            Dictionary<string, string> iniInfo = ini.Read("Info");

            if (String.IsNullOrEmpty(iniInfo["directory"].ToString())
                || String.IsNullOrEmpty(iniInfo["extension"].ToString())
                || String.IsNullOrEmpty(iniInfo["goodfiles"].ToString()))
            {
                iniInfo["directory"] = @"\\192.168.10.100\d$\SQL Backup";
                iniInfo["extension"] = "bak";
                iniInfo["goodfiles"] = "3";

                ini.Write("Info", iniInfo);
                return;
            }

            //bool showConsole = false;
            bool moreFiles = true;

            short goodfiles;
            short.TryParse(iniInfo["goodfiles"].ToString(), out goodfiles);
            goodfiles = goodfiles <= 1 ? (short)1 : goodfiles;

            string directory = iniInfo["directory"].ToString();

            string extension = iniInfo["extension"].ToString();

            List<FileInfo> FileInfoList = new List<FileInfo>();
            //List<FileInfo> DeletingFilesInfoList = new List<FileInfo>();

            DirectoryInfo d = new DirectoryInfo(directory);
            FileInfo[] Files = d.GetFiles("*." + extension);

            FileInfoList.AddRange(Files);
            //if (showConsole) Console.WriteLine("Selected files:");
            //foreach (FileInfo file in Files)
            //{
            //    if (showConsole) Console.WriteLine(file.Name);
            //    FileInfoList.Add(file);
            //}
            if (FileInfoList.Count > goodfiles)
            {
                FileInfoList = FileInfoList.OrderBy(x => x.CreationTime).ToList();
                try
                {
                    if (!moreFiles)
                    {
                        FileInfo fi = FileInfoList[0];
                        DateTime dt = FileInfoList[0].CreationTime;
                        for (int i = 1; i < FileInfoList.Count; i++)
                        {
                            //if (showConsole) Console.WriteLine(FileInfoList[i].CreationTime);
                            if (dt > FileInfoList[i].CreationTime)
                            {
                                dt = FileInfoList[i].CreationTime;
                                fi = FileInfoList[i];
                            }
                        }
                    }
                    else
                    {
                        File.SetAttributes(Path.Combine(directory, ""), FileAttributes.Normal);
                        for (int i = 0; i < FileInfoList.Count - goodfiles; i++)
                        {
                            // Check if file exists with its full path    
                            if (File.Exists(Path.Combine(directory, FileInfoList[i].Name)))
                            {
                                try
                                {
                                    // If file found, delete it    
                                    File.Delete(Path.Combine(directory, FileInfoList[i].Name));
                                    //if (showConsole) Console.WriteLine("File deleted.");
                                }
                                catch (Exception) { }
                            }
                            //else if (showConsole) Console.WriteLine("File not found");
                        }
                    }
                }
                catch (IOException /*ioExp*/)
                {
                    //if (showConsole) Console.WriteLine(ioExp.Message);
                }
            }
            //if (showConsole) Console.ReadKey();
        }
    }

    public class IniFile
    {
        public string path { get; set; }
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue, [In, Out] char[] value, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string section, IntPtr keyValue, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        public IniFile(string IniPath = null)
        {
            path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }

        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }

        public void Write(string section, Dictionary<string, string> Dvalue)
        {
            IniFile ini = new IniFile(path);
            foreach (var item in Dvalue)
            {
                ini.IniWriteValue(section, item.Key, WriteEncoding(item.Value));
            }
        }

        public Dictionary<string, string> Read(string section)
        {
            IniFile ini = new IniFile(path);
            Dictionary<string, string> formValue = new Dictionary<string, string>();
            formValue.Add("directory", ini.IniReadValue("Info", "directory"));
            formValue.Add("extension", ini.IniReadValue("Info", "extension"));
            formValue.Add("goodfiles", ini.IniReadValue("Info", "goodfiles"));
            return formValue;
        }

        private string ReadEncoding(string str)
        {
            str = str.Replace("ЖЏ", "Ə");
            str = str.Replace("Й™", "ə");
            str = str.Replace("Ећ", "Ş");
            str = str.Replace("Еџ", "ş");
            str = str.Replace("Гњ", "Ü");
            str = str.Replace("Гј", "ü");
            str = str.Replace("Дћ", "Ğ");
            str = str.Replace("Дџ", "ğ");
            str = str.Replace("Г–", "Ö");
            str = str.Replace("Г¶", "ö");
            str = str.Replace("Д±", "ı");
            str = str.Replace("Д°", "İ");
            return str;
        }

        private string WriteEncoding(string str)
        {
            str = str.Replace("Ə", "ЖЏ");
            str = str.Replace("ə", "Й™");
            str = str.Replace("Ş", "Ећ");
            str = str.Replace("ş", "Еџ");
            str = str.Replace("Ü", "Гњ");
            str = str.Replace("ü", "Гј");
            str = str.Replace("Ğ", "Дћ");
            str = str.Replace("ğ", "Дџ");
            str = str.Replace("Ö", "Г–");
            str = str.Replace("ö", "Г¶");
            str = str.Replace("ı", "Д±");
            str = str.Replace("İ", "Д°");
            return str;
        }
    }
}
