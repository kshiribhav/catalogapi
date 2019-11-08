﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;

namespace CatalogAPI.Models
{
    public class CatalogItem
    {
        public CatalogItem()
        {
            Vendors = new List<Vendor>();
        }

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("reorderLevel")]
        public int ReorderLevel { get; set; }

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; }

        [BsonElement("ManufacturingDate")]
        public DateTime ManufacturingDate { get; set; }

        [BsonElement("vendors")]
        public List<Vendor> Vendors { get; set; }
    }

    public class Vendor
    {
        public string Name { get; set; }

        public string ContactNo { get; set; }

        public string Address { get; set; }
    }
}
