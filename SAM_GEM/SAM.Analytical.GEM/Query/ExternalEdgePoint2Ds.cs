using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.GEM
{
    public static partial class Query
    {
        public static HashSet<Geometry.Planar.Point2D> ExternalEdgePoint2Ds(this Aperture aperture)
        {
            return Geometry.GEM.Query.ExternalEdgePoint2Ds(aperture?.GetFace3D());
        }
    }
}
