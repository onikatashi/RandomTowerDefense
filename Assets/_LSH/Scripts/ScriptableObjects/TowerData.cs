using UnityEngine;
using static GameEnums;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("타워 기본 스펙")]
    public TowerType towerType;             // 타워 타입 (궁수, 캐논 등)
    public TowerRank rank;                  // 타워 등급 (Normal, Epic 등)
    public AttackType attackType;           // 타워 공격 타입 (물리, 마법 등)
    public string towerName;                // 타워 이름
    public float attack;                    // 타워 기본 공격력
    public float attackSpeed;               // 타워 기본 공격속도
    public float attackRange;               // 타워 기본 사거리
    public float attackSplashRange;         // 타워 기본 스플래시 범위
    public int attackTargetCount;           // 타워 동시 공격 타깃 수
    public GameObject towerPrefab;          // 타워 프리팹

    [Header("타워 업그레이드 가중치")]
    public float attackBonus;               // 업그레이드 1강당 공격력 증가량
    public float attackSpeedBonus;          // 업그레이드 1강당 공격속도 증가량
    public float attackRangeBonus;          // 업그레이드 1강당 사거리 증가량
    public float attackSplashRangeBonus;    // 업그레이드 1강당 스플래시 범위 증가량
    public float attackTargetCountBonus;    // 업그레이드 1강당 타깃 수 증가량
}