using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrigamiCraneGH
{
    internal class CreateSolid
    {
        public CreateSolid() { }

        public static Brep[] Sweep1(Curve rail, Curve[] sections)
        {
            Brep side = new SweepOneRail().PerformSweep(rail, sections)[0]; // sweep surface
            Brep start_lid = Brep.CreatePatch(new GeometryBase[] { sections[0] }, 10, 10, 0.001);
            Brep end_lid = Brep.CreatePatch(new GeometryBase[] { sections.LastOrDefault() }, 10, 10, 0.001);

            return Brep.JoinBreps(new Brep[] { side, start_lid, end_lid }, 0.001);
        }

        public static Brep[] Sweep2(Curve rail1, Curve rail2, Curve[] sections)
        {
            Brep side = Brep.CreateFromSweep(rail1, rail2, sections, false, 0.001)[0];
            Brep start_lid = Brep.CreatePatch(new GeometryBase[] { rail1 }, 10, 10, 0.001);
            Brep end_lid = Brep.CreatePatch(new GeometryBase[] { rail2 }, 10, 10, 0.001);

            return Brep.JoinBreps(new Brep[] { side, start_lid, end_lid }, 0.001);
        }

        public static Brep[] PatchOffset(Curve[] geometries, double offset)
        {
            Brep patch = Brep.CreatePatch(geometries, 10, 10, 0.001);
            return Brep.CreateOffsetBrep(patch, offset, true, false, 0.001,
                                         out Brep[] _, out Brep[] _);
        }
    }
}
