using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UIRaycastDebug : MonoBehaviour
{
    void Update()
    {
        // New Input System click
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        var es = EventSystem.current;
        if (es == null)
        {
            Debug.LogError("No EventSystem.current in scene!");
            return;
        }

        var data = new PointerEventData(es)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        es.RaycastAll(data, results);

        if (results.Count == 0)
        {
            Debug.Log("UI RaycastAll hit NOTHING.");
        }
        else
        {
            Debug.Log("UI RaycastAll top hit: " + results[0].gameObject.name);
            // Optional: print full stack
            // foreach (var r in results) Debug.Log(" - " + r.gameObject.name);
        }
    }
}