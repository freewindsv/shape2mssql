using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

using Catfood.Shapefile;
using shape2mssql.Processing;
using shape2mssql.Entities;
using shape2mssql.Adapter.Ext;

namespace shape2mssql
{
    class Program
    {
        static void Main(string[] args)
        {
            shape2mssql.Entities.Configuration config = null;
            try
            {
                config = CmdArgParser.GetConfiguration(args);
            }
            catch (Exception ex)
            {
                Exit(ex.Message, 1);
            }

            if (args.Length == 0)
            {
                string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.shp");
                if (files.Length == 0)
                {
                    Exit("No files to process", 0);
                }
                foreach (string file in files)
                {
                    config.FileName = file;
                    ProcessShapeFile(config);
                }
            }
            else
            {
                ProcessShapeFile(config);
            }

            Exit("All done!", 0);
        }

        static void Exit(string message, int code)
        {
            Console.WriteLine(message);
            Console.ReadLine();
            Environment.Exit(code);
        }

        static void ProcessShapeFile(shape2mssql.Entities.Configuration config)
        {
            SqlGeographyBuilder sqlGeoBuilder = new SqlGeographyBuilder(config.Srid);
            GeoObjectCollection geoObjectCollection = new GeoObjectCollection(Path.GetFileNameWithoutExtension(config.FileName));

            using (Shapefile shapefile = new Shapefile(Path.GetFullPath(config.FileName)))
            {
                Console.WriteLine("Reading shapefile {0}", config.FileName);

                Console.WriteLine("Shapes count: {0}", shapefile.Count);

                foreach (Shape shape in shapefile)
                {
                    Console.Write("\rProcessing shape {0}", shape.RecordNumber);

                    GeoObject geoObject = new GeoObject();

                    // each shape may have associated metadata
                    string[] metadataNames = shape.GetMetadataNames();
                    if (metadataNames != null)
                    {
                        geoObject.SemanticInfo = new Dictionary<string, object>();
                        foreach (string metadataName in metadataNames)
                        {
                            string dataType = shape.DataRecord.GetDataTypeName(shape.DataRecord.GetOrdinal(metadataName));
                            string strValue = shape.GetMetadata(metadataName);
                            object metadataValue = null;
                            TypeInfo typeInfo = null;
                            switch (dataType)
                            {
                                case "DBTYPE_R8":
                                    typeInfo = new TypeInfo() { Type = typeof(double) };
                                    metadataValue = string.IsNullOrEmpty(strValue) ? null : (object)double.Parse(strValue);
                                    break;
                                case "DBTYPE_I4":
                                    typeInfo = new TypeInfo() { Type = typeof(int) };
                                    metadataValue = string.IsNullOrEmpty(strValue) ? null : (object)int.Parse(strValue);
                                    break;
                                case "DBTYPE_WVARCHAR":
                                    if (config.DestEncoding != null && config.SourceEncoding != config.DestEncoding)
                                    {
                                        byte[] bytes = config.SourceEncoding.GetBytes(strValue);
                                        strValue = config.DestEncoding.GetString(bytes);
                                    }
                                    typeInfo = new TypeInfo() { Type = typeof(string), Length = strValue.Length };
                                    metadataValue = strValue;
                                    break;
                                default:
                                    //skip unknown type
                                    continue;
                            }
                            geoObject.SemanticInfo.Add(metadataName, metadataValue);
                            geoObjectCollection.AddMetaDataInfo(metadataName, typeInfo);
                        }
                    }

                    if (shape.Type == ShapeType.Point)
                    {
                        geoObject.Geography = sqlGeoBuilder.GetPoint((shape as ShapePoint).Point.ToPoint());
                    }
                    else if (shape.Type == ShapeType.MultiPoint)
                    {
                        geoObject.Geography = sqlGeoBuilder.GetMultiPoint((shape as ShapeMultiPoint).Points.ToPoints());
                    }
                    else if (shape.Type == ShapeType.PolyLine)
                    {
                        ShapePolyLine polyLine = shape as ShapePolyLine;
                        if (polyLine.Parts.Count == 1)
                        {
                            geoObject.Geography = sqlGeoBuilder.GetPolyLine(polyLine.Parts[0].ToPoints());
                        }
                        else if (polyLine.Parts.Count > 1)
                        {
                            geoObject.Geography = sqlGeoBuilder.GetMultiPolyLine(polyLine.Parts.ToMultiPoints());
                        }
                    }
                    else if (shape.Type == ShapeType.Polygon)
                    {
                        ShapePolygon polygon = shape as ShapePolygon;
                        if (polygon.Parts.Count == 1)
                        {
                            geoObject.Geography = sqlGeoBuilder.GetPolygon(polygon.Parts[0].ToPoints());
                        }
                        else if (polygon.Parts.Count > 1)
                        {
                            geoObject.Geography = sqlGeoBuilder.GetMultiPolygon(polygon.Parts.ToMultiPoints());
                        }
                    }
                    else
                    {
                        // and so on for other types...
                        Console.WriteLine("Unknown type {0}", shape.Type);
                    }

                    if (geoObject.Geography != null)
                    {
                        geoObjectCollection.Add(geoObject);
                    }
                }
                Console.WriteLine();

            }

            Console.WriteLine("Inserting data into database");
            SqlDataAccess dataAccess = new SqlDataAccess(ConfigurationManager.ConnectionStrings["SpatialDb"].ConnectionString, config.KeyFieldName);
            try
            {
                dataAccess.InsertGeoData(geoObjectCollection);
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting data. Exception: " + ex.Message);
            }
            Console.WriteLine("=================================");
            Console.WriteLine();
        }
    }    
}
