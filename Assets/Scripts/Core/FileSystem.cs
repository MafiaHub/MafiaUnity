using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MafiaUnity.MafiaFormats;

namespace MafiaUnity
{
    public class FileSystem
    {
        public string gamePath { get; private set; }
        List<string> paths = new List<string>();
        public Dictionary<string, DTALoader> dtaFiles = new Dictionary<string, DTALoader>();

        /// <summary>
        /// Adds an optional path, such as native mod path containing custom files.
        /// </summary>
        /// <param name="path"></param>
        public void AddOptionalPath(string path)
        {
            paths.Insert(0, Path.Combine(FixPath(path), "Data"));
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
        /// Clears out all optional paths.
        /// </summary>
        public void ClearOptionalPaths()
        {
            paths.Clear();
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
            path = FixPath(path, true);

            if (GameAPI.instance.blockMods)
                    foreach (var mod in paths)
                    if (File.Exists(Path.Combine(mod, path)))
                        return true;

            if (GameAPI.instance.avoidLooseFiles)
                if (File.Exists(Path.Combine(gamePath, path)))
                    return true;

            if (DTAFileExists(path))
                return true;

            return false;
        }

        /// <summary>
        /// Mounts all dta files
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void DTAMountFiles()
        {
            //DTALoadFiles("A0.dta", 0xD8D0A975, 0x467ACDE0);
            DTALoadFiles("A1.dta", 0x3D98766C, 0xDE7009CD);
            DTALoadFiles("A2.dta", 0x82A1C97B, 0x2D5085D4);
            DTALoadFiles("A3.dta", 0x43876FEA, 0x900CDBA8);
            DTALoadFiles("A4.dta", 0x43876FEA, 0x900CDBA8);
            DTALoadFiles("A5.dta", 0xDEAC5342, 0x760CE652);
            DTALoadFiles("A6.dta", 0x64CD8D0A, 0x4BC97B2D);
            DTALoadFiles("A7.dta", 0xD6FEA900, 0xCDB76CE6);
            DTALoadFiles("A8.dta", 0xD8DD8FAC, 0x5324ACE5);
            DTALoadFiles("A9.dta", 0x6FEE6324, 0xACDA4783);
            DTALoadFiles("AA.dta", 0x5342760C, 0xEDEAC652);
            DTALoadFiles("AB.dta", 0xD8D0A975, 0x467ACDE0);
            DTALoadFiles("AC.dta", 0x43876FEA, 0x900CDBA8);
        }

        /// <summary>
        /// Load file names from DTA file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool DTALoadFiles(string dtaFileName, uint key1, uint key2)
        {
            if (gamePath.Length <= 0)
                return false;

            var dtaReader = new DTALoader(key1, key2);

            FileStream fs;

            try
            {
                fs = new FileStream(gamePath + dtaFileName, FileMode.Open);
            }
            catch
            {
                return false;
            }

            var reader = new BinaryReader(fs);
            dtaReader.Load(ref reader);

            foreach (var fileName in dtaReader.fileNames)
            {
                var lowered = fileName.ToLower();

                if (dtaFiles.ContainsKey(lowered))
                    dtaFiles.Remove(lowered);

                dtaFiles.Add(lowered, dtaReader);
            }

            return true;
        }

        /// <summary>
        /// Get contents of file in dta as stream
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Stream DTAGetFileContent(string file)
        {
            var fileName = file.ToLower().Replace(@"/", @"\");

            if (dtaFiles.ContainsKey(fileName))
                return new MemoryStream(dtaFiles[fileName].GetFile(fileName));
            
            return null;
        }

        /// <summary>
        /// Checks if file is inside DTA file system
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool DTAFileExists(string file)
        {
            var fileName = file.ToLower().Replace(@"/", @"\");
            return dtaFiles.ContainsKey(fileName);
        }

        /// <summary>
        /// Returns first valid path to a requested file.
        /// USE THIS AT 100% when accessing native Mafia files.
        /// </summary>
        /// <param name="path">Postfix path to check against.</param>
        /// <returns></returns>
        public string GetPath(string path)
        {
            path = FixPath(path, true);

            if (GameAPI.instance.blockMods)
                foreach (var mod in paths)
                    if (File.Exists(Path.Combine(mod, path)))
                        return Path.Combine(mod, path);

            if (GameAPI.instance.avoidLooseFiles)
                if (File.Exists(Path.Combine(gamePath, path)))
                    return Path.Combine(gamePath, path);

            return path;
        }

        /// <summary>
        /// Returns file stream to first valid path.
        /// USE THIS AT 100% when accessing native Mafia files.
        /// </summary>
        /// <param name="path">Postfix path to check against.</param>
        /// <returns></returns>
        public Stream GetStreamFromPath(string path)
        {
            path = FixPath(path, true);

            //Check files in normal file system
            if (GameAPI.instance.blockMods)
                foreach (var mod in paths)
                    if (File.Exists(Path.Combine(mod, path)))
                        return new FileStream(Path.Combine(mod, path), FileMode.Open);

            if (GameAPI.instance.avoidLooseFiles)
                if (File.Exists(Path.Combine(gamePath, path)))
                    return new FileStream(Path.Combine(gamePath, path), FileMode.Open);

            //If we didn't found any file let's search in DTA File system
            if (DTAFileExists(path))
                return DTAGetFileContent(path);

            throw new FileNotFoundException(path);
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
                ClearOptionalPaths();

                //Mount dta files after correct path is set
                DTAMountFiles();

                return true;
            }

            return false;
        }


        string FixPath(string path, bool isFile = false)
        {
            path = path.ToLower();
            path = path.Replace("\\", "/");

            if (!isFile && !path.EndsWith("/"))
                return path + "/";

            return path;
        }

        public bool ValidateGamePath(string path)
        {
            path = FixPath(path);

            if (File.Exists(Path.Combine(path, "Game.exe")) && File.Exists(Path.Combine(path, "Setup.exe")))
                return true;
            else return false;
        }
    }
}