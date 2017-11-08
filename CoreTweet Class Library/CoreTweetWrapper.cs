using System;
using System.Threading.Tasks;
using CoreTweet;

namespace CoreTweet_Class_Library
{
    public class Program {
        static void Main(string[] args) {
            CoreTweetLibrary lib = new CoreTweetLibrary();
        }
    }
    
    public class CoreTweetLibrary
    {
        private string key = "V8RTupGOUX6lnpChvBw2TKN2g";

        private string secret = "j64qb5cIMwE32jNNUsYssc2erdCOuAn70FzDysUHKxC7Qoh4of";

        public CoreTweetLibrary() {
            OAuth2Token token = GetOAuth2Token(key, secret).Result;
        }
 
        public static async Task<OAuth2Token> GetOAuth2Token(string key, string secret)
        {
            var token = await OAuth2.GetTokenAsync(key, secret);
            return token;
        }

        public static SearchResult Search(OAuth2Token tokens, string query)
        {
            var tweets = tokens.Search.TweetsAsync(q => query).Result;
            return tweets;
        }
    }
}
