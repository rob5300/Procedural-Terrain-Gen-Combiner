﻿using Assets.Scripts.CombinedTerrainGeneration.GenerationMethods;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Assets.Scripts.CombinedTerrainGeneration
{
    public class GenerationInstance : MonoBehaviour
    {
        public int Length;
        public int Width;
        public int Height;

        public Text StatusText;
        public Text ResultText;
        public Text DurationText;

        public UnityHelper helper;
        public GeneratorUIManager UIManager;
        public Transform GenerateTarget;

        private GeneratorSystem _generator;
        private GeneratorConfigManager _configManager;
        private Task<ResultData> _generateTask;

        private bool _isRunning = false;
        private float _startTime;

        public void Awake()
        {
            _generator = new GeneratorSystem(helper, Length, Width, Height);
            _configManager = new GeneratorConfigManager();
        }

        public void AddMethod(Method method)
        {
            if(method != null)
            {
                if (method is ConversionMethod) _generator.ConversionMethod = (ConversionMethod)method;
                else if (method is GenerationMethod) _generator.MethodList.Add((GenerationMethod)method);
                else ResultText.text = "Method object type is invalid.";
            }
        }

        public void Update()
        {
            StatusText.text = _generator.Status;
            if (_isRunning && _generateTask.IsCompleted)
            {
                ResultData data = _generateTask.Result;
                ResultText.text = data.Message;
                _isRunning = false;

                //We are done!
                float duration = Time.time - _startTime;

                DurationText.text = duration.ToString() + " s";
            }
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (_isRunning) return;

            CleanOldObjects();

            //Record the start time
            _startTime = Time.time;

            //Get the entered data for each method and attempt to apply the values via reflection
            for (int i = 0; i < _generator.MethodList.Count; i++)
            {
                _configManager.ApplyConfigurableValuesTo(_generator.MethodList[i], UIManager.GetDataFromPairs(i));
            }
            //Get the entered data for the conversion method and also apply that data
            _configManager.ApplyConfigurableValuesTo(_generator.ConversionMethod, UIManager.GetDataFromPairsOnConversionMethod());

            Func<ResultData> generateFunc= () => { return _generator.Generate(GenerateTarget); };
            _generateTask = Task.Run(generateFunc);
            _isRunning = true;
        }

        public List<ConfigurableField> GetConfigList(Type type)
        {
            return _configManager.GetConfigList(type);
        }

        private void CleanOldObjects()
        {
            foreach(GameObject ob in _generator.TerrainObjects)
            {
                Destroy(ob);
            }
            _generator.TerrainObjects.Clear();
        }
    }
}
