using System;
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

            SearchResult results = Search(token, "#PetersenMusem");

            foreach (var tweet in results)
            {
                // Temporary, account retweeted the same thing over and over making weird data.
                if (tweet.User.ScreenName != "hugoscaglia")
                {
                    Console.WriteLine($"{tweet.User.ScreenName}: {tweet.Text}");
                    Console.WriteLine($"{tweet.FavoriteCount} Favorites, {tweet.RetweetCount} Retweets");
                    Console.WriteLine("");
                }
            }
        }

        static async Task<OAuth2Token> GetOAuth2Token(string key, string secret)
        {
            var token = await OAuth2.GetTokenAsync(key, secret);
            return token;
        }

        static SearchResult Search(OAuth2Token tokens, string query)
        {
            var tweets = tokens.Search.TweetsAsync(q => query).Result;
            return tweets;
        }
    }
}
