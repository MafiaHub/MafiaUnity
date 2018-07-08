using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MafiaUnity;

public class StartupGUI : MonoBehaviour {

    public GameObject drivesScroll;
    public GameObject drivePrefab;
    public GameObject dirPrefab;

    public GameObject folderScroll;
    public GameObject finalPath;
    public SetupGUI canvas;

    private string selectedDrive = "";
    private string currentPath;

    private void EnterDirectory(GameObject button)
    {
        currentPath = Path.Combine(currentPath, button.name);
        UpdateDirectoryList();
    }

    private void LeaveDirectory()
    {
        var dir = Directory.GetParent(currentPath);
        if (dir == null) return;
        currentPath = dir.Name;
        UpdateDirectoryList();
    }

    public void SelectPath()
    {
        var finalTextComponent = finalPath.GetComponent<InputField>();
        if(GameManager.instance.SetGamePath(finalTextComponent.text))
        {
            PlayerPrefs.SetString("gamePath", finalTextComponent.text);
            PlayerPrefs.Save();

            canvas.mainMenu.SetActive(true);
            gameObject.SetActive(false);
            canvas.SetupDefaultBackground();
        }
    }

    private void UpdateDirectoryList()
    {
        var finalTextComponent = finalPath.GetComponent<InputField>();
        finalTextComponent.text = currentPath;

        var contentList = folderScroll.transform.FindDeepChild("Content");
        foreach(Transform child in contentList.transform)
            GameObject.Destroy(child.gameObject);
        
        var upButton = GameObject.Instantiate(dirPrefab);
        upButton.transform.SetParent(contentList.transform);
        upButton.name = "..";

        var upTextComponent = upButton.transform.GetComponentInChildren<Text>();
        upTextComponent.text = "..";

        var upButtonComponent = upButton.GetComponent<Button>();
        upButtonComponent.onClick.AddListener(delegate { LeaveDirectory(); });

        foreach (var dir in Directory.GetDirectories(currentPath))
        {
            var clonedButton = GameObject.Instantiate(dirPrefab);
            clonedButton.transform.SetParent(contentList.transform);
            clonedButton.name = dir;

            var textComponent = clonedButton.transform.GetComponentInChildren<Text>();
            var splitPath = dir.Split(Path.DirectorySeparatorChar);
            textComponent.text = splitPath[splitPath.Length - 1];

            var buttonComponent = clonedButton.GetComponent<Button>();
            buttonComponent.onClick.AddListener(delegate { EnterDirectory(clonedButton); });
        }
    }

    public void DriveSelect(GameObject driveButton)
    {
        selectedDrive = driveButton.name;
        currentPath = selectedDrive;
        UpdateDirectoryList();
    }

	// Use this for initialization
	void Start () {

        var drives = DriveInfo.GetDrives();
        selectedDrive = drives[0].Name;
        currentPath = selectedDrive;
        UpdateDirectoryList();

        foreach (var drive in drives)
        {   
            var clonedButton = GameObject.Instantiate(drivePrefab);
            clonedButton.transform.SetParent(drivesScroll.transform.FindDeepChild("Content").transform);
            clonedButton.name = drive.Name;

            var buttonComponent = clonedButton.GetComponent<Button>();
            buttonComponent.onClick.AddListener(delegate { DriveSelect(clonedButton); });

            var textComponent = clonedButton.transform.GetComponentInChildren<Text>();
            textComponent.text = drive.Name;

        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
