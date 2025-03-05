using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Commands;
using Rhino.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PInvokeCSharp;

namespace OrigamiCraneGH
{
    internal class Extractor
    {
        private readonly List<Curve> m_curves = new();
        private readonly DataTree<object> m_tree = new();
        Extractor() { }

        public Extractor(List<Mesh> meshes)
        {
            foreach (Mesh mesh in meshes)
            {
                if (true) // mesh is line-like object
                {

                }
                m_curves = meshes.Select(x => (Curve)TestCGAL.CreateMeshSkeleton(x)[0].ToNurbsCurve()).ToList();
                m_curves.AddRange(GetPerpendicularSections(m_curves[0], meshes[0], 5.0));
            }
        }

        public List<Curve> GetCurves()
        {
            return m_curves;
        }

        private static List<Curve> GetPerpendicularSections(Curve skeleton, Mesh mesh, double spacing)
        {
            List<Curve> sections = new();

            // Get curve length and calculate number of points
            double curve_length = skeleton.GetLength();
            int num_points = (int)(curve_length / spacing) + 1;

            // Get normalized spacing
            double normalized_spacing = 1.0 / (num_points - 1);

            // For each point along the curve
            for (int i = 0; i < num_points; i++)
            {
                // Get the point
                double parameter = skeleton.Domain.Min + (skeleton.Domain.Length * (i * normalized_spacing));
                Point3d point = skeleton.PointAt(parameter);

                // Get the tangent vector
                Vector3d tangent = skeleton.TangentAt(parameter);

                // Create a section plane
                Plane section_plane = new(point, tangent);

                // Created intersections
                Polyline[] polylines = Rhino.Geometry.Intersect.Intersection.MeshPlane(mesh, section_plane);

                // Add the closest curve
                sections.Add(GetClosestCurveFromPolylines(polylines, point).Rebuild(10, 3, true));
            }

            return sections;
        }

        private static Curve GetClosestCurveFromPolylines(Polyline[] polylines, Point3d point)
        {
            if (polylines.Length == 1)
                return polylines[0].ToNurbsCurve();

            // Find the curve that contains or is closest to the origin
            Curve closest_curve = null;
            double min_distance = double.MaxValue;

            foreach (Polyline polyline in polylines)
            {
                // Check if curve is closed (forms a loop)
                if (polyline.IsClosed)
                {
                    Curve curve = polyline.ToNurbsCurve();

                    // Get the closest point on curve
                    curve.ClosestPoint(point, out double t);
                    Point3d closest_point = curve.PointAt(t);

                    // Calculate the distance between the points
                    double distance = point.DistanceTo(closest_point);

                    // If the point is inside the curve and it's closer than previous findings
                    if (distance < min_distance)
                    {
                        closest_curve = curve;
                        min_distance = distance;
                    }
                }
            }

            return closest_curve;
        }

        private static DataTree<object> CombineDataTreeByGraft(List<DataTree<object>> trees)
        {
            DataTree<object> combined_tree = new();

            for (int t_idx = 0; t_idx < trees.Count; t_idx++)
            {
                var curr_tree = trees[t_idx];

                // Prepend for each branch
                foreach (GH_Path path in curr_tree.Paths)
                {
                    var new_path = path.PrependElement(t_idx);
                    combined_tree.AddRange(curr_tree.Branch(path), new_path);
                }
            }

            return combined_tree;
        }
    }

    internal class Constructor
    {
        private readonly List<Brep> m_breps = new();
        Constructor() { }

        public Constructor(GH_Structure<IGH_Goo> tree)
        {
            int part_count = tree.Paths.Max(p => p[0]) + 1;
            for (int i = 0; i < part_count; ++i)
            {
                var part = GetSubTree(tree, i);
                int unit_count = part.Paths.Max(p => p[0]) + 1;
                for (int j = 0; j < unit_count; ++j)
                {
                    var unit = GetSubTree(part, j);
                    m_breps.AddRange(CreateGeometry(unit));
                }
            }
        }

        public List<Brep> GetBreps()
        {
            return m_breps;
        }

        private static GH_Structure<IGH_Goo> GetSubTree(GH_Structure<IGH_Goo> tree, int first_dim_value)
        {
            GH_Structure<IGH_Goo> result = new();
            foreach (GH_Path path in tree.Paths)
            {
                if (path[0] == first_dim_value)
                    result.AppendRange(tree[path], new GH_Path(path.Indices[1..]));
            }
            return result;
        }

        private static List<Brep> CreateGeometry(GH_Structure<IGH_Goo> unit)
        {
            List<Brep> tmp_unit = unit[0][0].ToString() switch
            {
                "Sweep1" => CreateSolid.Sweep1(
                                ((GH_Curve)unit[1][0]).Value,
                                unit[2].Cast<GH_Curve>().Select(x => x.Value).ToArray()
                            ).ToList(),
                "Sweep2" => CreateSolid.Sweep2(
                                ((GH_Curve)unit[1][0]).Value,
                                ((GH_Curve)unit[2][0]).Value,
                                unit[3].Cast<GH_Curve>().Select(x => x.Value).ToArray()
                            ).ToList(),
                "PatchOffset" => CreateSolid.PatchOffset(
                                    unit[1].Cast<GH_Curve>().Select(x => x.Value).ToArray(),
                                    ((GH_Number)unit[2][0]).Value
                                ).ToList(),
                _ => throw new ArgumentException("Unknown shape type"),
            };
            return tmp_unit;
        }

        //private DataTree<object> GetSubTree(DataTree<object> tree, int first_dim_value)
        //{
        //    DataTree<object> result = new DataTree<object>();
        //    foreach (GH_Path path in tree.Paths)
        //    {
        //        if (path[0] == first_dim_value)
        //            result.AddRange(tree.Branch(path), new GH_Path(path.Indices[1..]));
        //    }
        //    return result;
        //}

        //private List<Brep> CreateGeometry(DataTree<object> unit)
        //{
        //    List<Brep> tmp_unit = new List<Brep>();
        //    switch ((String)unit.Branch(0)[0])
        //    {
        //        case "Sweep1":
        //            tmp_unit = CreateSolid.Sweep1(
        //                (Curve)unit.Branch(1)[0],
        //                unit.Branch(2).Cast<Curve>().ToArray()
        //            ).ToList();
        //            break;
        //        case "Sweep2":
        //            tmp_unit = CreateSolid.Sweep2(
        //                (Curve)unit.Branch(1)[0],
        //                (Curve)unit.Branch(2)[0],
        //                unit.Branch(3).Cast<Curve>().ToArray()
        //            ).ToList();
        //            break;
        //        case "PatchOffset":
        //            tmp_unit = CreateSolid.PatchOffset(
        //                unit.Branch(1).Cast<Curve>().ToArray(),
        //                (double)unit.Branch(2)[0]
        //            ).ToList();
        //            break;
        //        default:
        //            throw new ArgumentException("Unknown shape type");
        //    }
        //    return tmp_unit;
        //}

    }
}
