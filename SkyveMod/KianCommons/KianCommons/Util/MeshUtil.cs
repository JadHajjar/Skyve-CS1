using ObjUnity3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KianCommons {
    internal static class MeshUtil {
        public static Mesh LoadMesh(string fileName) {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            var stream = executingAssembly.GetManifestResourceStream("HideUnconnectedTracks.Resources." + fileName);
            var mesh = new Mesh();
            mesh.LoadOBJ(OBJLoader.LoadOBJ(stream));
            return mesh;
        }

        public static void DumpMesh(this Mesh mesh, string fileName) {
            foreach (char c in @"\/:<>|" + "\"") {
                fileName = fileName.Replace(c.ToString(), "");
            }

            string dir = "DC_Dumps";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, fileName + ".obj");
            Log.Info($"dumping mesh {mesh.name} to " + path);
            using (FileStream fs = new FileStream( path, FileMode.Create)) {
                OBJLoader.ExportOBJ(mesh.EncodeOBJ(), fs);
            }
        }

        /// <summary>
        /// drops unused vertices.
        /// </summary>
        public static Mesh CutMesh(this Mesh mesh, bool keepLeftSide) {
            const int EMPTY = -2;

            #region Calculate triangles and indexes
            var indexArray = new int[mesh.vertexCount];
            for (int i = 0; i < indexArray.Length; ++i) {
                const float EPSILON = 0.0001f;
                bool goodSide = keepLeftSide ? mesh.vertices[i].x < 0 + EPSILON : mesh.vertices[i].x > 0 - EPSILON;
                if (goodSide)
                    indexArray[i] = i;
                else
                    indexArray[i] = EMPTY; // mark for removal
            }
            indexArray = indexArray.Where(v => v != EMPTY).ToArray();

            // create inverse array
            int[] indexArrayInverse = new int[mesh.vertexCount];
            for (int i = 0; i < indexArrayInverse.Length; ++i) indexArrayInverse[i] = EMPTY;

            // switch value and index.
            for (int i = 0; i < indexArray.Length; ++i) {
                indexArrayInverse[indexArray[i]] = i;
            }

            // update triangle indeces and add only if all their vertices is on the right side.
            var newTriangleList = new List<int>(mesh.triangles.Length);
            for (var i = 0; i < mesh.triangles.Length; i += 3) {
                int newTriangle0 = indexArrayInverse[mesh.triangles[i]];
                int newTriangle1 = indexArrayInverse[mesh.triangles[i + 1]];
                int newTriangle2 = indexArrayInverse[mesh.triangles[i + 2]];
                if (newTriangle0 != EMPTY && newTriangle1 != EMPTY && newTriangle2 != EMPTY) {
                    newTriangleList.Add(newTriangle0);
                    newTriangleList.Add(newTriangle1);
                    newTriangleList.Add(newTriangle2);
                }
            }
            #endregion

            var newVertices = new Vector3[indexArray.Length];
            var newUV = new Vector2[indexArray.Length];
            var newNormals = new Vector3[indexArray.Length];
            var newTangents = new Vector4[indexArray.Length];
            for (int i = 0; i < indexArray.Length; ++i) {
                int j = indexArray[i];
                newVertices[i] = mesh.vertices[j];
                newUV[i] = mesh.uv[i];
                newNormals[i] = mesh.normals[j];
                newTangents[i] = mesh.tangents[j];
            }
            var newMesh = new Mesh { name = mesh.name + (keepLeftSide ? "_CutLeftHalf": "_CutRightHalf") };
            newMesh.bounds = mesh.bounds;
            newMesh.vertices = newVertices;
            newMesh.normals = newNormals;
            newMesh.tangents = newTangents;
            newMesh.triangles = newTriangleList.ToArray(); // triangle must be added after vertices.
            return newMesh;
        }

        /// <summary>
        /// does not drop unused vertices.
        /// </summary>
        public static Mesh CutMesh2(this Mesh mesh, bool keepLeftSide) {
            List<int> indexList = new List<int>(mesh.vertexCount);
            for (int i = 0; i < indexList.Count; ++i) indexList[i] = i; // create linear list

            // add only triangles on the right side.
            var newTriangleList = new List<int>(mesh.triangles.Length);
            for (var i = 0; i < mesh.triangles.Length; i += 3) {
                const float EPSILON = 0.0001f;
                bool GoodSide(int _i) {
                    var t = mesh.triangles[_i];
                    return keepLeftSide ? mesh.vertices[t].x < 0 + EPSILON : mesh.vertices[t].x > 0 - EPSILON;
                }
                if (GoodSide(i) && GoodSide(i + 1) && GoodSide(i + 2)) {
                    //Log.Debug($"Adding triangle[i:i+2]i={i}");
                    newTriangleList.Add(mesh.triangles[i]);
                    newTriangleList.Add(mesh.triangles[i + 1]);
                    newTriangleList.Add(mesh.triangles[i + 2]);
                }
            }

            var newMesh = new Mesh { name = mesh.name + (keepLeftSide ? "_CutLeftHalf" : "_CutRightHalf") };
            newMesh.bounds = mesh.bounds;
            newMesh.vertices = mesh.vertices.ToArray();
            newMesh.uv = mesh.uv.ToArray();
            newMesh.normals = mesh.normals.ToArray();
            newMesh.tangents = mesh.tangents.ToArray();
            newMesh.triangles = newTriangleList.ToArray();
            return newMesh;
        }

        public delegate bool IsGoodHandler(Vector3 vertex);

        /// <summary>
        /// does not drop unused vertices.
        /// </summary>
        public static Mesh CutMeshGeneric2(this Mesh mesh, IsGoodHandler IsGoodFunc) {
            List<int> indexList = new List<int>(mesh.vertexCount);
            for (int i = 0; i < indexList.Count; ++i) indexList[i] = i; // create linear list

            // add only triangles on the right side.
            var newTriangleList = new List<int>(mesh.triangles.Length);
            for (var i = 0; i < mesh.triangles.Length; i += 3) {
                bool IsGood(int _i) {
                    var t = mesh.triangles[_i];
                    return IsGoodFunc(mesh.vertices[t]);
                }
                if (IsGood(i) && IsGood(i + 1) && IsGood(i + 2)) {
                    //Log.Debug($"Adding triangle[i:i+2]i={i}");
                    newTriangleList.Add(mesh.triangles[i]);
                    newTriangleList.Add(mesh.triangles[i + 1]);
                    newTriangleList.Add(mesh.triangles[i + 2]);
                }
            }

            var newMesh = new Mesh { name = mesh.name + "_CutMeshGeneric" };
            newMesh.bounds = mesh.bounds;
            newMesh.vertices = mesh.vertices.ToArray();
            newMesh.uv = mesh.uv.ToArray();
            newMesh.normals = mesh.normals.ToArray();
            newMesh.tangents = mesh.tangents.ToArray();
            newMesh.triangles = newTriangleList.ToArray(); // triangle must be added after vertices.
            return newMesh;
        }

        /// <summary>
        /// drops unused vertices.
        /// </summary>
        public static Mesh CutMeshGeneric(this Mesh mesh, IsGoodHandler IsGoodFunc) {
            const int EMPTY = -2;

            #region Calculate triangles and indexes
            var indexArray = new int[mesh.vertexCount];
            for (int i = 0; i < indexArray.Length; ++i) {
                bool good = IsGoodFunc(mesh.vertices[i]);
                indexArray[i] = good ? i : EMPTY /*mark for removal*/;
            }
            indexArray = indexArray.Where(v => v != EMPTY).ToArray();

            // create inverse array
            int[] indexArrayInverse = new int[mesh.vertexCount];
            for (int i = 0; i < indexArrayInverse.Length; ++i) indexArrayInverse[i] = EMPTY;

            // switch value and index.
            for (int i = 0; i < indexArray.Length; ++i) {
                indexArrayInverse[indexArray[i]] = i;
            }

            // update triangle indeces and add only if all their vertices is on the right side.
            var newTriangleList = new List<int>(mesh.triangles.Length);
            for (var i = 0; i < mesh.triangles.Length; i += 3) {
                int newTriangle0 = indexArrayInverse[mesh.triangles[i]];
                int newTriangle1 = indexArrayInverse[mesh.triangles[i + 1]];
                int newTriangle2 = indexArrayInverse[mesh.triangles[i + 2]];
                if (newTriangle0 != EMPTY && newTriangle1 != EMPTY && newTriangle2 != EMPTY) {
                    newTriangleList.Add(newTriangle0);
                    newTriangleList.Add(newTriangle1);
                    newTriangleList.Add(newTriangle2);
                }
            }
            #endregion

            var newVertices = new Vector3[indexArray.Length];
            var newUV = new Vector2[indexArray.Length];
            var newNormals = new Vector3[indexArray.Length];
            var newTangents = new Vector4[indexArray.Length];
            for (int i = 0; i < indexArray.Length; ++i) {
                int j = indexArray[i];
                newVertices[i] = mesh.vertices[j];
                newUV[i] = mesh.uv[i];
                newNormals[i] = mesh.normals[j];
                newTangents[i] = mesh.tangents[j];
            }
            var newMesh = new Mesh { name = mesh.name + "_CutMeshGeneric" };
            newMesh.bounds = mesh.bounds;
            newMesh.vertices = newVertices;
            newMesh.normals = newNormals;
            newMesh.tangents = newTangents;
            newMesh.triangles = newTriangleList.ToArray(); // triangle must be added after vertices.
            return newMesh;
        }
    }
}
