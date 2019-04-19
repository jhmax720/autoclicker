using SimWinInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using MongoDB.Driver;
using AutoClicker.Models;
using AutoClicker.Helpers;
using System.Drawing;
using Tesseract;
using System.Drawing.Imaging;

namespace AutoClicker
{
    class Program
    {
        static int ContinueFailingMaxCap = 3;
        static int currentFailureIndex = 0;
        static IMongoCollection<ChatRoomWebModel> _roomModels;
        static IMongoCollection<ChatRoomWebModel> _roomInvitedModels;
        static IMongoCollection<WeishangWebModel> _weishangModels;
        static MongoClient client;
        static IMongoDatabase database;
        static string BotWebUrl = "http://localhost:3001";

        

        [STAThread]
        static void Main(string[] args)
        {
            client = new MongoClient("mongodb://localhost:27017/");
            database = client.GetDatabase("roombot");
            _roomModels = database.GetCollection<ChatRoomWebModel>("roomModels");
            _weishangModels = database.GetCollection<WeishangWebModel>("weishangModels");

            _roomInvitedModels = database.GetCollection<ChatRoomWebModel>("roomInvitedModels");
            try
            {





                Console.WriteLine("----------Wechaty roombot -------------");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();
                
                string input = string.Empty;

                input = Choice();
                switch (input.ToLower())
                {
                    case "1":
                        //TEST OCR SOFTWARE
                        //get the latest screenshot
                        var text = ReadLatestScreenShot();
                        Console.Write(text);

                        break;
                    case "2":

                        SetupAllRoomMembers();
                        break;
                    case "3":

                        
                        AutoInviteBot();

                        break;
                    case "5":
                        // now add the following C# line in the code page  
                        //var text = ReadLatestScreenShot();

                        break;


                    case "9":
                        if (Confirm("ResetDb?").ToLower().Equals("y"))
                        {

                            //await InitialSetup();
                            Console.WriteLine("done");

                        }
                        break;
                    default:
                        break;
                }
            }

            catch (Exception ex)
            {

                var cColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = cColor;
                Console.ReadLine();
            }






            Console.WriteLine("Prese key to close window");
            Console.ReadLine();


       


            //try to click the search box
            //SearchGroupChat("墨尔本租房互助群");

            //InitialMemberList();

            //AddMember(0 +1);
            //AddMember(1 +1);
            //AddMember(2 + 1);
            //AddMember(3 + 1);
            //AddMember(4 + 1);
            //AddMember(5 + 1);
            //AddMember(6 + 1, OCR);
            //AddMember(7 + 1, OCR);
            //AddMember(8 + 1, OCR);
            //AddMember(9 + 1, OCR);
            //AddMember(10 + 1, OCR);
            //AddMember(11 + 1, OCR);
            //AddMember(12 + 1, OCR);
            //AddMember(13 + 1, OCR);


           

          

            //var testIndex = 4;
            //var row = Convert.ToInt32(Math.Ceiling(testIndex / 5d));
            //should be in row 3
            //var yu = testIndex % 5d;


            var input2 = Console.ReadKey();
        }

        

        static string Choice()
        {
            var cColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Press a number from the choice below. X for eXit");
            Console.WriteLine("1. Loop through Subscription and broadcast"); //FLOW 1 SHANGJIA BOT
            Console.WriteLine("2. Rebuild All Members in rooms");
            Console.WriteLine("3. Loop through all Rooms and request friends ");//FLOW 2 SHANGJIA BOT, get everyone's ID

            //FLOW 3 FOR FANSBOT, INVITE FRIENDS FROM DB
            //FLOW 4 FOR SHANGJIABOT INVITE SHANG JIA FROM DB
            Console.WriteLine("9. Initial Setup");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPress x to exit");
            Console.ForegroundColor = cColor;
            return Console.ReadLine();

        }
        static string Confirm(string message)
        {
            Console.Clear();
            var cColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 50));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.WriteLine("Press y to confirm, any other key to exit");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 50));
            Console.ForegroundColor = cColor;
            return Console.ReadLine();
        }


        #region AutoMouseClick
        private static void InitialMemberList()
        {
            //NOW WE ARE INSIDE THE GROUP CHAT
            //Expand the group list + ICON            
            MouseLClick(370, 118, 2000);
            //scroll down
            SimMouse.Act(SimMouse.Action.LeftButtonDown, 310, 553);
            Thread.Sleep(100);
            SimMouse.Act(SimMouse.Action.LeftButtonUp, 310, 0);
            //click the search bar
            MouseLClick(270, 520);
            //CLICK THE KEYWORD FIELD
            MouseLClick(150, 166);
        }

        private static void SearchGroupChat(string topic)
        {
            //Click Search button            
            MouseLClick(307, 111, 5000);
            //RE-FOCUS THE SEARCH BAR
            MouseLClick(100, 111);
            CopyAndPaste(topic);

            //SELECT THE FIRST SEARCH RESULT
            SimMouse.Click(MouseButtons.Left, 207, 213);
            Thread.Sleep(2000);
        }

        private static void CopyAndPaste(string text)
        {
            Clipboard.SetText(text);
            Thread.Sleep(1000);

            //PASTE CTRL+V
            SimKeyboard.KeyDown(17);
            Thread.Sleep(1000);
            SimKeyboard.KeyDown((byte)'V');
            Thread.Sleep(1000);
            
            
            SimKeyboard.KeyUp(17);
            Thread.Sleep(500);
            SimKeyboard.KeyUp((byte)'V');
            Thread.Sleep(500);
        }

        private static void MouseLClick(int x, int y, int? wait = 1500)
        {

            SimMouse.Click(MouseButtons.Left, x, y);


            Thread.Sleep(wait.Value);
        }

        //NEXT LEFT 75, DOWN 90
        //1 based index
        //private static void AddMember(int index, AutoOcr OCR)
        //{
        //    var row = Convert.ToInt32(Math.Floor(index / 5d));
        //    //should be in row 3
        //    var col = Convert.ToInt32(index % 5d);

        //    if (col == 0)
        //    {
        //        col = 5;
        //        row--;
        //    }

        //    //select the first member avator
        //    MouseLClick(49 + ((col - 1) * 75), 192 + row * 90);

        //    AddMemberImpl(OCR);

        //}

        private static void AddMemberImpl()
        {
            //generate screenshot
            GenerateScreenShot();
            //get the latest screenshot
            var text = ReadLatestScreenShot();
            var hasQianMing = text.Contains("Up");
            var hasPengYouQuan = text.Contains("Moments");
            var isScreen2 = IsScreen2(text);
            var hasSendMessageButton = text.Contains("Message");
            

            if(!isScreen2 && !hasQianMing && !hasPengYouQuan)
            {

                Logger.Instance.Log(LogLevel.Information, "Back to search. found no member");

                currentFailureIndex++;
                if(currentFailureIndex > ContinueFailingMaxCap)
                {
                    Logger.Instance.Log(LogLevel.Error, "3 times failure in a row, exiting");
                }

                return;
            }
            if(hasSendMessageButton)
            {
                Logger.Instance.Log(LogLevel.Information, "Back to search. found a friend");
                //BACK TO THE MEMBER LIST
                MouseLClick(17, 110);
                //RE-FOCUS ON SEARCH BAR
                MouseLClick(310, 166);

                return;
            }
            SmartAdd(hasQianMing, hasPengYouQuan);
        }

        private static bool ValidateScreenContainsText(  string keyword)
        {
            GenerateScreenShot();
            //get the latest screenshot
            
            var text = ReadLatestScreenShot();
            return text.Contains(keyword);
        }

       

        private static void GenerateScreenShot()
        {
            MouseLClick(437, 211, 2000);
        }

        private static void SmartAdd(bool hasSignature, bool hasPengYouQuan)
        {
            //CLICK 'Add' button
            if (hasSignature && hasPengYouQuan)
            {
                Logger.Instance.Log(LogLevel.Information, "Found both 簽名 和朋友圈 in this profile");
                //click the add button (2 possible locations)
                MouseLClick(191, 544, 3000);
                MouseLClick(191, 495, 3000);
            }
            else if(hasPengYouQuan || hasSignature)
            {
                Logger.Instance.Log(LogLevel.Information, "Found either 簽名 or 朋友圈  in this profile");
                MouseLClick(191, 444, 3000);
            }
            else
            {
                Logger.Instance.Log(LogLevel.Information, "Found neither 簽名 or 朋友圈  in this profile");
                MouseLClick(310, 364, 3000);
               
            }

            //TWO POSSIBLE OUTCOMES
            //1. NO INVITE REQUIRED, SOME JUST BECOME FRIENDS AT THIS STAGE
            //2. INVITE REQUIRED, GO TO THE NEXT PAGE TO THE GREEN 'SEND' BUTTON
            //3. THE TARGET ACCOUNT IS FUCKED UP, NOTHING HAPPENS
            //Regenerate screenshot
            GenerateScreenShot();            
            var text = ReadLatestScreenShot();

            var hasSendMessage = text.Contains("Message"); //MEANS CASE 1
            var hasAddButton = IsScreen2(text); //CASE 3

            if(hasSendMessage || hasAddButton)
            {
                //CASE 1


            }
            else
            {
                //click on the send  button
                MouseLClick(345, 120, 4000);



                //TWO POSSIBLE OUTCOMES
                //1. SUCCESS, RETURN TO THE PREVIOUS PAGE AUTOMATICALLY
                //2. FAILED FOR WHATEVER REASON
                GenerateScreenShot();
                //get the latest screenshot
                var screenText = ReadLatestScreenShot();
                var validScreen = IsScreen2(screenText);


                //extra click empty area in case any extra dialog
                MouseLClick(12, 720, 500);
                MouseLClick(12, 720, 500);

                if (!validScreen)
                {
                    //extra click to go back to the previous menu
                    Logger.Instance.Log(LogLevel.Information, "Found no 通讯录, extra click asumming adding faied?");
                    MouseLClick(17, 110);
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Information, "added friend successfuly");
                }
            }




            //back to member list
            MouseLClick(17, 110);
            //clean up all screenshots
            //Array.ForEach(Directory.GetFiles(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror"), delegate (string path) { File.Delete(path); });
        }

        private static bool IsScreen2(string screenText)
        {
            return screenText.Contains("Add") || screenText.Contains("Contact");
        }
        //private static bool IsScreen3(string screenText)
        //{
        //    return screenText.Contains("Send") || screenText.Contains("Friend Request");
        //}


        #endregion

        #region Steps

        private static void SetupAllRoomMembers()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BotWebUrl);
                var rooms = TryGetRoomsFromWebwx(client).Result;
                foreach(var room in rooms)
                {
                    LoadAllMembersInRoom(room, client).Wait();
                }
            }
                
        }

        //SETP 3
        private static void AutoInviteBot()
        {
        

            var room = _roomModels.Find(x => x.Id != null).FirstOrDefaultAsync().Result;
            //auto search for the room and go to the member list scree
            SearchGroupChat(room.Name);
            //now toggle the search bar inside the room
           InitialMemberList();



            ///////////////////////////////////////////
            List<string> members = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BotWebUrl);
                var obj = new { topic = room.Name };


                var sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                var mmRep =  client.PostAsync("/members", sc).Result;
                string memberString =  mmRep.Content.ReadAsStringAsync().Result;

                try
                {
                    members = JsonConvert.DeserializeObject<List<string>>(memberString);

                }
                catch (Exception e)
                {
                    Logger.Instance.Log(LogLevel.Error, e.Message);
                }
            }

               

           

            //////////////////////////////////////////


            foreach (var member in members.Where(x=>!string.IsNullOrEmpty(x)).OrderByDescending(x=>x))
            {
                var safeMember = member.Trim();

                //Prechecks
                var foundInWeishang = _weishangModels.Find(x => x.Members.Contains(safeMember) && x.Name == room.Name).FirstOrDefaultAsync().Result;
                var foundInInvited = _roomInvitedModels.Find(x => x.Members.Contains(safeMember) && x.Name == room.Name).FirstOrDefaultAsync().Result;

                if (foundInWeishang!=null || foundInInvited!=null)
                {
                    //found a weishang who posted in the room before
                    Logger.Instance.Log(LogLevel.Information, "skipping member:" + safeMember);

                    continue;
                }
                else
                {
                    //RE-FOCUS on the top search bar
                    //Click Search button            
                    MouseLClick(310, 166);

                    CopyAndPaste(safeMember);

                    Logger.Instance.Log(LogLevel.Information, "Dealing member " + safeMember);

                    //Select the first result
                    MouseLClick(57, 233,3000);
                    AddMemberImpl();

                                                 
                    HardDelete();


                    //ADD TO THE DB so they won't be bothered again
                    //Helper.Instance.Exists(_roomInvitedModels,)
                    var invitedRoomRecord = _roomInvitedModels.Find(x => x.Name == room.Name).FirstOrDefaultAsync().Result;
                    if(invitedRoomRecord!=null)
                    {
                        if(!invitedRoomRecord.Members.Contains(safeMember))
                        {
                            var lst = invitedRoomRecord.Members.ToList();
                            lst.Add(safeMember);
                            //invitedRoomRecord.Members = lst.ToArray();
                            _roomInvitedModels.FindOneAndUpdate(x => x.Name == room.Name, Builders<ChatRoomWebModel>.Update.Set("members", lst.ToArray()));
                        }
                    }
                    else
                    {
                        var aInvitedRoom = new ChatRoomWebModel
                        {
                            Captured = DateTime.UtcNow,
                            Members = new string[] { safeMember },
                            Name = room.Name,
                            Created = DateTime.UtcNow

                        };
                        _roomInvitedModels.InsertOne(aInvitedRoom);
                    }
                    
                }
            }
        }

        private static void HardDelete()
        {
            //Re focus on the top search bar
            //Click Search button            
            MouseLClick(338, 166);
            //HOLD DOWN BACKSPACE FOR 3 SECS TO CLEAR THE RESULT    

            for (int i = 0; i < 25; i++)
            {
                SimKeyboard.Press(8);
                Thread.Sleep(100);
            }
        }

        #endregion
        #region Http Helpers


        private static async Task<List<string>> TryGetRoomsFromWebwx(HttpClient client)
        {
            //var definition = new[] { new { topic = "", wxRef = "" } };

            bool inProgress = true;
            List<string> returns = new List<string>();
            var maxAttempt = 3;
            var currentAttempt = 1;

            while (inProgress && currentAttempt <= maxAttempt)
            {
                try
                {
                    var roomRep = await client.PostAsync("/rooms", null);
                    string rmTopics = await roomRep.Content.ReadAsStringAsync();


                    var rmTopicList = JsonConvert.DeserializeObject<List<string>>(rmTopics);
                    inProgress = rmTopicList.Count() == 0;


                    returns = rmTopicList;
                    //var objects = JsonConvert.DeserializeObject<List<object>>(rmTopics);
                    //rmTopicList = objects.Select(obj => JsonConvert.SerializeObject(obj).Replace("\"", string.Empty)) .OrderByDescending(x => x).ToList();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Error getting the rooms, will try later");
                    Thread.Sleep(10000);
                    currentAttempt++;

                }
            }



            return returns;
        }


        private static async Task LoadAllMembersInRoom(string topicString,HttpClient client)
        {
            var obj = new { topic = topicString };


            var sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            var mmRep = await client.PostAsync("/members", sc);
            string memberString = await mmRep.Content.ReadAsStringAsync();

            try
            {
                var members = JsonConvert.DeserializeObject<List<string>>(memberString);

                var aRoom = new ChatRoomWebModel
                {
                    Created = DateTime.UtcNow,
                    Members = members.ToArray(),
                    Name = topicString


                };

                //add or update
                AddRoomMemberDataOrUpdate(_roomInvitedModels, topicString, members, aRoom);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(LogLevel.Error, e.Message);
            }
            
         
            
            
        }

        private static void AddRoomMemberDataOrUpdate(IMongoCollection<ChatRoomWebModel> _col, string topicString, List<string> members, ChatRoomWebModel aRoom)
        {
            var createdOk = Helper.Instance.AddIfNotExist(_col, aRoom);
            if (!createdOk)
            {
                _col.FindOneAndUpdate(x => x.Name == topicString, Builders<ChatRoomWebModel>.Update.Set("members", members.ToArray()));
            }
        }
        #endregion
        #region Functions
        private static string ReadLatestScreenShot()
        {
            var dir = new DirectoryInfo(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror");
            var sd = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Logger.Instance.Log(LogLevel.Information, "scanning the latest screenshot " + sd.Name);

            var image = new Bitmap(sd.FullName);
            // turn to the correct format
            var image2 = Helper.Instance.Get24bppRgb(image);

            var ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
            var page = ocr.Process(image2);
            var text = page.GetText();
            return text;
        }
        #endregion
    }


}
