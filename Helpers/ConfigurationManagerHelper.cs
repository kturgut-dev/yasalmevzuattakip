using KYS.YasalMevzuatTakip.Models;
using Newtonsoft.Json;
using System.IO;

namespace KYS.YasalMevzuatTakip.Helpers
{
    public class ConfigurationManagerHelper
    {
        public static ProjectSetting GetProjectSetting()
        {
            JsonSerializer serializer = new JsonSerializer();
            ProjectSetting res = new ProjectSetting();
            string runPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            using (FileStream s = File.Open(runPath + @"/appsettings.json", FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        res = serializer.Deserialize<ProjectSetting>(reader);
                    }
                }

            return res;
        }
    }
}
