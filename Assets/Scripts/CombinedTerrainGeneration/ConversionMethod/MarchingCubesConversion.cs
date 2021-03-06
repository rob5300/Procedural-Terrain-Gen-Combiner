﻿using Assets.Scripts.CombinedTerrainGeneration.Configuration;
using MarchingCubesProject;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.CombinedTerrainGeneration
{
    public struct MarchingCubesTaskData
    {
        public List<Vector3> Verts;
        public List<int> Indicies;

        public MarchingCubesTaskData(List<Vector3> verts, List<int> indicies)
        {
            Verts = verts;
            Indicies = indicies;
        }
    }

    public class MarchingCubesConversion : ConversionMethod
    {
        [Configurable]
        public float Surface = 0f;
        [Configurable]
        public int ThreadSplit = 3;
        
        List<Vector3> vertsAll = new List<Vector3>();
        List<int> indicesAll = new List<int>();

        int _length;
        int _width;
        int _height;


        public override ResultData Convert(Volumetric3 data)
        {
            _length = data.Length;
            _width = data.Width;
            _height = data.Height;

            vertsAll.Clear();
            indicesAll.Clear();

            //Calculate segment sizes
            int widthS = _width / ThreadSplit;

            //Store the tasks we create
            List<Task<MarchingCubesTaskData>> segmentTasks = new List<Task<MarchingCubesTaskData>>();

            try
            {
                for (int i = 1; i <= ThreadSplit; i++)
                {
                    int widtha = widthS * i;
                    int minRange = widtha - widthS;
                    int maxRange = widtha;

                    Func<MarchingCubesTaskData> generateTask = () =>
                    {
                        List<Vector3> verts = new List<Vector3>();
                        List<int> indices = new List<int>();

                        Marching marching = new MarchingCubes(Surface);

                        marching.Generate(data.Voxels, _width, _height, _length, verts, indices, minRange, maxRange);

                        return new MarchingCubesTaskData(verts, indices);
                    };
                    segmentTasks.Add(Task.Run(generateTask));
                }
            }
            catch (Exception e)
            {
                return new ResultData(false, "Exception: " + e.Message);
            }

            //Loop waiting for all tasks to finish.
            while (true)
            {
                int tasksCompleted = 0;
                for (int i = 0; i < segmentTasks.Count; i++)
                {
                    if (segmentTasks[i].IsCompleted) tasksCompleted++;
                    if (segmentTasks[i].IsCanceled || segmentTasks[i].IsFaulted) return new ResultData(false, "One or more tasks faulted.");
                }
                if (tasksCompleted >= ThreadSplit) break;
            }

            //Combine all lists into one
            for (int i = 0; i < segmentTasks.Count; i++)
            {
                vertsAll.AddRange(segmentTasks[i].Result.Verts);
            }

            Converted = true;
            return new ResultData(true);
        }

        public override void Display(HashSet<GameObject> objects, Transform target)
        {
            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = vertsAll.Count / maxVertsPerMesh + 1;

            Material mat = Resources.Load<Material>("MarchingCubesMaterial");

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < vertsAll.Count)
                    {
                        splitVerts.Add(vertsAll[idx]);
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
                go.transform.parent = target;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = mat;
                go.GetComponent<MeshFilter>().mesh = mesh;
                objects.Add(go);

                go.transform.localPosition = new Vector3(-_width / 2, -_height / 2, -_length / 2);
                go.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }
}
