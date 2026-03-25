using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a boss intro card (name + fade in/out).
/// Addresses issues #38, #41 (boss intro UI, boss name display).
/// </summary>
public class BossIntroUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text bossSubtitleText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 0.6f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float fadeOutTime = 0.8f;

    private void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    /// <summary>Show the boss intro panel with the given name and subtitle.</summary>
    public void Show(string bossName, string subtitle = "")
    {
        if (bossNameText != null)    bossNameText.text = bossName;
        if (bossSubtitleText != null) bossSubtitleText.text = subtitle;
        gameObject.SetActive(true);
        StartCoroutine(IntroSequence());
    }

    private System.Collections.IEnumerator IntroSequence()
    {
        // Fade in
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup != null) canvasGroup.alpha = t / fadeInTime;
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup != null) canvasGroup.alpha = 1f - t / fadeOutTime;
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
