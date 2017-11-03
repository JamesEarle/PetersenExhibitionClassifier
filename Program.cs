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
            string key = "9LXo5THOds4nCgMbknVTLlWED";

            // Consumer Secret
            string secret = "VohT2bxlsmmYgexIdbhPuMzhPMpa5eLZEWuqRLUwTRIxfPEra7";

            var session = Authorize(key, secret);

            var tokens = GetTokens(key, secret, session);

            var search = tokens.Search;

            Console.WriteLine(tokens.Search);

            // tokens.Statuses.Update(status => "hello");
            Console.WriteLine(tokens);
        }

        static async Task<OAuth.OAuthSession> Authorize(string key, string secret) {
            ConnectionOptions options = new ConnectionOptions();
            var session = await OAuth.AuthorizeAsync(key, secret);  
            return session;     
        }

        static Tokens GetTokens(string key, string secret, Task<OAuth.OAuthSession> session) {
            Tokens tokens = Tokens.Create(key, secret, session.Result.RequestToken, session.Result.RequestTokenSecret);
            return tokens;
        }
    }
}
