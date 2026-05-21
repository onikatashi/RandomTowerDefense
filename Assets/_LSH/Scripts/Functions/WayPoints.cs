using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{
    // 웨이포인트들을 저장할 리스트
    public static List<Transform> points = new List<Transform>();

    private void Awake()
    {
        // 기존 웨이포인트 데이터 초기화
        points.Clear();

        // 웨이포인트 오브젝트의 모든 자식 오브젝트들을 순회하며
        // 웨이포인트 리스트에 추가
        for (int i = 0; i < transform.childCount; i++)
        {
            points.Add(transform.GetChild(i));
        }
    }

    // 씬 뷰에서 웨이포인트의 연결 상태를 시각적으로 확인하기 위한 기즈모 그리기
    private void OnDrawGizmos()
    {
        // 각 웨이포인트 위치와 연결선 그리기 (노란색)
        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Vector3 current = transform.GetChild(i).position;
            Vector3 next = transform.GetChild(i + 1).position;
            Gizmos.DrawLine(current, next);
            Gizmos.DrawSphere(current, 0.2f);
        }

        // 시작점은 파란색, 마지막 도착점은 빨간색으로 표시
        if (transform.childCount > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.GetChild(0).position, 0.3f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.GetChild(transform.childCount - 1).position, 0.3f);
        }
    }
}