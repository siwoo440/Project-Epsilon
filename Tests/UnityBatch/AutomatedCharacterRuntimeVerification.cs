using System; // 예외 형식
using System.Reflection; // 실제 캐릭터 연결 조회
using ProjectEpsilon.Combat; // 실제 무기와 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Player; // 실제 플레이어 구성
using ProjectEpsilon.Progression; // 실제 시작 후보와 무기 풀
using UnityEditor; // 유니티 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedCharacterRuntimeVerification // 실제 여섯 캐릭터 실행 시험
    {
        private const string Key = "EpsilonCharacterRuntimeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedCharacterRuntimeVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 플레이 감시 연결
            }
        }
        public static void Run() // 실제 씬 시험 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 시작 화면 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 플레이 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            {
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            }
            StartingWeaponSelection selection = UnityEngine.Object.FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 선택기
            if (injected || selection == null || !selection.IsSelecting || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 시작 상태와 실행 여부 확인
            {
                return; // 실제 시작 화면 대기
            }
            injected = true; // 실행 중복 방지
            selection.gameObject.AddComponent<CharacterRuntimeVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[CharacterRuntimeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과 기록
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }
    public sealed class CharacterRuntimeVerificationRunner : MonoBehaviour // 실제 캐릭터 적용 실행기
    {
        private void Start() // 실제 시작 상태 검증
        {
            try // 실제 실패 처리
            {
                Verify(); // 실제 캐릭터 시험
                AutomatedCharacterRuntimeVerification.Finish(0, "Six actual starting profiles, passive effects and start-only guard."); // 전체 성공 기록
            }
            catch (Exception error) // 실제 실패 확인
            {
                AutomatedCharacterRuntimeVerification.Finish(1, error.ToString()); // 실제 실패 기록
            }
        }
        private void Verify() // 실제 여섯 캐릭터 적용 확인
        {
            Require(Application.productName.EndsWith("-Isolated", StringComparison.Ordinal), "Character test requires an isolated save profile."); // 실제 사용자 저장 보호 확인
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            if (meta != null && !meta.IsCharacterUnlocked("mobile")) // 실제 기동형 해금 상태 확인
            {
                Require(meta.Service != null && meta.Service.TryCompleteRun("character-runtime-unlock", 0d, 0, 0, true, 0) && meta.IsCharacterUnlocked("mobile"), "Could not prepare the isolated character unlock."); // 독립 시험 프로필의 보스 해금 준비
            }
            Type runtimeType = typeof(CharacterData).Assembly.GetType("ProjectEpsilon.Player.CharacterRunInitializer"); // 실제 캐릭터 시작 처리기 형식
            Require(runtimeType != null, "Character run initializer is missing."); // 실제 적용 구성 존재 확인
            Component initializer = UnityEngine.Object.FindFirstObjectByType(runtimeType) as Component; // 실제 씬 캐릭터 처리기
            MethodInfo apply = runtimeType.GetMethod("TryApply"); // 실제 시작 적용 메서드
            Require(initializer != null && apply != null, "Character run initializer is not attached to the scene."); // 실제 씬 연결 확인
            PropertyInfo activeProfile = runtimeType.GetProperty("ActiveCharacter"); // 실제 현재 런 캐릭터 조회
            CharacterData initialProfile = activeProfile == null ? null : activeProfile.GetValue(null) as CharacterData; // 실제 기본 캐릭터
            Require(initialProfile != null && initialProfile.Id == "balanced", "Default balanced character was not applied at startup."); // 실제 기본 캐릭터 자동 적용 확인
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            SnakeMovement movement = GetComponent<SnakeMovement>(); // 실제 머리 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeStamina stamina = GetComponent<SnakeStamina>(); // 실제 스태미나
            StartingWeaponSelection selection = GetComponent<StartingWeaponSelection>(); // 실제 시작 후보 선택기
            FieldInfo optionsField = typeof(StartingWeaponSelection).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 후보 필드
            PropertyInfo recovery = typeof(SnakeStamina).GetProperty("EffectiveRecoveryPerSecond"); // 실제 스태미나 회복률
            MethodInfo weight = typeof(WeaponRewardPool).GetMethod("GetCandidateWeight"); // 실제 후보 가중치 계산
            Require(body != null && weapons != null && movement != null && health != null && stamina != null && selection != null && optionsField != null && recovery != null && weight != null, "Character runtime fixtures are missing."); // 실제 구성 확인
            string[] names = { "Balanced", "Mobile", "Heavy", "Fusion", "Holy", "Dark" }; // 실제 캐릭터 에셋 이름
            float[] speed = { 3f, 3.3f, 2.7f, 3f, 3f, 3f }; // 실제 기본 이동 속도
            float[] turns = { 145f, 166.75f, 123.25f, 145f, 145f, 145f }; // 실제 기본 회전 속도
            float[] recoveries = { 10.5f, 12.5f, 10f, 10f, 10f, 10f }; // 절반으로 줄인 캐릭터별 초당 회복률
            CharacterData[] profiles = new CharacterData[names.Length]; // 실제 캐릭터 데이터 목록
            for (int index = 0; index < names.Length; index++) // 실제 여섯 캐릭터 순회
            {
                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Prototype" + names[index] + ".asset"); // 실제 캐릭터 데이터
                profiles[index] = data; // 실제 캐릭터 보관
                Require(data != null && (bool)apply.Invoke(initializer, new object[] { data }), "Character was not applied: " + names[index]); // 실제 시작 적용 확인
                Require(body.CurrentBodyCount == data.StartingBodyCount && body.EarnedBodyCount == data.StartingBodyCount && body.MaximumBodyCount == 20 && weapons.SlotCount == data.StartingBodyCount, "Character body or slots mismatch: " + names[index]); // 실제 몸통과 슬롯 확인
                Require(health.MaximumHealth == (int)data.BodyMaxHealth && health.CurrentHealth == (int)data.BodyMaxHealth, "Character HP mismatch: " + names[index]); // 실제 시작 체력 확인
                Require(Mathf.Abs(movement.MoveSpeed - speed[index]) < 0.01f && Mathf.Abs(movement.TurnSpeed - turns[index]) < 0.01f && Mathf.Abs((float)recovery.GetValue(stamina) - recoveries[index]) < 0.01f, "Character movement or stamina mismatch: " + names[index]); // 실제 이동과 회복 확인
                Require(optionsField.GetValue(selection) == data.StartingWeapons && selection.IsSelecting, "Character starting choices did not refresh: " + names[index]); // 실제 선택 화면 후보 확인
            }
            CharacterData heavy = profiles[2]; // 실제 중장형 데이터
            Require((bool)apply.Invoke(initializer, new object[] { heavy }), "Heavy profile could not be reapplied."); // 중장형 실제 적용
            Require(health.TakeDamage(10) && health.CurrentHealth == 116, "Heavy passive did not reduce actual HP damage."); // 실제 중장형 피해 경감 확인
            CharacterData fusion = profiles[3]; // 실제 합성형 데이터
            Require((bool)apply.Invoke(initializer, new object[] { fusion }), "Fusion profile could not be reapplied."); // 합성형 실제 적용
            WeaponData owned = fusion.StartingWeapons.Weapons[0]; // 실제 보유 시험 무기
            WeaponData other = fusion.StartingWeapons.Weapons[1]; // 실제 다른 시험 무기
            Require(weapons.TryEquipAt(0, owned), "Fusion owned weapon missing."); // 실제 보유 무기 장착
            float ownedWeight = (float)weight.Invoke(null, new object[] { owned, weapons, fusion }); // 보유 무기 실제 추첨 가중치
            float otherWeight = (float)weight.Invoke(null, new object[] { other, weapons, fusion }); // 다른 무기 실제 추첨 가중치
            Require(Mathf.Abs(ownedWeight - 2f) < 0.01f && Mathf.Abs(otherWeight - 1f) < 0.01f, "Fusion candidate weighting mismatch."); // 합성형 후보 가중치 확인
            WeaponRewardPool pool = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 실제 레벨업 무기 풀
            Require(pool != null, "Reward pool missing."); // 실제 무기 풀 확인
            int ownedDraws = 0; // 보유 무기 추첨 횟수
            int otherDraws = 0; // 다른 무기 추첨 횟수
            UnityEngine.Random.InitState(44043); // 재현 가능한 후보 난수
            for (int draw = 0; draw < 6000; draw++) // 실제 무기 후보 추첨 반복
            {
                var candidates = pool.BuildCandidates(1, weapons, 1); // 실제 레벨업 후보 추첨
                Require(candidates.Count == 1, "Weighted candidate draw failed."); // 실제 후보 생성 확인
                ownedDraws += candidates[0].Weapon == owned ? 1 : 0; // 보유 무기 추첨 집계
                otherDraws += candidates[0].Weapon == other ? 1 : 0; // 다른 무기 추첨 집계
            }
            Require(otherDraws > 0 && ownedDraws > otherDraws * 1.3f, "Fusion weighted candidate draws did not favor the owned weapon."); // 실제 후보 추첨 가중 경향 확인
            CharacterData holy = profiles[4]; // 실제 신성형 데이터
            Require((bool)apply.Invoke(initializer, new object[] { holy }), "Holy profile could not be reapplied."); // 신성형 실제 적용
            Require(health.TakeDamage(10) && health.Heal(4) && health.CurrentHealth == 95, "Holy passive did not improve actual healing."); // 실제 신성 회복 효율 확인
            WeaponData holyWeapon = holy.StartingWeapons.Weapons[0]; // 실제 신성 시험 무기
            WeaponAttributeAttackSnapshot holyAttack = weapons.AttributeCombatEffects.CreateAttackSnapshot(holyWeapon, 1, Vector3.zero, 10f); // 실제 신성 피해 계산
            Require(Mathf.Abs(holyAttack.DirectDamage - 11f) < 0.01f, "Holy passive did not improve holy damage."); // 실제 신성 피해 강화 확인
            Require(Mathf.Abs((float)weight.Invoke(null, new object[] { holyWeapon, weapons, holy }) - 2f) < 0.01f, "Holy candidate weight missing."); // 신성 후보 가중치 확인
            CharacterData dark = profiles[5]; // 실제 암흑형 데이터
            Require((bool)apply.Invoke(initializer, new object[] { dark }), "Dark profile could not be reapplied."); // 암흑형 실제 적용
            Require(health.TakeDamage(10), "Dark health setup failed."); // 실제 암흑 회복 시험 준비
            WeaponAttributePlayerEffects effects = weapons.AttributeCombatEffects.PlayerEffects; // 실제 암흑 처치 효과 관리자
            UnityEngine.Random.InitState(44044); // 재현 가능한 처치 회복 난수
            for (int attempt = 0; attempt < 200 && health.CurrentHealth == 80; attempt++) // 실제 확률 회복 발생 대기
            {
                effects.HandleDarkKill(6, 1, Vector3.zero); // 실제 저주 적 처치 효과
            }
            Require(health.CurrentHealth == 82, "Dark passive did not improve cursed-kill healing."); // 실제 암흑 처치 회복 확인
            Require(Mathf.Abs((float)weight.Invoke(null, new object[] { dark.StartingWeapons.Weapons[0], weapons, dark }) - 2f) < 0.01f, "Dark candidate weight missing."); // 암흑 후보 가중치 확인
            int currentBodies = body.CurrentBodyCount; // 기존 진행 몸통 수
            int currentHealth = health.CurrentHealth; // 기존 진행 체력
            GameManager.Instance.StartGame(); // 실제 게임 시작
            Require(!(bool)apply.Invoke(initializer, new object[] { heavy }) && body.CurrentBodyCount == currentBodies && health.CurrentHealth == currentHealth, "Character selection reset an active run."); // 진행 중 초기화 방지 확인
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실제 실패 전달
            }
        }
    }
}
