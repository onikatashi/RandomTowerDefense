using UnityEngine;

public class TowerYSortController : MonoBehaviour
{
    // Y-Sorting 배수
    private const float Y_SORT_FACTOR = -100f;

    // 타워 오브젝트의 SpriteRenderer 배열
    private SpriteRenderer[] towerRenderers;

    void Start()
    {
        towerRenderers = GetComponentsInChildren<SpriteRenderer>();

        ApplyYSorting();
    }

    private void ApplyYSorting()
    {
        // 타워 루트 오브젝트의 월드 Y좌표
        float worldY = transform.position.y;

        // Sorting Order 계산 (Y값이 낮을수록 높은 정렬 순서)
        int sortingOrder = Mathf.RoundToInt(worldY * Y_SORT_FACTOR);

        // 검색된 모든 SpriteRenderer에 Sorting Order 적용
        foreach (SpriteRenderer renderer in towerRenderers)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }
    }
}