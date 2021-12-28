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

        private const string apiKey = //"";
        

        public YoutubePlaylistDataResponce LambdaHandler(ImportDataSerializer input)
        {
            // LambdaLogger.Log(input.id);
            return this.Run(input.id);
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

            // if(input.Equals("-1"))
            //     return data;

            var service = Credentional();

            if(service == null) return data;

            var request = service.Playlists.List(new Repeatable<string>(new List<string>
            {
                "snippet"
            }));
            request.Id = input;
            var response = request.Execute();
            var nextPageToken = "";
            var videoDataList = new List<VideoData>();
            while (nextPageToken != null)
            {
                var playlistItemsListRequest = service.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = input;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                var playlistItemsListResponse = playlistItemsListRequest.Execute();


                foreach (var playlistItem in playlistItemsListResponse.Items)
                {
                    // Console.WriteLine(String.Format("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId));
                    if(playlistItem.Snippet.Title.Equals("Private video") || playlistItem.Snippet.Title.Equals("Deleted video"))
                        continue;
                    var videoData = new VideoData();
                    videoData.id = playlistItem.Snippet.ResourceId.VideoId;
                    videoData.title = playlistItem.Snippet.Title;
                    videoDataList.Add(videoData);
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }

            data.name = $"{response.Items[0].Snippet.Title ?? input}";
            data.videos = videoDataList.ToArray();

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
        [JsonProperty(PropertyName = "id")]
        public string id {get; set;}
    }
}
