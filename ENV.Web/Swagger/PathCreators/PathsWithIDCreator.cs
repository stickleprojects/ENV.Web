using ENV.Web.Swagger.PathCreators.Operations;

using Swashbuckle.Swagger;

using System;
using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators
{
    class PathWithIDCreator
    {
        private readonly DataApi _dataApi;

        private IDictionary<string, IOperationWithIdCreator> _cachedOperations = new Dictionary<string, IOperationWithIdCreator> {
            {"get",new Get()},
            {"delete", new Delete() },
            {"put", new Put() }
            };

        public PathWithIDCreator(DataApi dataApi)
        {
            this._dataApi = dataApi;
        }

        internal PathItem Create(string entityName, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            // add CRUD operations
            var ret = new PathItem();
            var idparameter = SwaggerUtils.CreateIdParameter(_dataApi);

            Func<string, Operation> operationCaller = (tgt) => _cachedOperations[tgt].Create(entityName, idparameter, controller, vm, globalSchemas);

            if (vm.AllowRead) ret.get = operationCaller("get");

            if (vm.AllowUpdate) ret.put = operationCaller("put");

            if (vm.AllowDelete) ret.delete = operationCaller("delete");
            
            return ret;
        }

    }
}