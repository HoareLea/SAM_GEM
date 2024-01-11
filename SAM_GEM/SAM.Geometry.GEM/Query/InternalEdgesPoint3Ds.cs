using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static List<List<Point3D>> InternalEdgesPoint3Ds(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
            if (internalEdges == null || internalEdges.Count == 0)
                return null;

            List<List<Point3D>> result = new List<List<Point3D>>();
            foreach(IClosedPlanar3D closedPlanar3D in internalEdges)
            {
                ISegmentable3D internalEdge = closedPlanar3D as ISegmentable3D;
                if (internalEdge == null)
                    throw new NotImplementedException();

                List<Point3D> point3Ds = internalEdge.GetPoints();
                if (point3Ds == null || point3Ds.Count < 3)
                    continue;

                point3Ds.ForEach(x => x.Round(tolerance));
                result.Add(point3Ds);
            }

            return result;
        }

        public static List<List<Point3D>> InternalEdgesPoint3Ds(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            return InternalEdgesPoint3Ds(face3DObject?.Face3D, tolerance);
        }
    }
}
