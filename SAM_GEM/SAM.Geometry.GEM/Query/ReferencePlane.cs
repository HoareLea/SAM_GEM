using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static Plane ReferencePlane(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            Vector3D normal = plane.Normal;

            Vector3D axis_Y = new Plane(plane, Point3D.Zero).Project(Vector3D.WorldZ);
            if (axis_Y.Length <= tolerance)
            {
                axis_Y = Vector3D.WorldY;
                if (normal.Z > 0)
                    axis_Y.Negate();
            }

            Vector3D axis_X = axis_Y.CrossProduct(normal); 
            if(axis_X.Length <= tolerance)
            {
                axis_X = Vector3D.WorldX;
                if (normal.Z > 0)
                    axis_X.Negate();
            }

            Plane result = new Plane(plane.Origin, axis_X, axis_Y);

            List<Point2D> point2Ds = face3D.ExternalEdgePoint3Ds(tolerance)?.ToList().ConvertAll(x => result.Convert(x));
            if (point2Ds == null || point2Ds.Count == 0)
                return result;

            Point2D point2D_Min = point2Ds.Min();

            return new Plane(result.Convert(point2D_Min), axis_X, axis_Y);
        }

        public static Plane ReferencePlane(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            return ReferencePlane(face3DObject?.Face3D, tolerance);
        }
    }
}
