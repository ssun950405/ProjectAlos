using UnityEngine;

public class FootBarFollowWorld : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;           // 따라갈 대상(플레이어)
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, -0.8f, 0f); // 발밑 오프셋

    [Header("Camera")]
    [SerializeField] private Camera cam;                // 보통 Main Camera

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (cam == null) cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position + worldOffset);

        // 화면 뒤로 가면 숨기기(선택)
        if (screenPos.z < 0f)
        {
            rt.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!rt.gameObject.activeSelf) rt.gameObject.SetActive(true);
        }

        // Overlay Canvas면 position에 스크린 좌표를 그대로 넣으면 됨
        rt.position = screenPos;
    }

    // 인스펙터에서 못 넣었을 때, 런타임에 할당용
    public void SetTarget(Transform t) => target = t;
}
