using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using CommandTerminal;

namespace MafiaUnity
{
    /// <summary>
    /// Handles execution of config/console commands.
    /// </summary>
    public class ConsoleManager
    {
        public Dictionary<string, Func<string, string>> commands = new Dictionary<string, Func<string, string>>();

        private string[] toKeyword = new string[] { " to " };
        private bool debugMode = false;

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

            if (debugMode)
                Debug.Log("> \"" + buffer + "\"");

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

            string outputString = output.ToString();
            Debug.Log(outputString);

            return outputString;
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
                var content = File.ReadAllText(fileSystem.GetPath(fileName));

                ExecuteString(content.Trim());

                Debug.Log("Config file " + fileName + " has been executed!");
            }

            return "ok";
        }

        public ConsoleManager()
        {
            AddCommand("set", "Set a value to cvar", (string text) => {
                return SetCvar(text, CvarManager.CvarMode.None);
            });

            AddCommand("pset", "Set a persistent value to cvar", (string text) => {
                return SetCvar(text, CvarManager.CvarMode.Archived);
            });

            AddCommand("get", "Get a value from a cvar", (string text) => {
                return GameManager.instance.cvarManager.Get(text, "(null)");
            });

            AddCommand("dbg", "Toggle console debug mode", (string text) => {
                debugMode = !debugMode;
                return debugMode.ToString();
            });

            AddCommand("loadMission", "Loads a mission", (string text) => {
                GameManager.instance.missionManager.LoadMission(text);

                return "ok";
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

            return "Value for \"" + cvar + "\" set to" + " \"" + value + "\".";
        }

        /// <summary>
        /// Registers a console command to the system.
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="help">Help text displayed in the console</param>
        /// <param name="cb">Method to execute</param>
        public void AddCommand(string name, string help, Func<string, string> cb)
        {
            commands.Add(name, cb);

            if (Terminal.Shell == null)
            {
                Debug.LogWarning("No Terminal is available. Falling back to internal routine!");
                return;
            }

            Terminal.Shell.AddCommand(name, (CommandArg[] args) => {
                GameManager.instance.consoleManager.ExecuteString(name + " " + string.Join(" ", args).Trim());
            }, 0, -1, help);
        }
    }
}