using UnityEngine;

public class MonsterYSortController : MonoBehaviour
{
    // Y-Sorting 배수
    private const float Y_SORT_FACTOR = -100f;

    // 몬스터의 SpriteRenderer 배열
    [SerializeField]
    private SpriteRenderer[] monsterRenderers;

    // 체력바는 몬스터보다 항상 맨 앞에 보이도록 설정
    [SerializeField]
    private Canvas healthBarCanvas;

    // 이전 위치 저장 (최적화용)
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        ApplySortingOrder();
    }

    private void LateUpdate()
    {
        if (transform.position != lastPosition)
        {
            ApplySortingOrder();
            lastPosition = transform.position;
        }
    }

    private void ApplySortingOrder()
    {
        // 현재 위치의 Y좌표 획득
        float worldY = transform.position.y;

        // Sorting Order 계산
        int sortingOrder = Mathf.RoundToInt(worldY * Y_SORT_FACTOR);

        // 몬스터의 모든 SpriteRenderer에 적용
        foreach (SpriteRenderer renderer in monsterRenderers)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }

        if (healthBarCanvas != null)
        {
            healthBarCanvas.sortingOrder = sortingOrder + 1;
        }
    }
}