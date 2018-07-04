using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OpenMafia
{
    public class ConsoleManager
    {
        public Dictionary<string, Func<string, string>> commands = new Dictionary<string, Func<string, string>>();

        private string[] toKeyword = new string[] { " to " };

        /// <summary>
        /// Executes console commands separated by newline.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public string ExecuteString(string buffer)
        {
            // TODO improve the parser
            var cvarManager = GameManager.instance.cvarManager;

            var output = new StringBuilder();

            var lines = buffer.Split('\n');

            foreach (var line in lines)
            {
                var parts = new List<string>(line.Split(' '));
                var cmd = parts[0];
                string args = "";

                if (parts.Count > 1)
                    args = String.Join(" ", parts.GetRange(1, parts.Count - 1)).Trim();

                if (commands.ContainsKey(cmd))
                    output.AppendLine(commands[cmd](args));
                else
                    output.AppendLine(cvarManager.Get(cmd, ""));
            }

            return output.ToString();
        }

        /// <summary>
        /// Loads a config from a specified path and executes it as a list of commands.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ExecuteConfig(string fileName)
        {
            var fileSystem = GameManager.instance.fileSystem;

            if (fileSystem.Exists(fileName))
            {
                var content = File.ReadAllText(fileSystem.GetCanonicalPath(fileName));

                ExecuteString(content.Trim());

                Debug.Log("Config file " + fileName + " has been executed!");
            }

            return "ok";
        }

        public ConsoleManager()
        {
            commands.Add("test", (string text) =>
            {
                return "Testing " + text;
            });

            commands.Add("set", (string text) =>
            {
                return SetCvar(text, CvarManager.CvarMode.None);
            });

            commands.Add("pset", (string text) =>
            {
                return SetCvar(text, CvarManager.CvarMode.Archived);
            });
        }

        string SetCvar(string text, CvarManager.CvarMode mode)
        {
            if (text == "")
                return "Cvar name is missing.";

            if (!text.Contains(" to "))
                return "Wrong format: Use set <cvar> to <value>!";

            var parts = text.Split(toKeyword, StringSplitOptions.None);

            if (parts.Length < 2)
                return "Wrong format: Use set <cvar> to <value>!";

            var cvar = parts[0].Trim();
            var value = parts[1].Trim();

            if (mode == CvarManager.CvarMode.Archived)
                GameManager.instance.cvarManager.ForceSet(cvar, value, mode);
            else
                GameManager.instance.cvarManager.Set(cvar, value);

            return "Value set to" + " \"" + value + "\".";
        }
    }
}