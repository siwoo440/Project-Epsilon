using System.IO; // 파일 존재 검사
using ProjectEpsilon.Combat; // 전투 컴포넌트 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using ProjectEpsilon.Debugging; // 디버그 컴포넌트 사용
using ProjectEpsilon.Player; // 플레이어 컴포넌트 사용
using ProjectEpsilon.UI; // HUD 컴포넌트 사용
using UnityEditor; // Unity 편집기 기능 사용
using UnityEditor.SceneManagement; // Scene 저장 기능 사용
using UnityEngine; // Unity 기본 기능 사용
using UnityEngine.SceneManagement; // Scene 형식 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 네임스페이스 시작
    [InitializeOnLoad] // 스크립트 로드 시 실행
    public static class ProjectEpsilonDay18Setup // Day18 자동 구성기
    { // 클래스 시작
        private const string GameScenePath = "Assets/Scenes/Game.unity"; // 게임 Scene 경로
        private const string LegacySetupPath = "Assets/Editor/ProjectEpsilonDay17Setup.cs"; // 이전 Setup 경로
        private const string PhysicalWeaponPath = "Assets/Data/Weapons/Day14/DebugPhysical.asset"; // 물리 Debug 무기 경로
        private const string FireWeaponPath = "Assets/Data/Weapons/Day14/DebugFire.asset"; // 화염 Debug 무기 경로
        private const string ColdWeaponPath = "Assets/Data/Weapons/Day14/DebugCold.asset"; // 냉기 Debug 무기 경로
        private const string ElectricWeaponPath = "Assets/Data/Weapons/Day14/DebugElectric.asset"; // 전기 Debug 무기 경로
        private const string PoisonWeaponPath = "Assets/Data/Weapons/Day14/DebugPoison.asset"; // 독 Debug 무기 경로
        private const string ExplosionWeaponPath = "Assets/Data/Weapons/Day14/DebugExplosion.asset"; // 폭발 Debug 무기 경로
        private const string HolyWeaponPath = "Assets/Data/Weapons/Day14/DebugHoly.asset"; // 신성 Debug 무기 경로
        private const string DarkWeaponPath = "Assets/Data/Weapons/Day14/DebugDark.asset"; // 암흑 Debug 무기 경로

        static ProjectEpsilonDay18Setup() // 정적 생성자
        { // 생성자 시작
            EditorApplication.delayCall += RunAutoSetup; // 자동 구성 예약
        } // 생성자 끝

        [MenuItem("Project Epsilon/Day 18/Run Setup")] // 수동 실행 메뉴
        public static void RunSetupFromMenu() // 메뉴 실행 처리
        { // 메서드 시작
            RunSetup(true); // 강제 구성 실행
        } // 메서드 끝

        private static void RunAutoSetup() // 자동 구성 처리
        { // 메서드 시작
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) // 실행 가능 상태 확인
            { // 조건 시작
                EditorApplication.delayCall += RunAutoSetup; // 다음 프레임 재시도
                return; // 현재 실행 중단
            } // 조건 끝

            RunSetup(false); // 일반 구성 실행
        } // 메서드 끝

        private static void RunSetup(bool force) // Day18 전체 구성
        { // 메서드 시작
            if (!File.Exists(GameScenePath)) // 게임 Scene 존재 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day18 구성을 건너뜁니다."); // Scene 누락 경고
                return; // 구성 중단
            } // 조건 끝

            Scene previousActiveScene = SceneManager.GetActiveScene(); // 기존 활성 Scene 저장
            Scene gameScene = SceneManager.GetSceneByPath(GameScenePath); // 기존 Game Scene 조회
            bool openedForSetup = !gameScene.IsValid() || !gameScene.isLoaded; // 임시 열기 여부 계산

            if (openedForSetup) // Game Scene 미로드 확인
            { // 조건 시작
                gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive); // Game Scene 추가 열기
            } // 조건 끝

            try // Scene 복원 보장
            { // 예외 보호 시작
                ConfigureScene(gameScene, force); // Game Scene 구성 실행
            } // 예외 보호 끝
            finally // Scene 복원 처리
            { // 정리 시작
                if (openedForSetup && gameScene.IsValid() && gameScene.isLoaded) // 임시 Scene 닫기 가능 확인
                { // 조건 시작
                    EditorSceneManager.CloseScene(gameScene, true); // 임시 Game Scene 닫기
                } // 조건 끝

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded) // 기존 Scene 복원 가능 확인
                { // 조건 시작
                    SceneManager.SetActiveScene(previousActiveScene); // 기존 활성 Scene 복원
                } // 조건 끝
            } // 정리 끝
        } // 메서드 끝

        private static void ConfigureScene(Scene scene, bool force) // 단일 Scene 구성
        { // 메서드 시작
            if (!scene.IsValid() || !scene.isLoaded) // Scene 유효성 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Game Scene이 유효하지 않아 Day18 구성을 중단합니다."); // Scene 오류 출력
                return; // 구성 중단
            } // 조건 끝

            if (scene.isDirty) // 사용자 미저장 변경 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] Game Scene에 미저장 변경이 있어 Day18 구성을 보류합니다. 먼저 Scene을 저장하세요."); // 변경 보호 경고
                return; // 자동 덮어쓰기 방지
            } // 조건 끝

            Day18Context context = BuildContext(scene); // Scene 참조 수집

            if (!context.HasRequiredReferences) // 필수 참조 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Day17 기반 참조 또는 Holy·Dark Debug Data가 없어 Day18 구성을 중단합니다."); // 기반 누락 오류
                return; // 구성 중단
            } // 조건 끝

            if (!force && IsDay18Configured(context)) // 기존 Day18 구성 확인
            { // 조건 시작
                CleanupLegacySetup(); // 검증 완료된 Day17 Setup 정리
                return; // 중복 구성 방지
            } // 조건 끝

            SnakeShieldController shield = EnsureComponent<SnakeShieldController>(context.SnakeBody.gameObject); // 공유 보호막 확보
            WeaponAttributePlayerEffects playerEffects = EnsureComponent<WeaponAttributePlayerEffects>(context.SnakeBody.gameObject); // 플레이어 속성 효과 확보
            context.Health.BindShield(shield); // 체력에 보호막 연결
            playerEffects.Configure(context.Health, shield, context.EffectHooks, context.Player); // 플레이어 효과 참조 연결
            context.CombatEffects.Configure(context.SynergyManager, context.EffectHooks, context.CombatEffects.PulseSprite, playerEffects); // 전투 효과 연결
            context.HealthPresenter.Bind(context.Health, shield, context.HUDController); // HUD 보호막 연결
            context.AttributeDebug.Configure(context.BodyManager, context.WeaponManager, context.FireWeapon, context.PhysicalWeapon, context.ColdWeapon, context.ElectricWeapon, context.PoisonWeapon, context.ExplosionWeapon, context.HolyWeapon, context.DarkWeapon); // Day18 Debug 연결
            MarkDirty(context, shield, playerEffects); // 변경 객체 표시
            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 표시
            bool sceneSaved = EditorSceneManager.SaveScene(scene, GameScenePath); // Scene 저장 결과
            bool baselineValid = sceneSaved && IsBaselineConfigured(context); // 저장 후 기반 검증
            bool day18Valid = sceneSaved && IsDay18Configured(BuildContext(scene)); // 저장 후 Day18 검증

            if (!ProjectEpsilonDay18SetupRules.CanCleanupLegacySetup(sceneSaved, baselineValid, day18Valid)) // 완료 조건 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Day17 보존, Day18 참조 또는 Scene 저장 검증에 실패해 Day17 Setup을 유지합니다."); // 검증 실패 오류
                return; // 삭제와 완료 처리 중단
            } // 조건 끝

            CleanupLegacySetup(); // 전체 검증 성공 후 Day17 Setup 삭제
            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 목록 갱신
            Debug.Log("[Project Epsilon] Day18 Holy and Dark setup complete."); // 완료 로그 출력
        } // 메서드 끝

        private static Day18Context BuildContext(Scene scene) // Scene 참조 수집
        { // 메서드 시작
            Day18Context context = new Day18Context(); // 참조 묶음 생성
            GameObject gameplayRoot = FindRootObject(scene, "===Gameplay==="); // Gameplay Root 탐색

            if (gameplayRoot == null) // Root 없음 확인
            { // 조건 시작
                return context; // 빈 참조 반환
            } // 조건 끝

            context.Player = gameplayRoot.transform.Find("Player"); // Player 탐색
            context.SnakeBody = gameplayRoot.transform.Find("SnakeBody"); // SnakeBody 탐색

            if (context.Player == null || context.SnakeBody == null) // 핵심 Transform 확인
            { // 조건 시작
                return context; // 불완전 참조 반환
            } // 조건 끝

            context.BodyManager = context.SnakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            context.WeaponManager = context.SnakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            context.SynergyManager = context.SnakeBody.GetComponent<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            context.EffectHooks = context.SnakeBody.GetComponent<WeaponAttributeEffectHooks>(); // 속성 Hook 조회
            context.CombatEffects = context.SnakeBody.GetComponent<WeaponAttributeCombatEffects>(); // 전투 효과 조회
            context.Health = FindComponentInScene<SnakeHealth>(scene); // 공유 체력 조회
            context.HealthPresenter = FindComponentInScene<SnakeHealthHUDPresenter>(scene); // 체력 HUD Presenter 조회
            context.HUDController = FindComponentInScene<HUDController>(scene); // HUD 조회
            context.AttributeDebug = context.Player.GetComponent<WeaponAttributeDebugControls>(); // 속성 Debug 조회
            context.PhysicalWeapon = LoadWeapon(PhysicalWeaponPath); // 물리 무기 로드
            context.FireWeapon = LoadWeapon(FireWeaponPath); // 화염 무기 로드
            context.ColdWeapon = LoadWeapon(ColdWeaponPath); // 냉기 무기 로드
            context.ElectricWeapon = LoadWeapon(ElectricWeaponPath); // 전기 무기 로드
            context.PoisonWeapon = LoadWeapon(PoisonWeaponPath); // 독 무기 로드
            context.ExplosionWeapon = LoadWeapon(ExplosionWeaponPath); // 폭발 무기 로드
            context.HolyWeapon = LoadWeapon(HolyWeaponPath); // 신성 무기 로드
            context.DarkWeapon = LoadWeapon(DarkWeaponPath); // 암흑 무기 로드
            return context; // 수집 참조 반환
        } // 메서드 끝

        private static bool IsBaselineConfigured(Day18Context context) // Day17 기반 검사
        { // 메서드 시작
            if (!context.HasRequiredReferences || !context.AttributeDebug.IsDay17Configured) // 공통 기반 확인
            { // 조건 시작
                return false; // 기반 미구성 반환
            } // 조건 끝

            bool combatMatches = ProjectEpsilonDay18SetupRules.AreSameReference(context.SynergyManager, context.CombatEffects.SynergyManager) && ProjectEpsilonDay18SetupRules.AreSameReference(context.EffectHooks, context.CombatEffects.EffectHooks); // 전투 참조 검증
            bool debugMatches = ProjectEpsilonDay18SetupRules.AreSameReference(context.PoisonWeapon, context.AttributeDebug.PoisonWeapon) && ProjectEpsilonDay18SetupRules.AreSameReference(context.ExplosionWeapon, context.AttributeDebug.ExplosionWeapon); // Day17 Debug 검증
            return combatMatches && debugMatches; // 기반 구성 결과 반환
        } // 메서드 끝

        private static bool IsDay18Configured(Day18Context context) // Day18 구성 검사
        { // 메서드 시작
            if (!IsBaselineConfigured(context)) // Day17 기반 확인
            { // 조건 시작
                return false; // Day18 미구성 반환
            } // 조건 끝

            SnakeShieldController shield = context.SnakeBody.GetComponent<SnakeShieldController>(); // 보호막 조회
            WeaponAttributePlayerEffects playerEffects = context.SnakeBody.GetComponent<WeaponAttributePlayerEffects>(); // 플레이어 효과 조회
            bool debugMatches = context.AttributeDebug.IsDay18Configured && ProjectEpsilonDay18SetupRules.AreSameReference(context.HolyWeapon, context.AttributeDebug.HolyWeapon) && ProjectEpsilonDay18SetupRules.AreSameReference(context.DarkWeapon, context.AttributeDebug.DarkWeapon); // Day18 Debug 검증
            bool playerMatches = shield != null && playerEffects != null && playerEffects.IsConfigured && ProjectEpsilonDay18SetupRules.AreSameReference(shield, context.Health.ShieldController) && ProjectEpsilonDay18SetupRules.AreSameReference(playerEffects, context.CombatEffects.PlayerEffects) && ProjectEpsilonDay18SetupRules.AreSameReference(context.Player, playerEffects.EffectOrigin); // 플레이어 효과 검증
            bool hudMatches = ProjectEpsilonDay18SetupRules.AreSameReference(context.Health, context.HealthPresenter.Health) && ProjectEpsilonDay18SetupRules.AreSameReference(shield, context.HealthPresenter.ShieldController) && ProjectEpsilonDay18SetupRules.AreSameReference(context.HUDController, context.HealthPresenter.HUDController); // HUD 참조 검증
            return debugMatches && playerMatches && hudMatches; // Day18 전체 결과 반환
        } // 메서드 끝

        private static void MarkDirty(Day18Context context, SnakeShieldController shield, WeaponAttributePlayerEffects playerEffects) // 변경 객체 표시
        { // 메서드 시작
            EditorUtility.SetDirty(context.Health); // 체력 변경 표시
            EditorUtility.SetDirty(shield); // 보호막 변경 표시
            EditorUtility.SetDirty(playerEffects); // 플레이어 효과 변경 표시
            EditorUtility.SetDirty(context.CombatEffects); // 전투 효과 변경 표시
            EditorUtility.SetDirty(context.HealthPresenter); // HUD Presenter 변경 표시
            EditorUtility.SetDirty(context.AttributeDebug); // Debug 변경 표시
        } // 메서드 끝

        private static WeaponData LoadWeapon(string path) // 무기 데이터 로드
        { // 메서드 시작
            return AssetDatabase.LoadAssetAtPath<WeaponData>(path); // 지정 무기 반환
        } // 메서드 끝

        private static GameObject FindRootObject(Scene scene, string rootName) // Scene Root 탐색
        { // 메서드 시작
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // Root 순회
            { // 반복 시작
                if (roots[index].name == rootName) // Root 이름 확인
                { // 조건 시작
                    return roots[index]; // 일치 Root 반환
                } // 조건 끝
            } // 반복 끝

            return null; // Root 없음 반환
        } // 메서드 끝

        private static T FindComponentInScene<T>(Scene scene) where T : Component // Scene 컴포넌트 탐색
        { // 메서드 시작
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // Root 순회
            { // 반복 시작
                T component = roots[index].GetComponentInChildren<T>(true); // 비활성 포함 컴포넌트 조회

                if (component != null) // 컴포넌트 발견 확인
                { // 조건 시작
                    return component; // 발견 컴포넌트 반환
                } // 조건 끝
            } // 반복 끝

            return null; // 컴포넌트 없음 반환
        } // 메서드 끝

        private static T EnsureComponent<T>(GameObject target) where T : Component // 컴포넌트 확보
        { // 메서드 시작
            T component = target.GetComponent<T>(); // 기존 컴포넌트 조회

            if (component == null) // 컴포넌트 없음 확인
            { // 조건 시작
                component = target.AddComponent<T>(); // 새 컴포넌트 추가
            } // 조건 끝

            return component; // 컴포넌트 반환
        } // 메서드 끝

        private static void CleanupLegacySetup() // 이전 Setup 정리
        { // 메서드 시작
            if (AssetDatabase.LoadAssetAtPath<Object>(LegacySetupPath) == null) // 이전 Setup 존재 확인
            { // 조건 시작
                return; // 삭제 생략
            } // 조건 끝

            AssetDatabase.DeleteAsset(LegacySetupPath); // Day17 Setup 삭제
        } // 메서드 끝

        private sealed class Day18Context // Scene 참조 묶음
        { // 클래스 시작
            public Transform Player; // Player 참조
            public Transform SnakeBody; // SnakeBody 참조
            public SnakeBodyManager BodyManager; // Body 관리자 참조
            public SnakeWeaponManager WeaponManager; // 무기 관리자 참조
            public WeaponAttributeSynergyManager SynergyManager; // 시너지 관리자 참조
            public WeaponAttributeEffectHooks EffectHooks; // 속성 Hook 참조
            public WeaponAttributeCombatEffects CombatEffects; // 전투 효과 참조
            public SnakeHealth Health; // 공유 체력 참조
            public SnakeHealthHUDPresenter HealthPresenter; // 체력 HUD Presenter 참조
            public HUDController HUDController; // HUD 참조
            public WeaponAttributeDebugControls AttributeDebug; // 속성 Debug 참조
            public WeaponData PhysicalWeapon; // 물리 무기 참조
            public WeaponData FireWeapon; // 화염 무기 참조
            public WeaponData ColdWeapon; // 냉기 무기 참조
            public WeaponData ElectricWeapon; // 전기 무기 참조
            public WeaponData PoisonWeapon; // 독 무기 참조
            public WeaponData ExplosionWeapon; // 폭발 무기 참조
            public WeaponData HolyWeapon; // 신성 무기 참조
            public WeaponData DarkWeapon; // 암흑 무기 참조

            public bool HasRequiredReferences => Player != null && SnakeBody != null && BodyManager != null && WeaponManager != null && SynergyManager != null && EffectHooks != null && CombatEffects != null && Health != null && HealthPresenter != null && HUDController != null && AttributeDebug != null && PhysicalWeapon != null && FireWeapon != null && ColdWeapon != null && ElectricWeapon != null && PoisonWeapon != null && ExplosionWeapon != null && HolyWeapon != null && DarkWeapon != null; // 필수 참조 상태
        } // 클래스 끝
    } // 클래스 끝
} // 네임스페이스 끝
