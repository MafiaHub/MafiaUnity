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
        
        /// <summary>
        /// Initializes the manager from the disk.
        /// </summary>
        public void Init()
        {
            if (isInitialized)
                return;

            if (values == null)
                values = new Dictionary<string, Cvar>();

            InitDefaultValues();
            LoadConfig("openmf.json");
            SaveConfig("openmf.json");

            // execute config commands in openmf.cfg as well
            GameManager.instance.consoleManager.ExecuteConfig("autoexec.cfg");

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

        /// <summary>
        /// Saves current config into an optional directory.
        /// </summary>
        /// <param name="path"></param>
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

        /// <summary>
        /// Loads config from a specified directory.
        /// </summary>
        /// <param name="path"></param>
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

        /// <summary>
        /// Getter method for retrieving a cvar value.
        /// </summary>
        /// <param name="key">Cvar name</param>
        /// <param name="defaultValue">Fallback value if cvar wasn't present</param>
        /// <returns></returns>
        public string Get(string key, string defaultValue)
        {
            if (!values.ContainsKey(key))
                return defaultValue;
            else
                return values[key].value;
        }

        /// <summary>
        /// Setter method for assigning a value to a cvar.
        /// NOTE: Use ForceSet if you want to overwrite archived cvar.
        /// </summary>
        /// <param name="key">Cvar name</param>
        /// <param name="value">Value to set</param>
        /// <param name="mode">Cvar type (None, Archived)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Setter method for assigning a value to a cvar. Regardless of its type.
        /// </summary>
        /// <param name="key">Cvar name</param>
        /// <param name="value">Value to set</param>
        /// <param name="mode">Cvar type (None, Archived)</param>
        /// <returns></returns>
        public void ForceSet(string key, string value, CvarMode mode = CvarMode.None)
        {
            if (!values.ContainsKey(key))
                values.Add(key, new Cvar { mode = mode, value = value });
            else
                values[key].value = value;
        }
    }
}