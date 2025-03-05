using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace OrigamiCraneGH
{
    public class CreateGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateGeometry class.
        /// </summary>
        public CreateGeometry()
          : base("CreateGeometry", "CG",
            "Description",
            "OrigamiCrane", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "Tree of extracted components.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Result", "R", "Generated parts.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<IGH_Goo> tree)) return;

            Constructor constructor = new(tree);
            DA.SetDataList(0, constructor.GetBreps());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OrigamiCraneGH.Resources.CreateCloud.png"))
                {
                    return stream != null ? new Bitmap(stream) : null;
                }
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9AD5521E-0F1E-4870-937B-7498C9A93CBD"); }
        }
    }
}