using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AkemiSelfBot
{
    public class JsonConfig
    {
        public string Token { get; set; }

        private readonly string saveFile = string.Format("{0}/{1}", Environment.CurrentDirectory, "saveFile.json");

        public void Save()
        {
            using(StreamWriter writer = new StreamWriter(saveFile))
            {
                string json = JsonConvert.SerializeObject(this);
                Console.WriteLine("json: " + json);
                writer.Write(json);
                writer.Close();
            }
        }

        public void Load()
        {
            if (!File.Exists(saveFile))
            {
                Save();
                return;
            }
            using(StreamReader reader = new StreamReader(saveFile))
            {
                JsonConfig config = JsonConvert.DeserializeObject<JsonConfig>(reader.ReadToEnd());
                Token = config.Token;
                reader.Close();
            }
        }
    }
}
