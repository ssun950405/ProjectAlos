using System.Collections;
using UnityEngine;

public class HitFX : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform shakeTarget;     // 흔들 대상(보통 캐릭 Root)
    [SerializeField] private SpriteRenderer sprite;     // 플래시 대상

    [Header("Tuning")]
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.08f;
    [SerializeField] private float flashDuration = 0.10f;

    private Vector3 originalLocalPos;
    private Color originalColor;
    private Coroutine running;

    private void Awake()
    {
        if (shakeTarget == null) shakeTarget = transform;

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        originalLocalPos = shakeTarget.localPosition;
        if (sprite != null) originalColor = sprite.color;
    }

    public void Play()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        // Flash (Red)
        if (sprite != null)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            sprite.color = originalColor;
        }

        // Shake
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            Vector2 rnd = Random.insideUnitCircle * shakeStrength;
            shakeTarget.localPosition = originalLocalPos + new Vector3(rnd.x, rnd.y, 0f);
            yield return null;
        }

        shakeTarget.localPosition = originalLocalPos;
        running = null;
    }
}
