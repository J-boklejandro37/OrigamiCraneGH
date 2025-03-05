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
    public class ExtractComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExtractComponents class.
        /// </summary>hhghygtyttcyrtrtytyrrty567567iuyiuyoiupoi0-0989879879788
        public ExtractComponent()
          : base("ExtractComponent", "EC",
            "Description",
            "OrigamiCrane", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to extract components from.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "Tree of extracted components.", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Curves", "C", "Curves of extracted components.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new();
            if (!DA.GetDataList(0, meshes)) return;

            Extractor extractor = new(meshes);
            //DA.SetDataTree(0, extractor.GetTree());
            DA.SetDataList(1, extractor.GetCurves());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OrigamiCraneGH.Resources.MeshSkeleton.png"))
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
            get { return new Guid("D69A39CD-461D-44AD-AD14-33933CE8874A"); }
        }
    }
}