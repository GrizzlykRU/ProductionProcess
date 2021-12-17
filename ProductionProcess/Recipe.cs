using System.Collections.Generic;

namespace ProductionProcess
{
    public class Recipe
    {
        public string Name { get; set; }

        public List <Product>  Components  { get; set; }

        public Product Product { get; set; }
    }
}
