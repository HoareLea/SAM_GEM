using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.GEM
{
    public static partial class Query
    {
        public static HashSet<Point3D> ExternalEdgePoint3Ds(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.GEM.Query.ExternalEdgePoint3Ds(panel?.GetFace3D(), tolerance);
        }

        public static HashSet<Point3D> ExternalEdgePoint3Ds(this Aperture aperture, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.GEM.Query.ExternalEdgePoint3Ds(aperture?.GetFace3D(), tolerance);
        }

        public static HashSet<Point3D> ExternalEdgePoint3Ds(this IEnumerable<Panel> panels, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            HashSet<Point3D> result = new HashSet<Point3D>();
            foreach (Panel panel in panels)
            {
                HashSet<Point3D> point3Ds = panel?.ExternalEdgePoint3Ds(tolerance);
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                foreach (Point3D point3D in point3Ds)
                    result.Add(point3D);
            }
            
            return result;
        }
    }
}
