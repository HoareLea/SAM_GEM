using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Analytical.Grasshopper.GEM.Properties;
using System;
using SAM.Core;
using SAM.Analytical.Grasshopper;

namespace SAM.Analytical.GEM.Grasshopper
{
    public class SAMAnalyticalToGEM : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("88f14afb-c6b7-4d0b-b2fb-6979e673867b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_IES24;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalToGEM()
          : base("ToGEM", "ToGEM",
              "Writes SAM objects to GEM file ",
              "SAM", "GEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            string path = null;

            int index = -1;

            index = inputParamManager.AddParameter(new GooAnalyticalModelParam(), "_analyticalModel", "_analyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
            //inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddTextParameter("path_", "path_", "GEM file path including extension .gem", GH_ParamAccess.item, path);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddNumberParameter("_tolerance_", "_tolerance_", "Tolerance", GH_ParamAccess.item, Tolerance.Distance);

            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run, set to True to export GEM to given path", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddTextParameter("GEM", "GEM", "GEM", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(1, false);

            bool run = false;
            if (!dataAccess.GetData(3, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(0, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string path = null;
            dataAccess.GetData(1, ref path);

            double tolerance = Tolerance.Distance;
            if (!dataAccess.GetData(2, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string gEM = Convert.ToGEM(analyticalModel, Tolerance.MacroDistance, tolerance);

            if (!string.IsNullOrWhiteSpace(path))
                System.IO.File.WriteAllText(path, gEM);

            dataAccess.SetData(0, gEM);
            dataAccess.SetData(1, true);
        }
    }
}