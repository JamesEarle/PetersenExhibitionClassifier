using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetersenFunctionsApp.Models
{
    public class PostEntity : TableEntity
    {
        public PostEntity(String Id)
        {
            this.PartitionKey = DateTime.Today.ToLongDateString();
            this.RowKey = Id;
        }

        public String Username { get; set; }
        public String Text { get; set; }
        public int LikesCount { get; set; }
        public int SharesCount { get; set; }
        public String Location { get; set; }           // User's posted home location, if any is public
        public String Media { get; set; }
        public String ExhibitsMentioned { get; set; }
        public String CarsMentioned { get; set; }
        public DateTime PostedAt { get; set; }
        public int FollowerCount { get; set; }
        public String Platform { get; set; }

        public String[] GetMediaLinks()
        {
            return this.Media.Split(',');
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
