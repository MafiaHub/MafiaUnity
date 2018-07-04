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
                else if (parts.Count == 1)
                    output.AppendLine(cvarManager.Get(cmd, ""));
                else
                    cvarManager.Set(cmd, args);
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
        }
    }
}