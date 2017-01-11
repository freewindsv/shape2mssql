using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shape2mssql.Entities
{
    public class Configuration
    {
        public Configuration()
        {
            Srid = int.Parse(ConfigurationManager.AppSettings["srid"]);
            KeyFieldName = ConfigurationManager.AppSettings["keyFieldName"];
            SourceEncoding = Encoding.GetEncoding(ConfigurationManager.AppSettings["SourceEncoding"]);
            DestEncoding = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DestEncoding"]) ? null :
                Encoding.GetEncoding(ConfigurationManager.AppSettings["DestEncoding"]);
        }

        public string FileName { get; set; }
        public int Srid { get; set; }
        public string KeyFieldName { get; set; }
        public Encoding SourceEncoding { get; set; }
        public Encoding DestEncoding { get; set; }
    }
}
