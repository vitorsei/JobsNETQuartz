using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace Quartz.Presentation.Modules
{
    public class JobConfiguration
    {
        public void Edit(string name, int interval, TimeSpan firstExecution, 
            TimeSpan lastExecution, IDictionary<string, string> parameters)
        {
            var serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(new
            {
                Name = name,
                Interval = interval,
                FirstRun = firstExecution,
                LastRun = lastExecution,
                Parameters = parameters
            });

            string path = GetPath(name);
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.Write(json);
            }
        }

        public IDictionary<string, object> Get(string name)
        {
            string path = GetPath(name);

            using (StreamReader sw = new StreamReader(path))
            {
                string file = sw.ReadToEnd();
                var config = new JavaScriptSerializer().Deserialize<IDictionary<string, object>>(file);

                return config;
            }
        }

        private string GetPath(string name)
        {
            return HttpContext.Current.Server.MapPath("~/App_Data/Jobs/" + name + ".json");
        }
    }
}