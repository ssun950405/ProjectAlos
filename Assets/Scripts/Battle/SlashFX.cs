using System.Collections;
using UnityEngine;

public class SlashFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float lifeTime = 0.10f;
    [SerializeField] private float startScaleMult = 1.0f;
    [SerializeField] private float endScaleMult = 1.25f;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(CoLife());
    }

    private IEnumerator CoLife()
    {
        float t = 0f;

        // 시작값 저장
        Vector3 baseScale = transform.localScale;
        Color baseColor = new Color(1f, 0.85f, 0.85f, 1f); // 살짝 붉은 흰색

        while (t < lifeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / lifeTime);

            // 살짝 커졌다가 사라지는 느낌
            float sm = Mathf.Lerp(startScaleMult, endScaleMult, a);
            transform.localScale = baseScale * sm;

            // 알파 페이드아웃
            if (sr != null)
            {
                Color c = baseColor;
                c.a = 1f - a;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
