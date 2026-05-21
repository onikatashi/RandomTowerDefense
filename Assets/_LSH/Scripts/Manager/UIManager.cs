using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GameEnums;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("플레이어 정보: 체력/보유골드")]
    public TextMeshProUGUI playerHpValue;                   // 플레이어 체력
    public TextMeshProUGUI playerGoldValue;                 // 플레이어 보유골드

    [Header("웨이브 정보")]
    public TextMeshProUGUI currentWave;                     // 현재 웨이브 단계
    public TextMeshProUGUI waveTimeValue;                   // 다음 웨이브까지 남은 시간

    [Header("일반 웨이브 적 정보")]
    public TextMeshProUGUI enemyMaxHpValue;                 // 적 최대 체력
    public TextMeshProUGUI enemyArmorValue;                 // 적 방어력
    public TextMeshProUGUI enemyArmorTypeValue;             // 적 방어 유형 타입
    public TextMeshProUGUI enemySpeedValue;                 // 적 이동속도

    [Header("미션 적 정보")]
    public Button missionButton;                            // 미션 소환 버튼
    public TextMeshProUGUI missionEnemyMaxHpValue;          // 미션 적 최대체력
    public TextMeshProUGUI missionEnemyArmorValue;          // 미션 적 방어력
    public TextMeshProUGUI missionEnemyArmorTypeValue;      // 미션 적 방어 유형 타입
    public TextMeshProUGUI missionEnemySpeedValue;          // 미션 적 이동속도
    public Image missionButtonImage;                        // 미션 쿨타임을 표시하는 버튼 이미지 (Fill Amount용)

    [Header("보스 적 정보")]
    public GameObject bossEnemyInfoPanel;
    public TextMeshProUGUI bossEnemyMaxHpValue;             // 보스 몬스터 최대체력
    public TextMeshProUGUI bossEnemyArmorValue;             // 보스 몬스터 방어력
    public TextMeshProUGUI bossEnemyArmorTypeValue;         // 보스 몬스터 방어 유형 타입
    public TextMeshProUGUI bossEnemySpeedValue;             // 보스 몬스터 이동속도

    [Header("타워 정보")]
    public GameObject towerInfoPanel;                       // 타워 정보 패널
    public TextMeshProUGUI towerNameValue;                  // 타워 이름
    public TextMeshProUGUI towerAttackTypeValue;            // 타워 공격 타입
    public TextMeshProUGUI towerAttackValue;                // 타워 공격력
    public TextMeshProUGUI towerAttackSpeedValue;           // 타워 공격속도
    public TextMeshProUGUI towerAttackRangeValue;           // 타워 사거리
    public TextMeshProUGUI towerAttackSplashRangeValue;     // 스플래시 범위
    public TextMeshProUGUI towerAttackTargetValue;          // 동시 공격 가능한 타깃 수

    [Header("타워 액션 컴포넌트")]
    public Button buildTowerButton;                         // 타워 건설 버튼
    public GameObject towerActionPanel;                     // 합성, 판매 버튼 패널
    public Button combineButton;                            // 합성 버튼
    public Button sellButton;                               // 판매 버튼

    [Header("UI 배경 이미지")]
    public GameObject bossEnemyInfoBackground;              // 보스 정보 배경 이미지
    public GameObject towerInfoBackground;                  // 타워 정보 배경 이미지

    [Header("게임 종료 UI")]
    public GameObject endGameUIPanel;       // 게임 종료 시 뜨는 팝업 패널
    public TextMeshProUGUI gameEndText;     // 게임 승리 또는 패배에 따른 결과 텍스트
    public Button yesButton;                // 게임을 다시 시작하기 위한 "예" 버튼
    public Button noButton;                 // 게임을 완전히 종료하기 위한 "아니오" 버튼

    [Header("설정창 관련 버튼")]
    // 결과창 창 패널에 같이 들어가도 상관없음
    public GameObject settingPanel;                         // 설정 패널
    public Button settingButton;                            // 설정 버튼
    public TextMeshProUGUI currentBGMTitle;                 // 현재 BGM 제목
    public Button nextButton;                               // 다음 BGM 재생 버튼
    public Button previousButton;                           // 이전 BGM 재생 버튼

    [Header("일시정지 관련")]
    public GameObject pausePanel;                           // 일시정지 패널
    public Button pauseButton;                              // 일시정지 버튼
    public Image pauseImage;                                // 일시정지 토글용 이미지
    public Image resumeImage;                               // 이어하기용 이미지
    public bool isPause = false;                            // 일시정지 상태인지 확인
    public bool isSetting = false;                          // 설정창이 열려있는지 확인

    [Header("볼륨 조절 컴포넌트")]
    public Button settingExitButton;                        // 설정 나가기 버튼
    public Slider masterSlider;                             // 전체 볼륨 슬라이더
    public Slider bgmSlider;                                // BGM 볼륨 슬라이더
    public Slider sfxSlider;                                // SFX 볼륨 슬라이더
    public TextMeshProUGUI masterText;                      // 전체 볼륨 텍스트 (%)
    public TextMeshProUGUI bgmText;                         // BGM 볼륨 텍스트 (%)
    public TextMeshProUGUI sfxText;                         // SFX 볼륨 텍스트 (%)

    // 타워 클릭 시 나타나는 합성, 판매 버튼의 패널 크기
    float panelWidth;                                       // 패널의 가로 길이                
    float panelHeight;                                      // 패널의 세로 길이

    // 업그레이드 타워 타입별 UI 클래스
    [System.Serializable]
    public class TotalUpgradeUI
    {
        public TextMeshProUGUI upgradeLevelValue;           // 현재 업그레이드 단계 (레벨)
        public TextMeshProUGUI upgradeCostValue;            // 다음 업그레이드 비용 (버튼 텍스트)
        public Button upgradeButton;                        // 업그레이드 버튼
        public TowerType towerType;                         // 타워 타입
    }

    [Header("업그레이드 패널")]
    public TotalUpgradeUI archerUI;
    public TotalUpgradeUI cannonUI;
    public TotalUpgradeUI magicUI;
    public TotalUpgradeUI flameUI;
    public TotalUpgradeUI lightningUI;

    public Tower selectedTower = null;                      // 현재 필드에서 선택된 타워 인스턴스

    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private WaveManager waveManager;
    private TowerManager towerManager;
    private SoundManager soundManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        upgradeManager = UpgradeManager.Instance;
        waveManager = WaveManager.Instance;
        towerManager = TowerManager.Instance;
        soundManager = SoundManager.Instance;

        panelWidth = towerActionPanel.GetComponent<RectTransform>().rect.width;
        panelHeight = towerActionPanel.GetComponent<RectTransform>().rect.height;

        // 초기 패널 상태 설정
        HideTowerInfoPanel();
        HideBossInfoPanel();
        endGameUIPanel.SetActive(false);    // 게임 결과 알림 패널 비활성화
        settingPanel.SetActive(false);      // 설정 패널 비활성화
        pausePanel.SetActive(false);        // 일시정지 패널 비활성화

        // 이어하기 이미지 기본 비활성화
        resumeImage.enabled = false;

        // 업그레이드 버튼 이벤트 바인딩
        SetupUpgradeButton(archerUI);
        SetupUpgradeButton(cannonUI);
        SetupUpgradeButton(magicUI);
        SetupUpgradeButton(flameUI);
        SetupUpgradeButton(lightningUI);

        UpdateMissionEnemyUI();

        // 관리 매니저 인스턴스 버튼 이벤트 연결
        // 타워 건설 버튼
        if (buildTowerButton != null && towerManager != null)
        {
            buildTowerButton.onClick.AddListener(towerManager.StartBuilding);
        }

        // 미션 버튼
        if (missionButton != null && waveManager != null)
        {
            missionButton.onClick.AddListener(waveManager.SpawnMissionEnemy);
            SetMissionButtonInteractable(true);
        }

        // 게임 결과창 예/아니오 버튼
        yesButton.onClick.AddListener(gameManager.RestartGame);
        noButton.onClick.AddListener(gameManager.QuitGame);

        // 일시정지 버튼
        pauseButton.onClick.AddListener(PauseGame);

        // BGM 변경 버튼
        nextButton.onClick.AddListener(() => ChangeBGM(1));
        previousButton.onClick.AddListener(() => ChangeBGM(-1));

        // 사운드 관련 슬라이더 값 초기화
        masterSlider.value = soundManager.masterVolume;
        bgmSlider.value = soundManager.bgmVolume;
        sfxSlider.value = soundManager.sfxVolume;

        // 슬라이더 변경 이벤트 리스너 추가
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // 설정 버튼 및 설정창 내의 닫기(X) 버튼
        settingButton.onClick.AddListener(SettingPanelOnOF);
        settingExitButton.onClick.AddListener(ClickSettingExitButton);

        // 사운드 퍼센트 텍스트 초기화 배치
        masterText.text = $"{(masterSlider.value * 100):0} %";
        bgmText.text = $"{(bgmSlider.value * 100):0} %";
        sfxText.text = $"{(sfxSlider.value * 100):0} %";
    }

    // 미션 쿨타임에 따른 미션 버튼 활성화/비활성화 함수
    public void SetMissionButtonInteractable(bool isInteractable)
    {
        if (missionButton != null)
        {
            missionButton.interactable = isInteractable;

            if (missionButtonImage != null)
            {
                if (isInteractable)
                {
                    missionButtonImage.fillAmount = 0f;
                }
            }
        }
    }

    // 골드 보유 상황에 맞게 타워 건설 버튼 활성화 상태 갱신
    public void SetBuildTowerButtonInteractable()
    {
        if (buildTowerButton != null)
        {
            if (gameManager.gold >= towerManager.towerCost)
            {
                buildTowerButton.interactable = true;
            }
            else
            {
                buildTowerButton.interactable = false;
            }
        }
    }

    // 미션 쿨타임 진행 상태를 게이지 이미지로 업데이트
    public void UpdateMissionCooldown(float ratio)
    {
        if (missionButtonImage != null)
        {
            missionButtonImage.fillAmount = ratio;
        }
    }

    // 업그레이드 버튼 클릭 이벤트 세팅
    private void SetupUpgradeButton(TotalUpgradeUI upgradeUI)
    {
        if (upgradeUI != null && upgradeUI.upgradeButton != null)
        {
            upgradeUI.upgradeButton.onClick.AddListener(() =>
            {
                if (upgradeManager != null)
                {
                    upgradeManager.TryUpgradeTower(upgradeUI.towerType);
                    UpdateUpgradeUI(upgradeUI);
                }
            });
        }
    }

    void Update()
    {
        // 플레이어 상태 UI 상시 갱신
        UpdatePlayerStateUI();

        // 웨이브 남은 시간 UI 상시 갱신
        UpdateWaveUI();

        // 웨이브 단계 및 적 세부 정보 UI 상시 갱신
        UpdateEnemyUI();

        // 타워 전체 업그레이드 레벨/비용 UI 상시 갱신
        UpdateTotalUpgradeStateUI();

        // 타워 건설 버튼의 활성화/비활성화 상태를 소지 골드 조건에 따라 상시 체크
        SetBuildTowerButtonInteractable();
    }

    // 플레이어 체력, 보유 골드 텍스트 갱신
    public void UpdatePlayerStateUI()
    {
        if (gameManager == null) return;
        playerHpValue.text = gameManager.playerHp.ToString();
        playerGoldValue.text = gameManager.gold.ToString() + " G";
    }

    // 웨이브 대기 시간 텍스트 갱신
    public void UpdateWaveUI()
    {
        if (waveManager == null) return;

        waveTimeValue.text = String.Format("{0:N1}", waveManager.timeUntilNextWave) + " sec";
    }

    // 현재 웨이브 및 등장할 적의 스펙 정보 UI 갱신
    public void UpdateEnemyUI()
    {
        if (waveManager == null) return;

        if (waveManager.currentWave < waveManager.waveData.waves.Length)
        {
            currentWave.text = "Wave " + (waveManager.currentWave + 1).ToString();
            enemyMaxHpValue.text = waveManager.CurrentWaveEnemyData.hp.ToString();
            enemyArmorValue.text = waveManager.CurrentWaveEnemyData.armor.ToString();
            enemyArmorTypeValue.text = waveManager.CurrentWaveEnemyData.armorType.ToString();
            enemySpeedValue.text = waveManager.CurrentWaveEnemyData.speed.ToString();
        }
    }

    // 소환될 미션 몬스터의 스펙 정보 UI 갱신
    public void UpdateMissionEnemyUI(float multiplier = 1f)
    {
        if (waveManager == null) return;

        waveManager.SetRandomArmorType();

        missionEnemyMaxHpValue.text = String.Format("{0:N1}", waveManager.missionEnemy.hp * multiplier);
        missionEnemyArmorValue.text = String.Format("{0:N1}", waveManager.missionEnemy.armor * multiplier);
        missionEnemyArmorTypeValue.text = waveManager.randomArmorType.ToString();
        missionEnemySpeedValue.text = waveManager.missionEnemy.speed.ToString();
    }

    // 모든 타워 종류별 업그레이드 상태 동기화
    private void UpdateTotalUpgradeStateUI()
    {
        if (upgradeManager == null) return;

        UpdateUpgradeUI(archerUI);
        UpdateUpgradeUI(cannonUI);
        UpdateUpgradeUI(flameUI);
        UpdateUpgradeUI(magicUI);
        UpdateUpgradeUI(lightningUI);
    }

    // 개별 타워 업그레이드 요소(레벨, 비용, 버튼 상호작용성) UI 동기화
    private void UpdateUpgradeUI(TotalUpgradeUI upgradeUI)
    {
        if (upgradeUI == null || upgradeManager == null) return;

        var state = upgradeManager.GetUpgradeState(upgradeUI.towerType);
        if (state == null) return;

        int cost = state.CurrentCost;
        bool canUpgrade = gameManager != null && gameManager.gold >= cost;

        upgradeUI.upgradeLevelValue.text = state.level.ToString();
        upgradeUI.upgradeCostValue.text = cost.ToString() + " G";
        upgradeUI.upgradeButton.interactable = canUpgrade;
    }

    // 타워 오브젝트를 클릭했을 때 액션 및 정보 패널을 띄우는 함수
    public void ShowTowerPanel(Tower tower)
    {
        selectedTower = tower;
        towerInfoPanel.SetActive(true);
        towerInfoBackground.SetActive(true);
        towerActionPanel.SetActive(true);

        // 액션 팝업 패널의 위치를 선택한 타워의 월드 스크린 좌표 기준으로 보정
        Vector3 screenPos = Camera.main.WorldToScreenPoint(tower.transform.position);
        towerActionPanel.transform.position = screenPos + new Vector3(panelWidth / 2, -panelHeight / 2, 0);

        // 기존 팝업 버튼 리스너들을 청소하고 현재 타워에 맞는 이벤트 새로 연결
        combineButton.onClick.RemoveAllListeners();
        combineButton.onClick.AddListener(OnCombineClicked);

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(OnSellClicked);

        if (combineButton != null)
        {
            // 현재 최고 등급(Epic) 타워라면 더 이상 합성 버튼을 누르지 못하게 제어
            bool isMaxRank = tower.towerData.rank == TowerRank.Epic;

            combineButton.interactable = !isMaxRank;
        }

        UpdateTowerInfoPanel(tower);
    }

    // 합성 버튼 클릭 이벤트
    public void OnCombineClicked()
    {
        if (selectedTower != null)
        {
            towerManager.TryCombineTower(selectedTower);
            HideTowerInfoPanel();
            HideTowerActionPanel();
        }
    }

    // 판매 버튼 클릭 이벤트
    public void OnSellClicked()
    {
        if (selectedTower != null)
        {
            towerManager.SellTower(selectedTower);
            HideTowerInfoPanel();
            HideTowerActionPanel();
        }
    }

    // 타워를 선택 해제하거나 빈 곳을 눌렀을 때 정보 패널을 닫는 함수
    public void HideTowerInfoPanel()
    {
        selectedTower = null;
        towerInfoPanel.SetActive(false);
        towerInfoBackground.SetActive(false);
    }

    // 타워 클릭 시 팝업되었던 조작 패널(합성/판매)을 닫는 함수
    public void HideTowerActionPanel()
    {
        towerActionPanel.SetActive(false);
    }

    // 실시간 스펙 변화가 적용된 타워 정보 패널 데이터 업데이트
    public void UpdateTowerInfoPanel(Tower tower)
    {
        // 타워 패널이 활성화되어 있지 않거나, 갱신하려는 타워가 현재 선택된 타워와 다른 경우 예외 처리
        if (!towerInfoPanel.activeInHierarchy || selectedTower != tower)
        {
            return;
        }

        towerNameValue.text = tower.towerData.towerName;
        towerAttackTypeValue.text = tower.towerData.attackType.ToString();
        towerAttackValue.text = String.Format("{0:N1}", tower.CurrentAttack);
        towerAttackSpeedValue.text = String.Format("{0:N2}", tower.CurrentAttackSpeed);
        towerAttackRangeValue.text = String.Format("{0:N1}", tower.CurrentAttackRange);
        towerAttackSplashRangeValue.text = String.Format("{0:N1}", tower.CurrentAttackSplashRange);
        towerAttackTargetValue.text = tower.CurrentAttackTargetCount.ToString();
    }

    // 보스 웨이브가 끝났거나 보스가 처치되었을 때 보스 정보 화면을 숨기는 함수
    public void HideBossInfoPanel()
    {
        bossEnemyInfoPanel.SetActive(false);
        bossEnemyInfoBackground.SetActive(false);
    }

    // 보스 출현 시 보스 정보 스펙 패널 오픈 및 데이터 연결 함수
    public void ShowBossPanel(EnemyData bossData)
    {
        if (bossData == null || bossEnemyInfoPanel == null) return;

        bossEnemyInfoPanel.SetActive(true);
        bossEnemyInfoBackground.SetActive(true);

        bossEnemyMaxHpValue.text = bossData.hp.ToString();
        bossEnemyArmorValue.text = bossData.armor.ToString();
        bossEnemyArmorTypeValue.text = bossData.armorType.ToString();
        bossEnemySpeedValue.text = bossData.speed.ToString();
    }

    // 게임 승리/패배 조건 달성 시 결과를 보여주는 함수
    public void EndGame(bool isVictory)
    {
        Time.timeScale = 0f;
        int randomBGM = UnityEngine.Random.Range(1, 4);
        if (isVictory)
        {
            gameEndText.text = "Victory!";
            soundManager.StopBGM();
            soundManager.PlayBGM("victory" + randomBGM.ToString());
        }
        else
        {
            gameEndText.text = "Defeat!";
            soundManager.StopBGM();
            soundManager.PlayBGM("defeat" + randomBGM.ToString());
        }
        endGameUIPanel.SetActive(true);
    }

    // 게임 일시정지 및 다시 시작 처리 함수
    public void PauseGame()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0f;
            PauseAllButton(false);
            soundManager.PauseBGM();
        }
        else
        {
            Time.timeScale = 1f;
            PauseAllButton(true);
            soundManager.ResumeBGM();
        }

        pausePanel.SetActive(isPause);
        pauseImage.enabled = !isPause;
        resumeImage.enabled = isPause;
    }

    // 일시정지 상태에서 오작동을 막기 위해 맵 상의 다른 모든 UI 버튼 비활성화/활성화 처리
    public void PauseAllButton(bool enabled)
    {
        missionButton.enabled = enabled;
        buildTowerButton.enabled = enabled;
        archerUI.upgradeButton.enabled = enabled;
        cannonUI.upgradeButton.enabled = enabled;
        flameUI.upgradeButton.enabled = enabled;
        lightningUI.upgradeButton.enabled = enabled;
        magicUI.upgradeButton.enabled = enabled;
        previousButton.enabled = enabled;
        nextButton.enabled = enabled;
        settingButton.enabled = enabled;
    }

    // 인게임 플레이어 임의 BGM 변경 처리 트랙 함수
    public void ChangeBGM(int next)
    {
        gameManager.currentBgmNumber += next;

        // BGM 파일 에셋 리스트 번호가 1번부터 9번 사이라고 가정 시 언더/오버플로 순환 제어
        if (gameManager.currentBgmNumber == 0)
        {
            gameManager.currentBgmNumber = 9;
        }

        if (gameManager.currentBgmNumber == 10)
        {
            gameManager.currentBgmNumber = 1;
        }

        soundManager.StopBGM();
        soundManager.PlayBGM("bgm" + gameManager.currentBgmNumber.ToString());
        currentBGMTitle.text = soundManager.GetCurrentBGMTitle();
    }

    // 마스터 볼륨 조절 이벤트 함수
    void OnMasterChanged(float value)
    {
        soundManager.SetMasterVolume(value);
        UpdateVolumeText();
    }

    // BGM 볼륨 조절 이벤트 함수
    void OnBGMChanged(float value)
    {
        soundManager.SetBGMVolume(value);
        UpdateVolumeText();
    }

    // SFX 볼륨 조절 이벤트 함수
    void OnSFXChanged(float value)
    {
        soundManager.SetSFXVolume(value);
        UpdateVolumeText();
    }

    // 볼륨 오디오 설정창 내 퍼센티지 실시간 수치화 업데이트
    void UpdateVolumeText()
    {
        masterText.text = $"{(masterSlider.value * 100):0} %";
        bgmText.text = $"{(bgmSlider.value * 100):0} %";
        sfxText.text = $"{(sfxSlider.value * 100):0} %";
    }

    // 환경설정 톱니바퀴 버튼 온오프 토글 함수
    void SettingPanelOnOF()
    {
        isSetting = !isSetting;

        if (isSetting)
        {
            settingPanel.SetActive(true);
        }
        else
        {
            settingPanel.SetActive(false);
        }
    }

    // 설정 패널 닫기 버튼(X) 클릭 시 호출
    void ClickSettingExitButton()
    {
        isSetting = false;
        settingPanel.SetActive(false);
    }
}