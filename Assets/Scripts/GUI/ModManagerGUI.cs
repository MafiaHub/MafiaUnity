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
    public Text modName, modAuthor, modVersion, modGameVersion, modDependencies, modMissingDeps;

    ModManager modManager;

    public Color activeColor = Color.green;
    public Color inactiveColor = Color.red;
    public Color incompleteColor = Color.yellow;

    public List<ModEntry> modEntries;

    ModEntry selectedMod = null;

    void Start () {
        modManager = GameAPI.instance.modManager;

        var modNames = new List<string>(modManager.GetAllModNames());
        var mods = new List<ModEntry>();
        modEntries = new List<ModEntry>();

        foreach (var modName in modNames)
        {
            var modEntry = new ModEntry();
            modEntry.modMeta = modManager.ReadModInfo(modName);

            if (modEntry.modMeta == null)
                continue;

            modEntry.modName = modName;
            modEntry.status = 0;
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
                        mod.status = ModEntryStatus.Active;
                    
                    modEntries.Add(mod);
                    newMods.Remove(mod);
                }
            }
        }

        foreach (var newMod in newMods)
        {
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
            clonedImageComponent.color = (mod.status == ModEntryStatus.Active) ? activeColor : inactiveColor;

            var clonedTextComponent = clonedButton.transform.GetComponentInChildren<Text>();
            clonedTextComponent.text = mod.modMeta.name;

            mod.missingDependencies.Clear();

            foreach (var dep in mod.modMeta.dependencies)
            {
                var modDep = modEntries.Find(x => x.modName == dep);

                if (modDep == null || (modDep != null && modDep.status != ModEntryStatus.Active))
                {
                    mod.missingDependencies.Add(dep);
                }
            }

            if (mod.missingDependencies.Count > 0)
            {
                mod.status = ModEntryStatus.Incomplete;
                clonedImageComponent.color = incompleteColor;
            }
        }
    }

    string GetModNameFromFolderName(string folderName)
    {
        var mod = modEntries.Find(x => x.modName == folderName);

        if (mod != null)
        {
            return string.Format("{0} ({1})", mod.modMeta.name, mod.modName);
        }
        else return folderName;
    }

    void SelectModInfo(ModEntry mod)
    {
        if (mod == null)
            return;

        var entry = mod.modMeta;

        modName.text = string.Format("Name: {0} ({1})", entry.name, mod.modName);
        modAuthor.text = string.Format("Author: {0}", entry.author);
        modVersion.text = string.Format("Version: {0}", entry.version);
        modGameVersion.text = string.Format("Game Version: {0}", entry.gameVersion);

        if (entry.dependencies.Count > 0)
            modDependencies.text = string.Format("Dependencies: {0}", string.Join(", ", entry.dependencies.Select(x => GetModNameFromFolderName(x)).ToArray()));
        else
            modDependencies.text = "Dependencies: None";

        if (mod.missingDependencies.Count > 0)
            modMissingDeps.text = string.Format("Missing Dependencies: {0}", string.Join(", ", mod.missingDependencies.Select(x => GetModNameFromFolderName(x)).ToArray()));
        else
        {
            if (mod.status == ModEntryStatus.Incomplete)
                mod.status = ModEntryStatus.Inactive;

            modMissingDeps.text = "";
        }

        UpdateStatusButton(mod);

        selectedMod = mod;
    }

    void UpdateStatusButton(ModEntry mod)
    {
        var statusTextComponent = status.GetComponentInChildren<Text>();
        statusTextComponent.text = (mod.status == ModEntryStatus.Active) ? "Active" : "Inactive";

        if (mod.status == ModEntryStatus.Incomplete)
            statusTextComponent.text = "Incomplete";

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
        modEntries.ForEach(x => x.status = ModEntryStatus.Active);
        UpdateModList();
    }

    public void ToggleAllOff()
    {
        var baseMod = modEntries.First(x => x.modName == "MafiaBase");
        var baseActive = baseMod.status;

        modEntries.ForEach(x => x.status = ModEntryStatus.Inactive);
        baseMod.status = baseActive;

        UpdateModList();
    }

    public void ToggleModStatus()
    {
        if (selectedMod.status == ModEntryStatus.Incomplete)
            return;

        selectedMod.status = 1 - selectedMod.status;
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
            newLoadOrder.Add(new KeyValuePair<string, string>(mod.modName, mod.status == ModEntryStatus.Active ? "1" : "0"));
        }

        modManager.StoreLoadOrder(newLoadOrder.ToArray());
    }
}
