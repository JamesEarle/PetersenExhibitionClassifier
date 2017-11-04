using System;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json;

namespace PetersenExhibitionClassifier
{
    class Program
    {
        // Tokens tokens;
        static void Main(string[] args)
        {
            // Consumer Key 
            string key = "V8RTupGOUX6lnpChvBw2TKN2g";

            // Consumer Secret
            string secret = "j64qb5cIMwE32jNNUsYssc2erdCOuAn70FzDysUHKxC7Qoh4of";

            OAuth2Token token = GetOAuth2Token(key, secret).Result;

            SearchResult results = Search(token, "#PetersenMuseum");

            foreach (var tweet in results)
            {
                // Temporary, account retweeted the same thing over and over making weird data.
                if (tweet.User.ScreenName != "hugoscaglia")
                {
                    Console.WriteLine("{0}: {1}", tweet.User.ScreenName, tweet.Text);
                    Console.WriteLine("{0} Favorites, {1} Retweets", tweet.FavoriteCount, tweet.RetweetCount);
                    Console.WriteLine("\n");
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

        // App only authentication
        static async Task<OAuth.OAuthSession> Authorize(string key, string secret)
        {
            var session = await OAuth.AuthorizeAsync(key, secret);
            return session;
        }

        static Tokens GetTokens(string key, string secret, Task<OAuth.OAuthSession> session)
        {
            Tokens tokens = Tokens.Create(key, secret, session.Result.RequestToken, session.Result.RequestTokenSecret);
            return tokens;
        }
    }
}
