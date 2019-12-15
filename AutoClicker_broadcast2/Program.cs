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


namespace AutoClicker_broadcast2
{
    class Program
    {
        public static string sourceCSV = ConfigurationManager.AppSettings["sourceCSV"].ToString();

        public static string imageFolder = ConfigurationManager.AppSettings["imageFolder"].ToString();
        public static string textFolder = ConfigurationManager.AppSettings["textFolder"].ToString();


        //        public static string text = @".🌹🌹 测试
        //1🌹🌹 test
        //.🌹🌹 测试
        //1🌹🌹 test";

        public static IList<string> groups = new List<string>();
        

        [STAThread]
        static void Main(string[] args)
        {
            //loop through images and paste
            var images = GetAllFilePaths(imageFolder);
            var words = GetAllFilePaths(textFolder);

            using (var reader = new StreamReader(sourceCSV))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //Byte[] timeoutStrTemp = Encoding.Default.GetBytes(line);
                    //var line2 = Encoding.UTF8.GetString(timeoutStrTemp);


                    //var sb = new StringBuilder();
                    ////string cTxt = dt.Rows[0][4].ToString();//乱码字符
                    //foreach (EncodingInfo ei in Encoding.GetEncodings())
                    //{
                    //    Byte[] mybyte = System.Text.Encoding.GetEncoding(ei.CodePage).GetBytes(line);
                    //    sb.Append(ei.Name + "(" + ei.CodePage + "):" + System.Text.Encoding.GetEncoding("gb2312").GetString(mybyte, 0, mybyte.Length) + "\r\n");
                    //}
                    //Console.WriteLine(sb.ToString());



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


            foreach (var groupName in groups)
            {
                //go to search bar
                MouseLClick(1558, 39);
                MouseLClick(1558, 39);
                //paste the groupName
                CopyAndPaste(groupName);
                //select the first one in the 

                MouseLClick(1512, 115);
                //select the chat panel
                MouseLClick(1793, 572);

                
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
                    var content = File.ReadAllText(wordPath);
                    CopyAndPaste(content);                
                }


                //press enter
                SimKeyboard.KeyDown(13);
                Thread.Sleep(100);
                SimKeyboard.KeyUp(13);
            }

        }
        private static void CopyAndPaste(string text)
        {
            Clipboard.SetText(text);
            Thread.Sleep(100);
            Paste();
        }

        //PASTE CTRL+V
        private static void Paste()
        {
            SimKeyboard.KeyDown(17);
            Thread.Sleep(100);
            SimKeyboard.KeyDown((byte)'V');
            Thread.Sleep(100);

            
            SimKeyboard.KeyUp(17);
            Thread.Sleep(100);
            SimKeyboard.KeyUp((byte)'V');
            Thread.Sleep(100);
        }

        private static List<string> GetAllFilePaths(string path)
        {
            //var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //System.IO.Path.GetDirectoryName
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .ToList();
        }



        private static void MouseLClick(int x, int y, int? wait = 500)
        {

            SimMouse.Click(MouseButtons.Left, x, y);


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
