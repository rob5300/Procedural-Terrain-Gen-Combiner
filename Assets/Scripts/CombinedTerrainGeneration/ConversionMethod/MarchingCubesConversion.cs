﻿using MarchingCubesProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CombinedTerrainGeneration
{
    public class MarchingCubesConversion : ConversionMethod
    {
        Marching marching = new MarchingCubes();
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        int _length;
        int _width;
        int _height;

        public MarchingCubesConversion()
        {
            marching.Surface = 0.0f;
        }

        public override void Convert(Volumetric3 data)
        {
            verts = new List<Vector3>();
            indices = new List<int>();

            _length = data.Length;
            _width = data.Width;
            _height = data.Height;

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(data.Voxels, _width, _height, _length, verts, indices);

        }

        public override void Display()
        {
            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            Material mat = Resources.Load<Material>("MarchingCubesMaterial");

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                GameObject go = new GameObject("Mesh");
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = mat;
                go.GetComponent<MeshFilter>().mesh = mesh;

                go.transform.localPosition = new Vector3(-_width / 2, -_height / 2, -_length / 2);
            }
        }
    }
}