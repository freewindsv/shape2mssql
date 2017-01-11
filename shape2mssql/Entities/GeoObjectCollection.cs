using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shape2mssql.Entities
{
    public class GeoObjectCollection
    {
        public GeoObjectCollection(string name)
        {
            GeoObjects = new List<GeoObject>();
            TypeInfos = new Dictionary<string, TypeInfo>();
            Name = name;
        }

        public string Name { get; private set; }
        public IList<GeoObject> GeoObjects { get; set; }
        public IDictionary<string, TypeInfo> TypeInfos { get; private set; }

        public void Add(GeoObject obj)
        {
            GeoObjects.Add(obj);
        }

        public void AddMetaDataInfo(string name, TypeInfo info)
        {
            if (TypeInfos.ContainsKey(name))
            {
                if (info.Type == TypeInfos[name].Type && info.Length > TypeInfos[name].Length)
                {
                    TypeInfos[name].Length = info.Length;
                }
            }
            else
            {
                TypeInfos.Add(name, info);
            }
        }
    }
}
