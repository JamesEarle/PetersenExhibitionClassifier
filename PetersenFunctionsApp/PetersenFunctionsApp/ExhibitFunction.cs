using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PetersenFunctionsApp.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;

namespace PetersenFunctionsApp
{
    public static class ExhibitFinder
    {
        private static readonly string _exhibitTableName = "Exhibits";
        private static readonly string _postsTableName = "Posts";
        private static readonly string _startDateColumnName = "StartDate";
        private static readonly string _endDateColumnName = "EndDate";

        [FunctionName("ExhibitFinder")]
        //public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        public static async void Run([QueueTrigger("posts-queue", Connection = "CloudStorageAccountEndpoint"]PostEntity post, TraceWriter log)
        {
            log.Info("Exhibit Finder trigger function processed a request.");

            // Create Tweets object from body
            //var tweet = await req.Content.ReadAsAsync<PostEntity>();

            // Set up table client
            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("CloudStorageAccountEndpoint", EnvironmentVariableTarget.Process));
            var tableClient = storageAccount.CreateCloudTableClient();

            // Get all active Exhibits from table storage
            var exhibitTable = tableClient.GetTableReference(_exhibitTableName);
            exhibitTable.CreateIfNotExists();
            var activeExhibitsQuery = new TableQuery<ExhibitEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate(_startDateColumnName, QueryComparisons.LessThanOrEqual, DateTime.UtcNow),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate(_endDateColumnName, QueryComparisons.GreaterThanOrEqual, DateTime.UtcNow)));

            var foundExhibit = false;
            var foundCar = false;
            var lowerTweetText = post.Text.ToLower();

            var list = exhibitTable.ExecuteQuery(activeExhibitsQuery);

            // Loop through each exhibit and look for exhibit and car name matches in the tweet
            foreach (ExhibitEntity e in exhibitTable.ExecuteQuery(activeExhibitsQuery))
            {
                // Look for exact exhibit name matches in the tweet
                if (lowerTweetText.Contains(e.RowKey.ToLower()) && !foundExhibit)
                {
                    post.ExhibitsMentioned += e.RowKey;
                    foundExhibit = true;
                }

                // Look for alternate exhibit name matches in the tweet
                foreach (String n in e.GetAltExhibitNames())
                {
                    if (lowerTweetText.Contains(n.ToLower()) && !foundExhibit)
                    {
                        post.ExhibitsMentioned += n;
                        foundExhibit = true;
                        break;
                    }
                }

                // Look for car name matches in the tweet
                foreach (String c in e.GetCars())
                {
                    if (lowerTweetText.Contains(c.ToLower()) && !foundCar)
                    {
                        post.CarsMentioned += c;
                        foundCar = true;

                        // If we got a car name, but didn't find an exhibit yet, find the matching exhibit for that car
                        if (!foundExhibit)
                        {
                            post.ExhibitsMentioned = e.RowKey;
                            foundExhibit = true;
                            break;
                        }

                        break;
                    }
                }

                // If we found a car and an exhibit, stop searching
                if (foundExhibit && foundCar)
                {
                    break;
                }
            }

            // If we didn't find both an exhibit and a car, check if we can get some tags from Cognitive Services
            if(!foundExhibit && !foundCar && post.Media != null)
            {
                foreach (var m in post.GetMediaLinks())
                {
                    var predict = new PredictionEndpoint() { ApiKey = Environment.GetEnvironmentVariable("CustomVision.APIKey", EnvironmentVariableTarget.Process) };
                    var url = new ImageUrl() { Url = m };

                    var result = await predict.PredictImageUrlWithHttpMessagesAsync(
                        new Guid(Environment.GetEnvironmentVariable("CustomVision.ProjectId", EnvironmentVariableTarget.Process)),
                        url,
                        null,
                        Environment.GetEnvironmentVariable("CustomVision.ProjectName", EnvironmentVariableTarget.Process));

                    // TODO: Decide if we will store top tag only or go by a threshold
                    var exhibit = result.Body.Predictions[0];
                    var car = result.Body.Predictions[0];
                    foreach (var p in result.Body.Predictions)
                    {
                        // First tag could be for a car not an exhibit, so check the current prediction is for the right type
                        if ((p.Tag.Contains("Exhibit") && p.Probability > exhibit.Probability) || (p.Tag.Contains("Exhibit") && !exhibit.Tag.Contains("Exhibit")))
                        {
                            exhibit = p;
                        }

                        // First tag could be for an exhibit not a car, so check the current prediction is for the right type
                        if ((!p.Tag.Contains("Exhibit") && p.Probability > car.Probability) || (!p.Tag.Contains("Exhibit") && car.Tag.Contains("Exhibit")))
                        {
                            car = p;
                        }
                    }

                    // TODO: this assumes it always will find an exhibit from a media link, some scores might be VERY low though so hard to set a threshold
                    post.ExhibitsMentioned = exhibit.Tag;
                    post.CarsMentioned = car.Tag;
                }
            }

            // Insert tweet to table storage
            var tweetTable = tableClient.GetTableReference(_postsTableName);
            exhibitTable.CreateIfNotExists();
            TableOperation insertTweet = TableOperation.Insert(post);
            tweetTable.Execute(insertTweet);
            
            // TODO: fail case(s)?
            //return req.CreateResponse(HttpStatusCode.OK, "Success");
        }
    }
}
