using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENV.Web.Swagger
{
    static class SwaggerUtils
    {
        internal static Schema CreateSchema(DataList dl)
        {
            var ret = new Schema();
            ret.type = "object";
            ret.required = GetRequiredDataItems(dl).ToList();
            ret.properties = CreateProperties(dl);

            return ret;
        }
        internal static  IEnumerable<string> GetRequiredDataItems(DataList l)
        {
            return l.Where(x => x["Required"].Bool).Select(y => y["Key"].Text.ToString());
        }

        internal static Parameter CreateIdParameter(DataApi dataApi)
        {
            return new Parameter
            {
                name = dataApi.IdParameterName,
                type = "integer",
                @in = "path",
                schema =new Schema() { type = "integer" },
                format = "int64",
                required = true
            };
        }

        internal static IDictionary<string, Schema> CreateProperties(DataList dl, bool includeReadOnly = false)
        {
            var ret = new Dictionary<string, Schema>();

            foreach (var col in dl)
            {
                if (includeReadOnly || !col["Readonly"].Bool)
                {
                    var v = col["Key"].Text;
                    var t = col["Type"].Text;
                    var c = col["Caption"].Text;
                    var ro = col["Readonly"].Bool;

                    ret.Add(v, new Schema { title = v, type = t, readOnly = ro, description = c });
                }
            }
            return ret;

        }
    }
}
