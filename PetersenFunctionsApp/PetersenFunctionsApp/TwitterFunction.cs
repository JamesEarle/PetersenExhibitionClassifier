using System.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using CoreTweet;
using PetersenFunctionsApp.Models;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace PetersenFunctionsApp
{
    public static class TwitterFunction
    {
        [FunctionName("TwitterFunction")]
        [return: Queue("posts-queue", Connection = "CloudStorageAccountEndpoint")]
        public static async Task<PostEntity> Run([TimerTrigger("0/30 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            string key = Environment.GetEnvironmentVariable("TwitterAPIKey", EnvironmentVariableTarget.Process);
            string secret = Environment.GetEnvironmentVariable("TwitterAPISecret", EnvironmentVariableTarget.Process);
            OAuth2Token token = await OAuth2.GetTokenAsync(key, secret);

            SearchResult results = Search(token, "@realDonaldtrump", 1);

            // Only runs once, doesn't like IEnumerator
            foreach (Status tweet in results)
            {
                // Check if tweet contains media
                List<string> urls = new List<string>();
                if (tweet.Entities.Media != null)
                {
                    foreach (var entity in tweet.Entities.Media)
                    {
                        urls.Add(entity.MediaUrlHttps);
                    }
                }

                //var test = String.Join(",", urls);

                //var tweetData = new {
                //    tweet.Id,
                //    Username = tweet.User.ScreenName,
                //    Text = tweet.Text,
                //    LikesCount = tweet.RetweetedStatus == null ? 0 : (int)tweet.RetweetedStatus.FavoriteCount,
                //    SharesCount = (int)tweet.RetweetCount,
                //    Location = tweet.User.Location == "" ? null : tweet.User.Location,
                //    Media = urls.Count == 0 ? null : String.Join(",", urls),
                //    PostedAt = tweet.CreatedAt.UtcDateTime,
                //    FollowerCount = tweet.User.FollowersCount,
                //    Platform = "Twitter"
                //};

                PostEntity tweetData = new PostEntity(tweet.Id.ToString())
                {
                    Username = tweet.User.ScreenName,
                    Text = tweet.Text,
                    LikesCount = tweet.RetweetedStatus == null ? 0 : (int)tweet.RetweetedStatus.FavoriteCount,
                    SharesCount = (int)tweet.RetweetCount,
                    Location = tweet.User.Location == "" ? null : tweet.User.Location,
                    Media = urls.Count == 0 ? null : String.Join(",", urls),
                    PostedAt = tweet.CreatedAt.UtcDateTime,
                    FollowerCount = tweet.User.FollowersCount,
                    Platform = "Twitter"
                };

                //var client = new HttpClient();

                //var url = "https://socialdashboardfunc.azurewebsites.net/api/ExhbitFinder?code=Nhblavq8zADr7NFcf/aHJZpVnPDai3CGaNdyBtAwoIHHPlCrPT0XlQ==";

                //using (var content = new ObjectContent<PostEntity>(tweetData, new JsonMediaTypeFormatter()))
                //{
                //    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //    var response = await client.PostAsync(url, content);
                //    var responseContent = response.Content.ReadAsStringAsync().Result;

                //    return req.CreateResponse(HttpStatusCode.OK, responseContent);
                //}
                return tweetData;
                //return req.CreateResponse(HttpStatusCode.OK, tweetData.ToString());
            }
            return null;
            //return req.CreateResponse(HttpStatusCode.NoContent);
        }

        static SearchResult Search(OAuth2Token tokens, string query, int count)
        {
            var myParams = new Dictionary<string, object>
            {
                {"q", query },
                {"count", count }
            };
            var tweets = tokens.Search.TweetsAsync(myParams).Result;
            return tweets;
        }
    }
}
