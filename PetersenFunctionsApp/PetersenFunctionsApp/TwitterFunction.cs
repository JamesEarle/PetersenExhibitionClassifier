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

namespace PetersenFunctionsApp
{
    public static class TwitterFunction
    {
        [FunctionName("TwitterFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
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

                var tweetData = new
                {
                    Id = tweet.Id,
                    Username = tweet.User.ScreenName,
                    Text = tweet.Text,
                    LikesCount = tweet.RetweetedStatus == null ? 0 : tweet.RetweetedStatus.FavoriteCount,
                    SharesCount = tweet.RetweetCount,
                    Location = tweet.User.Location == "" ? null : tweet.User.Location,
                    Media = urls.Count == 0 ? null : urls.ToArray(),
                    PostedAt = tweet.CreatedAt,
                    FollowerCount = tweet.User.FollowersCount,
                    Platform = "Twitter"
                };

                return req.CreateResponse(HttpStatusCode.OK, tweetData);
            }
            return req.CreateResponse(HttpStatusCode.NoContent);
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
