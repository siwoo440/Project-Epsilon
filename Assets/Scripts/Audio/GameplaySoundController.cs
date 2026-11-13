using System; // 재생 이벤트
using System.Collections.Generic; // 반복 간격 저장
using ProjectEpsilon.Combat; // 포탑 공격과 합성
using ProjectEpsilon.Enemies; // 엘리트와 보스 출현
using ProjectEpsilon.Player; // 체력과 몸통
using ProjectEpsilon.Progression; // 레벨업
using UnityEngine; // Unity 오디오 기능
using UnityEngine.SceneManagement; // 씬 전환 알림

namespace ProjectEpsilon.Audio // 게임 소리 영역
{ // 영역 시작
    [DefaultExecutionOrder(400)] // 전투 구성 후 이벤트 연결
    public sealed class GameplaySoundController : MonoBehaviour // 전투 효과음 재생
    { // 제어기 시작
        private readonly Dictionary<string, float> nextAllowed = new Dictionary<string, float>(); // 종류별 다음 재생 시각
        private GameplaySoundSettings settings; // 실제 효과음 데이터
        private AudioSource source; // 공용 효과음 재생기
        private WeaponGradeEffectHooks weapons; // 실제 포탑 이벤트
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeExperience experience; // 실제 경험치 관리자
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private int previousBodies; // 직전 몸통 수
        private bool initialized; // 재생기 초기화 여부
        private bool subscribed; // 이벤트 구독 여부
        public event Action<string> SoundPlayed; // 실제 재생 요청 알림
        private void Start() // 실제 효과음 구성
        { // 함수 시작
            settings = Resources.Load<GameplaySoundSettings>("Audio/GameplaySoundSettings"); // 조정 가능한 음원 설정
            source = gameObject.AddComponent<AudioSource>(); // 전투 효과음 재생기
            source.playOnAwake = false; // 자동 재생 방지
            source.spatialBlend = 0f; // 화면 전체에 같은 음량
            initialized = true; // 실제 초기화 완료
            Bind(); // 기존 전투 이벤트 연결
        } // 함수 종료
        private void OnEnable() // 재활성화 처리
        { // 함수 시작
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 전환 구독
            if (initialized) // 재생기 준비 확인
            { // 조건 시작
                Bind(); // 기존 전투 이벤트 재연결
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 비활성화 처리
        { // 함수 시작
            SceneManager.sceneLoaded -= OnSceneLoaded; // 씬 전환 구독 해제
            Unbind(); // 실제 전투 이벤트 해제
            if (source != null) // 재생기 존재 확인
            { // 조건 시작
                source.Stop(); // 재생 중인 효과음 중지
            } // 조건 종료
        } // 함수 종료
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // 씬별 전투 구성 교체
        { // 함수 시작
            if (!initialized) // 초기 구성 여부 확인
            { // 조건 시작
                return; // 초기 구성 전 전환 무시
            } // 조건 종료
            Unbind(); // 이전 씬 이벤트 해제
            nextAllowed.Clear(); // 이전 씬 반복 간격 초기화
            Bind(); // 새 씬 전투 이벤트 연결
        } // 함수 종료
        private void Bind() // 실제 전투 이벤트 연결
        { // 함수 시작
            if (subscribed) // 기존 연결 확인
            { // 조건 시작
                return; // 중복 구독 방지
            } // 조건 종료
            weapons = FindFirstObjectByType<WeaponGradeEffectHooks>(); // 실제 포탑 이벤트
            health = FindFirstObjectByType<SnakeHealth>(); // 실제 플레이어 체력
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 플레이어 몸통
            experience = FindFirstObjectByType<SnakeExperience>(); // 실제 레벨업 이벤트
            spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현 이벤트
            if (weapons != null) // 포탑 이벤트 확인
            { // 조건 시작
                weapons.EffectTriggered += OnWeaponEffect; // 포탑 공격과 합성 구독
            } // 조건 종료
            if (health != null) // 체력 관리자 확인
            { // 조건 시작
                health.DamageApplied += OnHit; // 실제 피해 구독
            } // 조건 종료
            if (body != null) // 몸통 관리자 확인
            { // 조건 시작
                previousBodies = body.CurrentBodyCount; // 초기 몸통 수 저장
                body.BodyCountChanged += OnBodyChanged; // 실제 몸통 변경 구독
            } // 조건 종료
            if (experience != null) // 경험치 관리자 확인
            { // 조건 시작
                experience.LevelUpRequested += OnLevelUp; // 실제 레벨업 구독
            } // 조건 종료
            if (spawner != null) // 적 출현기 확인
            { // 조건 시작
                spawner.EliteSpawned += OnElite; // 실제 엘리트 출현 구독
                spawner.BossTransitionStarted += OnBoss; // 실제 보스 전환 구독
            } // 조건 종료
            subscribed = true; // 구독 상태 저장
        } // 함수 종료
        private void Unbind() // 실제 전투 이벤트 해제
        { // 함수 시작
            if (!subscribed) // 구독 상태 확인
            { // 조건 시작
                return; // 중복 해제 방지
            } // 조건 종료
            if (weapons != null) // 포탑 이벤트 확인
            { // 조건 시작
                weapons.EffectTriggered -= OnWeaponEffect; // 포탑 이벤트 해제
            } // 조건 종료
            if (health != null) // 체력 관리자 확인
            { // 조건 시작
                health.DamageApplied -= OnHit; // 피해 이벤트 해제
            } // 조건 종료
            if (body != null) // 몸통 관리자 확인
            { // 조건 시작
                body.BodyCountChanged -= OnBodyChanged; // 몸통 이벤트 해제
            } // 조건 종료
            if (experience != null) // 경험치 관리자 확인
            { // 조건 시작
                experience.LevelUpRequested -= OnLevelUp; // 레벨업 이벤트 해제
            } // 조건 종료
            if (spawner != null) // 적 출현기 확인
            { // 조건 시작
                spawner.EliteSpawned -= OnElite; // 엘리트 이벤트 해제
                spawner.BossTransitionStarted -= OnBoss; // 보스 이벤트 해제
            } // 조건 종료
            subscribed = false; // 구독 상태 초기화
        } // 함수 종료
        private void OnWeaponEffect(WeaponGradeEffectContext context) // 포탑 이벤트 처리
        { // 함수 시작
            if (settings == null) // 효과음 설정 부재 확인
            { // 조건 시작
                return; // 설정 부재 시 재생 생략
            } // 조건 종료
            if (context.Trigger == WeaponGradeEffectTrigger.Attack) // 실제 공격 확인
            { // 조건 시작
                Play("Attack", settings.Attack); // 실제 공격음
            } // 조건 종료
            if (context.Trigger == WeaponGradeEffectTrigger.Merged) // 실제 합성 확인
            { // 조건 시작
                Play("Merge", settings.Merge); // 실제 합성음
            } // 조건 종료
        } // 함수 종료
        private void OnHit() // 실제 플레이어 피격
        { // 함수 시작
            if (settings != null) // 효과음 설정 확인
            { // 조건 시작
                Play("Hit", settings.Hit); // 실제 피격음
            } // 조건 종료
        } // 함수 종료
        private void OnBodyChanged(int current, int earned) // 실제 몸통 수 변경
        { // 함수 시작
            if (settings != null && current < previousBodies) // 실제 몸통 손실 확인
            { // 조건 시작
                Play("BodyLoss", settings.BodyLoss); // 실제 몸통 손실음
            } // 조건 종료
            previousBodies = current; // 현재 몸통 수 저장
        } // 함수 종료
        private void OnLevelUp(int level) // 실제 레벨업
        { // 함수 시작
            if (settings != null) // 효과음 설정 확인
            { // 조건 시작
                Play("LevelUp", settings.LevelUp); // 실제 레벨업음
            } // 조건 종료
        } // 함수 종료
        private void OnElite() // 실제 엘리트 출현
        { // 함수 시작
            if (settings != null) // 효과음 설정 확인
            { // 조건 시작
                Play("Elite", settings.Elite); // 실제 엘리트 출현음
            } // 조건 종료
        } // 함수 종료
        private void OnBoss() // 실제 최종 보스 전환
        { // 함수 시작
            if (settings != null) // 효과음 설정 확인
            { // 조건 시작
                Play("Boss", settings.Boss); // 실제 보스 전환음
            } // 조건 종료
        } // 함수 종료
        private void Play(string name, GameplaySoundCue cue) // 효과음 재생과 반복 제한
        { // 함수 시작
            if (!isActiveAndEnabled || source == null || cue == null || cue.Clip == null || cue.Volume <= 0f) // 재생 가능 상태 확인
            { // 조건 시작
                return; // 재생 불가 상태 생략
            } // 조건 종료
            if (nextAllowed.TryGetValue(name, out float next) && Time.unscaledTime < next) // 반복 재생 간격 확인
            { // 조건 시작
                return; // 과도한 중복 재생 방지
            } // 조건 종료
            nextAllowed[name] = Time.unscaledTime + cue.MinimumInterval; // 다음 재생 가능 시각
            source.PlayOneShot(cue.Clip, cue.Volume); // 기존 전체 음량이 적용되는 효과음
            SoundPlayed?.Invoke(name); // 실제 재생 요청 알림
        } // 함수 종료
    } // 제어기 종료
} // 영역 종료
