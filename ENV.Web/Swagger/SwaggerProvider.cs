using System;
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


            var doc = creator.Create();
            return doc;
        }
    }
}