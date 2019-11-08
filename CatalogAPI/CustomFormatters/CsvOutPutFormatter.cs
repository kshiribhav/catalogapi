using CatalogAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CatalogAPI.CustomFormatters
{
    public class CsvOutPutFormatter : TextOutputFormatter
    {
        public CsvOutPutFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add("text/csv");
            SupportedMediaTypes.Add("application/csv");

        }

        protected override bool CanWriteType(Type type)
        {
            return (typeof(CatalogItem).IsAssignableFrom(type) || typeof(IEnumerable<CatalogItem>).IsAssignableFrom(type)) ? true : false;
        }

        public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var buffer = new StringBuilder();
            var response = context.HttpContext.Response;
            if (context.Object is CatalogItem)
            {
                var item = context.Object as CatalogItem;
                buffer.Append("Id,Name,Price,Quantity,ReorderLevel,ManufacturingDate,ImageUrl"+Environment.NewLine);
                buffer.Append($"{item.Id},{item.Name},{item.Price},{item.Quantity},{item.ReorderLevel},{item.ManufacturingDate},{item.ImageUrl}");
            }
            else if (context.Object is IEnumerable<CatalogItem>)
            {
                var itemArr = context.Object as IEnumerable<CatalogItem>;
                buffer.Append("Id,Name,Price,Quantity,ReorderLevel,ManufacturingDate,ImageUrl"+Environment.NewLine);
                foreach (var item in itemArr)
                {
                    buffer.Append($"{item.Id}, {item.Name}, {item.Price}, {item.Quantity}, {item.ReorderLevel}, {item.ManufacturingDate}, {item.ImageUrl}{Environment.NewLine}");
                }
            }
            await response.WriteAsync(buffer.ToString(), selectedEncoding);
        }
    }
}
