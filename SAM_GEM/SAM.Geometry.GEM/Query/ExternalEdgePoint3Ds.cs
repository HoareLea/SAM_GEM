using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static HashSet<Point3D> ExternalEdgePoint3Ds(this Face3D face3D)
        {
            if (face3D == null)
                return null;

            ISegmentable3D externalEdge = face3D.GetExternalEdge() as ISegmentable3D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Point3D> point3Ds = externalEdge.GetPoints();
            if (point3Ds == null)
                return null;

            HashSet<Point3D> result = new HashSet<Point3D>();
            if (point3Ds.Count == 0)
                return result;

            foreach (Point3D point3D in point3Ds)
                result.Add(point3D);

            return result;
        }
    }
}
