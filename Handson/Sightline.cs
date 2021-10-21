using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GrasshopperAsyncComponentDemo.SampleImplementations
{
    public class Sightline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Sightline()
          : base("Sightline", "Sightline",
              "check sight-lines",
              "Async Handson", "Samples")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddPointParameter("Target", "T", "Target Point", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius", "R", "Radius", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Result", "R", "Result", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /// Initialize
            List<Point3d> iPoints = new List<Point3d>();
            Point3d iTarget = new Point3d();
            double iRadius = 0.0;
            int iIterations = 0;
            List<int> oResults = new List<int>();

            /// Get Data
            DA.GetDataList("Points", iPoints);
            DA.GetData(1, ref iTarget);
            DA.GetData(2, ref iRadius);
            iIterations = iPoints.Count;

            /// Check Sight Line
            List<Sphere> TestSpheres = new List<Sphere>();
            for (int i = 0; i < iIterations; i++)
            {
                TestSpheres.Clear();
                for (int j = 0; j < iIterations; j++)
                {
                    if (i != j) { TestSpheres.Add(new Sphere(iPoints[j], iRadius)); }
                }
                var res = CheckSightLine(iPoints[i], TestSpheres, iTarget);
                oResults.Add(res);
            }

            /// Set Data
            DA.SetDataList(0, oResults);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("58FA6AA3-D5E1-4E4A-9D4D-467ED58DD380"); }
        }

        public int CheckSightLine(Point3d pt, List<Sphere> TestSpheres, Point3d target)
        {
            var count = 0;
            var l = new Line(pt, target);
            Point3d pt1;
            Point3d pt2;
            for (int i = 0; i < TestSpheres.Count; i++)
            {
                var events = Rhino.Geometry.Intersect.Intersection.LineSphere(l, TestSpheres[i], out pt1, out pt2);
                if (events != Rhino.Geometry.Intersect.LineSphereIntersection.None) { count += 1; }
            }
            return count;
        }
    }
}