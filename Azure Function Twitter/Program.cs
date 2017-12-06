using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace PetersenExhibitionClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            // Consumer Key 
            string key = "V8RTupGOUX6lnpChvBw2TKN2g";

            // Consumer Secret
            string secret = "j64qb5cIMwE32jNNUsYssc2erdCOuAn70FzDysUHKxC7Qoh4of";

            OAuth2Token token = GetOAuth2Token(key, secret).Result;
            SearchResult results = Search(token, "#PetersenMuseum", 1);

            // Only runs once, doesn't like IEnumerator
            foreach(var tweet in results)
            {
                List<string> urls = new List<string>();
                if (tweet.Entities.Media != null)
                {
                    foreach (var entity in tweet.Entities.Media)
                    {
                        urls.Add(entity.MediaUrlHttps);
                        Console.WriteLine(entity.MediaUrlHttps);
                    }
                }

                Console.WriteLine(urls.ToArray().ToString());

                var tweetData = new
                {
                    Id = tweet.Id,
                    Username = tweet.User.ScreenName,
                    Text = tweet.Text,
                    RetweetsCount = tweet.RetweetCount,
                    FavoritesCount = tweet.RetweetedStatus == null ? 0 : tweet.RetweetedStatus.FavoriteCount,
                    Location = tweet.User.Location,
                    Media = urls.Count == 0 ? null : urls.ToArray()
                };
            }
        }

        static async Task<OAuth2Token> GetOAuth2Token(string key, string secret)
        {
            var token = await OAuth2.GetTokenAsync(key, secret);
            return token;
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
