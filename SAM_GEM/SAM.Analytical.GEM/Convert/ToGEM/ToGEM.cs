using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;


namespace SAM.Analytical.GEM
{
    public static partial class Convert
    {
        public static string ToGEM(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            string result = null;

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces != null && spaces.Count != 0)
            {
                foreach(Space space in spaces)
                {
                    List<Panel> panels = adjacencyCluster.GetRelatedObjects<Panel>(spaces);
                    if (panels == null || panels.Count == 0)
                        continue;

                    string name = space.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = space.Guid.ToString();


                    string result_space = ToGEM(panels, name, GEMType.Space);
                    if (result_space == null)
                        continue;

                    if (result == null)
                        result = result_space;
                    else
                        result += result_space;
                }
            }

            List<Panel> panels_Shading = adjacencyCluster.GetShadingPanels();
            if(panels_Shading != null)
            {
                for(int i=0; i < panels_Shading.Count; i++)
                {
                    string result_shade = ToGEM(new Panel[] { panels_Shading[i] }, string.Format("SHADE {0}", i + 1), GEMType.Shade);
                    if (result_shade == null)
                        continue;

                    if (result == null)
                        result = result_shade;
                    else
                        result += result_shade;

                }
            }

            return result;
        }


        private static string ToGEM(this IEnumerable<Panel> panels, string name, GEMType gEMType)
        {
            if (panels == null)
                return null;

            string result = string.Empty;
            
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Layer(), 1);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Colour(), 1);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Category(), 1);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Type(), (int)gEMType);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_ColourRGB(), 16711690);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Name(), name);

            List<Point3D> point3Ds = Query.ExternalEdgePoint3Ds(panels)?.ToList();
            if (point3Ds != null || point3Ds.Count > 2)
            {
                result += string.Format("{0} {1}\n", point3Ds.Count, panels.Count());
                foreach (Point3D point3D in point3Ds)
                    result += string.Format(" {0} {1} {2}\n", point3D.X, point3D.Y, point3D.Z);

                foreach (Panel panel in panels)
                {
                    List<Point3D> externalEdge = Query.ExternalEdgePoint3Ds(panel)?.ToList();
                    if (externalEdge == null)
                        continue;

                    List<List<Point2D>> holes = Query.InternalEdgesPoint2Ds(panel);
                    if (holes == null)
                        holes = new List<List<Point2D>>();

                    List<List<Point2D>> windows = new List<List<Point2D>>();
                    List<List<Point2D>> doors = new List<List<Point2D>>();

                    List<Aperture> apertures = panel.Apertures;
                    if (apertures != null && apertures.Count != 0)
                    {
                        foreach (Aperture aperture in apertures)
                        {
                            ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                            if (apertureConstruction == null)
                                continue;

                            ApertureType apertureType = apertureConstruction.ApertureType;
                            if (apertureType == ApertureType.Undefined)
                                continue;

                            HashSet<Point2D> point2Ds = aperture.ExternalEdgePoint2Ds();
                            if (point2Ds == null || point2Ds.Count == 0)
                                continue;

                            List<List<Point2D>> point2Ds_apertures = null;
                            switch (apertureType)
                            {
                                case ApertureType.Door:
                                    point2Ds_apertures = doors;
                                    break;

                                case ApertureType.Window:
                                    point2Ds_apertures = windows;
                                    break;
                            }

                            if (point2Ds_apertures == null)
                                continue;

                            point2Ds_apertures.Add(point2Ds.ToList());
                        }
                    }

                    result += string.Format("{0} {1}\n", externalEdge.Count, string.Join(" ", externalEdge.ConvertAll(x => point3Ds.IndexOf(x))));

                    result += string.Format("{0}\n", windows.Count + doors.Count + holes.Count);

                    foreach (List<Point2D> hole in holes)
                        result += ToGEM(hole, OpeningType.Hole);

                    foreach (List<Point2D> window in windows)
                        result += ToGEM(window, OpeningType.Window);

                    foreach (List<Point2D> door in doors)
                        result += ToGEM(door, OpeningType.Door);
                }
            }

            return result;
        }

        private static string ToGEM(this IEnumerable<Point2D> point2Ds, OpeningType openingType)
        {
            if (point2Ds == null)
                return null;

            string result = string.Format("{0} {1}\n", point2Ds.Count(), (int)openingType);

            foreach(Point2D point2D in point2Ds)
                result += string.Format(" {0} {1}\n", point2D.X, point2D.Y);

            return result;
        }
    }
}
