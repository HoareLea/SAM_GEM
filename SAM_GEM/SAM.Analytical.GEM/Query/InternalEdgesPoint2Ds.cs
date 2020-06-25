using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Analytical.GEM
{
    public static partial class Query
    {
        public static List<List<Point2D>> InternalEdgesPoint2Ds(this Panel panel)
        {
            return Geometry.GEM.Query.InternalEdgesPoint2Ds(panel.GetFace3D());
        }
    }
}
