using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public class FileSystem
    {

        public string gamePath { get; private set; }
        List<string> paths = new List<string>();

        /// <summary>
        /// Adds an optional path, such as native mod path containing custom files.
        /// </summary>
        /// <param name="path"></param>
        public void AddOptionalPath(string path)
        {
            paths.Insert(0, FixPath(path));
        }

        /// <summary>
        /// Removes an optional path from the list.
        /// </summary>
        /// <param name="path"></param>
        public void RemoveOptionalPath(string path)
        {
            paths.Remove(FixPath(path));
        }

        /// <summary>
        /// Retrieves all paths except the game path.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllPaths()
        {
            if (paths.Count == 0)
                return new string[] { };

            return paths.ToArray();
        }

        /// <summary>
        /// Checks if the path supplied does point to a valid location from
        /// the list of registered paths.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Exists(string path)
        {
            foreach (var mod in paths)
                if (File.Exists(mod + path))
                    return true;

            if (File.Exists(gamePath + path))
                return true;

            return false;
        }
        
        /// <summary>
        /// Returns first valid path to a requested file.
        /// USE THIS AT 100% when accessing native Mafia files.
        /// </summary>
        /// <param name="path">Postfix path to check against.</param>
        /// <returns></returns>
        public string GetCanonicalPath(string path)
        {
            foreach (var mod in paths)
                if (File.Exists(mod + path))
                    return mod + path;

            if (File.Exists(gamePath + path))
                return gamePath + path;

            return "";
        }

        /// <summary>
        /// Adds the game path to the list. Game path is always loaded last in the list.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool SetGamePath(string path)
        {
            path = FixPath(path);

            if (ValidateGamePath(path))
            {
                gamePath = path;

                return true;
            }

            return false;
        }


        string FixPath(string path)
        {
            if (!path.EndsWith("/"))
                return path + "/";

            return path;
        }

        bool ValidateGamePath(string path)
        {
            // TODO: Validate if game files are present there.
            return true;
        }
    }
}