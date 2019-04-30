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
using System.Runtime.InteropServices;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;

namespace AutoClicker
{
    class Program
    {

        static int ScreenYOffset = 51;        
        static int ContinueFailingMaxCap = 6;
        static int currentFailureIndex = 0;
        
        static IMongoCollection<Customer> _customers;
        static IMongoCollection<Sub> _subs;
        static IMongoCollection<Bot> _bots;
        
        static IMongoCollection<ChatRoomWebModel> _roomInvitedModels;
        static IMongoCollection<WeishangWebModel> _weishangModels;
        static IMongoCollection<LocalJob> _jobs;
        
        static IMongoCollection<ChatRoom> _chatRooms;

        static MongoClient client;
        static IMongoDatabase database;
        static string BotWebUrl = "http://localhost:3001";
        static string JOBNAME = "AutoClicker";
        static string _revisitedName = null;

        private static string Analyis(string str)
        {
            var arr = str.Split(new char[] { '\n' }).ToList();
            var prcs = arr.Select(x => x.Replace("\n", string.Empty)).Where(y=>y!=string.Empty).ToList();
            var contactIndex = prcs.FindIndex(x => x.ToLowerInvariant().Contains("contact"));
            var whatsupIndex = prcs.FindIndex(x => x.ToLowerInvariant().Contains("wha"));
            var momentsIndex = prcs.FindIndex(x => x.ToLowerInvariant().Contains("momen"));
            var addIndex = prcs.FindIndex(x => x.ToLowerInvariant().Contains("add"));
            return null;
        }

        [STAThread]
        static void Main(string[] args)
        {
            
            client = new MongoClient("mongodb://localhost:27017/");
            database = client.GetDatabase("autoClicker");

            _customers = database.GetCollection<Customer>("customers");
            _subs = database.GetCollection<Sub>("subs");

            
            _weishangModels = database.GetCollection<WeishangWebModel>("weishangModels");
            _jobs = database.GetCollection<LocalJob>("jobs");
            _roomInvitedModels = database.GetCollection<ChatRoomWebModel>("roomInvitedModels");
            _chatRooms = database.GetCollection<ChatRoom>("chatRooms");
            _bots = database.GetCollection<Bot>("bots");
            try
            {





                Console.WriteLine("----------Wechaty roombot -------------");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();
                
                string input = args!=null && args.Length>0 ? args[0]: string.Empty;

                if(string.IsNullOrEmpty(input))
                {
                    input = Choice();
                }
               
                
                switch (input.ToLower())
                {
                    case "1":
                        //TEST OCR SOFTWARE
                        var page2 = ReadLatestSC();
                        var addButtonPosition2 = GetPositionFromKeyword(page2, "add");
                        var pause = Console.ReadLine();
                        break;
                    case "2":

                        //SetupAllRoomMembers();
                        using (HttpClient client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(BotWebUrl);
                            var webbot = GetBotId(client).Result;
                            var dbBot = GetBotByName(webbot);

                            var rooms = TryGetRoomsFromWebwx(client).Result;
                            foreach (var room in rooms)
                            {
                                var chatRoom = new ChatRoom
                                {
                                    Name = room,
                                    BotRef = dbBot.Id.ToString(),
                                    Created = DateTime.UtcNow
                                };
                                _chatRooms.InsertOne(chatRoom);
                            }
                        }
                        break;
                    case "3":

                        
                        AutoInviteBot();

                        break;
                    case "5":
                        AllSubAndBroadcast().Wait();
                        break;


                    case "9":
                        if (Confirm("ResetDb?").ToLower().Equals("y"))
                        {

                            InitialSetup().Wait();
                            Console.WriteLine("done");

                        }
                        break;
                    case "11":

                        for(int i =0; i<300; i++)
                        {
                            Thread.Sleep(3000);

                            //PASTE CTRL+V
                            SimKeyboard.KeyDown(17);
                            Thread.Sleep(1000);
                            SimKeyboard.KeyDown((byte)'R');
                            Thread.Sleep(1000);


                            SimKeyboard.KeyUp(17);
                            Thread.Sleep(500);
                            SimKeyboard.KeyUp((byte)'R');
                            Thread.Sleep(500);
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
            Thread.Sleep(1000);
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

        private static void ScreenTwoImpl(string member)
        {
            Logger.Instance.Log(LogLevel.Information, "Screen two expected");
            //generate screenshot
            GenerateScreenShot();
            //get the latest screenshot
            //var text = ReadLatestScreenShot();
            var scPage = ReadLatestSC();
            var addButtonPosition = GetPositionFromKeyword(scPage, "add");

            //var hasWhatsup = HasWhatsUp(text);
            //var hasMoments = HasMoments(text);
            var isScreen2 = addButtonPosition.X1 > 0;
            var isFriendly = IsFriendly(scPage); 

            if (isFriendly)
            {
                Logger.Instance.Log(LogLevel.Information, "Back to search. found a friend");
                //BACK TO THE MEMBER LIST
                MouseLClick(17, 110);
                //RE-FOCUS ON SEARCH BAR
                MouseLClick(310, 166);

                ResetIndex();
                return;
            } 
            if (!isScreen2 )
            {
                var doubleCheckScreen2 = IsScreen2(scPage.GetText());
                
                if(doubleCheckScreen2)
                {
                    //OCR FAILED TO READ AGAIN SKIP THIS MEMBER
                    //back to member list
                    MouseLClick(17, 110);
                    //RE-FOCUS ON SEARCH BAR
                    MouseLClick(310, 166);
                    return;
                }
                //DO NOTHING 
                Logger.Instance.Log(LogLevel.Information, "Back to search. found no member");

                currentFailureIndex++;
                if (currentFailureIndex > ContinueFailingMaxCap)
                {
                    Logger.Instance.Log(LogLevel.Error, "3 times failure in a row, exiting");

                    //_jobs.FindOneAndUpdate(x => x.Name == JOBNAME, Builders<LocalJob>.Update.Set("status", "end"));

                    Environment.Exit(0);
                }
                if(_revisitedName==null)
                {
                    //ONE MORE ATTEMPT BEFORE GIVE UP
                    var chiOnly = RetainChinesePartial(member);
                    if(chiOnly.Length>0)
                    {
                        Logger.Instance.Log(LogLevel.Information, "SETTING REVISITED NAME FOR 2ND ATTEMPT " + _revisitedName);
                        _revisitedName = chiOnly;
                    }
                    
                }
                
                //RE-FOCUS ON SEARCH BAR
                MouseLClick(310, 166);
                return;
            }

            //CLICK 'Add' button and proceed to screen 3
            //if (hasWhatsup && hasMoments)
            //{
            //    Logger.Instance.Log(LogLevel.Information, "Found both 簽名 和朋友圈 in this profile");
            //    //click the add button (2 possible locations)
            //    MouseLClick(191, 544, 5000);
            //    MouseLClick(191, 495, 5000);
            //}
            //else if (hasWhatsup || hasMoments)
            //{
            //    Logger.Instance.Log(LogLevel.Information, "Found either 簽名 or 朋友圈  in this profile");
            //    MouseLClick(191, 444, 5000);
            //}
            //else
            //{
            //    Logger.Instance.Log(LogLevel.Information, "Found neither 簽名 or 朋友圈  in this profile");
            //    MouseLClick(310, 364, 5000);

            //}
            //CLICK THE ADD BUTTON
            MouseLClick(addButtonPosition.X1+addButtonPosition.Width/2, addButtonPosition.Y1+ ScreenYOffset + addButtonPosition.Height/2, 5000);


            //extra click empty area in case any extra dialog
            MouseLClick(12, 720, 500);

            ScreenThreeImpl();
        }


        private static void GenerateScreenShot()
        {
            MouseLClick(437, 211, 2000);
        }

        private static void ScreenThreeImpl()
        {
            Logger.Instance.Log(LogLevel.Information, "Screen 3 expected");


            //3 POSSIBLE OUTCOMES
            //1. NO INVITE REQUIRED, SOME JUST BECOME FRIENDS AT THIS STAGE (STAY IN SCREEN 2)
            //2. INVITE REQUIRED, GO TO THE NEXT PAGE TO THE GREEN 'SEND' BUTTON (COME TO SCREEN 3)
            //3. THE TARGET ACCOUNT IS FUCKED UP, NOTHING HAPPENS (STAY IN SCREEN 2)
            //Regenerate screenshot
            
            GenerateScreenShot();
            var sc3 = ReadLatestSC();
            var addButtonPosition = GetPositionFromKeyword(sc3, "add");                        
            var isScreen2 = addButtonPosition.X1 > 0 || IsScreen2(sc3.GetText());
            var isFriendly = IsFriendly(sc3); // CASE 1
            if (isFriendly || isScreen2)
            {
                //CASE 1 OR CASE 3

                Logger.Instance.Log(LogLevel.Warning, "Didn't proceed to the next screen for unkown reason");
            }
            else
            {
                //click on the send  button
                MouseLClick(345, 120, 7000);



                //TWO POSSIBLE OUTCOMES
                //1. SUCCESS, RETURN TO THE PREVIOUS PAGE AUTOMATICALLY
                //2. FAILED FOR WHATEVER REASON 
                GenerateScreenShot();
                var sc2 = ReadLatestSC();
                var addButtonPosition2 = GetPositionFromKeyword(sc2, "add");
                var isScreen2a = addButtonPosition2.X1 > 0 || IsScreen2(sc2.GetText());

                //extra click empty area in case any extra dialog
                MouseLClick(12, 720, 500);


                if (!isScreen2a)
                {
                    //extra click to go back to the previous menu
                    Logger.Instance.Log(LogLevel.Information, "Found no 通讯录, extra click asumming adding failed?");
                    MouseLClick(17, 110);
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Information, "added friend successfuly");
                }
                //RESET THE FAILTURE INDEX
                ResetIndex();
            }




            //back to member list
            MouseLClick(17, 110);
            //clean up all screenshots
            //Array.ForEach(Directory.GetFiles(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror"), delegate (string path) { File.Delete(path); });
        }

        private static void ResetIndex()
        {
            currentFailureIndex = 0;
        }


        #endregion
        #region Screen Logic
        private static void ReturnToTopLevel()
        {
            //back to member list
            MouseLClick(17, 110);
            //back to member list
            MouseLClick(17, 110);
            //back to member list
            MouseLClick(17, 110);
            //back to member list
            MouseLClick(17, 110);
        }


        private static bool IsScreen2(string screenText)
        {
            return screenText.ToLowerInvariant().Contains("add") || screenText.ToLowerInvariant().Contains("contact") || screenText.ToLowerInvariant().Contains("edit");
        }
        private static bool IsFriendly(Page scPage)
        {            
            var messageButtonPostion = GetPositionFromKeyword(scPage, "message");
            var voiceButtonPostion = GetPositionFromKeyword(scPage, "voice");

            return  messageButtonPostion.X1 > 0 || voiceButtonPostion.X1 > 0;
            //return screenText.ToLowerInvariant().Contains("send") || screenText.ToLowerInvariant().Contains("message") || screenText.ToLowerInvariant().Contains("voice");
        }
        private static bool HasMoments(string text)
        {
            return text.ToLowerInvariant().Contains("mome");
        }

        private static bool HasWhatsUp(string text)
        {
            return text.ToLowerInvariant().Contains("u") && text.ToLowerInvariant().Contains("wha");
        }
        #endregion

        #region Steps
        //STEP 
        private static async Task AllSubAndBroadcast()
        {

            var jobs = _jobs.Find(x=>x.Id!=null).ToList();

            //GET ALL JOBS CURRENT
            foreach(var job in jobs)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(job.Url);

                    //GET THE WORKING BOT IN EACH JOB
                    var bot = _bots.Find(x => x.Name == job.Name || x.WxRef == job.BotWxRef).FirstOrDefault();
                    if (bot != null)
                    {
                        var botObjId = bot.Id;

                        //FIND ALL SUBSCRIBTIONS FOR EACH BOT
                        var mySubs = _subs.FindAsync(x => x.BotIds.Contains(botObjId)).Result;
                  
                        //GET ALL THE ROOMS FOR THIS BOT
                        var roomRep = await client.PostAsync("/rooms", null);
                        string rmTopics = await roomRep.Content.ReadAsStringAsync();


                        var rmTopicList = JsonConvert.DeserializeObject<List<string>>(rmTopics);
                        var subsList = mySubs.ToList();

                        //loop through all the rooms for the bot
                        var index = 0;
                        foreach (var aSubscribtion in subsList)
                        {
                            if (aSubscribtion.LastBroadcast.AddHours(aSubscribtion.Interval) < DateTime.UtcNow)
                            {

                                foreach (var topic in rmTopicList)
                                {

                                    var exemption = bot.Exemptions.FirstOrDefault(x => topic.ToLowerInvariant().Contains(x.ToLowerInvariant()));

                                    if (exemption != null)
                                    {
                                        Logger.Instance.Log(LogLevel.Information, "skip exemption room: " + topic);
                                        
                                        continue;
                                    }
                                    //Pause for 1 sec
                                    Thread.Sleep(1000);


                                    Logger.Instance.Log(LogLevel.Information, string.Format(@"processing room topic {0} ", topic));
                                    
                                    //loop through all the text content
                                    Console.WriteLine(string.Format(@"sending text.."));
                                    foreach (var msg in aSubscribtion.Text)
                                    {
                                        SendText(client, topic, msg).Wait();
                                        //Pause for 1 sec
                                        Thread.Sleep(1000);
                                    }




                                }
                                _subs.FindOneAndUpdate(x => x.Id == aSubscribtion.Id, Builders<Sub>.Update.Set("lastBroadcast", DateTime.UtcNow));

                            }
                            else
                            {
                                Logger.Instance.Log(LogLevel.Information, "sub is getting called but not ready: " + aSubscribtion.Id);
                            }


                          
                            

                            index++;
                            Console.WriteLine(string.Format(@"done {0}/{1}", index, rmTopicList.Count));
                        }

                    }

                }


            }      

        }


        //STEP 9
        static async Task InitialSetup()
        {




            var newCustomer = new Customer
            {
                Name = "Mybeauty",
                WxRef = "null"

            };
            await _customers.InsertOneAsync(newCustomer);

            var sBot1 = new Bot
            {
                City = "Melbourne",
                Name = "墨小白",
                Region = "Australia",
                WxRef = "jhmax720",                
                Exemptions = new string[] { "wechaty", "wechaty-puppet-padchat" },
                LatestRooms = 0,
                BanKeywords = new string[] {
                    "墨尔本","澳洲"
                }

            };
            var sBot2 = new Bot
            {
                City = "Melbourne",
                Name = "MyBeauty",
                Region = "Australia",
                WxRef = "xiaoxiao1992423",                
                LatestRooms = 0,
                Exemptions = new string[] { "mybeauty", "澳大利亚四川", "金悦全家亚超" },
                BanKeywords = new string[] {
                    "墨尔本","澳洲"
                }
            };
            var sBot3 = new Bot
            {
                City = "Sdyney",
                Name = "簡單的幸福4",
                Region = "Australia",
                WxRef = "j8mzrhabrbv",
                LatestRooms = 0,
                Exemptions = new string[] { "mybeauty", "澳大利亚四川", "金悦全家亚超" },
                BanKeywords = new string[] {
                    "墨尔本","澳洲"
                }
            };
            await _bots.InsertOneAsync(sBot1);
            await _bots.InsertOneAsync(sBot2);
            await _bots.InsertOneAsync(sBot3);

            
                        




            var sub = new Sub
            {
                CustomerId = newCustomer.Id,
                BotIds = new List<ObjectId>() { sBot2.Id },
                Text = new string[] { "<img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>墨尔本东区美容院4月特价项目出来袭！<br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>一，韩式半永久纹眉仅需$300，美瞳线仅$250！包免费补色一次。<br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>二，2019最新技术（无针雾化祛眼袋、法令纹、鱼尾纹，任选一个部位体验，推广价仅需480！做一次能最少维持6-8月！按疗程做可维持3年以上！<br>️<img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>三，王牌项目“秘制祛痘”疗程，现仅需$1200/8次（我们是包去掉，无效全额退款！）<br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>四，台式山茶花嫁接睫毛，本月仅$88/次（不限根数，包证你的睫毛又长又浓密）<br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>五，产后盆骨、腹直肌修复，体验价仅$158！一次可收紧盆骨1-3毫米！回到少女时期<img class='emoji emoji2665' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'><br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>六，胸部乳腺疏通.现体验价128！做一次胸部变大变饱满，让你远离乳腺疾病！<br><img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>七，韩国进口—水光针，本月仅$280！做一次维持大半年！<br><img class='emoji emoji1f514' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>有兴趣的宝宝➕微信咨询和体验！我们做了十年，值得你信赖！只要你来问都有特价给你<img class='emoji emoji1f33a' text='_web' src='/zh_CN/htmledition/v2/images/spacer.gif'>" },
                ImageUrl = new string[] { },
                Interval = 0.1,
                LastBroadcast = DateTime.UtcNow


            };
            _subs.InsertOne(sub);



        }
        //private static void SetupAllRoomMembers()
        //{
        //    using (HttpClient client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(BotWebUrl);
        //        var rooms = TryGetRoomsFromWebwx(client).Result;
        //        foreach(var room in rooms)
        //        {
        //            LoadAllMembersInRoom(room, client).Wait();
        //        }
        //    }
                
        //}

        //SETP 3
        private static void AutoInviteBot()
        {
        
       

            var rooms = _chatRooms.Find(x => x.Captured == null).SortByDescending(x=>x.Id).ToList();
            foreach(var room in rooms)
            {
                Logger.Instance.Log(LogLevel.Information, "processing room:" + room.Name);
                //auto search for the room and go to the member list scree
                SearchGroupChat(room.Name);
                //now toggle the search bar inside the room
                InitialMemberList();



                ///////////////////////////////////////////
                List<string> members = null;
                Bot bot = null;
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BotWebUrl);
                    var obj = new { topic = room.Name };

                    var webbot = GetBotId(client).Result;
                    bot = GetBotByName(webbot);

                    var sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                    var mmRep = client.PostAsync("/members", sc).Result;
                    string memberString = mmRep.Content.ReadAsStringAsync().Result;

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


                foreach (var member in members.Where(x => !string.IsNullOrEmpty(x)).OrderByDescending(x => x))
                {
                    string safeMember = member.Trim();
                    if (string.IsNullOrEmpty(safeMember)) continue;
            
                    //Prechecks
                    var foundInWeishang = _weishangModels.Find(x => x.Members.Contains(safeMember) && x.Name == room.Name).FirstOrDefaultAsync().Result;
                    var foundInInvited = _roomInvitedModels.Find(x => x.Members.Contains(safeMember) && x.Name == room.Name).FirstOrDefaultAsync().Result;
                    var hasBanName = Helper.Instance.CheckAgaistBanNames(safeMember, bot.BanKeywords);
                    if (foundInWeishang != null )
                    {
                        //found a weishang who posted in the room before
                        Logger.Instance.Log(LogLevel.Information, "skipping member:" + safeMember + "for he's weishang");

                        continue;
                    }
                    if(foundInInvited != null)
                    {
                        Logger.Instance.Log(LogLevel.Information, "skipping member:" + safeMember + "for he's invited before");

                        continue;
                    }
                    if(hasBanName)
                    {
                        Logger.Instance.Log(LogLevel.Information, "skipping member:" + safeMember + "for his bad name");

                        continue;
                    }
                    else
                    {
                        
                        SearchAndProceed(member, safeMember);

                        //ONE OFF,LAST ATTEMPT
                        if(_revisitedName!=null)
                        {
                            SearchAndProceed(member, _revisitedName);
                            _revisitedName = null;
                        }
                        //ADD TO THE DB so they won't be bothered again
                        //Helper.Instance.Exists(_roomInvitedModels,)
                        var invitedRoomRecord = _roomInvitedModels.Find(x => x.Name == room.Name).FirstOrDefaultAsync().Result;
                        if (invitedRoomRecord != null)
                        {
                            if (!invitedRoomRecord.Members.Contains(safeMember))
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
                ReturnToTopLevel();


                _chatRooms.FindOneAndUpdate(x=>x.Name == room.Name, Builders<ChatRoom>.Update.Set("captured", DateTime.UtcNow));
            }

        }

        private static void SearchAndProceed(string member, string safeMember)
        {
            //RE-FOCUS on the top search bar
            //Click Search button            
            MouseLClick(310, 166);

            CopyAndPaste(safeMember);

            Logger.Instance.Log(LogLevel.Information, "Dealing member " + safeMember);

            //Select the first result
            MouseLClick(57, 233, 3000);
            ScreenTwoImpl(safeMember);


            HardDelete(member);
        }

        private static void HardDelete(string name)
        {
            //Re focus on the top search bar
            //Click Search button            
            MouseLClick(338, 166);
            //HOLD DOWN BACKSPACE FOR 3 SECS TO CLEAR THE RESULT    

            for (int i = 0; i < name.Length + 5; i++)
            {
                SimKeyboard.Press(8);
                Thread.Sleep(500);
            }
        }

        #endregion
        #region Http Helpers

        private static async Task<ServerBotModel> GetBotId(HttpClient client)
        {
            var res = await client.GetAsync("/me");
            string botString = await res.Content.ReadAsStringAsync();



            var me = JsonConvert.DeserializeObject<ServerBotModel>(botString);


            return me;
        }

        public static Bot GetBotByName(ServerBotModel webBot)
        {
            var mybot = _bots.Find(x => x.Name == webBot.name || x.WxRef == webBot.wxRef).FirstOrDefault();
            return mybot;
        }
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
        private static async Task SendText(HttpClient client, string topicString, string msg)
        {
            var obj = new { content = msg, topic = topicString };


            var sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/text", sc);
            var rep = await response.Content.ReadAsStringAsync();
            Console.WriteLine(rep);
        }

        private static async Task SendImage(HttpClient client, string topic, string imagePath)
        {
            var obj = new
            {
                content = imagePath,
                topic = topic

            };

            var sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/image", sc);
            var rep = await response.Content.ReadAsStringAsync();
            Console.WriteLine(rep);
        }


        private static string RetainChinesePartial(string str)
        {
            
            //声明存储结果的字符串
            string chineseString = "";


            //将传入参数中的中文字符添加到结果字符串中
            for (int i = 0; i < str.Length; i++)
            {
                
                if (str[i] >= 0x4E00 && str[i] <= 0x9FA5) //汉字
                {
                    chineseString += str[i];
                }
                else
                {
                    if(chineseString.Length>0 )
                    {
                        break;
                    }
                }
            }


            //返回保留中文的处理结果
            return chineseString;
        }

        static void imageEnhancer()
        {
            //if (file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".jpeg"))
            //{
            //    using (var bmpInput = Image.FromFile(file))
            //    {
            //        using (var bmpOutput = new Bitmap(bmpInput))
            //        {
            //            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            //            Encoder myEncoder = Encoder.Quality;

            //            var myEncoderParameters = new EncoderParameters(1);
            //            var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            //            myEncoderParameters.Param[0] = myEncoderParameter;

            //            bmpOutput.SetResolution(96.0f, 96.0f); // Change to any dpi
            //            if (!Directory.Exists(_path + "\\96dpi\\"))
            //                Directory.CreateDirectory(_path + "\\96dpi\\");
            //            bmpOutput.Save(_path + "\\96dpi\\" + Path.GetFileName(file), jgpEncoder, myEncoderParameters);
            //            txtLog.AppendText(string.Format("\r\nResolution set to 96dpi for image {0}.", Path.GetFileName(file)));
            //        }
            //    }
            //}
        }

        private static Page ReadLatestSC()
        {
            var dir = new DirectoryInfo(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror");
            var sd = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            Logger.Instance.Log(LogLevel.Information, "scanning the latest screenshot " + sd.Name);

            using (var image = new Bitmap(sd.FullName))
            {
                var ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
                var page = ocr.Process(image);
                return page;             
            }
        }


        private static Rect GetPositionFromKeyword(Page page,  string keyword)
        {            

            var myLevel = PageIteratorLevel.TextLine;
            Rect found = new Rect();
            using (var iter = page.GetIterator())
            {
                iter.Begin();
                do
                {
                    if (iter.TryGetBoundingBox(myLevel, out var rect))
                    {
                        var curText = iter.GetText(myLevel);

                        if (curText.ToLowerInvariant().Contains(keyword))
                        {

                            found = rect;
                        }
                        // Your code here, 'rect' should containt the location of the text, 'curText' contains the actual text itself
                    }
                } while (iter.Next(myLevel));

            }
            return found;
        }
        //private static string ReadLatestScreenShot()
        //{
        //    var dir = new DirectoryInfo(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror");
        //    var sd = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
        //    Logger.Instance.Log(LogLevel.Information, "scanning the latest screenshot " + sd.Name);

        //    using (var image = new Bitmap(sd.FullName))
        //    {
        //        // var dpiX = image.HorizontalResolution;
        //        //var dpiY = image.VerticalResolution;
        //        // turn to the correct format
        //        //var image2 = Helper.Instance.Get24bppRgb(image);
        //        //image.SetResolution(300, 300);

        //        //var ni = new Bitmap(image, 588, 1281);
        //        //var dpiX2 = image2.HorizontalResolution;
        //        //var dpiY2 = image2.VerticalResolution;
        //        var ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
        //        var page = ocr.Process(image);
                

        //        var text = page.GetText();
        //        return text;
        //    }
                
      
        //}

        //private static Bitmap ResizeImage(Image image, int width, int height)
        //{
            
        //    var newDimensions = ImageFunctions.GenerateImageDimensions(image.Width, image.Height, width, height);

        //    var resizedImage = new Bitmap(newDimensions.Width, newDimensions.Height);

        //    //we have a normal image
        //    using (var gfx = Graphics.FromImage(resizedImage))
        //    {
        //        gfx.SmoothingMode = SmoothingMode.HighQuality;
        //        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        var targRectangle = new Rectangle(0, 0, newDimensions.Width, newDimensions.Height);
        //        var srcRectangle = new Rectangle(0, 0, image.Width, image.Height);

        //        gfx.DrawImage(image, targRectangle, srcRectangle, GraphicsUnit.Pixel);
        //    }

        //    return resizedImage;
        //}
        //private static string ReadLatestScreenShotWEnhancer()
        //{
        //    var dir = new DirectoryInfo(@"C:\Users\msi-laptop\Documents\Apowersoft\Windows ApowerMirror");
        //    var sd = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
        //    Logger.Instance.Log(LogLevel.Information, "scanning the latest screenshot " + sd.Name);

        //    using (var bmpInput = new Bitmap(sd.FullName))
        //    {
        //        var dpiX = bmpInput.HorizontalResolution;
        //        var dpiY = bmpInput.VerticalResolution;

        //        bmpInput.SetResolution(300, 300);
        //        //bmpInput.Save(@"D:\images\300.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        //        var ni = new Bitmap(bmpInput, 588, 1281);
        //        //ni.Save(@"D:\images\588.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        //        //using (var bmpOutput = new Bitmap(bmpInput))
        //        //{
        //        //    ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
        //        //    Encoder myEncoder = Encoder.Quality;

        //        //    var myEncoderParameters = new EncoderParameters(1);
        //        //    var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
        //        //    myEncoderParameters.Param[0] = myEncoderParameter;

        //        //    bmpOutput.SetResolution(96.0f, 96.0f); // Change to any dpi
        //        //    if (!Directory.Exists(_path + "\\96dpi\\"))
        //        //        Directory.CreateDirectory(_path + "\\96dpi\\");
        //        //    bmpOutput.Save(_path + "\\96dpi\\" + Path.GetFileName(file), jgpEncoder, myEncoderParameters);
        //        //    txtLog.AppendText(string.Format("\r\nResolution set to 96dpi for image {0}.", Path.GetFileName(file)));
        //        //}

        //        //var dpiX2 = bmpInput.HorizontalResolution;
        //        //var dpiY2 = bmpInput.VerticalResolution;

        //        var ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndCube);
        //        var page = ocr.Process(ni);
        //        var text = page.GetText();
        //        return text;
        //    }
        //}
        #endregion


    }


}
