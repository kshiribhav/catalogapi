using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CatalogAPI.Models
{
    public class CatalogEntity : TableEntity
    {
        public CatalogEntity(string name, string id)
        {
            this.PartitionKey = name;
            this.RowKey = id;
        }

        public double Price { get; set; }

        public int Quantity { get; set; }

        public int ReorderLevel { get; set; }

        public DateTime ManufacturingDate { get; set; }

        public string ImageUrl { get; set; }
    }
}
