using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using Swashbuckle.Swagger;

namespace ENV.Web
{
    class SwaggerProvider : ISwaggerProvider
    {
        private readonly ISwaggerProvider defaultProvider;
        private readonly Func<DataApi> _getRegisteredDataApiInstance;
     
        public SwaggerProvider(ISwaggerProvider defaultProvider, Func<DataApi> getRegisteredDataApiInstance)
        {
            this.defaultProvider = defaultProvider;
           _getRegisteredDataApiInstance = getRegisteredDataApiInstance;
        }
        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
           
            var dataApi = _getRegisteredDataApiInstance();
            var creator = new SwaggerDocumentCreator(dataApi);


            var doc = creator.CreateDoc();
                        return doc;
        }
    }
  
    internal class SwaggerDocumentCreator
    {
        private readonly DataApi _dataApi;

        public SwaggerDocumentCreator(DataApi dataApi)
        {
            this._dataApi = dataApi;
        }
       

        private Operation createAddOperation(ViewModel vm)
        {
            return new Operation()
            {
                summary = "Adds a record",
                parameters = getParameters(vm)
            };
        }

        private IList<Parameter> getParameters(ViewModel vm)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);
            var ret = new List<Parameter>();

            var bodyParam = new Parameter() { @in = "body", description = "the record to create" };
            bodyParam.schema = createSchema(dl);

            ret.Add(bodyParam);

            return ret;
        }
        private IEnumerable<string> getRequiredDataItems(DataList l)
        {
            return l.Where(x => x["Required"].Bool).Select(y => y["Key"].Text.ToString());
        }
        private Schema createSchema(DataList dl)
        {
            var ret = new Schema();
            ret.type = "object";
            ret.required = getRequiredDataItems(dl).ToList();
            ret.properties = createProperties(dl);

            return ret;
        }

        private IDictionary<string, Schema> createProperties(DataList dl)
        {
            var ret = new Dictionary<string, Schema>();

            foreach (var col in dl)
            {
                if (!col["Readonly"].Bool)
                {
                    var v = col["Key"].Text;
                    var t = col["Type"].Text;
                    var c = col["Caption"].Text;
                    var pt = "body";
                    ret.Add(v, new Schema { title = v, type = t, description = c });
                }
            }
                return ret;

        }

        private Schema createSchema(string type)
        {
            return new Schema()
            {
                type = type
            };
        }
        private Parameter createIDParameter()
        {
            return new Parameter { name = _dataApi.IdParameterName,
                type = "integer",
                @in = "query",
                schema = createSchema("integer"),
                format ="int64",
                required =true };
        }
        private KeyValuePair<string, PathItem> createDeletePathitem( ViewModel vm)
        {
            var op = createDeleteOperation(vm);
            var pi = new PathItem()
            {
                delete = op
            };
            var url = createUrl(vm); ;

            return new KeyValuePair<string, PathItem>(url, pi);

        }

        private string createUrl(ViewModel vm)
        {
            return $"/{_dataApi.ApiParameterName}/{vm.From.EntityName.ToLower()}";
        }

        private Operation createDeleteOperation(ViewModel vm)
        {
            var ret = new Operation();
            
            ret.summary = "Deletes a record";
            ret.operationId = $"{_dataApi.ApiParameterName}{vm.From.EntityName}Delete";
            ret.responses = new Dictionary<string, Response>{
                { "200", new Response() { description = "Success "}}
                };


            ret.parameters = new List<Parameter> { createIDParameter() };
            
            return ret;
        }

        private string createUrl(KeyValuePair<string, DataApi.ApiItem> src)
        {
            return $"/{_dataApi.ApiParameterName}/{src.Key}";
        }
        private PathItem createPathItem(string name, DataApi.ApiItem src)
        {
            var ret = new PathItem();
            var vm = src.Create();
            
            if (vm.AllowDelete)
            {
                ret.delete = createDeleteOperation(vm);
            }
            if (vm.AllowInsert)
            {
                ret.post = createAddOperation(vm);
            }
        
            return ret;
        }
        public SwaggerDocument CreateDoc()
        {
            var doc = createDocument();
            var controllers = _dataApi.GetControllers();


            doc.paths = createPaths(controllers);
            return doc;
        }

        private IDictionary<string, PathItem> createPaths(IDictionary<string, DataApi.ApiItem> controllers)
        {
            var ret = new Dictionary<string, PathItem>();

            foreach(var controller in controllers)
            {
                var paths = createPaths(controller);
                if (paths!=null)
                {
                    foreach(var p in paths)
                    {
                        ret.Add(p.Key, p.Value);
                    }
                }
            }
            
            return ret;
        }

        private IDictionary<string, PathItem> createPaths(KeyValuePair<string, DataApi.ApiItem> controller)
        {
            var ret = new Dictionary<string, PathItem>();

            var vm = controller.Value.Create();

            var entityName = vm.From.EntityName.Replace("dbo.", "");

            // add the list path
            var pi = new PathItem()
            {
                get = new Operation()
                {
                    description = $"Lists all the {entityName}",
                    operationId = $"LIST{entityName}",
                    summary = "Lists all the records"

                }
            };
            var url = $"/{entityName}";
            
            if (vm.AllowInsert)
            {
                pi.post = createAddOperation(vm);
            }
            
            ret.Add(url, pi);


            //var p = createPathItem(controller.Key, controller.Value );

            //if (vm.AllowDelete)
            //{

            //    ret.Add(p.Key, p.Value);
            //}
            //if (vm.AllowInsert)
            //{
            //    var p = createInsertPathItem(vm);
            //    if (!ret.ContainsKey(p.Key))
            //    {
            //        ret.Add(p.Key, p.Value);
            //    }
            //}

            return ret;
        }

        private KeyValuePair<string, PathItem> createInsertPathItem(ViewModel vm)
        {
            var url = createUrl(vm);
            var pi = new PathItem()
            {
                post = createAddOperation(vm)
            };

            return new KeyValuePair<string, PathItem>(url, pi);

        }

        private SwaggerDocument createDocument()
        {
            var ret = new SwaggerDocument();
            ret.info = new Info() {
                version = "v1",
                title = "the web app",
                contact =new Contact() { name="contact name"},
                description = "First swagger wepapp"
            };
            ret.tags = new List<Tag>
            {
                new Tag() { name="todo"}
            };

            ret.host = "localhost:56557";
            ret.basePath = "/dataApi";
            
            return ret;
        }
    }
}
