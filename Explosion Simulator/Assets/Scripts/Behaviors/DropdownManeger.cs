using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using OpenCV2;
using UnityEngine.EventSystems;

public class DropdownManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown AmmoDropdown;
    [SerializeField] private Launch LaunchSkript;
    [SerializeField] private Button AddAmmoButton;
    [SerializeField] private GameObject Inputpanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField radiusInputField;
    [SerializeField] private TMP_InputField numberOfExplosionsInputField;
    [SerializeField] private TMP_InputField scatterRadiusInputField;
    public Dictionary<string, (int, int, int)> AmmoDict = new Dictionary<string, (int, int, int)>();
    public int IndexToDelete;
    private string filePath;

    public class AmmoOption
    {
        public string Name { get; set; }
        public int Radius { get; set; }
        public int NumberOfExplosions { get; set; }
        public int ScatterRadius { get; set; }
    }

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "ammoOptions.json");
        LoadAmmo();
        UpdateAmmoDropdown();
        AmmoDropdown.onValueChanged.AddListener(OnAmmoDropdownChanged);
        AddAmmoButton.onClick.AddListener(OnAddAmmoButtonClicked);
    }
    private void LoadAmmo()
    {
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, List<AmmoOption>>>(jsonString);
            List<AmmoOption> ammoOptions = jsonObject["ammoOptions"];

            foreach (var ammo in ammoOptions)
            {
                AmmoDict[ammo.Name] = (ammo.Radius, ammo.NumberOfExplosions, ammo.ScatterRadius);
            }
        }
    }

    private void UpdateAmmoDropdown()
    {
        int currentSelectedIndex = AmmoDropdown.value;
        AmmoDropdown.ClearOptions();
        List<string> ammoNames = new List<string>(AmmoDict.Keys);
        AmmoDropdown.AddOptions(ammoNames);
        AmmoDropdown.RefreshShownValue();
        AmmoDropdown.value = currentSelectedIndex;
    }

    private void OnAmmoDropdownChanged(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < AmmoDict.Count)
        {
            string selectedKey = AmmoDict.Keys.ElementAt(selectedIndex);
            (int radius, int explosions, int scatter) = AmmoDict[selectedKey];
            LaunchSkript.ThreatRadius = radius;
            LaunchSkript.NumberOfExplosions = explosions;
            LaunchSkript.ScatterRadius = scatter;
        }
        AmmoDropdown.RefreshShownValue();
    }

    private void OnAddAmmoButtonClicked()
    {
        string newAmmoName = nameInputField.text;
        int newAmmoRadius = Convert.ToInt32(radiusInputField.text) * 5;
        int newAmmoNumberOfExplosions = Convert.ToInt32(numberOfExplosionsInputField.text);
        int newAmmoScatterRadius = Convert.ToInt32(scatterRadiusInputField.text) * 5;

        AmmoDict[newAmmoName] = (newAmmoRadius, newAmmoNumberOfExplosions, newAmmoScatterRadius);
        SaveAmmoList();
        UpdateAmmoDropdown();

        int newItemIndex = AmmoDict.Keys.ToList().IndexOf(newAmmoName);
        AmmoDropdown.value = newItemIndex;
        AmmoDropdown.RefreshShownValue();
        Inputpanel.SetActive(false);
    }

    private void SaveAmmoList()
    {
        List<AmmoOption> ammoOptions = AmmoDict.Select(kvp => new AmmoOption
        {
            Name = kvp.Key,
            Radius = kvp.Value.Item1,
            NumberOfExplosions = kvp.Value.Item2,
            ScatterRadius = kvp.Value.Item3
        }).ToList();

        var jsonObject = new Dictionary<string, List<AmmoOption>> { { "ammoOptions", ammoOptions } };
        string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        File.WriteAllText(filePath, jsonString);
        Debug.Log($"Список снарядов сохранен в {filePath}");
    }
    public void DeleteAmmo()
    {
        AmmoDropdown.value = 0;
        AmmoDict.Remove(AmmoDict.Keys.ElementAt(IndexToDelete-1));
        SaveAmmoList();
        UpdateAmmoDropdown();
        IndexToDelete = -1;
    }

}
