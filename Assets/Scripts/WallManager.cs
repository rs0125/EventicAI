using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaWalls
{
    public string areaName;
    public List<Renderer> wallRenderers;
}

public class WallManager : MonoBehaviour
{
    public List<AreaWalls> areaWalls;

    private Dictionary<string, List<Renderer>> wallMap;

    private void Awake()
    {
        wallMap = new Dictionary<string, List<Renderer>>(StringComparer.OrdinalIgnoreCase);
        foreach (var aw in areaWalls)
        {
            if (!wallMap.ContainsKey(aw.areaName))
                wallMap[aw.areaName] = new List<Renderer>();
            if (aw.wallRenderers != null) wallMap[aw.areaName].AddRange(aw.wallRenderers);
        }
    }

    public void SetWallColor(string area, string hexColor)
    {
        if (!wallMap.TryGetValue(area, out var list))
        {
            Debug.LogWarning($"No walls for area {area}");
            return;
        }

        if (!ColorUtility.TryParseHtmlString(hexColor, out Color col))
        {
            Debug.LogWarning($"Invalid hex color {hexColor}");
            return;
        }

        foreach (var r in list)
        {
            if (r == null) continue;
            // use instance material to avoid changing shared
            foreach (var mat in r.materials)
            {
                mat.color = col;
            }
        }
    }
}
