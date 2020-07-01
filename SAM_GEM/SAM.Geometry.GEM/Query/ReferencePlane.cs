using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
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

            Vector3D axis_Y = plane.Project(Vector3D.WorldZ);
            if (axis_Y.Length <= tolerance)
                axis_Y = normal;

            Vector3D axis_X = axis_Y.CrossProduct(normal);
            if(axis_X.Length <= tolerance)
            {
                if (axis_Y.Z > 0)
                    axis_X = Vector3D.WorldX;
                else
                    axis_X = Vector3D.WorldX.GetNegated();
            }

            Plane result = new Plane(plane.Origin, axis_X, axis_Y);

            HashSet<Point3D> point3Ds = face3D.ExternalEdgePoint3Ds(tolerance);
            if (point3Ds == null || point3Ds.Count == 0)
                return result;

            List<Point2D> point2Ds = point3Ds.ToList().ConvertAll(x => result.Convert(x));
            if (point2Ds == null || point2Ds.Count == 0)
                return result;

            Point2D point2D_Min = point2Ds.Min();

            return new Plane(result.Convert(point2D_Min), axis_X, axis_Y);
        }
    }
}
