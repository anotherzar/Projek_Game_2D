using UnityEngine;
using System.Collections;

public class LostChildFade : MonoBehaviour
{
    public float fadeSpeed = 2f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    public void StartFade()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;

            Color c = sr.color;
            c.a = alpha;

            sr.color = c;

            yield return null;
        }
    }
}