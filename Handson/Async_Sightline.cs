using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrasshopperAsyncComponent;
using System.Windows.Forms;
using Rhino.Geometry;

namespace GrasshopperAsyncComponentDemo.SampleImplementations
{
    public class Async_SightlineComponent : GH_AsyncComponent
    {
        public override Guid ComponentGuid { get => new Guid("824441b2-6f70-459c-b5d5-b8a0260f2424"); }

        protected override System.Drawing.Bitmap Icon { get => Properties.Resources.logo32; }

        public override GH_Exposure Exposure => GH_Exposure.primary;
      public Async_SightlineComponent() : base("Async Sightline", "Async Sightline", "check sight-lines", "Async Handson", "Samples")
        {
            BaseWorker = new SightlineWorker();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddPointParameter("Target", "T", "Target Point", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius", "R", "Radius", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Result", "R", "Result", GH_ParamAccess.list);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel", (s, e) =>
            {
                RequestCancellation();
            });
        }
    }

    public class SightlineWorker : WorkerInstance
    {
        List<Point3d> iPoints = new List<Point3d>();
        Point3d iTarget = new Point3d();
        double iRadius = 0.0;
        int iIterations = 0;
        List<int> oResults = new List<int>();


        public SightlineWorker() : base(null) { }

        public override void DoWork(Action<string, double> ReportProgress, Action Done)
        {
            // Checking for cancellation
            if (CancellationToken.IsCancellationRequested) { return; }

            
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

                ReportProgress(Id, ((double)(i + 1) / (double)iIterations));

                // Checking for cancellation
                if (CancellationToken.IsCancellationRequested) { return; }
            }


            Done();
        }

        public override WorkerInstance Duplicate() => new SightlineWorker();

        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
        {
            if (CancellationToken.IsCancellationRequested) return;

            
            DA.GetDataList("Points", iPoints);
            DA.GetData(1, ref iTarget);
            DA.GetData(2, ref iRadius);
            iIterations = iPoints.Count;

        }

        public override void SetData(IGH_DataAccess DA)
        {
            if (CancellationToken.IsCancellationRequested) return;
            DA.SetDataList(0, oResults);
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
