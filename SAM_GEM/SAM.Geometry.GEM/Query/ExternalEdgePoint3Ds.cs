using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static HashSet<Point3D> ExternalEdgePoint3Ds(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            Face3D face3D_Temp = new Face3D(face3D);
            face3D_Temp.Normalize(tolerance_Distance: tolerance);

            ISegmentable3D externalEdge = face3D_Temp.GetExternalEdge3D() as ISegmentable3D;
            if(externalEdge == null)
                throw new NotImplementedException();

            List<Point3D> point3Ds = externalEdge.GetPoints();
            if (point3Ds == null)
                return null;

            //Vector3D normal = face3D.GetPlane().Normal;
            //if (normal == null)
            //    return null;

            //ISegmentable3D externalEdge = face3D.GetExternalEdge3D() as ISegmentable3D;
            //if (externalEdge == null)
            //    throw new NotImplementedException();

            //List<Point3D> point3Ds = externalEdge.GetPoints();
            //if (point3Ds == null)
            //    return null;

            //if (!Spatial.Query.Clockwise(point3Ds, normal, Core.Tolerance.Angle, tolerance))
            //    point3Ds.Reverse();

            HashSet<Point3D> result = new HashSet<Point3D>();
            if (point3Ds.Count == 0)
                return result;

            for (int i = 0; i < point3Ds.Count; i++)
            {
                point3Ds[i].Round(tolerance);
                result.Add(point3Ds[i]);
            }

            return result;
        }

        public static HashSet<Point3D> ExternalEdgePoint3Ds(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            return ExternalEdgePoint3Ds(face3DObject?.Face3D, tolerance);
        }

        public static HashSet<Point3D> ExternalEdgePoint3Ds<T>(this IEnumerable<T> face3DObjects, double tolerance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            if (face3DObjects == null)
                return null;

            HashSet<Point3D> result = new HashSet<Point3D>();
            foreach (T face3DObject in face3DObjects)
            {
                HashSet<Point3D> point3Ds = face3DObject?.ExternalEdgePoint3Ds(tolerance);
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                foreach (Point3D point3D in point3Ds)
                    result.Add(point3D);
            }

            return result;
        }
    }
}
