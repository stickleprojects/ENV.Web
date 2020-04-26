using Swashbuckle.Swagger;

using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{
    /// <summary>
    /// Describes the operation to update an existing record
    /// </summary>
    class Put : IOperationWithIdCreator
    {
        public Operation Create(string entityName, Parameter idParameter, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            return new Operation()
            {
                parameters = getParameters(vm),
                
                summary=$"Updates a {entityName}"
            };
        }

        private IList<Parameter> getParameters(ViewModel vm)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);
            
            var ret = new List<Parameter>();

            var bodyParam = new Parameter() { @in = "body", description = "the record to update" };
            bodyParam.schema = SwaggerUtils.CreateSchema(dl);

            ret.Add(bodyParam);

            return ret;
        }
    }
}
