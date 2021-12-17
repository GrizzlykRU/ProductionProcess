using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;

namespace ProductionProcess
{
    class YamlContent
    {
        public List <Product> Products { get; set; }

        public List <Recipe> Recipes { get; set; }

        public List <Project> Projects { get; set; }

        public List <Building> Buildings { get; set; }

        public static YamlContent ParseYaml(string filename)
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
            YamlContent output = new YamlContent();
            using (TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\" + filename))
            {
                output = deserializer.Deserialize<YamlContent>(reader);
            }
            return output;
        }
    }
    
}
