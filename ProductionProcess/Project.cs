using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ProductionProcess
{
    public class Project
    {
        public string Name { get; set; }

        public List <Abilitie> Abilities { get; set; }
    }
    public class Abilitie
    {
        public string Name { get; set; }

        public int Duration { get; set; }

        [YamlIgnore]
        public Recipe Recipe { get; set; }
    }
}
