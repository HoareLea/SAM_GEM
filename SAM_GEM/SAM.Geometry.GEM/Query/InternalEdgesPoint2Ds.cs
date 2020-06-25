using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.GEM
{
    public static partial class Query
    {
        public static List<List<Point2D>> InternalEdgesPoint2Ds(this Face3D face3D)
        {
            if (face3D == null)
                return null;

            List<IClosed2D> internalEdges = face3D.InternalEdges;
            if (internalEdges == null || internalEdges.Count == 0)
                return null;


            List<List<Point2D>> result = new List<List<Point2D>>();
            foreach(IClosed2D closed2D in internalEdges)
            {
                ISegmentable2D internalEdge = closed2D as ISegmentable2D;
                if (internalEdge == null)
                    throw new NotImplementedException();

                List<Point2D> point2Ds = internalEdge.GetPoints();
                if (point2Ds == null || point2Ds.Count < 3)
                    continue;

                result.Add(point2Ds);
            }

            return result;
        }
    }
}
