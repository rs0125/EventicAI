using System;
using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher : MonoBehaviour
{
    public LightManager lightManager;
    public WallManager wallManager;
    public PlantManager plantManager;

    private Dictionary<string, Action<Dictionary<string, object>>> handlers;

    private void Awake()
    {
        if (lightManager == null) lightManager = GetComponent<LightManager>();
        if (wallManager == null) wallManager = GetComponent<WallManager>();
        if (plantManager == null) plantManager = GetComponent<PlantManager>();

        handlers = new Dictionary<string, Action<Dictionary<string, object>>>()
        {
            { "SetLightState", HandleSetLightState },
            { "SetLightIntensity", HandleSetLightIntensity },
            { "SetWallColor", HandleSetWallColor },
            { "TogglePlant", HandleTogglePlant } // unified handler
        };
    }

    public void ProcessBackendResponse(string json)
    {
        BackendResponse resp = null;
        try
        {
            resp = Newtonsoft.Json.JsonConvert.DeserializeObject<BackendResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed parsing backend JSON: {e.Message}");
            return;
        }

        if (resp == null)
        {
            Debug.LogWarning("Backend response null");
            return;
        }

        Debug.Log($"NPC response: {resp.npc_response}");

        if (resp.events == null || resp.events.Count == 0) return;

        foreach (var ev in resp.events)
        {
            if (string.IsNullOrEmpty(ev.name)) continue;

            Dictionary<string, object> paramDict = ev.parameters ?? new Dictionary<string, object>();

            if (handlers.TryGetValue(ev.name, out var handler))
            {
                try { handler.Invoke(paramDict); }
                catch (Exception ex) { Debug.LogError($"Error executing handler for {ev.name}: {ex}"); }
            }
            else
            {
                Debug.LogWarning($"No handler registered for event name: {ev.name}");
            }
        }
    }

    // ---- Handlers ----

    private void HandleSetLightState(Dictionary<string, object> p)
    {
        if (!TryGetString(p, "area", out string area)) return;
        if (!TryGetBoolOrStringBool(p, "state", out bool state)) return;
        lightManager.SetLightsState(area, state);
    }

    private void HandleSetLightIntensity(Dictionary<string, object> p)
    {
        if (!TryGetString(p, "area", out string area)) return;
        if (!TryGetFloat(p, "intensity", out float intensity)) return;
        lightManager.SetLightsIntensity(area, intensity);
    }

    private void HandleSetWallColor(Dictionary<string, object> p)
    {
        if (!TryGetString(p, "area", out string area)) return;
        if (!TryGetString(p, "color", out string hex)) return;
        wallManager.SetWallColor(area, hex);
    }

    private void HandleTogglePlant(Dictionary<string, object> p)
    {
        if (!TryGetString(p, "area", out string area)) return;

        // Accept both "add" and "active"
        bool active;
        if (TryGetBool(p, "add", out active) || TryGetBool(p, "active", out active))
        {
            plantManager.SetPlantsActive(area, active);
        }
        else
        {
            Debug.LogWarning("TogglePlant event missing 'add' or 'active' parameter.");
        }
    }


    // ---- helpers ----

    private bool TryGetString(Dictionary<string, object> p, string key, out string value)
    {
        value = null;
        if (!p.TryGetValue(key, out object o)) return false;
        value = o?.ToString();
        return !string.IsNullOrEmpty(value);
    }

    private bool TryGetFloat(Dictionary<string, object> p, string key, out float val)
    {
        val = 0f;
        if (!p.TryGetValue(key, out object o)) return false;
        if (o is long l) { val = l; return true; }
        if (o is int i) { val = i; return true; }
        if (o is float f) { val = f; return true; }
        if (o is double d) { val = (float)d; return true; }
        if (float.TryParse(o.ToString(), out float parsed)) { val = parsed; return true; }
        return false;
    }

    private bool TryGetBool(Dictionary<string, object> p, string key, out bool val)
    {
        val = false;
        if (!p.TryGetValue(key, out object o)) return false;
        if (o is bool b) { val = b; return true; }
        string s = o.ToString().ToLower();
        if (s == "true" || s == "1" || s == "on") { val = true; return true; }
        if (s == "false" || s == "0" || s == "off") { val = false; return true; }
        return false;
    }

    private bool TryGetBoolOrStringBool(Dictionary<string, object> p, string key, out bool val)
    {
        return TryGetBool(p, key, out val) || (p.TryGetValue(key, out object o) && TryParseOnOffString(o.ToString(), out val));
    }

    private bool TryParseOnOffString(string s, out bool val)
    {
        val = false;
        if (string.IsNullOrEmpty(s)) return false;
        s = s.ToLower();
        if (s == "on") { val = true; return true; }
        if (s == "off") { val = false; return true; }
        return false;
    }
}