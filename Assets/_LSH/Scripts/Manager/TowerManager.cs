using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static GameEnums;

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance;

    public const int TOWER_SIZE = 2;

    [Header("타일맵 설정")]
    public Tilemap overlayTilemap;              // 가이드라인 표시용 타일맵
    public Tile validTile;                      // 타워 배치 가능을 나타내는 타일
    public Tile invalidTile;                    // 타워 배치 불가를 나타내는 타일
    public Tilemap towerGround;                 // 타워 배치 가능 타일맵
    public Tilemap enemyPath;                    // 적 이동 경로 타일맵 Asset

    [Header("타워 설정")]
    public GameObject[] allTowerPrefabList;     // 모든 타워 프리팹을 저장하는 리스트
    private Dictionary<TowerRank, List<GameObject>> rankTowerPrefabs;       // 등급별로 타워 프리팹 분류
    public int towerCost = 100;                                             // 타워 건설 비용

    private bool isBuilding = false;                        // 건설 모드 상태 여부
    private Vector3Int lastCellPosition = Vector3Int.zero;  // 이전 프레임의 마우스 위치 타일 좌표
    private LayerMask towerLayer;                           // 타워 오브젝트 검출용 레이어 마스크

    private Dictionary<TowerRank, List<Tower>> activeTowers
        = new Dictionary<TowerRank, List<Tower>>();         // 현재 필드에 배치된 타워 관리용 딕셔너리

    private Dictionary<TowerRank, int> rankTotalCost;       // 등급별 타워의 누적 환산 가치(가중치)

    GameManager gameManager;
    UIManager uiManager;
    SoundManager soundManager;

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
        InitRankTowerPrefabs();
        InitRankSellPrice();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
        soundManager = SoundManager.Instance;

        // 타워 레이캐스트에 필요한 'Tower' 레이어 마스크 초기화
        towerLayer = LayerMask.GetMask("Tower");
        if (towerLayer.value == 0)
        {
            Debug.LogWarning("Tower 레이어가 할당되지 않았습니다.");
        }
    }

    // 타워 프리팹들을 등급별로 분류하여 딕셔너리에 저장
    private void InitRankTowerPrefabs()
    {
        rankTowerPrefabs = new Dictionary<TowerRank, List<GameObject>>();

        foreach (GameObject prefab in allTowerPrefabList)
        {
            Tower tower = prefab.GetComponent<Tower>();

            if (tower == null || tower.towerData == null)
            {
                Debug.LogError("프리팹에 Tower 컴포넌트나 towerData가 존재하지 않습니다.");
                return;
            }

            TowerRank rank = tower.towerData.rank;

            // 딕셔너리에 해당 등급 키가 없다면 새로운 리스트 생성
            if (!rankTowerPrefabs.ContainsKey(rank))
            {
                rankTowerPrefabs.Add(rank, new List<GameObject>());
            }

            // 해당 등급 리스트에 프리팹 추가
            rankTowerPrefabs[rank].Add(prefab);
        }
    }

    // 타워 등급별 기본 누적 비용(판매가 산정용) 초기화 
    private void InitRankSellPrice()
    {
        rankTotalCost = new Dictionary<TowerRank, int>();

        rankTotalCost.Add(TowerRank.Normal, towerCost);
        rankTotalCost.Add(TowerRank.Magic, towerCost * 2);
        rankTotalCost.Add(TowerRank.Rare, towerCost * 4);
        rankTotalCost.Add(TowerRank.Unique, towerCost * 8);
        rankTotalCost.Add(TowerRank.Epic, towerCost * 16);
    }

    // 건설 모드 시작 (건설 버튼 클릭 시 호출)
    public void StartBuilding()
    {
        // 게임매니저 인스턴스 확인
        if (gameManager == null)
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 골드 보유량 확인
        if (gameManager.Gold >= towerCost)
        {
            isBuilding = true;
            uiManager.HideTowerInfoPanel();
            uiManager.HideTowerActionPanel();
        }
    }

    void Update()
    {
        if (isBuilding)
        {
            MousePositionOverlay();
            HandlePlacementInput();

            // 마우스 우클릭 또는 ESC 키를 누르면 건설 모드 취소
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                isBuilding = false;
                ClearOverlay();
            }
        }
        else
        {
            // 필드 위의 타워 선택 처리
            TowerSelection();
        }
    }

    // 클릭한 타워 선택 처리
    private void TowerSelection()
    {
        // 마우스 왼쪽 클릭
        if (Input.GetMouseButtonDown(0))
        {
            if (uiManager.isPause)
            {
                return;
            }
            // 마우스가 UI 위에 있을 때는 타워 선택 처리를 하지 않음
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 마우스 위치를 월드 좌표로 변환
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, towerLayer);

            if (hit != null)
            {
                // 충돌한 오브젝트에서 Tower 컴포넌트 참조
                Tower tower = hit.GetComponent<Tower>();

                if (tower != null)
                {
                    if (uiManager.selectedTower != tower)
                    {
                        // UI에 타워 정보 및 타워 액션 패널 활성화
                        uiManager.ShowTowerPanel(tower);
                    }
                }
            }
            // 타워가 아닌 맨땅을 클릭했을 때 패널 비활성화
            else
            {
                uiManager.HideTowerActionPanel();
                uiManager.HideTowerInfoPanel();
                uiManager.selectedTower = null;
            }
        }
    }

    // 마우스 위치에 건설 가능 여부 오버레이 표시
    private void MousePositionOverlay()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 월드 좌표를 셀(타일) 좌표로 변환
        Vector3Int startCellPosition = overlayTilemap.WorldToCell((Vector3)mouseWorldPos);

        if (startCellPosition != lastCellPosition)
        {
            ClearOverlay();

            // 2x2 영역의 배치 유효성 검사
            bool isValidPlacement = CheckPlacemnetValidity(startCellPosition);
            Tile displayTile = isValidPlacement ? validTile : invalidTile;

            // 2x2 영역에 가이드 타일 표시
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector3Int currentCell = startCellPosition + new Vector3Int(x, y, 0);
                    overlayTilemap.SetTile(currentCell, displayTile);
                }
            }

            lastCellPosition = startCellPosition;
        }
    }

    // 건설 유효성 검사 (2x2 영역)
    private bool CheckPlacemnetValidity(Vector3Int startCell)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector3Int currentCell = startCell + new Vector3Int(x, y, 0);

                // 배치 가능한 타일맵 경계를 벗어났는지 확인
                if (!towerGround.HasTile(currentCell))
                {
                    return false;
                }

                // 셀 중심의 월드 좌표 구하기
                Vector2 cellCenterWorld = towerGround.GetCellCenterWorld(currentCell);

                // 해당 위치에 타워 레이어의 콜라이더가 이미 존재하는지 검사
                Vector2 checkSize = new Vector2(0.01f, 0.01f);
                Collider2D hit = Physics2D.OverlapBox(cellCenterWorld, checkSize, 0f, towerLayer);

                if (hit != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 마우스 클릭 시 타워 배치 처리
    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 현재 마우스 위치에 배치가 가능할 때
            if (CheckPlacemnetValidity(lastCellPosition))
            {
                // 재화 보유량 확인 (이중 체크)
                if (gameManager.Gold < towerCost)
                {
                    // TODO: UI에 보유 골드 부족 메시지 표시 로직 필요
                    return;
                }

                // 일반 등급 리스트가 비어있는지 확인
                if (rankTowerPrefabs[TowerRank.Normal].Count == 0)
                {
                    Debug.LogError("일반 등급 타워 프리팹이 존재하지 않습니다.");
                    return;
                }

                List<GameObject> normalTowers;
                if (rankTowerPrefabs.TryGetValue(TowerRank.Normal, out normalTowers)
                    && normalTowers.Count > 0)
                {
                    // 일반 등급 타워 중 랜덤으로 하나 선택
                    int randomIndex = Random.Range(0, normalTowers.Count);
                    GameObject selectedTowerPrefab = normalTowers[randomIndex];

                    // 2x2 기준 타일(좌측 하단)의 중심 월드 좌표 구하기
                    Vector3 bottomLeftCellCenter = towerGround.GetCellCenterWorld(lastCellPosition);

                    // 2x2 영역의 정중앙에 배치하기 위해 타일 절반 크기만큼 오프셋 추가
                    Vector3 colliderCenterWorldPosition = bottomLeftCellCenter +
                        new Vector3(towerGround.cellSize.x * 0.5f, towerGround.cellSize.y * 0.5f, 0f);

                    // 프리팹의 BoxCollider2D 오프셋을 고려하여 루트 오브젝트 배치 좌표 정렬
                    BoxCollider2D collider = selectedTowerPrefab.GetComponent<BoxCollider2D>();
                    if (collider == null)
                    {
                        Debug.LogError("프리팹에 BoxCollider2D 컴포넌트가 없습니다.");
                        return;
                    }

                    // 최종 생성 위치 = 계산된 중앙 좌표 - 콜라이더의 자체 오프셋
                    Vector3 placementWorldPosition = colliderCenterWorldPosition -
                        new Vector3(collider.offset.x, collider.offset.y, 0f);

                    // 타워 생성
                    GameObject newTowerGO = Instantiate(selectedTowerPrefab, placementWorldPosition, Quaternion.identity);
                    Tower newTower = newTowerGO.GetComponent<Tower>();

                    if (newTower != null)
                    {
                        AddActiveTower(newTower);
                    }

                    // 건설 사운드 재생
                    soundManager.Play("TowerBuild");

                    // 골드 차감
                    gameManager.Gold -= towerCost;
                    // TODO: UIManager를 통한 상단 UI 골드 텍스트 갱신 로직 추가 필요

                    // 건설 모드 종료 및 가이드라인 클리어
                    isBuilding = false;
                    ClearOverlay();
                }
            }
        }
    }

    private void ClearOverlay()
    {
        if (overlayTilemap != null)
        {
            overlayTilemap.ClearAllTiles();
        }
        lastCellPosition = Vector3Int.zero;
    }

    // 활성화된 타워 리스트에 추가
    private void AddActiveTower(Tower tower)
    {
        TowerRank rank = tower.towerData.rank;
        if (!activeTowers.ContainsKey(rank))
        {
            activeTowers.Add(rank, new List<Tower>());
        }
        activeTowers[rank].Add(tower);
    }

    // 활성화된 타워 리스트에서 제거
    private void RemoveActiveTower(Tower tower)
    {
        TowerRank rank = tower.towerData.rank;
        if (activeTowers.ContainsKey(rank))
        {
            activeTowers[rank].Remove(tower);
        }
    }

    // 타워 판매 처리 함수
    public void SellTower(Tower tower)
    {
        if (tower == null) return;

        // 판매할 타워의 등급 확인
        TowerRank rank = tower.towerData.rank;

        // 등급별 환산 가격 변수
        int totalCost;

        // 딕셔너리에서 해당 등급의 누적 비용을 검색
        if (rankTotalCost.TryGetValue(rank, out totalCost))
        {
            // 판매 금액은 누적 비용의 50%
            int sellGold = totalCost / 2;

            // 골드 지급
            if (gameManager != null)
            {
                gameManager.Gold += sellGold;
            }
        }

        // 판매 사운드 재생
        soundManager.Play("TowerSell");

        // 관리 리스트에서 제거
        RemoveActiveTower(tower);

        // 타워 오브젝트 파괴
        Destroy(tower.gameObject);
    }

    // 동일 등급, 동일 종류의 타워 합성을 시도하는 함수
    public bool TryCombineTower(Tower baseTower)
    {
        if (baseTower == null) return false;

        TowerRank currentRank = baseTower.towerData.rank;
        TowerType currentType = baseTower.towerData.towerType;

        List<Tower> sameRankTowers = new List<Tower>();
        if (!activeTowers.TryGetValue(currentRank, out sameRankTowers) ||
            sameRankTowers.Count < 2)
        {
            Debug.Log("합성 가능한 동일 등급 타워가 부족합니다.");
            return false;
        }

        if (sameRankTowers == null) return false;

        // 합성 대상 타워 탐색
        Tower combineTarget = null;
        foreach (Tower otherTower in sameRankTowers)
        {
            // 자기 자신은 제외
            if (otherTower == baseTower)
            {
                continue;
            }

            // 등급이 같고 타워 타입마저 일치하는 경우 대상 선정
            if (otherTower.towerData.towerType == currentType)
            {
                combineTarget = otherTower;
                break;
            }
        }

        if (combineTarget == null)
        {
            Debug.Log("동일한 종류의 타워를 찾지 못했습니다.");
            return false;
        }

        // 다음 상위 등급 계산
        int nextRankIndex = (int)currentRank + 1;
        TowerRank nextRank = (TowerRank)nextRankIndex;

        // 최고 등급을 초과하는 경우 합성 불가 처리
        if (nextRankIndex > (int)TowerRank.Epic)
        {
            Debug.Log("이미 최고 등급이므로 합성이 불가능합니다.");
            return false;
        }

        List<GameObject> nextRankPrefabs;
        if (!rankTowerPrefabs.TryGetValue(nextRank, out nextRankPrefabs) ||
            nextRankPrefabs.Count == 0)
        {
            Debug.LogError("다음 등급의 타워 프리팹이 딕셔너리에 없거나 비어있습니다.");
            return false;
        }

        // 다음 상위 등급의 타워 중 하나를 랜덤으로 선택
        int randomIndex = Random.Range(0, nextRankPrefabs.Count);
        GameObject newTowerPrefab = nextRankPrefabs[randomIndex];

        // 새로운 타워가 생성될 위치 지정 (합성을 시도한 기준 타워 위치)
        Vector3 placementPos = baseTower.transform.position;

        // 기존에 동작하던 두 타워를 리스트에서 제거
        RemoveActiveTower(baseTower);
        RemoveActiveTower(combineTarget);

        // 이전 두 타워 오브젝트 파괴
        Destroy(baseTower.gameObject);
        Destroy(combineTarget.gameObject);

        // 상위 타워 생성 및 리스트에 등록
        GameObject newTowerGo = Instantiate(newTowerPrefab, placementPos, Quaternion.identity);
        Tower newtower = newTowerGo.GetComponent<Tower>();

        if (newtower != null)
        {
            AddActiveTower(newtower);
        }

        return true;
    }
}