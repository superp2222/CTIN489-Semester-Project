using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup group;
    public float defaultDuration = 1f;

    void Awake()
    {
        if (group == null)
            group = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, duration));
    }

    public void FadeOut(float duration = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, duration));
    }

    IEnumerator FadeRoutine(float target, float duration)
    {
        float start = group.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        group.alpha = target;
    }
}
