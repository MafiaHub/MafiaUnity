using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    [Serializable]
    public class CvarManager
    {
        const int CONFIG_VERSION = 1;

        public enum CvarMode
        {
            None, Archived
        }

        [Serializable]
        public class Cvar
        {
            public CvarMode mode;
            public string value;
        }

        [Serializable]
        public class CvarList
        {
            [SerializeField] public List<string> keys;
            [SerializeField] public List<string> values;

            public CvarList()
            {
                keys = new List<string>();
                values = new List<string>();
            }
        }
        
        public Dictionary<string, Cvar> values;

        public string configPath;
        private bool isInitialized = false;
        
        public void Init()
        {
            if (isInitialized)
                return;

            if (values == null)
                values = new Dictionary<string, Cvar>();

            InitDefaultValues();
            LoadConfig("openmf.json");
            SaveConfig("openmf.json");

            isInitialized = true;
        }

        private void InitDefaultValues()
        {
            ForceSet("gameVersion", GameManager.GAME_VERSION.ToString(), CvarMode.Archived);
        }

        public void SaveMainConfig()
        {
            SaveConfig("openmf.json");
        }

        public void SaveConfig(string path)
        {
            var archivedVars = new CvarList();

            foreach (var cvar in values)
            {
                if (cvar.Value.mode == CvarMode.Archived)
                {
                    archivedVars.keys.Add(cvar.Key);
                    archivedVars.values.Add(cvar.Value.value);
                }
            }

            var jsonContent = JsonUtility.ToJson(archivedVars);

            try
            {
                File.WriteAllText(configPath + path, jsonContent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void LoadConfig(string path)
        {
            try
            {
                var jsonContent = File.ReadAllText(configPath + path);

                try
                {
                    var data = JsonUtility.FromJson<CvarList>(jsonContent);

                    for (int i = 0; i < data.keys.Count; i++)
                    {
                        ForceSet(data.keys[i], data.values[i], CvarMode.Archived);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Config file: " + path + " is not a valid JSON formatted file! " + ex.ToString());
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Config file: " + path + " doesn't exist. " + ex.ToString());
            }
        }

        public string Get(string key, string defaultValue)
        {
            if (!values.ContainsKey(key))
                return defaultValue;
            else
                return values[key].value;
        }

        public bool Set(string key, string value, CvarMode mode = CvarMode.None)
        {
            if (!values.ContainsKey(key))
            {
                values.Add(key, new Cvar { mode = mode, value = value });
                return true;
            }
            else if (values[key].mode != CvarMode.Archived)
            {
                values[key].value = value;
                return true;
            }

            return false;
        }

        public void ForceSet(string key, string value, CvarMode mode = CvarMode.None)
        {
            if (!values.ContainsKey(key))
                values.Add(key, new Cvar { mode = mode, value = value });
            else
                values[key].value = value;
        }
    }
}