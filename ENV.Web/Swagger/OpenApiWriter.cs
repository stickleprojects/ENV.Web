using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace ENV.Web
{
    /// <summary>
    /// Class to ensure datetimeoffsets are written correctly https://github.com/Microsoft/OpenAPI.NET/issues/291
    /// </summary>
    class MyOpenApiJsonWriter : OpenApiJsonWriter
    {
        public MyOpenApiJsonWriter(TextWriter w):base(w)
        {

        }

        public override void WriteValue(DateTimeOffset value)
        {
            WriteValue(value.ToString("o"));
        }
    }
    class OpenApiWriter 
    {
       

        public string CreateDoc(IDictionary<string, DataApi.ApiItem> items)
        {
            var doc = createDocument();


            doc.Paths = createPaths(items);
            var sw = new StringWriter();
            var w = new MyOpenApiJsonWriter(sw);

                doc.SerializeAsV2(w);


            return sw.ToString();

        }

        private OpenApiPaths createPaths(IDictionary<string, DataApi.ApiItem> items)
        {
            var ret = new OpenApiPaths();

            foreach(var i in items)
            {
                var path = i.Key;
                ret.Add(path, createOpenApiPathItem(i.Value));
            }
            return ret;
        }

        private OpenApiPathItem createOpenApiPathItem(DataApi.ApiItem value)
        {
            var ret = new OpenApiPathItem();
            ret.Description = string.Empty;
            

            return ret;
        }

        private string getApplicationVersion() { return "1.0.0"; }
        private string getApplicationTitle() { return "webapp"; }
        private OpenApiDocument createDocument()
        {
            return new OpenApiDocument()
            {
                Info = new OpenApiInfo
                {
                    Version = getApplicationVersion(),
                    Title = getApplicationTitle()
                }
            };
        }

       
    }
}
