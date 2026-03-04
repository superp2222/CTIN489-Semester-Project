using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public Transform hinge;              // Door_Hinge transform (usually this)
    public float openAngle = 90f;        // degrees
    public float openCloseTime = 0.35f;  // seconds

    private bool isOpen = false;
    private bool isBusy = false;

    void Reset()
    {
        // Auto-fill hinge if possible
        hinge = transform;
    }

    public void Interact()
    {
        if (isBusy) return;
        StartCoroutine(RotateDoor());
    }

    public string GetPrompt()
    {
        return isOpen ? "Press E to close door" : "Press E to open door";
    }

    private IEnumerator RotateDoor()
    {
        isBusy = true;

        float startY = hinge.localEulerAngles.y;
        // Convert to -180..180 style to avoid 0/360 snapping issues
        if (startY > 180f) startY -= 360f;

        float targetY = isOpen ? 0f : openAngle;

        float t = 0f;
        while (t < openCloseTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / openCloseTime);
            float y = Mathf.Lerp(startY, targetY, lerp);
            hinge.localRotation = Quaternion.Euler(0f, y, 0f);
            yield return null;
        }

        hinge.localRotation = Quaternion.Euler(0f, targetY, 0f);

        isOpen = !isOpen;
        isBusy = false;
    }
}