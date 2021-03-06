﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoClicker.Models
{
    //non persistent
    public class ServerBotModel
    {
        public string name { get; set; }
        public string wxRef { get; set; }
    }

    public interface WebModel
    {
        ObjectId Id { get; set; }
        string Name { get; set; }
    }

    public interface  DBModel
    {
        ObjectId Id { get; set; }
        string WxRef { get; set; }
    }
    public interface IBot : DBModel
    {
        string[] BanKeywords { get; set; }
        string GreetingMsg { get; set; }
        string Name { get; set; }
    }

    public class Customer : DBModel
    {

        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("wxRef")]
        public string WxRef { get; set; }        
        
        
    }

    public class History : DBModel
    {

        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("wxRef")]
        public string WxRef { get; set; }


    }



    public class Bot : DBModel, IBot
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("region")]
        public string Region { get; set; }
        [BsonElement("city")]
        public string City { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("wxRef")]
        public string WxRef { get; set; }
        [BsonElement("greetings")]
        public string GreetingMsg { get; set; }

        [BsonElement("welcomeMsg")]
        public string[] WelcomeMsg { get; set; }

        [BsonElement("banWords")]
        public string[] BanKeywords { get; set; }
        [BsonElement("exemptions")]
        public string[] Exemptions { get; set; }
        [BsonElement("roomNumber")]
        public int LatestRooms { get; set; }
    }

    
    public class ChatRoom: DBModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("topic")]
        public string Name { get; set; }
        [BsonElement("wxRef")]
        public string WxRef { get; set; }
        [BsonElement("botRef")]
        public string BotRef { get; set; }
        [BsonElement("created")]
        public DateTime Created { get; set; }
        [BsonElement("captured")]
        public DateTime? Captured { get; set; }

    }
   
    public class Sub : DBModel
    {
        [BsonElement("customerId")]
        public ObjectId CustomerId { get; set; }
        [BsonElement("botIds")]
        public List<ObjectId> BotIds { get; set; }
        [BsonElement("name")]
        public string WxRef { get; set; }

        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("text")]
        public string[] Text { get; set; }
        [BsonElement("imageUrl")]
        public string[] ImageUrl { get; set; }
        [BsonElement("interval")]
        public double Interval { get; set; }
        [BsonElement("lastBroadcast")]
        public DateTime LastBroadcast { get; set; }
    }

  

    public class WeishangWebModel :WebModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("topic")]
        public string Name { get; set; }
        [BsonElement("members")]
        public string[] Members { get; set; }
        [BsonElement("careted")]
        public DateTime? Created{ get; set; }
        [BsonElement("updated")]
        public DateTime? Updated { get; set; }

    }
    public class ChatRoomWebModel: WebModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("topic")]
        public string Name { get; set; }
        [BsonElement("members")]
        public string[] Members { get; set; }
        [BsonElement("careted")]
        public DateTime? Created { get; set; }
        [BsonElement("updated")]
        public DateTime? Updated { get; set; }
        [BsonElement("captured")]
        public DateTime? Captured { get; set; }
        [BsonElement("botWxRef")]
        public string BotWxRef { get; set; }
        


    }


    public class FriendRequest : DBModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("wxRef")]
        public string WxRef { get; set; }
        [BsonElement("botWxRef")]
        public string BotWxRef { get; set; }        
        [BsonElement("created")]
        public DateTime? Created { get; set; }
        [BsonElement("topic")]
        public string Topic { get; set; }
    }

    public class LocalJob
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("instance")]
        public int Instance { get; set; }
        [BsonElement("botWxRef")]
        public string BotWxRef { get; set; }
        [BsonElement("url")]
        public string Url { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
        [BsonElement("start")]
        public DateTime Start { get; set; }
        [BsonElement("end")]
        public DateTime End { get; set; }
        [BsonElement("log")]
        public string Log { get; set; }
        [BsonElement("jobType")]
        public int Type { get; set; }
    }

    public enum JobType
    {
        Broadcast=0,
        AddFans=2,
        
    }

    
}
