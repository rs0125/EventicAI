using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaLightGroup
{
    public string areaName;
    public Transform lightParent; // Parent object that contains all the lights of that area
}

public class LightManager : MonoBehaviour
{
    [Tooltip("Assign each area's parent object that groups its lights")]
    public List<AreaLightGroup> areaLightGroups;

    private Dictionary<string, Transform> lightGroupsMap;

    private void Awake()
    {
        lightGroupsMap = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in areaLightGroups)
        {
            if (!lightGroupsMap.ContainsKey(group.areaName) && group.lightParent != null)
            {
                lightGroupsMap[group.areaName] = group.lightParent;
            }
        }
    }

    public void SetLightsState(string area, bool on)
    {
        if (!lightGroupsMap.TryGetValue(area, out var parent))
        {
            Debug.LogWarning($"No light parent found for area {area}");
            return;
        }

        parent.gameObject.SetActive(on);
    }

    public void SetLightsIntensity(string area, float intensity)
    {
        if (!lightGroupsMap.TryGetValue(area, out var parent))
        {
            Debug.LogWarning($"No light parent found for area {area}");
            return;
        }

        // Change intensity of all Light components under this parent
        foreach (var light in parent.GetComponentsInChildren<Light>())
        {
            if (light != null)
                light.intensity = intensity;
        }
    }
}
