﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CombinedTerrainGeneration.GenerationMethods
{
    public class PerlinNoiseGeneration : GenerationMethod
    {
        public float Frequency = 1f;
        public int octaves = 1;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public float NoiseScale = 0.5f;

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
                    //_texture.SetPixel(x, y, new Color(point.x, point.y, point.z));
                    float sample = Noise.Sum(method, point, Frequency, octaves, lacunarity, persistence);
                    
                    sample = sample * 0.5f + 0.5f;
                    terrainheights[y, x] = sample * NoiseScale;
                }
            }

            for (int x = 0; x < startData.Width; x++)
            {
                for (int y = 0; y < startData.Height; y++)
                {
                    for (int z = 0; z < startData.Length; z++)
                    {
                        float heightValue = terrainheights[x, z] * (startData.Height - 1);
                        startData.SetData(x,y,z, y < heightValue ? 1 : 0);
                    }
                }
            }

            return startData;
        }
    }
}