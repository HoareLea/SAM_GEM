using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static Point2D MinPoint2D(this Face3D face3D)
        {
            if (face3D == null)
                return null;

            ISegmentable2D externalEdge = face3D.ExternalEdge as ISegmentable2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Point2D> point2Ds = externalEdge.GetPoints();
            if (point2Ds == null)
                return null;

            double x = double.MinValue;
            double y = double.MaxValue;
            foreach(Point2D point2D in point2Ds)
            {
                if (point2D == null)
                    continue;

                double x_Temp = point2D.X;
                if (x < x_Temp)
                    x = x_Temp;

                double y_Temp = point2D.Y;
                if (y > y_Temp)
                    y = y_Temp;
            }

            if (x == double.MinValue || y == double.MaxValue)
                return null;

            return new Point2D(x, y);
        }
    }
}
