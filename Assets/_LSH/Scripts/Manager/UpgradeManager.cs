using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    // 업그레이드 상태를 관리하기 위한 시리얼라이즈 클래스
    [System.Serializable]
    public class UpgradeState
    {
        public TowerType towerType;         // 업그레이드할 타워 타입
        public int level = 0;               // 업그레이드 수치 (현재 레벨)
        public int baseCost = 20;           // 초기 업그레이드 비용
        public int costIncrease = 3;        // 레벨업 시 비용 증가량

        // 현재 레벨 기준 다음 업그레이드 비용 계산 프로퍼티
        public int CurrentCost
        {
            get
            {
                return baseCost + (costIncrease * level);
            }
        }
    }

    private Dictionary<TowerType, UpgradeState> upgradeState;
    UIManager uiManager;
    SoundManager soundManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitUpgrade();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
        soundManager = SoundManager.Instance;
    }

    // 업그레이드 데이터 초기화
    private void InitUpgrade()
    {
        upgradeState = new Dictionary<TowerType, UpgradeState>();

        // 5가지 기본 타워 타입 업그레이드 정보 초기화 등록
        upgradeState.Add(TowerType.Archer, new UpgradeState { towerType = TowerType.Archer });
        upgradeState.Add(TowerType.Cannon, new UpgradeState { towerType = TowerType.Cannon });
        upgradeState.Add(TowerType.Flame, new UpgradeState { towerType = TowerType.Flame });
        upgradeState.Add(TowerType.Lightning, new UpgradeState { towerType = TowerType.Lightning });
        upgradeState.Add(TowerType.Magic, new UpgradeState { towerType = TowerType.Magic });
    }

    // UIManager 등에서 특정 타워의 업그레이드 상태를 가져오기 위한 Getter
    public UpgradeState GetUpgradeState(TowerType type)
    {
        upgradeState.TryGetValue(type, out var state);
        return state;
    }

    // 타워 업그레이드 버튼 클릭 시 호출되는 함수
    public bool TryUpgradeTower(TowerType type)
    {
        if (!upgradeState.TryGetValue(type, out var state))
        {
            Debug.LogError("해당 타워 타입을 찾을 수 없습니다.");
            return false;
        }

        int requireGold = state.CurrentCost;

        if (GameManager.Instance == null)
        {
            return false;
        }

        // 업그레이드 도중 UI 오작동을 방지하기 위해 열려있는 타워 정보창 닫기
        uiManager.HideTowerActionPanel();
        uiManager.HideTowerInfoPanel();

        // 소지 골드가 소모 골드보다 많을 때 (조건 만족 시)
        if (GameManager.Instance.gold >= requireGold)
        {
            soundManager.Play("UpgradeClick");
            GameManager.Instance.gold -= requireGold;
            state.level++;

            // 필드에 배치된 동일한 타입의 타워들에게 업그레이드 적용
            ApplyUpgradeToFieldTower(type, state);
            return true;
        }
        else
        {
            // 골드 부족으로 강화 실패
            return false;
        }
    }

    // 맵에 배치된 타워들을 전수조사하여 강화 수치를 적용하는 헬퍼 함수
    private void ApplyUpgradeToFieldTower(TowerType type, UpgradeState state)
    {
        // 씬에 배치된 모든 타워 컴포넌트 탐색
        Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);

        foreach (Tower tower in allTowers)
        {
            // 일치하는 타워 타입만 선별하여 동기화
            if (tower.towerData.towerType == type)
            {
                tower.ApplyUpgrade(state);
            }
        }
    }
}