using UnityEngine;

public class KeypadDoor : MonoBehaviour
{
    [Header("Door State")]
    [SerializeField] private bool isOpen = false;

    [Header("Opening (Choose One)")]
    public Animator doorAnimator;
    public string openTriggerName = "Open";

    [Header("Fallback: Rotate Open")]
    public Transform doorTransform;           // assign the actual door mesh transform if different
    public Vector3 openRotationEuler = new Vector3(0f, 90f, 0f);
    public float openSpeed = 2.5f;

    private Quaternion closedRot;
    private Quaternion openRot;

    void Awake()
    {
        if (doorTransform == null) doorTransform = transform;

        closedRot = doorTransform.rotation;
        openRot = closedRot * Quaternion.Euler(openRotationEuler);
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        // Animator path
        if (doorAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            doorAnimator.SetTrigger(openTriggerName);
            return;
        }

        // Rotation fallback
        StopAllCoroutines();
        StartCoroutine(RotateOpen());
    }

    private System.Collections.IEnumerator RotateOpen()
    {
        while (Quaternion.Angle(doorTransform.rotation, openRot) > 0.2f)
        {
            doorTransform.rotation = Quaternion.Slerp(doorTransform.rotation, openRot, Time.deltaTime * openSpeed);
            yield return null;
        }

        doorTransform.rotation = openRot;
    }

    public bool IsOpen() => isOpen;
}