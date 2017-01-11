using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catfood.Shapefile;
using shape2mssql.Entities;

namespace shape2mssql.Adapter.Ext
{
    public static class ExtConvertor
    {
        public static Point ToPoint(this PointD point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point[] ToPoints(this IEnumerable<PointD> points)
        {
            return points.Select(p => p.ToPoint()).ToArray();
        }

        public static Point[][] ToMultiPoints(this IEnumerable<IEnumerable<PointD>> multiPoints)
        {
            return multiPoints.Select(p => p.ToPoints().ToArray()).ToArray();
        }
    }
}
