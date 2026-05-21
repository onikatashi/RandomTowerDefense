using UnityEngine;
using static GameEnums;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("적 유닛 정보")]
    public string enemyName;            // 적 이름
    public float hp;                    // 적 체력
    public float speed;                 // 적 이동속도
    public float armor;                 // 적 방어력
    public ArmorType armorType;         // 적 방어 유형 타입
    public int rewardGold;              // 적 처치 시 획득 골드
    public GameObject enemyPrefab;      // 적 프리팹
    public bool isBoss = false;         // 보스 몬스터인가?
    public bool isMission = false;      // 미션 몬스터인가?
}