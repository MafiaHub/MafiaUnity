// adapted from https://gist.github.com/mminer/975374
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    /// <summary>
    /// Obsolete
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public KeyCode toggleKey = KeyCode.BackQuote;

        List<string> outputLines = new List<string>();
        bool isVisible;
        Vector2 scrollPosition;

        const int margin = 20;

        Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
        Rect titleBarRect = new Rect(0, 0, 10000, 20);
        GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        GUIContent sendLabel = new GUIContent("Send", "Send the content of the input field.");
        string inputLine = "";

        ConsoleManager consoleManager = null;

        private void OnGUI()
        {
            if (!isVisible)
                return;

            // TODO use number pool instead
            windowRect = GUILayout.Window(83489, windowRect, ConsoleWindow, "Debug Console");
        }

        private void Update()
        {
            if (GameManager.instance.GetInitialized())
                if (Input.GetKeyDown(toggleKey))
                    isVisible = !isVisible;
        }
        
        void ConsoleWindow(int windowID)
        {
            if (consoleManager == null && GameManager.instance.GetInitialized())
                consoleManager = GameManager.instance.consoleManager;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                foreach (var line in outputLines)
                {
                    GUILayout.Label(line);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                inputLine = GUILayout.TextField(inputLine);

                if (GUILayout.Button(sendLabel))
                {
                    SendInput();
                }
                
                if (GUILayout.Button(clearLabel))
                    outputLines.Clear();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(titleBarRect);
        }

        void SendInput()
        {
            outputLines.Add("> " + inputLine);
            outputLines.Add(consoleManager.ExecuteString(inputLine));
            inputLine = "";
        }
    }
}