using System.Collections.Generic;

namespace CORE.MVC
{
    public class JsonConfig
    {
        public Dictionary<string, JsonConnetion> Connetions { get; set; }
    }
    public class JsonConnetion
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
