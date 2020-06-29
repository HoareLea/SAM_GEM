using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static Point3D MinPoint3D(this Face3D face3D)
        {
            Point2D point2D = face3D?.MinPoint2D();
            if (point2D == null)
                return null;

            return face3D.GetPlane()?.Convert(point2D);
        }
    }
}
