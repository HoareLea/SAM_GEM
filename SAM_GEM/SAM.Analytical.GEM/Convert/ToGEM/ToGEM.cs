using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

using SAM.Geometry.GEM;


namespace SAM.Analytical.GEM
{
    public static partial class Convert
    {
        public static string ToGEM(this AnalyticalModel analyticalModel, bool includePerimeterData = false, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if(analyticalModel == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            adjacencyCluster.SimplifyByAngle(tolerance_Angle, tolerance_Distance);

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
            if (materialLibrary != null)
            {
                List<Panel> panels = null;

                //Updating Apertures: Changing ApertureType wich depends on Transparency (transparent Door will become Window and not transparent Window will become Door)
                panels = adjacencyCluster.GetPanels();
                if (panels != null && panels.Count != 0)
                {
                    foreach (Panel panel in panels)
                    {
                        if (panel == null)
                        {
                            continue;
                        }

                        List<Aperture> apertures = panel.Apertures;
                        if (apertures == null || apertures.Count == 0)
                        {
                            continue;
                        }

                        Panel panel_New = Create.Panel(panel);

                        bool updated = false;
                        foreach (Aperture aperture in apertures)
                        {
                            if (aperture == null)
                            {
                                continue;
                            }

                            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                            if (apertureConstruction == null)
                            {
                                continue;
                            }

                            bool transparent = aperture.Transparent(materialLibrary);
                            ApertureType apertureType = aperture.ApertureType;

                            if (transparent && apertureType == ApertureType.Door)
                            {
                                panel_New.RemoveAperture(aperture.Guid);
                                apertureConstruction = new ApertureConstruction(aperture.ApertureConstruction, ApertureType.Window);
                                Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                                panel_New.AddAperture(aperture_New);
                                updated = true;
                            }
                            else if (!transparent && apertureType == ApertureType.Window)
                            {
                                panel_New.RemoveAperture(aperture.Guid);
                                apertureConstruction = new ApertureConstruction(aperture.ApertureConstruction, ApertureType.Door);
                                Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                                panel_New.AddAperture(aperture_New);
                                updated = true;
                            }
                        }

                        if (!updated)
                        {
                            continue;
                        }

                        adjacencyCluster.AddObject(panel_New);
                    }
                }

                //Updating Panels: Changing PanelType which depends on Transparency (transparent Wall will become CurtainWall)
                panels = adjacencyCluster.TransparentPanels(materialLibrary);
                if (panels != null && panels.Count != 0)
                {
                    foreach (Panel panel in panels)
                    {
                        if (panel == null)
                        {
                            continue;
                        }

                        if (panel.PanelType != PanelType.CurtainWall)
                        {
                            Panel panel_New = Create.Panel(panel, PanelType.CurtainWall);
                            adjacencyCluster.AddObject(panel_New);
                        }
                    }
                }
            }

            return ToGEM(adjacencyCluster, includePerimeterData, silverSpacing, tolerance_Distance);
        }

        private static string ToGEM(this AdjacencyCluster adjacencyCluster, bool includePerimeterData = false, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
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
                    List<Panel> panels = adjacencyCluster_Temp.UpdateNormals(space, false, true,false, silverSpacing, tolerance);
                    if (panels == null || panels.Count == 0)
                        continue;

                    string name = space.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = space.Guid.ToString();

                    if(includePerimeterData)
                    {
                        bool isPermieter = Query.IsPerimeter(adjacencyCluster_Temp, space);
                        name += isPermieter ? "_p" : "_i";
                    }

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
        
        private static string ToGEM(this IEnumerable<Panel> panels, string name, GEMType gEMType, double tolerance = Tolerance.Distance)
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

            List<Point3D> point3Ds =  panels?.ExternalEdgePoint3Ds(tolerance)?.ToList();
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

        private static string ToGEM(this Panel panel, List<Point3D> point3Ds, double tolerance = Tolerance.Distance)
        {
            string result = string.Empty;

            List<Point3D> externalEdge = panel?.ExternalEdgePoint3Ds(tolerance)?.ToList();
            if (externalEdge == null)
                return result;

            Plane plane = null;

            PanelType panelType = panel.PanelType;

            Construction construction = panel.Construction;
            if(construction == null)
            {
                panelType = PanelType.Air;
            }

            //Handling Panels with Air PanelType and CurtainWall PanelType
            if (panelType == PanelType.Air || panelType == PanelType.CurtainWall)
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

            List<List<Point3D>> internalEdgesPoint3Ds = panel?.InternalEdgesPoint3Ds(tolerance);
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

        private static string ToGEM(this IPartition partition, List<Point3D> point3Ds, double tolerance = Tolerance.Distance)
        {
            string result = string.Empty;

            List<Point3D> externalEdge = partition?.ExternalEdgePoint3Ds(tolerance)?.ToList();
            if (externalEdge == null)
                return result;

            Plane plane = partition.ReferencePlane(tolerance);

            //Handling Air Partitions and CurtainWalls
            if(partition is AirPartition)//if (panelType == PanelType.Air || panelType == PanelType.CurtainWall)
            {
                if (plane != null)
                {
                    List<List<Point2D>> openings = new List<List<Point2D>>();

                    openings.Add(externalEdge.ConvertAll(x => plane.Convert(x)));

                    result += string.Format("{0} {1}\n", externalEdge.Count, string.Join(" ", externalEdge.ConvertAll(x => point3Ds.IndexOf(x) + 1)));
                    result += string.Format("{0}\n", openings.Count);

                    OpeningType openingType = OpeningType.Undefined;
                    if (partition is AirPartition)
                        openingType = OpeningType.Hole;
                    else
                        openingType = OpeningType.Window; //CurtainWall

                    foreach (List<Point2D> opening in openings)
                        result += ToGEM(opening, openingType);
                }

                return result;
            }

            List<List<Point2D>> holes = new List<List<Point2D>>();

            List<List<Point3D>> internalEdgesPoint3Ds = partition?.InternalEdgesPoint3Ds(tolerance);
            if (internalEdgesPoint3Ds != null)
            {
                if (plane != null)
                {
                    foreach (List<Point3D> internalEdge in internalEdgesPoint3Ds)
                        holes.Add(internalEdge.ConvertAll(x => plane.Convert(x)));
                }
            }

            List<List<Point2D>> windows = new List<List<Point2D>>();
            List<List<Point2D>> doors = new List<List<Point2D>>();

            if (plane != null && partition is IHostPartition)
            {
                List<IOpening> openings = ((IHostPartition)partition).GetOpenings();
                if (openings != null && openings.Count != 0)
                {
                    foreach (IOpening opening in openings)
                    {
                        List<Point3D> externalEdge_Aperture = opening.ExternalEdgePoint3Ds(tolerance)?.ToList();
                        if (externalEdge_Aperture == null || externalEdge_Aperture.Count == 0)
                            continue;

                        List<List<Point2D>> point2Ds_Apertures = null;
                        if(opening is Door)
                        {
                            point2Ds_Apertures = doors;
                        }
                        else if(opening is Window)
                        {
                            point2Ds_Apertures = windows;
                        }
                        else
                        {
                            continue;
                        }

                        if (point2Ds_Apertures == null)
                            continue;

                        point2Ds_Apertures.Add(externalEdge_Aperture.ConvertAll(x => plane.Convert(x)));
                    }
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

        private static string ToGEM(this IEnumerable<IPartition> partitions, string name, GEMType gEMType, double tolerance = Tolerance.Distance)
        {
            if (partitions == null)
                return null;

            string result = string.Empty;

            int layer = -1;
            int colourRGB = -1;
            int colour = -1;
            switch (gEMType)
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

            List<Point3D> point3Ds = partitions?.ExternalEdgePoint3Ds(tolerance)?.ToList();
            if (point3Ds != null || point3Ds.Count > 2)
            {
                result += string.Format("{0} {1}\n", point3Ds.Count, partitions.Count());
                foreach (Point3D point3D in point3Ds)
                    result += string.Format(" {0} {1} {2}\n", point3D.X, point3D.Y, point3D.Z);

                foreach (IPartition partition in partitions)
                {
                    result += ToGEM(partition, point3Ds, tolerance);
                }
            }

            return result;
        }

        public static string ToGEM(this BuildingModel buildingModel, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if(buildingModel == null)
            {
                return null;
            }

            buildingModel = new BuildingModel(buildingModel);
            buildingModel.SplitByInternalEdges(tolerance_Distance);
            buildingModel.SimplifyByAngle(tolerance_Angle, tolerance_Distance);

            string result = null;

            List<Space> spaces = buildingModel.GetSpaces();
            if (spaces != null && spaces.Count != 0)
            {
                foreach (Space space in spaces)
                {
                    List<IPartition> partitions = Query.OrientedPartitions(buildingModel, space, false, true, silverSpacing, tolerance_Distance);
                    if (partitions == null || partitions.Count == 0)
                        continue;

                    string name = space.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = space.Guid.ToString();

                    string result_space = ToGEM(partitions, name, GEMType.Space, tolerance_Distance);
                    if (result_space == null)
                        continue;

                    if (result == null)
                        result = result_space;
                    else
                        result += result_space;
                }
            }

            List<IPartition> partitions_Shading = buildingModel.GetShadePartitions();
            if (partitions_Shading != null)
            {
                for (int i = 0; i < partitions_Shading.Count; i++)
                {
                    string result_shade = ToGEM(new IPartition[] { partitions_Shading[i] }, string.Format("SHADE {0}", i + 1), GEMType.Shade, tolerance_Distance);
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
    }
}
