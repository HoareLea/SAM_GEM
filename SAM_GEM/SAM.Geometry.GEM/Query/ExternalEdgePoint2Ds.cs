using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static HashSet<Point2D> ExternalEdgePoint2Ds(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            ISegmentable2D externalEdge = face3D.ExternalEdge as ISegmentable2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Point2D> point2Ds = externalEdge.GetPoints();
            if (point2Ds == null)
                return null;

            HashSet<Point2D> result = new HashSet<Point2D>();
            if (point2Ds.Count == 0)
                return result;

            for(int i=0; i < point2Ds.Count; i++)
            {
                point2Ds[i].Round(tolerance);
                result.Add(point2Ds[i]);
            }

            return result;
        }
    }
}
