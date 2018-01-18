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

// TODO: fail case(s)?
namespace PetersenFunctionsApp
{
    public static class ExhibitFunction
    {
        private static readonly string _exhibitTableName = "Exhibits";
        private static readonly string _postsTableName = "Posts";
        private static readonly string _startDateColumnName = "StartDate";
        private static readonly string _endDateColumnName = "EndDate";
        private static readonly double _probabilityThreshold = 0.1;

        [FunctionName("ExhibitFunction")]
        public static async void Run([QueueTrigger("posts-queue", Connection = "CloudStorageAccountEndpoint")]PostEntity post, TraceWriter log)
        {
            log.Info("Exhibit Finder trigger function processed a request.");

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
            var lowerPostText = post.Text.ToLower();

            var list = exhibitTable.ExecuteQuery(activeExhibitsQuery);

            // Loop through each exhibit and look for exhibit and car name matches in the tweet
            foreach (ExhibitEntity e in exhibitTable.ExecuteQuery(activeExhibitsQuery))
            {
                // Look for exact exhibit name matches in the tweet
                if (lowerPostText.Contains(e.RowKey.ToLower()) && !foundExhibit)
                {
                    post.ExhibitsMentioned += e.RowKey;
                    foundExhibit = true;
                }

                // Look for alternate exhibit name matches in the tweet
                foreach (String n in e.GetAltExhibitNames())
                {
                    if (lowerPostText.Contains(n.ToLower()) && !foundExhibit)
                    {
                        post.ExhibitsMentioned += n;
                        foundExhibit = true;
                        break;
                    }
                }

                // Look for car name matches in the tweet
                foreach (String c in e.GetCars())
                {
                    if (lowerPostText.Contains(c.ToLower()) && !foundCar)
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
            if((!foundExhibit || !foundCar) && post.Media != null)
            {
                ImageTagPredictionModel exhibit = null;
                 ImageTagPredictionModel car = null;
                foreach (var m in post.GetMediaLinks())
                {
                    var predict = new PredictionEndpoint() { ApiKey = Environment.GetEnvironmentVariable("CustomVision.APIKey", EnvironmentVariableTarget.Process) };
                    var url = new ImageUrl() { Url = m };

                    var result = await predict.PredictImageUrlWithHttpMessagesAsync(
                        new Guid(Environment.GetEnvironmentVariable("CustomVision.ProjectId", EnvironmentVariableTarget.Process)),
                        url,
                        null,
                        Environment.GetEnvironmentVariable("CustomVision.ProjectName", EnvironmentVariableTarget.Process));

                    foreach (var p in result.Body.Predictions)
                    {
                        if (p.Tag.Contains("Exhibit") &&
                            p.Probability > _probabilityThreshold &&
                            (exhibit == null || p.Probability > exhibit.Probability))
                        {
                            exhibit = p;
                        }

                        if (!p.Tag.Contains("Exhibit") && 
                            p.Probability > _probabilityThreshold &&
                            (car == null || p.Probability > car.Probability))
                        {
                            car = p;
                        }
                    }

                    // If we found an exhibit from a tag and we didn't previously find an exhibit, then add the new exhibit
                    if (exhibit != null && !foundExhibit)
                    {
                        post.ExhibitsMentioned = exhibit.Tag;
                    }
                    // If we found an car from a tag and we didn't previously find a car, then add the new car
                    if (car != null && !foundCar)
                    {
                        post.CarsMentioned = car.Tag;
                    }
                }
            }

            // Insert post to table storage
            var postsTable = tableClient.GetTableReference(_postsTableName);
            postsTable.CreateIfNotExists();
            TableOperation insertPost = TableOperation.Insert(post);
            postsTable.Execute(insertPost);
        }
    }
}
