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
                description="Adds a new thingy to the whatsit",
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

        private IDictionary<string, Schema> createProperties(DataList dl, bool includeReadOnly = false )
        {
            var ret = new Dictionary<string, Schema>();

            foreach (var col in dl)
            {
                if (includeReadOnly || !col["Readonly"].Bool)
                {
                    var v = col["Key"].Text;
                    var t = col["Type"].Text;
                    var c = col["Caption"].Text;
                    var ro = col["Readonly"].Bool;

                    ret.Add(v, new Schema { title = v, type = t,readOnly=ro, description = c });
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
                @in = "path",
                schema = createSchema("integer"),
                format ="int64",
                required =true };
        }
        private KeyValuePair<string, PathItem> createDeletePathitem(string entityName, ViewModel vm)
        {
            var op = createDeleteOperation(vm);
            var pi = new PathItem()
            {
                delete = op
            };
            var url = createUrl(entityName); ;

            return new KeyValuePair<string, PathItem>(url, pi);

        }

        private string createUrl(string entityName)
        {
            return $"/{_dataApi.ApiParameterName}/{entityName}";
        }
       
        private Operation createDeleteOperation(ViewModel vm)
        {
            var ret = new Operation();
            
            ret.summary = "Deletes a record";
           
            ret.responses = new Dictionary<string, Response>{
                { "200", new Response() { description = "Success "}}
                };


            ret.parameters = new List<Parameter> { createIDParameter() };
            
            return ret;
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
        
        private void updateAllOperationIds(SwaggerDocument doc)
        {
            foreach (var p in doc.paths)
            {
                var pathItem = p.Value;

                if(pathItem.patch!=null)
                {
                    pathItem.patch.operationId = p.Key + "_patchById";
                }
                if(pathItem.post!=null)
                {
                    pathItem.post.operationId = p.Key + "_post";
                }
                if(pathItem.put!=null)
                {
                    pathItem.put.operationId = p.Key + "_putById";
                }
                if (pathItem.get != null) {
                    pathItem.get.operationId = p.Key + "_get";
                }
                if (pathItem.delete!=null)
                {
                    pathItem.delete.operationId = p.Key + "_delete";
                }
            }
        }
        public SwaggerDocument CreateDoc()
        {
            var doc = createEmptyDocument();
            var controllers = _dataApi.GetControllers();

         
            var globalSchemas = new Dictionary<string, Schema>();

            doc.paths = createPaths(controllers,globalSchemas );
            doc.definitions = globalSchemas;

            updateAllOperationIds(doc);
            return doc;
        }

        private IDictionary<string, PathItem> createPaths(IDictionary<string, DataApi.ApiItem> controllers, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, PathItem>();

            foreach(var controller in controllers)
            {
                var paths = createPaths(controller,  globalSchemas);
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

        private IDictionary<string, PathItem> createPaths(KeyValuePair<string, DataApi.ApiItem> controller, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, PathItem>();

            var vm = controller.Value.Create();

            var entityName = controller.Key;
            var tableName = vm.From.EntityName.Replace("dbo.", "");
            var url = $"/{entityName}";

            // add LIST operation
            if (vm.AllowRead)
            {
                var listPi = new PathItem()
                {
                    
                    get = new Operation()
                    {
                        description = $"Lists all the {tableName}",
                        operationId = $"LIST{entityName}",
                        summary = "Lists all the records",
                        responses = createListResponse(controller, vm, globalSchemas)

                    }
                };

                if(vm.AllowInsert)
                {


                    listPi.post = createAddOperation(vm);
                    

                }
                ret.Add(url, listPi);
            }

            // add CRUD operations
            var pi = new PathItem();
            url = url + "/{id}";

            if (vm.AllowRead)
            {
                pi.get = createGetOperation(controller, vm, globalSchemas);

            }
            if (vm.AllowDelete)
            {
                pi.delete = createDeleteOperation(vm);
            }
            ret.Add(url, pi);
            //var p = createPathItem(controller.Key, controller.Value );

            //if (vm.AllowDelete)
            //{

            //    ret.Add(p.Key, p.Value);
            //}
          

            return ret;
        }

        private Operation createGetOperation(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Operation();
            ret.description = "Retrieves a record";
            ret.parameters = new List<Parameter> { createIDParameter()};
            ret.summary = "Gets a record";

            ret.responses = new Dictionary<string, Response>
            {
                { "200", createGetSuccessResponse(controller, vm, globalSchemas) }
            };

            return ret;
        }

       

        private IDictionary<string, Response> createListResponse(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, Response>();
            ret.Add("200",createListSuccessResponse(controller, vm, globalSchemas) );
            
            return ret;
        }
        private Response createGetSuccessResponse(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);

            // create a schema of the record
            var entity = controller.Key;
            var schemaKey = $"#/definitions/{entity}";
            var recordSchema = createSchema(dl);

            if (!globalSchemas.ContainsKey(entity))
            {
               
                globalSchemas.Add(entity, recordSchema);
            }

          

            var ret = new Response();
            
            ret.schema = new Schema() { title = entity, @ref = schemaKey };
            ret.description = "Success";
           
            return ret;
        }
        private Response createListSuccessResponse(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);

            // create a schema of the record
            var entity = controller.Key;
            var schemaKey = $"#/definitions/{entity}";
            if (!globalSchemas.ContainsKey(entity))
            {
                var recordSchema = createSchema(dl);
               
                globalSchemas.Add(entity, recordSchema);
            }
            
            var schema = new Schema();
            schema.type = "array";
           
          
            var ret = new Response();
            ret.schema = schema;
            ret.description = "Success";
            schema.items = new Schema() { @ref = schemaKey, title=entity };
            
            return ret;
        }

        private KeyValuePair<string, PathItem> createInsertPathItem(string entityName, ViewModel vm)
        {
            var url = createUrl(entityName);
            var pi = new PathItem()
            {
                post = createAddOperation(vm)
            };

            return new KeyValuePair<string, PathItem>(url, pi);

        }

        private SwaggerDocument createEmptyDocument()
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
            
            // see https://swagger.io/docs/specification/2-0/api-host-and-base-path/
            ret.host = "localhost:56557";
            ret.basePath = "/dataApi";
            ret.externalDocs = new ExternalDocs() { description = "More details and sample code can be found here:", url = $"http://{ret.host}{ret.basePath}" };

            return ret;
        }
    }
}
