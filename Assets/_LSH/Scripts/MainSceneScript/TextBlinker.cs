using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextBlinker : MonoBehaviour
{
    // 깜빡일 텍스트 컴포넌트
    private TextMeshProUGUI textComponent;

    [Header("깜빡임 설정")]
    [Tooltip("텍스트가 켜져 있거나 꺼져 있는 시간 (초)")]
    public float blinkDuration = 0.5f;

    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

    void Start()
    {
        // 컴포넌트 가져오기
        textComponent = GetComponent<TextMeshProUGUI>();

        // 깜빡임 시작
        StartBlinking();
    }

    /// <summary>
    /// 깜빡임 코루틴을 시작합니다.
    /// </summary>
    public void StartBlinking()
    {
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (!isBlinking)
        {
            isBlinking = true;
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    /// <summary>
    /// 텍스트를 주기적으로 깜빡이도록 처리합니다.
    /// </summary>
    private System.Collections.IEnumerator BlinkRoutine()
    {
        // 무한 반복
        while (isBlinking)
        {
            // 텍스트를 보이게 합니다 (Alpha = 1.0)
            SetAlpha(1.0f);
            yield return new WaitForSeconds(blinkDuration); // 설정된 시간만큼 대기

            // 텍스트를 숨깁니다 (Alpha = 0.0)
            SetAlpha(0.0f);
            yield return new WaitForSeconds(blinkDuration); // 설정된 시간만큼 대기
        }
    }

    /// <summary>
    /// 텍스트 컬러의 알파(Alpha) 값을 변경하는 헬퍼 함수입니다.
    /// </summary>
    private void SetAlpha(float alpha)
    {
        Color color = textComponent.color;
        color.a = alpha; // 알파 값 설정
        textComponent.color = color;
    }

    /// <summary>
    /// 깜빡임을 중지하고 텍스트를 다시 보이게 합니다.
    /// </summary>
    public void StopBlinkingAndShow()
    {
        if (isBlinking)
        {
            isBlinking = false;
            StopCoroutine(blinkCoroutine);
            SetAlpha(1.0f); // 깜빡임 정지 후 항상 보이도록 설정
        }
    }
}