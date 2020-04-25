using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ENV.Web.Tests
{
    [TestClass]
   public class TestOpenApiWriter
    {
        [TestMethod]
        public void AllKeysShouldBeLowercase()
        {
            var svc = new OpenApiWriter();

            var items = new Dictionary<string, ENV.Web.DataApi.ApiItem>();

            var actual = svc.CreateDoc(items);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(string.Empty, actual);

            var json = (JObject)JsonConvert.DeserializeObject(actual);
            foreach(var item in json)
            {
                Assert.AreEqual(item.Key.ToLower(), item.Key);
                
            }
        }
        [TestMethod]
        public void VersionShouldBe2()
        {
            var svc = new OpenApiWriter();

            var items = new Dictionary<string, ENV.Web.DataApi.ApiItem>();

            var actual = svc.CreateDoc(items);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(string.Empty, actual);

            var json =(JObject) JsonConvert.DeserializeObject(actual);
            Assert.AreEqual("2.0", json["swagger"]);
        }
        [TestMethod]
        public void IteratesAllPaths()
        {

            var svc = new OpenApiWriter();
            var paths_and_names = new Dictionary<string, string>
            {
                { "/", "root" },
                { "/customers", "customerController" }
            };

            var items = paths_and_names.ToDictionary(p => p.Key, v => new DataApi.ApiItem(v.Value , null));
                        
            var actual = svc.CreateDoc(items);

            var json = (JObject)JsonConvert.DeserializeObject(actual);
            var paths =(JObject) json["paths"];
            Assert.IsNotNull(paths);
            Assert.AreEqual(JTokenType.Object, paths.Type);
            var d = paths.Properties().Select(x => x.Name).ToList();
            
            CollectionAssert.AreEqual(paths_and_names.Keys.ToArray(), d);

            
        }
    }
}
