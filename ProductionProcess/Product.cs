using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace ProductionProcess
{
    public class Product
    {
        public Product(){}
        public Product(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
        [YamlMember(Alias = "id")]
        public string Name { get; set; }

        [YamlMember(Alias = "num")]
        public int Quantity { get; set; }
    }
}
