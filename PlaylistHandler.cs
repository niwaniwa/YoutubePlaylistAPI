using System.Data.Common;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace youtube_playlist
{
    public class PlaylistHandler
    {

        private const string apiKey = "AIzaSyAq2F5mj5gGAz_3p1GE0epBCB2OFwcx9rQ";
        

        public YoutubePlaylistDataResponce LambdaHandler(ImportDataSerializer input)
        {
            LambdaLogger.Log(formatter(input.url));
            return this.Run(formatter(input.url));
            // return "";
        }

        private YouTubeService Credentional()
        {
            var service = new YouTubeService(new BaseClientService.Initializer
            {
                ApplicationName = "Youtube API Test app",
                ApiKey = apiKey,
            });

            return service;
            
        }

        private string formatter(string str)
        {

            if(!str.Contains('?'))
            {
                return "-1";
            }
            
            return str.Split("?v=")[1];
        }

        private YoutubePlaylistDataResponce Run(string input){
            var data = new YoutubePlaylistDataResponce();

            if(input.Equals("-1"))
                return data;

            var service = Credentional();

            if(service == null) return data;

            var request = service.Videos.List(new Repeatable<string>(new List<string>
            {
                "snippet,contentDetails,statistics"
            }));

            request.Id = input;
            var response = request.Execute();
            
            if(response.Items == null) return data;

            var videoData = new VideoData[response.Items.Count];
            LambdaLogger.Log($"response.Items.Count {response.Items.Count}");

            for (int i = 0; i < response.Items.Count; i++)
            {   
                var snippet = response.Items[i].Snippet;
                videoData[i] = new VideoData();
                videoData[i].title = snippet.Title;
                videoData[i].description = "";//snippet.Description;
                videoData[i].id = response.Items[i].Id;
            }

            data.name = "Youtube";
            data.videos = videoData;

            return data;
        
        }

    }

    public class YoutubePlaylistDataResponce 
    {

        [JsonProperty(PropertyName = "name")]
        public string name {get; set;}

        [JsonProperty(PropertyName = "videos")]
        public VideoData[] videos {get; set;}

    }

    public class VideoData
    {
        
        [JsonProperty(PropertyName = "title")]
        public string title {get; set;}

        [JsonProperty(PropertyName = "description")]
        public string description {get; set;}

        [JsonProperty(PropertyName = "id")]
        public string id {get; set;}


    }

    public class ImportDataSerializer
    {
        [JsonProperty(PropertyName = "url")]
        public string url {get; set;}
    }
}
