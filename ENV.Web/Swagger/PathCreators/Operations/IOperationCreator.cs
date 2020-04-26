using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace ENV.Web.Swagger.PathCreators.Operations
{
    /// <summary>
    /// Interface for the classes that create Operations that require the record identifier (ie delete and update)
    /// </summary>
    /// 
    interface IOperationWithIdCreator
    {
         Operation Create(string entityName, Parameter idParameter, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas);

    }

    /// <summary>
    /// Interface for the classes that create Operations that do not require the record identifier (ie add and list)
    /// </summary>
    interface IOperationCreator
    {
        Operation Create(string entityName, KeyValuePair<string, DataApi.ApiItem> controller, ViewModel vm, IDictionary<string, Schema> globalSchemas);

    }
}
