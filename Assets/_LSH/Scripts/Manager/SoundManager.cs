using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class Sound
{
    public string name;         // 사운드 이름 (키 값)
    public AudioClip clip;      // 오디오 클립 데이터 (MP3 파일 등)

    [Range(0f, 1f)]
    public float volume = 1f;   // 0 ~ 1 사이의 볼륨 설정
    [Range(0f, 1f)]
    public float pitch = 1f;    // 피치 (음고 및 재생 속도)

    public bool loop = false;   // 반복 재생 여부

    [HideInInspector]
    public AudioSource source;  // 오디오 소스 컴포넌트 (MP3 플레이어 역할)
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // 인스펙터에서 설정할 사운드 목록
    [Header("BGM 사운드 목록")]
    public Sound[] bgmSounds;               // 배경음 목록

    [Header("효과음 사운드 목록")]
    public Sound[] effectSounds;            // 효과음 목록

    // 사운드 검색을 위한 딕셔너리
    Dictionary<string, Sound> soundDictionary;

    AudioSource bgmSource;
    string currentBGM = "";

    [Header("볼륨 설정")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

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

        soundDictionary = new Dictionary<string, Sound>();

        foreach (Sound s in bgmSounds)
        {
            // 각 사운드 오브젝트를 자식으로 생성하고 오디오 소스 추가
            GameObject soundObject = new GameObject("BGM_" + s.name);
            soundObject.transform.SetParent(transform);

            s.source = soundObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // 딕셔너리에 추가
            soundDictionary.Add(s.name, s);
        }

        foreach (Sound s in effectSounds)
        {
            // 각 사운드 오브젝트를 자식으로 생성하고 오디오 소스 추가
            GameObject soundObject = new GameObject("EFFECT_" + s.name);
            soundObject.transform.SetParent(transform);

            s.source = soundObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // 딕셔너리에 추가
            soundDictionary.Add(s.name, s);
        }

        // BGM 재생 전용 오디오 소스 생성
        GameObject bgmObject = new GameObject("BGM");
        bgmObject.transform.SetParent(transform);
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        // BGM 프리로드 비동기 시작
        if (bgmSounds != null && bgmSounds.Length > 0)
        {
            StartCoroutine(PreloadBGMs());
        }
    }

    IEnumerator PreloadBGMs()
    {
        foreach (Sound bgm in bgmSounds)
        {
            // AudioClip이 로드되었는지 확인
            if (bgm.clip.loadState != AudioDataLoadState.Loaded)
            {
                // 오디오 데이터 로드 시작 (비동기 방식)
                bgm.clip.LoadAudioData();

                // 로딩이 완료될 때까지 대기
                while (bgm.clip.loadState == AudioDataLoadState.Loading)
                {
                    yield return null;
                }
            }
        }
    }

    public void Play(string name, float volumeScale = 1f)
    {
        // 딕셔너리에서 사운드 검색
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogError("효과음 사운드를 찾을 수 없습니다.");
            return;
        }

        Sound sound = soundDictionary[name];

        // 볼륨 계산 및 적용
        sound.source.volume = masterVolume * sfxVolume * sound.volume * volumeScale;

        // 사운드 재생
        sound.source.Play();
    }


    public void PlayBGM(string name, float volumScale = 1f)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogError("BGM을 찾을 수 없습니다.");
            return;
        }

        if (currentBGM == name && bgmSource.isPlaying)
        {
            Debug.Log($" {name}이 이미 재생 중입니다.");
            return;
        }

        Sound bgm = soundDictionary[name];

        // 사운드 정보 설정 후 재생 (BGM 소스 사용)
        bgmSource.clip = bgm.clip;
        bgmSource.volume = masterVolume * bgmVolume * bgm.volume * volumScale;
        bgmSource.Play();

        currentBGM = name;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGM = "";
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        if (bgmSource.isPlaying && !string.IsNullOrEmpty(currentBGM))
        {
            Sound bgm = soundDictionary[currentBGM];
            bgmSource.volume = masterVolume * bgmVolume * bgm.volume;
        }
    }

    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource.isPlaying && !string.IsNullOrEmpty(currentBGM))
        {
            Sound bgm = soundDictionary[currentBGM];
            bgmSource.volume = masterVolume * bgmVolume * bgm.volume;
        }
    }

    // 효과음 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        // 효과음은 재생할 때 마다 볼륨이 계산되므로 실시간으로 적용됨
    }

    public void PauseBGM()
    {
        soundDictionary[currentBGM].source.Pause();
    }

    public void ResumeBGM()
    {
        soundDictionary[currentBGM].source.UnPause();
    }

    public string GetCurrentBGMTitle()
    {
        return soundDictionary[currentBGM].clip.name;
    }
}