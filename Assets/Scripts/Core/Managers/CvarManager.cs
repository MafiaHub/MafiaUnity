using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MafiaUnity
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

            public string tempValue;
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
            
            LoadConfig("openmf.cfg");
            InitDefaultValues();

            SaveConfig("openmf.cfg");

            // execute config commands in autoexec.cfg as well
            GameAPI.instance.consoleManager.ExecuteConfig("autoexec.cfg");

            isInitialized = true;
        }

        private void InitDefaultValues()
        {
            ForceSet("gameVersion", GameAPI.GAME_VERSION.ToString(), CvarMode.Archived);
            Set("musicVolume", "0.35", CvarMode.Archived);
        }

        public void SaveMainConfig()
        {
            SaveConfig("openmf.cfg");
        }

        /// <summary>
        /// Saves current config into an optional directory.
        /// </summary>
        /// <param name="path"></param>
        public void SaveConfig(string path)
        {
            var data = new StringBuilder();

            foreach (var cvar in values)
            {
                if (cvar.Value.mode == CvarMode.Archived)
                {
                    data.AppendFormat("pset {0} to {1}\r\n", cvar.Key, cvar.Value.value);
                }
            }

            try
            {
                File.WriteAllText(configPath + path, data.ToString());
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
                GameAPI.instance.consoleManager.ExecuteConfig(path);
            }
            catch (FileNotFoundException ex)
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
            else if (values[key].tempValue != null)
                return values[key].tempValue;
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
            else
            {
                values[key].tempValue = value;
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
            {
                values[key].value = value;
                values[key].tempValue = null;
            }
        }
    }
}