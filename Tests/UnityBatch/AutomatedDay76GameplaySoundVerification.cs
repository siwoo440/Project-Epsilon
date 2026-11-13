using System; // 예외와 이벤트 처리
using System.Collections.Generic; // 재생 기록
using System.Reflection; // 실제 효과음 구성 조회
using ProjectEpsilon.Combat; // 실제 포탑 이벤트
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현
using ProjectEpsilon.Player; // 실제 체력과 몸통
using ProjectEpsilon.Progression; // 실제 레벨업
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 전투 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay76GameplaySoundVerification // 전투 효과음 연결 시험
    { // 시험 시작
        private const string Key = "EpsilonDay76GameplaySound"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 생성 경로 조회
        static AutomatedDay76GameplaySoundVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 상태 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 상태 확인
            { // 조건 시작
                return; // 전투 준비 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 효과음 연결 시험
            { // 시험 시작
                Verify(); // 실제 전투 이벤트 검사
                Finish(0, "Attack throttle, damage, body loss, merge, level-up, elite, boss and volume linkage passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 시험 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 전투 이벤트 확인
        { // 함수 시작
            Type type = Type.GetType("ProjectEpsilon.Audio.GameplaySoundController, Assembly-CSharp"); // 실제 효과음 제어기 형식
            Require(type != null, "전투 효과음 제어기 누락"); // 실제 구현 존재 확인
            Component component = GameManager.Instance.GetComponent(type); // 실제 전투 관리자 연결
            Require(component != null && component.GetComponent<AudioSource>() != null, "효과음 재생기 누락"); // 실제 재생기 확인
            UnityEngine.Object bank = Resources.Load("Audio/GameplaySoundSettings"); // 실제 효과음 설정
            Require(bank != null, "조정 가능한 효과음 설정 누락"); // 실제 설정 데이터 확인
            EventInfo played = type.GetEvent("SoundPlayed"); // 실제 효과음 재생 알림
            Require(played != null, "효과음 재생 알림 누락"); // 실제 연결 결과 확인
            List<string> names = new List<string>(); // 실제 재생 결과 목록
            Action<string> record = name => names.Add(name); // 실제 효과음 결과 수집
            played.AddEventHandler(component, record); // 실제 재생 알림 구독
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 실제 공용 체력
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            WeaponGradeEffectHooks hooks = UnityEngine.Object.FindFirstObjectByType<WeaponGradeEffectHooks>(); // 실제 포탑 이벤트
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 포탑
            Require(spawner != null && health != null && body != null && experience != null && weapons != null && hooks != null && weapon != null, "실제 전투 구성 누락"); // 실제 시험 구성 확인
            spawner.enabled = false; // 자연 출현 중지
            float previousVolume = AudioListener.volume; // 기존 전체 음량 저장
            AudioListener.volume = 0.25f; // 실제 전체 음량 변경
            hooks.Notify(new WeaponGradeEffectContext(weapon, 1, Vector3.zero, 10f, WeaponGradeEffectTrigger.Attack)); // 실제 공격 이벤트 전달
            hooks.Notify(new WeaponGradeEffectContext(weapon, 1, Vector3.zero, 10f, WeaponGradeEffectTrigger.Attack)); // 같은 프레임 반복 공격
            Require(Count(names, "Attack") == 1 && Mathf.Approximately(AudioListener.volume, 0.25f), "공격음 중복 제한 또는 전체 음량 연결 실패"); // 반복과 전체 음량 확인
            Require(health.TakeDamage(1), "실제 체력 피해 실패"); // 실제 피격 적용
            Require(Count(names, "Hit") == 1, "실제 피격음 누락"); // 실제 피격 이벤트 확인
            Require(body.TryRemoveBody() && Count(names, "BodyLoss") == 1, "실제 몸통 손실음 누락"); // 실제 몸통 손실 확인
            Require(weapons.TryEquipAt(0, weapon, 1) && weapons.TryEquipAt(1, weapon, 1) && weapons.TryMergeSlots(0, 1), "실제 포탑 합성 실패"); // 실제 합성 적용
            Require(Count(names, "Merge") == 1, "실제 합성음 누락"); // 실제 합성 이벤트 확인
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 적 설정
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 출현 경로
            Require(spawn.Invoke(spawner, new object[] { settings[3], Vector3.one * 40f, false }) != null && Count(names, "Elite") == 1, "실제 엘리트 출현음 누락"); // 실제 엘리트 출현 확인
            typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", Private).Invoke(spawner, null); // 실제 보스 전환 경로
            Require(Count(names, "Boss") == 1, "실제 보스 전환음 누락"); // 실제 보스 전환 확인
            experience.AddExperience(experience.RequiredExperience); // 실제 레벨업 발생
            Require(Count(names, "LevelUp") == 1, "실제 레벨업음 누락"); // 실제 레벨업 이벤트 확인
            ((Behaviour)component).enabled = false; // 실제 효과음 연결 중지
            int before = names.Count; // 중지 전 재생 수
            hooks.Notify(new WeaponGradeEffectContext(weapon, 1, Vector3.zero, 10f, WeaponGradeEffectTrigger.Merged)); // 중지 후 실제 이벤트
            Require(names.Count == before, "중지된 효과음 구독 잔류"); // 실제 구독 해제 확인
            played.RemoveEventHandler(component, record); // 시험 알림 구독 해제
            AudioListener.volume = previousVolume; // 기존 전체 음량 복구
        } // 함수 종료
        private static int Count(List<string> names, string expected) // 효과음 재생 수 조회
        { // 함수 시작
            return names.FindAll(name => name == expected).Count; // 동일 효과음 수 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day76GameplaySound] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day76GameplaySound] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
