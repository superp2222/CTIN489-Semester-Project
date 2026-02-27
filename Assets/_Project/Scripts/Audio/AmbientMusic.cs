using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    public AudioSource source;

    void Awake()
    {
        if (source == null)
            source = GetComponent<AudioSource>();

        if (!source.isPlaying)
            source.Play();
    }
}
