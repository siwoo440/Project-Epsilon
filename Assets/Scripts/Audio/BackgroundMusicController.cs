using ProjectEpsilon.Core; // 실제 게임 관리자
using ProjectEpsilon.Enemies; // 실제 보스 전환
using ProjectEpsilon.UI; // 실제 전투 씬 경로
using ProjectEpsilon.World; // 실제 선택 맵
using UnityEngine; // Unity 오디오 기능
using UnityEngine.SceneManagement; // 실제 씬 전환
namespace ProjectEpsilon.Audio // 게임 소리 영역
{ // 영역 시작
    public sealed class BackgroundMusicController : MonoBehaviour // 씬과 전투 배경음
    { // 제어기 시작
        private static BackgroundMusicController instance; // 중복 재생 방지
        private BackgroundMusicSettings settings; // 실제 배경음 설정
        private AudioSource source; // 반복 배경음 재생기
        private PrototypeMapSelection map; // 현재 맵 선택기
        private EarlyEnemyWaveSpawner spawner; // 현재 보스 전환기
        private bool bossStarted; // 실제 보스 전환 여부
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // 최초 씬 시작 연결
        private static void Bootstrap() // 배경음 관리자 생성
        { // 함수 시작
            if (instance != null) // 기존 재생기 확인
            { // 조건 시작
                return; // 중복 재생 방지
            } // 조건 종료
            new GameObject("BackgroundMusic").AddComponent<BackgroundMusicController>(); // 전역 배경음 생성
        } // 함수 종료
        private void Awake() // 실제 재생기 준비
        { // 함수 시작
            if (instance != null && instance != this) // 기존 재생기 확인
            { // 조건 시작
                Destroy(gameObject); // 중복 객체 제거
                return; // 중복 설정 방지
            } // 조건 종료
            instance = this; // 실제 재생기 등록
            DontDestroyOnLoad(gameObject); // 씬 간 음악 유지
            settings = Resources.Load<BackgroundMusicSettings>("Music/BackgroundMusicSettings"); // 조정 가능한 음원 설정
            source = gameObject.AddComponent<AudioSource>(); // 실제 배경음 재생기
            source.playOnAwake = false; // 무설정 자동 재생 방지
            source.loop = true; // 장면별 배경음 반복
            source.spatialBlend = 0f; // 전체 화면 동일 음량
            SceneManager.sceneLoaded += OnSceneLoaded; // 실제 씬 전환 구독
        } // 함수 종료
        private void Start() // 최초 씬 연결
        { // 함수 시작
            BindScene(); // 실제 맵과 보스 이벤트 연결
            UpdateTrack(); // 최초 화면 배경음 선택
        } // 함수 종료
        private void Update() // 실제 맵 선택 변화 확인
        { // 함수 시작
            UpdateTrack(); // 실제 장면에 맞는 배경음 유지
        } // 함수 종료
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 실제 씬 전환 처리
        { // 함수 시작
            BindScene(); // 새 씬의 맵과 보스 연결
            UpdateTrack(); // 새 씬 배경음 적용
        } // 함수 종료
        private void BindScene() // 씬별 실제 전투 구성 연결
        { // 함수 시작
            if (spawner != null) // 이전 출현기 확인
            { // 조건 시작
                spawner.BossTransitionStarted -= OnBoss; // 이전 보스 이벤트 해제
            } // 조건 종료
            bossStarted = false; // 이전 전투 보스 상태 초기화
            map = FindFirstObjectByType<PrototypeMapSelection>(); // 새 씬의 실제 선택 맵
            spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 새 씬의 실제 출현기
            if (spawner != null) // 실제 출현기 확인
            { // 조건 시작
                spawner.BossTransitionStarted += OnBoss; // 실제 보스 전환 구독
            } // 조건 종료
        } // 함수 종료
        private void OnBoss() // 실제 보스 전환
        { // 함수 시작
            bossStarted = true; // 현재 전투의 보스 상태 기록
            UpdateTrack(); // 보스 배경음 즉시 적용
        } // 함수 종료
        private void UpdateTrack() // 현재 화면의 음원 선택
        { // 함수 시작
            if (settings == null || source == null) // 실제 음원 설정 확인
            { // 조건 시작
                return; // 미설정 재생 방지
            } // 조건 종료
            bool gameplay = SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath; // 실제 전투 씬 확인
            bool boss = bossStarted || gameplay && GameManager.Instance != null && GameManager.Instance.IsFinalBossPhase; // 실제 보스 단계 확인
            AudioClip next = gameplay ? (boss ? settings.Boss : settings.Map(map == null ? 0 : map.SelectedIndex)) : settings.Menu; // 현재 화면 음원
            source.volume = settings.Volume; // 데이터의 개별 음량 적용
            if (source.clip == next) // 기존 배경음 확인
            { // 조건 시작
                return; // 동일 음원 재시작 방지
            } // 조건 종료
            source.Stop(); // 이전 장면 음원 정지
            source.clip = next; // 실제 새 장면 음원
            if (next != null) // 유효 음원 확인
            { // 조건 시작
                source.Play(); // 현재 배경음 재생
            } // 조건 종료
        } // 함수 종료
        private void OnDestroy() // 실제 재생기 제거
        { // 함수 시작
            SceneManager.sceneLoaded -= OnSceneLoaded; // 씬 이벤트 해제
            if (spawner != null) // 실제 출현기 확인
            { // 조건 시작
                spawner.BossTransitionStarted -= OnBoss; // 보스 이벤트 해제
            } // 조건 종료
            if (instance == this) // 실제 전역 재생기 확인
            { // 조건 시작
                instance = null; // 전역 참조 해제
            } // 조건 종료
        } // 함수 종료
    } // 제어기 종료
} // 영역 종료
