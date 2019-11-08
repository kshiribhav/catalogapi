using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CatalogAPI.Helpers;
using CatalogAPI.Infrastructure;
using CatalogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors("AllowPartners")]
    //[Authorize]
    public class CatalogController : ControllerBase
    {
        CatalogContext _catalogContext;
        IConfiguration config;

        public CatalogController(CatalogContext catalogContext, IConfiguration configuration)
        {
            _catalogContext = catalogContext;
            config = configuration;
        }

        [HttpGet("", Name = "GetProducts")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CatalogItem>>> GetProducts()
        {
            var result = await _catalogContext.Catalog.FindAsync<CatalogItem>(FilterDefinition<CatalogItem>.Empty);
            return result.ToList();
        }

        [HttpGet("{id}", Name = "FindById")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<CatalogItem>> FindProductById(string id)
        {
            var builder = Builders<CatalogItem>.Filter;
            var filter = builder.Eq("Id", id);
            var result = await _catalogContext.Catalog.FindAsync(filter);
            var item = result.FirstOrDefault();
            if (item == null)
            {
                return NotFound(); //404
            }
            else
            {
                return Ok(item); //200
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("", Name = "AddProduct")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult<CatalogItem> AddProduct(CatalogItem item)
        {
            TryValidateModel(item);
            if (ModelState.IsValid)
            {
                _catalogContext.Catalog.InsertOne(item);
                return Created("", item); // 201
            }
            else
            {
                return BadRequest(ModelState); //400
            }

        }

        [Authorize(Roles = "admin")]
        [HttpPost("product")]
        public ActionResult<CatalogItem> AddProduct()
        {
            // var imageName = SaveImageToLocal(Request.Form.Files[0]);
            var imageName = SaveImageToCloudAsync(Request.Form.Files[0]).GetAwaiter().GetResult();
            var catalogItem = new CatalogItem()
            {
                Name = Request.Form["name"],
                Price = double.Parse(Request.Form["price"]),
                Quantity = Int32.Parse(Request.Form["quantity"]),
                ReorderLevel = Int32.Parse(Request.Form["reorderLevel"]),
                ManufacturingDate = DateTime.Parse(Request.Form["manufacturingDate"]),
                Vendors = new List<Vendor>(),
                ImageUrl = imageName
            };
            _catalogContext.Catalog.InsertOne(catalogItem);
            //Backup to Azure Table Storage
            BackupToTableAsync(catalogItem).GetAwaiter().GetResult();
            return catalogItem;
        }

        [NonAction]
        private string SaveImageToLocal(IFormFile image)
        {
            var imageName = $"{Guid.NewGuid()}_{image.FileName}";

            var dirName = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            var filePath = Path.Combine(dirName, imageName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fs);
            }

            return $"/Images/{imageName}";
        }

        [NonAction]
        private async Task<string> SaveImageToCloudAsync(IFormFile image)
        {
            var imageName = $"{Guid.NewGuid()}_{image.FileName}";
            var tempFile = Path.GetTempFileName();
            using (FileStream fs = new FileStream(tempFile, FileMode.Create))
            {
                await image.CopyToAsync(fs);
            }
            var imageFile = Path.Combine(Path.GetDirectoryName(tempFile), imageName);
            System.IO.File.Move(tempFile, imageFile);
            StorageAccountHelper helper = new StorageAccountHelper
            {
                StorageConnectionString = config.GetConnectionString("StorageConnection")
            };
            var fileUri = await helper.UploadFileBlobAsync(imageFile, "eshopimages");
            System.IO.File.Delete(tempFile);
            return fileUri;
        }

        [NonAction]
        private async Task<CatalogEntity> BackupToTableAsync(CatalogItem item)
        {
            StorageAccountHelper accountHelper = new StorageAccountHelper();
            accountHelper.TableConnectionString = config.GetConnectionString("TableConnection");
            return await accountHelper.SaveToTableStorageAsync(item);
        }
    }
}