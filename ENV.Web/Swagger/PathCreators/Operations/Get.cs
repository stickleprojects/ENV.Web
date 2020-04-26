using Swashbuckle.Swagger;

using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{

    /// <summary>
    /// Describes the operation to get a single record by id
    /// </summary>
    class Get: IOperationWithIdCreator
    {
       
        public  Operation Create(string entityName, Parameter idParameter, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Operation();
           
            ret.parameters = new List<Parameter> { idParameter };
            ret.summary = $"Gets a {entityName}";

            ret.responses = new Dictionary<string, Response>
            {
                { "200", createGetSuccessResponse(controller, vm, globalSchemas) }
            };

            return ret;
        }


        private Response createGetSuccessResponse(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var dl = new DataList();
            vm.ProvideMembersTo(dl);

            // create a schema of the record
            var entity = controller.Key;
            var schemaKey = $"#/definitions/{entity}";
            var recordSchema = SwaggerUtils.CreateSchema(dl);

            if (!globalSchemas.ContainsKey(entity))
            {

                globalSchemas.Add(entity, recordSchema);
            }



            var ret = new Response();

            ret.schema = new Schema() { title = entity, @ref = schemaKey };
            ret.description = "Success";

            return ret;
        }
    }
}
