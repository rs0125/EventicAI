using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaPlants
{
    public string areaName;
    public GameObject plantParent; // empty parent object that holds all plants for this area
}

public class PlantManager : MonoBehaviour
{
    public List<AreaPlants> areaPlants;
    private Dictionary<string, GameObject> plantParents;

    private void Awake()
    {
        plantParents = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var ap in areaPlants)
        {
            if (ap.plantParent != null)
            {
                plantParents[ap.areaName] = ap.plantParent;
            }
        }
    }

    public void SetPlantsActive(string area, bool active)
    {
        if (!plantParents.TryGetValue(area, out var parent))
        {
            Debug.LogWarning($"No plant parent found for area '{area}'");
            return;
        }

        parent.SetActive(active);
    }
}