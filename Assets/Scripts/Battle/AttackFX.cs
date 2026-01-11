using System.Collections;
using UnityEngine;

public class AttackFX : MonoBehaviour
{
    [SerializeField] private Transform moveTarget; // 움직일 대상(보통 캐릭 Root)
    [SerializeField] private float dashDistance = 0.18f;
    [SerializeField] private float dashTime = 0.06f;

    private Vector3 originalLocalPos;
    private Coroutine running;

    private void Awake()
    {
        if (moveTarget == null) moveTarget = transform;
        originalLocalPos = moveTarget.localPosition;
    }

    // dir: +1이면 오른쪽으로, -1이면 왼쪽으로
    public void Play(int dir)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoPlay(dir));
    }

    private IEnumerator CoPlay(int dir)
    {
        Vector3 start = originalLocalPos;
        Vector3 peak = originalLocalPos + new Vector3(dashDistance * dir, 0f, 0f);

        // forward
        float t = 0f;
        while (t < dashTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / dashTime);
            moveTarget.localPosition = Vector3.Lerp(start, peak, a);
            yield return null;
        }

        // back
        t = 0f;
        while (t < dashTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / dashTime);
            moveTarget.localPosition = Vector3.Lerp(peak, start, a);
            yield return null;
        }

        moveTarget.localPosition = originalLocalPos;
        running = null;
    }
}
