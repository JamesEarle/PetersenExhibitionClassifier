using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExhibitFunction.Models
{
    class TweetEntity : TableEntity
    {
        public TweetEntity(String Id)
        {
            this.PartitionKey = DateTime.Today.ToLongDateString();
            this.RowKey = Id;
        }

        public String Username { get; set; }
        public String Text { get; set; }
        public int Favorites { get; set; }
        public int Retweets { get; set; }
        public String Location { get; set; }           // Twitter user's home location
        public String MediaLinks { get; set; }
        public String ExhibitsMentioned { get; set; }
        public String CarsMentioned { get; set; }
        public DateTime TweetedAt { get; set; }

        public String[] GetMediaLinks()
        {
            return this.MediaLinks.Split(',');
        }

        public void SetExhibitsMentioned(List<String> exhibits)
        {
            this.ExhibitsMentioned = String.Join(",", exhibits.Select(e => "'" + e + "'"));
        }

        public void SetCarsMentioned(List<String> cars)
        {
            this.CarsMentioned = String.Join(",", cars.Select(c => "'" + c + "'"));
        }
    }
}
