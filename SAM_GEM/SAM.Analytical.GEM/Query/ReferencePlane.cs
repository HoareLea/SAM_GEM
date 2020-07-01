using SAM.Geometry.Spatial;

namespace SAM.Analytical.GEM
{
    public static partial class Query
    {
        public static Plane ReferencePlane(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.GEM.Query.ReferencePlane(panel?.GetFace3D(), tolerance);
        }
    }
}
