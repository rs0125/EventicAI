using System.Collections.Generic;


[System.Serializable]
public class BackendEvent
{
    public string name;
    public Dictionary<string, object> parameters;
}

// Helper classes to deserialize the full backend response
[System.Serializable]
public class ApiEvent
{
    public string name;
    public Dictionary<string, object> parameters;
}

[System.Serializable]
public class BackendResponse
{
    public string npc_response;
    public List<ApiEvent> events;
}
