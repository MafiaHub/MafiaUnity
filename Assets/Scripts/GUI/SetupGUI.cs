using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MafiaUnity;

public class SetupGUI : MonoBehaviour {

    public GameObject pathSelection;
    public GameObject modManager;
    public GameObject mainMenu;
    public GameObject startupLight;
    public Text gameVersion;
    public Text buildTime;

    public List<Transform> pointsOfInterest = new List<Transform>();
    public int currentPOI = 0;

    Transform mainCamera = null;

    public void StartGame()
    {
        // Revert settings back to default.
        RenderSettings.ambientLight = new Color32(54, 58, 66, 1);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        GameObject.Destroy(startupLight);
        GameObject.Destroy(GameObject.Find("EventSystem"));
        GameObject.Destroy(gameObject);
        
        var modManager = GameAPI.instance.modManager;
        var mods = GetComponent<ModManagerGUI>();

        foreach (var mod in mods.modEntries)
        {
            if (mod.status == ModEntryStatus.Active)
            {
                modManager.LoadMod(mod.modName);
            }
        }

        modManager.InitializeMods();

        new GameObject("Game Instance").AddComponent<GameMain>();
    }

    public void PathSelectionMenu()
    {
        mainMenu.SetActive(false);
        pathSelection.SetActive(true);
    }

    public void ModManagerMenu()
    {
        mainMenu.SetActive(false);
        modManager.SetActive(true);
    }

    void Start() {
        mainCamera = GameObject.Find("Main Camera")?.transform;

        if (PlayerPrefs.HasKey("gamePath"))
        {
            if (!GameAPI.instance.SetGamePath(PlayerPrefs.GetString("gamePath")))
                PathSelectionMenu();
            else
                SetupDefaultBackground();
        }
        else
            PathSelectionMenu();

        CommandTerminal.Terminal.Shell.AddCommand("rgpnow", (CommandTerminal.CommandArg[] args) => {
            PlayerPrefs.DeleteKey("gamePath");
            PlayerPrefs.Save();
            Debug.Log("Game path was removed from PlayerPrefs!");
        }, 0, 0, "Resets the game path in PlayerPrefs");

        gameVersion.text = GameAPI.GAME_VERSION;
        buildTime.text = string.Format("Build Time: {0}", BuildInfo.BuildTime());
    }

    void SetupPOIs()
    {
        pointsOfInterest.Add(GameObject.Find("Group01")?.transform);
        pointsOfInterest.Add(GameObject.Find("fg")?.transform);
        pointsOfInterest.Add(GameObject.Find("Line03cv")?.transform);
        pointsOfInterest.Add(GameObject.Find("foto")?.transform);
        pointsOfInterest.Add(GameObject.Find("Group01")?.transform);
        pointsOfInterest.Add(GameObject.Find("Plane03")?.transform);
        pointsOfInterest.Add(GameObject.Find("Obr1")?.transform);
        pointsOfInterest.Add(GameObject.Find("bedna 02")?.transform);
        pointsOfInterest.Add(GameObject.Find("Doutnik")?.transform);

        pointsOfInterest.Shuffle();
    }

    private void Update()
    {
        if (pointsOfInterest.Count > 0 && mainCamera != null)
        {
            var poi = pointsOfInterest[currentPOI];
            
            if (poi == null)
            {
                currentPOI = (currentPOI == pointsOfInterest.Count-1) ? 0 : currentPOI + 1;
            }
            else
            {
                var rot = Quaternion.LookRotation(poi.position - mainCamera.position);
                mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, rot, 0.05f * Time.deltaTime);

                if (Quaternion.Angle(mainCamera.rotation, rot) < 35f)
                {
                    currentPOI = (currentPOI == pointsOfInterest.Count-1) ? 0 : currentPOI+1;
                }   
            }
        }
    }

    bool bgWasSetup = false;

    public void SetupDefaultBackground()
    {
        if (bgWasSetup)
            return;

        if (GameAPI.instance.GetInitialized())
        {
            bgWasSetup = true;

            GameAPI.instance.missionManager.LoadMission("00menu");

            SetupPOIs();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
