﻿using Assets.Scripts.CombinedTerrainGeneration.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CombinedTerrainGeneration.GenerationMethods
{
    public class PerlinNoiseGeneration : GenerationMethod
    {
        [Configurable]
        public bool Additive = false;
        [Configurable]
        public float Frequency = 1f;
        [Configurable]
        public int Octaves = 1;
        [Configurable]
        public float Lacunarity = 2f;
        [Configurable]
        public float Persistence = 0.5f;
        [Configurable]
        public float Scale = 0.5f;

        public override Volumetric3 Generate(Volumetric3 startData)
        {
            float[,] terrainheights = new float[startData.Width, startData.Width];

            Vector3 point00 = new Vector3(-0.5f, -0.5f);
            Vector3 point10 = new Vector3(0.5f, -0.5f);
            Vector3 point01 = new Vector3(-0.5f, 0.5f);
            Vector3 point11 = new Vector3(0.5f, 0.5f);

            NoiseMethod method = Noise.noiseMethods[(int)NoiseMethodType.Perlin][2];
            float stepSize = 1f / startData.Width;
            for (int y = 0; y < startData.Width; y++)
            {
                Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
                Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);

                for (int x = 0; x < startData.Width; x++)
                {
                    Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                    float sample = Noise.Sum(method, point, Frequency, Octaves, Lacunarity, Persistence);
                    
                    sample = sample * 0.5f + 0.5f;
                    terrainheights[y, x] = sample * Scale;
                }
            }

            for (int x = 0; x < startData.Width; x++)
            {
                for (int z = 0; z < startData.Length; z++)
                {
                    float endPoint = 0;
                    float heightExtra = 0;
                    if (Additive)
                    {
                        //We are in additive mode, we add our data only on top of the highest data points instead.
                        float highestPoint = GetHighestYPoint(startData, x, z);
                        endPoint = highestPoint;
                        heightExtra = highestPoint;
                    }
                    for (int y = 0; y < startData.Height; y++)
                    {
                        float heightValue = terrainheights[x, z] * (startData.Height - 1) + heightExtra;
                        
                        //Only set the value as 1 within the range we wish to modify.
                        if (y < heightValue && y >= endPoint) startData.SetData(x,y,z, 1);
                    }
                }
            }

            return startData;
        }
        
        private int GetHighestYPoint(Volumetric3 data, int x, int z)
        {
            int y;
            for (y = data.Height - 1; z >= 0; y--)
            {
                if (data.GetData(x, y, z) == 1) return y;
            }
            return y;
        }
    }
}
