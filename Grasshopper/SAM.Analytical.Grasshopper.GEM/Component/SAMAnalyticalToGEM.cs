// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Analytical.Grasshopper.GEM.Properties;
using System;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical.GEM.Grasshopper
{
    public class SAMAnalyticalToGEM : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("88f14afb-c6b7-4d0b-b2fb-6979e673867b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.6";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_IES24;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalToGEM()
          : base("SAMAnalytical.ToGEM", "SAMAnalytical.ToGEM",
              "Writes SAM objects to GEM file",
              "SAM", "GEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "path_", NickName = "path_", Description = "GEM file path including extension .gem", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_includePerimeterData = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includePerimeterData_", NickName = "_includePerimeterData_", Description = "Include perimeter data in space name", Access = GH_ParamAccess.item };
                param_includePerimeterData.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_includePerimeterData, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new Analytical.Grasshopper.GooSpaceParam() { Name = "adjacentBuildingSpaces_", NickName = "adjacentBuildingSpaces_", Description = "SAM Analytical Spaces for Adjacent Bulding Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_run = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run_", NickName = "_run_", Description = "Run, set to True to export GEM to given path", Access = GH_ParamAccess.item };
                param_run.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_run, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "GEM", NickName = "GEM", Description = "GEM", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, false);
            }

            bool run = false;
            index = Params.IndexOfInputParam("_run_");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string path = null;
            index = Params.IndexOfInputParam("path_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref path);
            }

            double tolerance = Tolerance.Distance;
            index = Params.IndexOfInputParam("_tolerance_");
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool includePerimeterData = false;
            index = Params.IndexOfInputParam("_includePerimeterData_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref includePerimeterData);
            }

            List<Space> adjacentBuildingSpaces = new List<Space>();
            index = Params.IndexOfInputParam("adjacentBuildingSpaces_");
            if (index == -1 || !dataAccess.GetDataList(index, adjacentBuildingSpaces))
            {
                adjacentBuildingSpaces = null;
            }

            string gEM = null;
            if (sAMObject is AnalyticalModel)
            {
                gEM = Convert.ToGEM((AnalyticalModel)sAMObject, adjacentBuildingSpaces, includePerimeterData, Tolerance.MacroDistance, Tolerance.Distance, tolerance);
            }
            else if (sAMObject is BuildingModel)
            {
                gEM = Convert.ToGEM((BuildingModel)sAMObject, Tolerance.MacroDistance, Tolerance.Distance, tolerance);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (gEM == null)
            {
                gEM = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(path))
                System.IO.File.WriteAllText(path, gEM);

            index = Params.IndexOfOutputParam("GEM");
            if (index != -1)
            {
                dataAccess.SetData(index, gEM);
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
