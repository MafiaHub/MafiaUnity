using MafiaUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ModManagerGUI : MonoBehaviour {

    public GameObject modList;
    public GameObject modPrefab;

    public Button status;
    public Text modName, modAuthor, modVersion, modGameVersion;

    ModManager modManager;

    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;

    public List<ModEntry> modEntries;

    ModEntry selectedMod = null;
    int selectedModIndex = 0;

    bool isInitialized = false;

    void Start () {
        modManager = GameManager.instance.modManager;

        var modNames = new List<string>(modManager.GetAllModNames());
        var mods = new List<ModEntry>();
        modEntries = new List<ModEntry>();

        foreach (var modName in modNames)
        {
            var modEntry = new ModEntry();
            modEntry.modMeta = modManager.ReadModInfo(modName);
            modEntry.modName = modName;
            modEntry.isActive = 0;
            mods.Add(modEntry);
        }

        var newMods = new List<ModEntry>(mods);

        var loadOrder = modManager.GetLoadOrder();

        foreach (var load in loadOrder)
        {
            foreach (var mod in mods)
            {
                if (mod.modName == load.Key)
                {
                    if (load.Value == "1")
                        mod.isActive = 1;
                    
                    modEntries.Add(mod);
                    newMods.Remove(mod);
                }
            }
        }

        foreach (var newMod in newMods)
        {
            if (newMod.modName == "MafiaBase")
            {
                newMod.isActive = 1;
                modEntries.Insert(0, newMod);
            }
            else
                modEntries.Add(newMod);
        }
            

        ApplyChanges();
        UpdateModList();

        if (modEntries.Count > 0)
            SelectModInfo(modEntries[0]);
    }

    void UpdateModList()
    {
        foreach (Transform obj in modList.transform)
            GameObject.Destroy(obj.gameObject);

        foreach (var mod in modEntries)
        {
            var clonedButton = GameObject.Instantiate(modPrefab);
            clonedButton.name = mod.modName;

            clonedButton.transform.SetParent(modList.transform);

            var clonedButtonComponent = clonedButton.GetComponent<Button>();
            clonedButtonComponent.onClick.AddListener(delegate { SelectModInfoButton(clonedButton); });

            var clonedImageComponent = clonedButton.GetComponent<Image>();
            clonedImageComponent.color = (mod.isActive != 0) ? activeColor : inactiveColor;

            var clonedTextComponent = clonedButton.transform.GetComponentInChildren<Text>();
            clonedTextComponent.text = mod.modMeta.name;
        }

    }

    void SelectModInfo(ModEntry mod)
    {
        var entry = mod.modMeta;

        modName.text = string.Format("Name: {0}", entry.name);
        modAuthor.text = string.Format("Author: {0}", entry.author);
        modVersion.text = string.Format("Version: {0}", entry.version);
        modGameVersion.text = string.Format("Game Version: {0}", entry.gameVersion);

        UpdateStatusButton(mod);

        selectedMod = mod;
    }

    void UpdateStatusButton(ModEntry mod)
    {
        var statusTextComponent = status.GetComponentInChildren<Text>();
        statusTextComponent.text = (mod.isActive != 0) ? "Active" : "Inactive";
        UpdateModList();
    }

    void SelectModInfoButton(GameObject button)
    {
        if (modEntries.Count(x => x.modName == button.name) < 1)
            return;

        var entry = modEntries.First(x => x.modName == button.name);
        SelectModInfo(entry);
    }

    public void MoveUp()
    {
        int i = modEntries.IndexOf(selectedMod);
        if (i > 0)
        {
            var oldMod = modEntries[i - 1];
            modEntries[i] = oldMod;
            modEntries[i - 1] = selectedMod;
            UpdateModList();
        }
    }

    public void MoveDown()
    {
        int i = modEntries.IndexOf(selectedMod);
        if (i + 1 < modEntries.Count)
        {
            var oldMod = modEntries[i + 1];
            modEntries[i] = oldMod;
            modEntries[i + 1] = selectedMod;
            UpdateModList();
        }
    }

    public void ToggleAllOn()
    {
        modEntries.ForEach(x => x.isActive = 1);
        UpdateModList();
    }

    public void ToggleAllOff()
    {
        var baseMod = modEntries.First(x => x.modName == "MafiaBase");
        var baseActive = baseMod.isActive;

        modEntries.ForEach(x => x.isActive = 0);
        baseMod.isActive = baseActive;

        UpdateModList();
    }

    public void ToggleModStatus()
    {
        selectedMod.isActive = 1 - selectedMod.isActive;
        UpdateStatusButton(selectedMod);
    }

    public void SaveAndQuit()
    {
        ApplyChanges();

        var setup = GetComponent<SetupGUI>();
        setup.mainMenu.SetActive(true);
        setup.modManager.SetActive(false);
    }

    void ApplyChanges()
    {
        var newLoadOrder = new List<KeyValuePair<string, string>>();

        foreach (var mod in modEntries)
        {
            newLoadOrder.Add(new KeyValuePair<string, string>(mod.modName, mod.isActive.ToString()));
        }

        modManager.StoreLoadOrder(newLoadOrder.ToArray());
    }
}
