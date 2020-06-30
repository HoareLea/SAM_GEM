using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static Plane MinPlane(this Face3D face3D)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            Point3D point3D = face3D.MinPoint3D();
            if (point3D == null)
                return null;

            return new Plane(plane, new Point3D(point3D.X, point3D.Y, face3D.GetBoundingBox().Min.Z));
        }
    }
}
