using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine;

public class AmmoDataManager
{
    private string jsonFilePath;

    public AmmoDataManager(string filePath)
    {
        jsonFilePath = filePath;
    }

    // Загрузка данных из JSON
    public AmmoOptionsList LoadAmmoOptions()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            return JsonUtility.FromJson<AmmoOptionsList>(json);
        }
        else
        {
            return new AmmoOptionsList(); // Если файла нет, возвращаем пустой список
        }
    }

    // Сохранение данных в JSON
    public void SaveAmmoOptions(AmmoOptionsList ammoOptionsList)
    {
        string json = JsonUtility.ToJson(ammoOptionsList, true);
        File.WriteAllText(jsonFilePath, json);
    }
}

[System.Serializable]
public class AmmoOption
{
    public string name;
    public int radius;
    public int numberOfExplosions;
    public int scatterRadius;

    public AmmoOption(string name, int radius, int numberOfExplosions, int scatterRadius)
    {
        this.name = name;
        this.radius = radius;
        this.numberOfExplosions = numberOfExplosions;
        this.scatterRadius = scatterRadius;
    }
}

[System.Serializable]
public class AmmoOptionsList
{
    public List<AmmoOption> ammoOptions;

    public AmmoOptionsList()
    {
        ammoOptions = new List<AmmoOption>();
    }
}
