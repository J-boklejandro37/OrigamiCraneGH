using Rhino.Geometry;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PInvokeCSharp {
    public static class TestCGAL {

        public static Polyline[] CreateMeshSkeleton(Mesh m_) {


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Clean Mesh
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            m_.Vertices.UseDoublePrecisionVertices = true;
            Mesh m = m_.DuplicateMesh();
            m.Vertices.UseDoublePrecisionVertices = true;
            m.Faces.ConvertQuadsToTriangles();
            m.Vertices.CombineIdentical(true, true);
            m.Vertices.CullUnused();
            m.Weld(3.14159265358979);
            m.FillHoles();
            m.RebuildNormals();
            m.Vertices.UseDoublePrecisionVertices = true;

            if (!m.IsValid && !m.IsClosed)
                return null;


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Send Vertices and Faces to C++
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            double[] ptCoordArr1 = new double[m.Vertices.Count * 3];
            for (int i = 0; i < m.Vertices.Count; i++) {
                ptCoordArr1[i * 3 + 0] = m.Vertices[i].X;
                ptCoordArr1[i * 3 + 1] = m.Vertices[i].Y;
                ptCoordArr1[i * 3 + 2] = m.Vertices[i].Z;
            }
            var ptCount1 = (ulong)m.Vertices.Count;


            int[] facesArr1 = m.Faces.ToIntArray(true);
            var facesCount1 = (ulong)m.Faces.Count;

            //Rhino.RhinoApp..WriteLine("Number of Vertices " + ptCount1.ToString());
            //Rhino.RhinoApp..WriteLine("Number of Faces " + facesCount1.ToString());

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Call unsafe method
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            IntPtr vertexCoordPointer = IntPtr.Zero;
            int nVertices = 0;
            IntPtr faceIndicesPointer = IntPtr.Zero;
            int nFaces = 0;

            UnsafeCGAL.MeshSkeleton_Create(ptCoordArr1, ptCount1, facesArr1, facesCount1, ref vertexCoordPointer, ref nVertices, ref faceIndicesPointer, ref nFaces);



            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Get Vertices and Faces from C++
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Convert faceIndicesPointer to C# int[]
            double[] verticesCoordinates = new double[nVertices * 3];
            Marshal.Copy(vertexCoordPointer, verticesCoordinates, 0, verticesCoordinates.Length);

            int[] faceIndices = new int[nFaces];
            Marshal.Copy(faceIndicesPointer, faceIndices, 0, faceIndices.Length);

            //Rhino.RhinoApp..WriteLine(verticesCoordinates.Length.ToString()  + " " + nVertices.ToString());
            //Rhino.RhinoApp..WriteLine(faceIndices.Length.ToString() + " " + nFaces.ToString());



            //Create mesh

            List<Polyline> plines = new List<Polyline>(4);
            Polyline polyline = new Polyline();
            int lastID = 1;
            for (int i = 0; i < verticesCoordinates.Length; i += 3) {

                // 0 2 5

                int currID = i == 0 ? faceIndices[0] : faceIndices[(int)(i / 3)];
                //Rhino.RhinoApp..WriteLine("CurrID " + currID.ToString());
                if (lastID != currID) {
                    plines.Add(polyline);
                    polyline = new Polyline();
                }

                polyline.Add(new Point3d(verticesCoordinates[i + 0], verticesCoordinates[i + 1], verticesCoordinates[i + 2]));


                lastID = currID;

            }
            plines.Add(polyline);//last pline
                                 //Rhino.RhinoApp..WriteLine("Number of plines " + plines.Count.ToString());

            UnsafeCGAL.ReleaseDouble(vertexCoordPointer, true);
            UnsafeCGAL.ReleaseInt(faceIndicesPointer, true);

            return plines.ToArray();


        }


    }
}
