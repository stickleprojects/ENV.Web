using System.Collections.Generic;
using System.Linq;
using ENV.Web.Swagger.PathCreators;
using Swashbuckle.Swagger;

namespace ENV.Web
{
    internal class SwaggerDocumentCreator
    {
        private readonly DataApi _dataApi;

        public SwaggerDocumentCreator(DataApi dataApi)
        {
            this._dataApi = dataApi;
        }

        /// <summary>
        /// Assign uniqe paths for each operationid
        /// </summary>
        /// <param name="doc"></param>
        private void updateAllOperationIds(SwaggerDocument doc)
        {
            foreach (var p in doc.paths)
            {
                var pathItem = p.Value;

                if (pathItem.patch != null)
                {
                    pathItem.patch.operationId = p.Key + "_patchById";
                }
                if (pathItem.post != null)
                {
                    pathItem.post.operationId = p.Key + "_post";
                }
                if (pathItem.put != null)
                {
                    pathItem.put.operationId = p.Key + "_putById";
                }
                if (pathItem.get != null)
                {
                    pathItem.get.operationId = p.Key + "_get";
                }
                if (pathItem.delete != null)
                {
                    pathItem.delete.operationId = p.Key + "_delete";
                }
            }
        }

        /// <summary>
        /// Creates a new Swagger document based on the controllers in the dataapi instance
        /// </summary>
        /// <returns></returns>
        public SwaggerDocument Create()
        {
            var doc = createEmptyDocument();
            var controllers = _dataApi.GetControllers();

            // hold a list of all the schemas (class defs)
            var globalSchemas = new Dictionary<string, Schema>();

            // create the API paths
            doc.paths = createPaths(controllers, globalSchemas);

            // add the schemas (entity definitions)
            doc.definitions = globalSchemas;

            // assign unique operationids for each op
            updateAllOperationIds(doc);

            return doc;
        }

        /// <summary>
        /// Create the API endpoints for all controllers
        /// </summary>
        /// <param name="controllers"></param>
        /// <param name="globalSchemas"></param>
        /// <returns></returns>
        private IDictionary<string, PathItem> createPaths(IDictionary<string, DataApi.ApiItem> controllers, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, PathItem>();

            foreach (var controller in controllers)
            {
                var paths = createPaths(controller, globalSchemas);
                if (paths != null)
                {
                    foreach (var p in paths)
                    {
                        ret.Add(p.Key, p.Value);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Create the api end-points for a given controller
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="globalSchemas"></param>
        /// <returns></returns>
        private IDictionary<string, PathItem> createPaths(KeyValuePair<string, DataApi.ApiItem> controller, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, PathItem>();

            var vm = controller.Value.Create();

            var entityName = controller.Key;
            var tableName = vm.From.EntityName.Replace("dbo.", "");
            var url = $"/{entityName}";

            var p = new PathWithoutIDCreator(_dataApi).Create(entityName, controller, vm, globalSchemas);
            if (p != null)
            {
                ret.Add(url, p);

            }
            p = new PathWithIDCreator(_dataApi).Create(entityName, controller, vm, globalSchemas);
            if (p != null)
            {
                url = url + "/{id}";
                ret.Add(url, p);
            }
            

            return ret;
        }


        private SwaggerDocument createEmptyDocument()
        {
            var ret = new SwaggerDocument();
            ret.info = new Info()
            {
                version = "v1",
                title = "the web app",
                contact = new Contact() { name = "contact name" },
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