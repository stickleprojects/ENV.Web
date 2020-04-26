using Swashbuckle.Swagger;

using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{
    /// <summary>
    /// Describes the operaiton to add a new record to a table
    /// </summary>
    class Post : IOperationCreator
    {
        public Operation Create (string entityName, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            return new Operation()
            {
                summary = $"Adds a {entityName}",
                
                parameters = getParameters(vm)
            };
        }
        private IList<Parameter> getParameters(ViewModel vm)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);
            var ret = new List<Parameter>();

            var bodyParam = new Parameter() { @in = "body", description = "the record to create" };
            bodyParam.schema = SwaggerUtils.CreateSchema(dl);

            ret.Add(bodyParam);

            return ret;
        }

    }

}
