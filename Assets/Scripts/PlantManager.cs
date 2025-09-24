using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaPlants
{
    public string areaName;
    public Transform plantParent; // parent transform for spawned plants in that area
    public List<GameObject> plantPrefabs; // available prefabs (index/lookup by name)
    public List<GameObject> existingPlants; // optional pre-placed plants to manage
}

public class PlantManager : MonoBehaviour
{
    public List<AreaPlants> areaPlants;
    private Dictionary<string, AreaPlants> plantMap;

    private void Awake()
    {
        plantMap = new Dictionary<string, AreaPlants>(StringComparer.OrdinalIgnoreCase);
        foreach (var ap in areaPlants)
        {
            plantMap[ap.areaName] = ap;
            if (ap.existingPlants == null) ap.existingPlants = new List<GameObject>();
        }
    }

    // positionStr could be parsed (e.g., "east_window") or ignored for now
    public void AddPlant(string area, string plantType, string positionStr)
    {
        if (!plantMap.TryGetValue(area, out var areaData))
        {
            Debug.LogWarning($"No plant area {area}");
            return;
        }

        GameObject prefab = null;
        if (!string.IsNullOrEmpty(plantType))
        {
            prefab = areaData.plantPrefabs.Find(p => p != null && p.name.Equals(plantType, StringComparison.OrdinalIgnoreCase));
        }
        if (prefab == null)
        {
            // fallback to first prefab
            if (areaData.plantPrefabs.Count > 0) prefab = areaData.plantPrefabs[0];
            else { Debug.LogWarning("No plant prefabs assigned"); return; }
        }

        Vector3 spawnPos = areaData.plantParent != null ? areaData.plantParent.position : Vector3.zero;
        // rudimentary position handling - you can expand mapping of named positions to transforms
        var instance = Instantiate(prefab, spawnPos, Quaternion.identity, areaData.plantParent);
        areaData.existingPlants.Add(instance);
    }

    public void RemovePlant(string area, string plantType)
    {
        if (!plantMap.TryGetValue(area, out var areaData))
        {
            Debug.LogWarning($"No plant area {area}");
            return;
        }

        // if plantType specified, try to find a plant with that prefab name
        GameObject toRemove = null;
        if (!string.IsNullOrEmpty(plantType))
        {
            toRemove = areaData.existingPlants.Find(g => g != null && g.name.StartsWith(plantType));
        }
        else
        {
            // remove last added by default
            if (areaData.existingPlants.Count > 0) toRemove = areaData.existingPlants[areaData.existingPlants.Count - 1];
        }

        if (toRemove != null)
        {
            areaData.existingPlants.Remove(toRemove);
            Destroy(toRemove);
        }
        else
        {
            Debug.LogWarning($"No plant found to remove in area {area} with type {plantType}");
        }
    }
}
