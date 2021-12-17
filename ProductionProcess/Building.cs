using YamlDotNet.Serialization;

namespace ProductionProcess
{
    public class Building
    {
        public string Name { get; set; }

        [YamlMember(Alias = "project")]
        public string ProjectName { get; set; }

        [YamlIgnore]
        public Project Project { get; set; }
    }
}
