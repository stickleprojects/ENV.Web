using Swashbuckle.Swagger;

using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{
    /// <summary>
    /// Describes the operation to list all records in the table
    /// </summary>
    class List : IOperationCreator
    {
       public Operation Create(string entityName, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            return  new Operation()
            {
                description = $"Lists all the {entityName}",
               
                summary = "Lists all the records",
                responses = createListResponse(controller, vm, globalSchemas)

            };
        }
        private IDictionary<string, Response> createListResponse(KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas)
        {
            var ret = new Dictionary<string, Response>();
            ret.Add("200", createListSuccessResponse(controller, vm, globalSchemas));

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
                var recordSchema = SwaggerUtils.CreateSchema(dl);

                globalSchemas.Add(entity, recordSchema);
            }

            var schema = new Schema();
            schema.type = "array";


            var ret = new Response();
            ret.schema = schema;
            ret.description = "Success";
            schema.items = new Schema() { @ref = schemaKey, title = entity };

            return ret;
        }
    }
}
