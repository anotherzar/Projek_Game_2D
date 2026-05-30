using UnityEngine;
using System.Collections;

public class ShadowInkController : MonoBehaviour
{
    private SpriteRenderer shadow;

    private Coroutine moveCoroutine;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        shadow = GetComponent<SpriteRenderer>();
    }

    // =========================
    // APPEAR
    // =========================

    public void Appear()
    {
        gameObject.SetActive(true);

        StartCoroutine(AppearRoutine());
    }

    IEnumerator AppearRoutine()
    {
        Color color = shadow.color;

        color.a = 0f;

        shadow.color = color;

        // muncul perlahan

        while (color.a < 0.2f)
        {
            color.a += Time.deltaTime * 0.2f;

            shadow.color = color;

            yield return null;
        }

        color.a = 0.2f;

        shadow.color = color;
    }

    // =========================
    // CHANGE ALPHA
    // =========================

    public void ChangeAlpha(float targetAlpha)
    {
        StartCoroutine(ChangeAlphaRoutine(targetAlpha));
    }

    IEnumerator ChangeAlphaRoutine(float targetAlpha)
    {
        Color color = shadow.color;

        float startAlpha = color.a;

        float timer = 0f;

        float duration = 1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    timer / duration
                );

            color.a =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            shadow.color = color;

            yield return null;
        }

        color.a = targetAlpha;

        shadow.color = color;
    }

    // =========================
    // START MOVING
    // =========================

    public void StartMoving()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine =
            StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        Vector3 startPos =
            transform.position;

        Vector3 endPos =
            startPos + new Vector3(3f, 0, 0);

        float timer = 0f;

        float duration = 5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    timer / duration
                );

            // gerak perlahan

            transform.position =
                Vector3.Lerp(
                    startPos,
                    endPos,
                    t
                );

            yield return null;
        }
    }

    // =========================
    // FADE OUT
    // =========================

    public void FadeOut()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine =
            StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        Color color = shadow.color;

        float startAlpha = color.a;

        float timer = 0f;

        float duration = 2f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    timer / duration
                );

            color.a =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            shadow.color = color;

            yield return null;
        }

        color.a = 0f;

        shadow.color = color;

        gameObject.SetActive(false);
    }
}