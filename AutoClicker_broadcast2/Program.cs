using SimWinInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using AutoClicker.Helpers;

namespace AutoClicker_broadcast2
{
    class Program
    {
        //use notepad++ to convert to UTF-8
        public static string sourceCSV = ConfigurationManager.AppSettings["sourceCSV"].ToString();

        public static string imageFolder = ConfigurationManager.AppSettings["imageFolder"].ToString();
        public static string textFolder = ConfigurationManager.AppSettings["textFolder"].ToString();
        public static string exclusionString = ConfigurationManager.AppSettings["exclusion"].ToString();

        //        public static string text = @".🌹🌹 测试
        //1🌹🌹 test
        //.🌹🌹 测试
        //1🌹🌹 test";

        public static IList<string> groups = new List<string>();
        

        [STAThread]
        static void Main(string[] args)
        {

            Logger.Instance.Log(LogLevel.Information, $"Program starts in 10 sec");
            Thread.Sleep(10000);
            //loop through images and paste
            var images = GetAllFilePaths(imageFolder);
            var words = GetAllFilePaths(textFolder);
            var exclusions = exclusionString.Split(';').Where(x=>!string.IsNullOrEmpty(x));

            using (var reader = new StreamReader(sourceCSV))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
        

                    if(values.Length>2)
                    {
                        var groupName = values[2];
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            var cleanName = RetainChinesePartial(groupName);

                            groups.Add(cleanName);
                        }
                    }
                    

                }
            }

            Logger.Instance.Log(LogLevel.Information, $"begin sending msgs to {groups.Count} groups");

            foreach (var groupName in groups)
            {
                if(exclusions.Any(g=> groupName.Contains(g)))
                {
                    Logger.Instance.Log(LogLevel.Information, $"skipping msg to group ${groupName}");

                    continue;
                }
                Logger.Instance.Log(LogLevel.Information, $"sending msg to group ${groupName}");



                //go to search bar
                MouseLClick(191, 37);
                MouseLClick(191, 37);
                //paste the groupName
                CopyAndPaste(groupName);
                //select the first result

                MouseLClick(128, 117);
                //select the chat panel
                MouseLClick(420, 967);

                
                foreach (var imgPath in images)
                {
                    //paste image
                    using(var img = Image.FromFile(imgPath))
                    {                        
                        Clipboard.SetImage(img);
                        Paste();
                    }
                    

                }
                foreach (var wordPath in words)
                {   
                    var content = File.ReadAllText(wordPath, Encoding.UTF8);
                    CopyAndPaste(content);                
                }


                //press enter
                SimKeyboard.KeyDown(13);
                Thread.Sleep(100);
                SimKeyboard.KeyUp(13);


                //wechat search has a bug
                var mod = groupName.Trim();
                if (mod.Contains(" "))
                {
                    //have to take the longest piece
                    mod = mod.Split(' ').OrderByDescending(s => s.Length).First();

                }

                //send out the contact card
                //1. click on the contacts tab
                //
                MouseLClick(14, 131);
                //2. R click first one in star firends
                MouseRClick(231, 223);
                //3. L click on the third option in the dropdown
                MouseLClick(296, 299);
                //4. paste in the group name
                CopyAndPaste(mod);
                //5.L click on the first result
                MouseLClick(824, 404, 1000);
                //6.L click on send button
                MouseLClick(1095, 747, 1000);

            }

            Logger.Instance.Log(LogLevel.Information, $"end sending msgs to {groups.Count} groups");
        }
        private static void CopyAndPaste(string text)
        {
            byte[] UTF8encodes = UTF8Encoding.UTF8.GetBytes(text);

            //get the string from UTF8 encoding
            string plainText = UTF8Encoding.UTF8.GetString(UTF8encodes);

            Clipboard.SetText(plainText);
            Thread.Sleep(300);
            Paste();
        }

        //PASTE CTRL+V
        private static void Paste()
        {
            SimKeyboard.KeyDown(17);
            Thread.Sleep(300);
            SimKeyboard.KeyDown((byte)'V');
            Thread.Sleep(300);

            
            SimKeyboard.KeyUp(17);
            Thread.Sleep(300);
            SimKeyboard.KeyUp((byte)'V');
            Thread.Sleep(300);
        }

        private static List<string> GetAllFilePaths(string path)
        {
            //var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //System.IO.Path.GetDirectoryName
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .ToList();
        }



        private static void MouseLClick(int x, int y, int? wait = 600)
        {

            SimMouse.Click(MouseButtons.Left, x, y);


            Thread.Sleep(wait.Value);
        }

        private static void MouseRClick(int x, int y, int? wait = 600)
        {

            SimMouse.Click(MouseButtons.Right, x, y);


            Thread.Sleep(wait.Value);
        }

        private static void DoubleClick(int x, int y, int? wait = 500)
        {

            SimMouse.Click(MouseButtons.Left, x, y);
            Thread.Sleep(100);
            SimMouse.Click(MouseButtons.Left, x, y);

            Thread.Sleep(wait.Value);
        }

        private static string RetainChinesePartial(string str)
        {

            //声明存储结果的字符串
            string chineseString = "";


            //将传入参数中的中文字符添加到结果字符串中
            for (int i = 0; i < str.Length; i++)
            {
                //汉字 //english
                if (str[i] >= 0x4E00 && str[i] <= 0x9FA5 || Regex.IsMatch(str[i].ToString(), "^[a-zA-Z0-9]*$")) 
                {
                    chineseString += str[i];
                }
                else
                {
                    chineseString += " ";

                }
            }


            //返回保留中文的处理结果
            return chineseString;
        }
    }
}
