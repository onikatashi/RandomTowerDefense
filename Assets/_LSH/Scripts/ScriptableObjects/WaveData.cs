using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class WaveInfo
    {
        [Header("웨이브 세부 설정")]
        public EnemyData enemyData;         // 웨이브에 등장할 적 데이터
        public int enemyCount = 30;         // 해당 웨이브에 스폰되는 적의 총 마리 수
        public float spawnInterval = 0.5f;  // 적과 적 사이의 스폰 시간 간격 (딜레이)
    }

    public WaveInfo[] waves;                // 전체 웨이브 정보 배열
}