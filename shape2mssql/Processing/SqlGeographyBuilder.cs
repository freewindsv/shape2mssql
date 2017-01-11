using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.SqlTypes;
using System.Globalization;

using shape2mssql.Entities;

namespace shape2mssql.Processing
{
    public class SqlGeographyBuilder
    {
        private int srid;
        private NumberFormatInfo nfi;

        public SqlGeographyBuilder(int srid)
        {
            this.nfi = new NumberFormatInfo() { CurrencyDecimalSeparator = "." };
            this.srid = srid;
        }

        public SqlGeography GetPoint(Point point)
        {
            return SqlGeography.Point(point.Y, point.X, srid);
        }

        public SqlGeography GetMultiPoint(Point[] points)
        {
            StringBuilder sb = new StringBuilder("MULTIPOINT (");
            for (int i = 0; i < points.Length; i++)
            {
                appendPointSegment(sb, points[i]);
                if (i < points.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return SqlGeography.STMPointFromText(new SqlChars(new SqlString(sb.ToString())), srid);
        }

        public SqlGeography GetPolygon(Point[] points)
        {
            StringBuilder sb = new StringBuilder("POLYGON ");
            sb.Append("(");
            appendPointsSegment(sb, points);
            sb.Append(")");
            SqlGeography polygon = SqlGeography.STPolyFromText(new SqlChars(new SqlString(sb.ToString())), srid).MakeValid();
            return getProperPolygon(polygon);
        }

        public SqlGeography GetMultiPolygon(Point[][] multiPoints)
        {
            StringBuilder sb = new StringBuilder("MULTIPOLYGON (");
            for (int i = 0; i < multiPoints.Length; i++)
            {
                sb.Append("(");
                appendPointsSegment(sb, multiPoints[i]);
                sb.Append(")");
                if (i < multiPoints.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            SqlGeography polygon = SqlGeography.STMPolyFromText(new SqlChars(new SqlString(sb.ToString())), srid).MakeValid();
            return getProperPolygon(polygon);
        }

        public SqlGeography GetPolyLine(Point[] points)
        {
            StringBuilder sb = new StringBuilder("LINESTRING ");
            appendPointsSegment(sb, points);
            return SqlGeography.STLineFromText(new SqlChars(new SqlString(sb.ToString())), srid);
        }

        public SqlGeography GetMultiPolyLine(Point[][] multiPoints)
        {
            StringBuilder sb = new StringBuilder("MULTILINESTRING  (");
            for (int i = 0; i < multiPoints.Length; i++)
            {
                appendPointsSegment(sb, multiPoints[i]);
                if (i < multiPoints.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
            return SqlGeography.STMLineFromText(new SqlChars(new SqlString(sb.ToString())), srid);
        }

        private SqlGeography getProperPolygon(SqlGeography polygon)
        {
            SqlGeography invertedPolygon = polygon.ReorientObject();
            if (polygon.STArea() > invertedPolygon.STArea())
            {
                polygon = invertedPolygon;
            }
            return polygon;
        }

        private void appendPointSegment(StringBuilder sb, Point point)
        {
            sb.Append("(");
            appendPoint(sb, point);
            sb.Append(")");
        }

        private void appendPointsSegment(StringBuilder sb, Point[] points)
        {
            sb.Append("(");
            for (int i = 0; i < points.Length; i++)
            {
                appendPoint(sb, points[i]);
                if (i < points.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");
        }

        private void appendPoint(StringBuilder sb, Point point)
        {
            sb.Append(point.X.ToString(nfi)).Append(' ').Append(point.Y.ToString(nfi));
        }
    }
}
