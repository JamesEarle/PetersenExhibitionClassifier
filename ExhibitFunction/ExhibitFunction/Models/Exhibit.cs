using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace ExhibitFunction.Models
{
    class Exhibit : TableEntity
    {
        public Exhibit()
        {
            this.PartitionKey = "Exhibit";
        }

        public String AltExhibitNames { get; set; }
        public String Cars { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public String[] GetAltExhibitNames()
        {
            return this.AltExhibitNames.Split(',');
        }

        public String[] GetCars()
        {
            return this.Cars.Split(',');
        }
    }
}
