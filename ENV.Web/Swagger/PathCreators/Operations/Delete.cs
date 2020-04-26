using Swashbuckle.Swagger;

using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{
    /// <summary>
    /// Describes the operation to delete a record
    /// </summary>
    class Delete : IOperationWithIdCreator
    {
        public Operation Create(string entityName, Parameter idParameter, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Operation();
            
            ret.summary = $"Deletes a {entityName}";

            ret.responses = new Dictionary<string, Response>{
                { "200", new Response() { description = "Success "}}
                };


            ret.parameters = new List<Parameter> {idParameter };

            return ret;
        }

       
    }
}
