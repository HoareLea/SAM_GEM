﻿using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;


namespace SAM.Analytical.GEM
{
    public static partial class Convert
    {
        public static string ToGEM(this AdjacencyCluster adjacencyCluster, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            AdjacencyCluster adjacencyCluster_Temp = adjacencyCluster?.SplitByInternalEdges(tolerance);
            if (adjacencyCluster_Temp == null)
                return null;

            string result = null;

            List<Space> spaces = adjacencyCluster_Temp.GetSpaces();
            if(spaces != null && spaces.Count != 0)
            {
                foreach(Space space in spaces)
                {                   
                    List<Panel> panels = adjacencyCluster_Temp.UpdateNormals(space, false, silverSpacing, tolerance);
                    if (panels == null || panels.Count == 0)
                        continue;

                    string name = space.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = space.Guid.ToString();

                    string result_space = ToGEM(panels, name, GEMType.Space, tolerance);
                    if (result_space == null)
                        continue;

                    if (result == null)
                        result = result_space;
                    else
                        result += result_space;
                }
            }

            List<Panel> panels_Shading = adjacencyCluster_Temp.GetShadingPanels();
            if(panels_Shading != null)
            {
                for(int i=0; i < panels_Shading.Count; i++)
                {
                    string result_shade = ToGEM(new Panel[] { panels_Shading[i] }, string.Format("SHADE {0}", i + 1), GEMType.Shade, tolerance);
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
        
        private static string ToGEM(this IEnumerable<Panel> panels, string name, GEMType gEMType, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            string result = string.Empty;

            int layer = -1;
            int colourRGB = -1;
            int colour = -1;
            switch(gEMType)
            {
                case GEMType.Shade:
                    layer = 64;
                    colourRGB = 65280;
                    colour = 0;
                    break;
                case GEMType.Space:
                    layer = 1;
                    colourRGB = 16711690;
                    colour = 1;
                    break;
                default:
                    return null;
            }
            
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Layer(), layer);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Colour(), colour);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Category(), 1);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_Type(), (int)gEMType);
            result += string.Format("{0}\n{1}\n", Core.GEM.Query.ParameterName_ColourRGB(), colourRGB);
            result += string.Format("{0} {1}\n", Core.GEM.Query.ParameterName_Name(), name);

            List<Point3D> point3Ds = Query.ExternalEdgePoint3Ds(panels, tolerance)?.ToList();
            if (point3Ds != null || point3Ds.Count > 2)
            {
                result += string.Format("{0} {1}\n", point3Ds.Count, panels.Count());
                foreach (Point3D point3D in point3Ds)
                    result += string.Format(" {0} {1} {2}\n", point3D.X, point3D.Y, point3D.Z);

                foreach (Panel panel in panels)
                {
                    result += ToGEM(panel, point3Ds, tolerance);
                }
            }

            return result;
        }

        private static string ToGEM(this Panel panel, List<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            string result = string.Empty;

            List<Point3D> externalEdge = Query.ExternalEdgePoint3Ds(panel, tolerance)?.ToList();
            if (externalEdge == null)
                return result;

            Plane plane = null;

            //Handling Panels with Air PanelType and CurtainWall PanelType
            if (panel.PanelType == PanelType.Air || panel.PanelType == PanelType.CurtainWall)
            {
                plane = panel.ReferencePlane(tolerance);
                if(plane != null)
                {
                    List<List<Point2D>> openings = new List<List<Point2D>>();

                    openings.Add(externalEdge.ConvertAll(x => plane.Convert(x)));

                    result += string.Format("{0} {1}\n", externalEdge.Count, string.Join(" ", externalEdge.ConvertAll(x => point3Ds.IndexOf(x) + 1)));
                    result += string.Format("{0}\n", openings.Count);

                    OpeningType openingType = OpeningType.Undefined;
                    if (panel.PanelType == PanelType.Air)
                        openingType = OpeningType.Hole;
                    else
                        openingType = OpeningType.Window;
                    
                    foreach (List<Point2D> opening in openings)
                        result += ToGEM(opening, openingType);
                }

                return result;
            }

            List<List<Point2D>> holes = new List<List<Point2D>>();

            List<List<Point3D>> internalEdgesPoint3Ds = Query.InternalEdgesPoint3Ds(panel, tolerance);
            if (internalEdgesPoint3Ds != null)
            {
                plane = panel.ReferencePlane(tolerance);
                if (plane != null)
                {
                    foreach (List<Point3D> internalEdge in internalEdgesPoint3Ds)
                        holes.Add(internalEdge.ConvertAll(x => plane.Convert(x)));
                }
            }

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

                    List<Point3D> externalEdge_Aperture = aperture.ExternalEdgePoint3Ds(tolerance)?.ToList();
                    if (externalEdge_Aperture == null || externalEdge_Aperture.Count == 0)
                        continue;

                    List<List<Point2D>> point2Ds_Apertures = null;
                    switch (apertureType)
                    {
                        case ApertureType.Door:
                            point2Ds_Apertures = doors;
                            break;

                        case ApertureType.Window:
                            point2Ds_Apertures = windows;
                            break;
                    }

                    if (point2Ds_Apertures == null)
                        continue;

                    if (plane == null)
                        plane = panel.ReferencePlane(tolerance);

                    if (plane == null)
                        break;

                    point2Ds_Apertures.Add(externalEdge_Aperture.ConvertAll(x => plane.Convert(x)));
                }

            }

            result += string.Format("{0} {1}\n", externalEdge.Count, string.Join(" ", externalEdge.ConvertAll(x => point3Ds.IndexOf(x) + 1)));

            result += string.Format("{0}\n", windows.Count + doors.Count + holes.Count);

            foreach (List<Point2D> hole in holes)
                result += ToGEM(hole, OpeningType.Hole);

            foreach (List<Point2D> window in windows)
                result += ToGEM(window, OpeningType.Window);

            foreach (List<Point2D> door in doors)
                result += ToGEM(door, OpeningType.Door);

            return result;
        }

        private static string ToGEM(this IEnumerable<Point2D> point2Ds, OpeningType openingType)
        {
            if (point2Ds == null)
                return null;

            string result = string.Format("{0} {1}\n", point2Ds.Count(), (int)openingType);

            foreach (Point2D point2D in point2Ds)
                result += string.Format(" {0} {1}\n", point2D.X, point2D.Y);

            //foreach (Point2D point2D in point2Ds)
            //    result += string.Format(" {0} {1}\n", System.Math.Abs(point2D.X), System.Math.Abs(point2D.Y));

            return result;
        }
    }
}
