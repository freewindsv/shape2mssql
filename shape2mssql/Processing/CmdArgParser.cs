using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shape2mssql.Entities;
using System.IO;

namespace shape2mssql.Processing
{
    public static class CmdArgParser
    {
        public static Configuration GetConfiguration(string[] args)
        {
            Configuration config = new Configuration();

            string fileName;
            if (args.Length > 0)
            {
                fileName = args[0];
                if (!File.Exists(fileName))
                {
                    throw new ArgumentException("Shapefile not found");
                }
                config.FileName = fileName;
            }

            if (args.Length > 1)
            {
                int srid;
                if (int.TryParse(args[1], out srid))
                {
                    config.Srid = srid;
                }
                else
                {
                    throw new ArgumentException("Error parsing srid");
                }
            }

            if (args.Length > 2)
            {
                config.KeyFieldName = args[2];
            }

            if (args.Length > 3)
            {
                try
                {
                    config.DestEncoding = Encoding.GetEncoding(args[3]);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("Error parsing ecoding param", ex);
                }
            }

            return config;
        }
    }
}
