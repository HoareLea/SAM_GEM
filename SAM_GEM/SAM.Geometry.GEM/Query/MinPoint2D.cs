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

            return point2Ds.Min();
        }
    }
}
