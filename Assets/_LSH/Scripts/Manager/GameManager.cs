using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("플레이어 초기 설정")]
    public int playerHp;            // 플레이어 체력
    public int gold;                // 플레이어 골드

    public int currentBgmNumber;    // 현재 BGM 번호

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
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
        soundManager = SoundManager.Instance;
        Time.timeScale = 1f;                // 게임 속도 정상화 (재시작 시 필요)

        // 시작 BGM 번호 1번, bgm1 재생
        currentBgmNumber = 1;
        soundManager.PlayBGM("bgm1");
        uiManager.currentBGMTitle.text = soundManager.GetCurrentBGMTitle();
    }

    // 재시작(예) 버튼 클릭 시 게임 다시 시작 (승리 및 패배 시)
    public void RestartGame()
    {
        Time.timeScale = 1f;

        DestroyAllManagers();
        // 현재 로드된 씬 이름과 동일한 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 종료(아니오) 버튼 클릭 시 게임 종료
    public void QuitGame()
    {
        // 유니티 에디터에서 재생 모드 종료 시
        //UnityEditor.EditorApplication.isPlaying = false;

        // 빌드된 게임 종료 시
        Application.Quit();
    }

    public void DecreasePlayerHp(int amount)
    {
        playerHp -= amount;

        if (playerHp <= 0)
        {
            uiManager.EndGame(false);
        }
    }

    private void DestroyAllManagers()
    {
        Destroy(gameObject);
    }
}