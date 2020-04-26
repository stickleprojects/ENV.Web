using ENV.Web.Swagger.PathCreators.Operations;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators
{
    public class PathWithoutIDCreator
    {
        private DataApi _dataApi;

        private IDictionary<string, IOperationCreator> _cachedOperations = new Dictionary<string, IOperationCreator> {
                {"get", new List()},
                {"post", new Post()},
            };

        public PathWithoutIDCreator(DataApi dataApi)
        {
            _dataApi = dataApi;
        }

        internal PathItem Create(string entityName, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            // add LIST + ADD/post operations
            var ret = new PathItem();
            var tableName = vm.From.EntityName.Replace("dbo.", "");

            Func<string, Operation> operationCaller = (tgtName) => _cachedOperations[tgtName].Create(entityName, controller, vm, globalSchemas);

            if (vm.AllowRead)
            {
                ret.get = operationCaller("get");
            }
            if (vm.AllowInsert)
            {
                ret.post = operationCaller("post");
            }

            return ret;

        }

    }
}
