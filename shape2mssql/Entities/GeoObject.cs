using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shape2mssql.Entities
{
    public class GeoObject
    {
        public IDictionary<string, object> SemanticInfo { get; set; }
        public SqlGeography Geography { get; set; }
    }
}
