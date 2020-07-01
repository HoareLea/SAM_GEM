using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.GEM
{
    public static partial class Query
    {
        public static List<List<Point3D>> InternalEdgesPoint3Ds(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.GEM.Query.InternalEdgesPoint3Ds(panel.GetFace3D(), tolerance);
        }
    }
}
