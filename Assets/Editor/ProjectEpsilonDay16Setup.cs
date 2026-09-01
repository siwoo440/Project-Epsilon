using System.Collections.Generic; // 목록 기능 사용
using System.IO; // 파일 존재 검사
using ProjectEpsilon.Combat; // 전투 컴포넌트 사용
using ProjectEpsilon.Data; // 데이터 에셋 사용
using ProjectEpsilon.Debugging; // 디버그 컴포넌트 사용
using ProjectEpsilon.Enemies; // Enemy 이동 사용
using ProjectEpsilon.Player; // 플레이어 컴포넌트 사용
using ProjectEpsilon.UI; // 속성 HUD 사용
using UnityEditor; // Unity 편집기 기능 사용
using UnityEditor.SceneManagement; // Scene 저장 기능 사용
using UnityEngine; // Unity 기본 기능 사용
using UnityEngine.SceneManagement; // Scene 형식 사용
using UnityEngine.UI; // UI Text 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 스크립트 로드 시 실행
    public static class ProjectEpsilonDay16Setup // Day16 자동 구성기
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity"; // 게임 Scene 경로
        private const string LegacySetupPath = "Assets/Editor/ProjectEpsilonDay15Setup.cs"; // 이전 Setup 경로
        private const string EnemyDataPath = "Assets/Data/Enemies/Day16/DebugChaser.asset"; // Debug Enemy 데이터 경로
        private const string PhysicalWeaponPath = "Assets/Data/Weapons/Day14/DebugPhysical.asset"; // 물리 Debug 무기 경로
        private const string FireWeaponPath = "Assets/Data/Weapons/Day14/DebugFire.asset"; // 화염 Debug 무기 경로
        private const string ColdWeaponPath = "Assets/Data/Weapons/Day14/DebugCold.asset"; // 냉기 Debug 무기 경로
        private const string ElectricWeaponPath = "Assets/Data/Weapons/Day14/DebugElectric.asset"; // 전기 Debug 무기 경로
        private const int ExpectedEnemyCount = 6; // 예상 Enemy 수

        static ProjectEpsilonDay16Setup() // 정적 생성자
        {
            EditorApplication.delayCall += RunAutoSetup; // 자동 구성 예약
        }

        [MenuItem("Project Epsilon/Day 16/Run Setup")] // 수동 실행 메뉴
        public static void RunSetupFromMenu() // 메뉴 실행 처리
        {
            RunSetup(true); // 강제 구성 실행
        }

        private static void RunAutoSetup() // 자동 구성 처리
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) // 실행 가능 상태 확인
            {
                EditorApplication.delayCall += RunAutoSetup; // 다음 프레임 재시도
                return; // 현재 실행 중단
            }

            RunSetup(false); // 일반 구성 실행
        }

        private static void RunSetup(bool force) // Day16 전체 구성
        {
            if (!File.Exists(GameScenePath)) // 게임 Scene 존재 확인
            {
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day16 구성을 건너뜁니다."); // Scene 누락 경고
                return; // 구성 중단
            }

            Scene previousActiveScene = SceneManager.GetActiveScene(); // 기존 활성 Scene 저장
            Scene gameScene = SceneManager.GetSceneByPath(GameScenePath); // 기존 Game Scene 조회
            bool openedForSetup = !gameScene.IsValid() || !gameScene.isLoaded; // 임시 열기 여부 계산

            if (openedForSetup) // Game Scene 미로드 확인
            {
                gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive); // Game Scene 추가 열기
            }

            try // Scene 복원 보장
            {
                ConfigureScene(gameScene, force); // Game Scene 구성 실행
            }
            finally // Scene 복원 처리
            {
                if (openedForSetup && gameScene.IsValid() && gameScene.isLoaded) // 임시 Scene 닫기 가능 확인
                {
                    EditorSceneManager.CloseScene(gameScene, true); // 임시 Game Scene 닫기
                }

                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded) // 기존 Scene 복원 가능 확인
                {
                    SceneManager.SetActiveScene(previousActiveScene); // 기존 활성 Scene 복원
                }
            }
        }

        private static void ConfigureScene(Scene scene, bool force) // 단일 Scene 구성
        {
            if (!scene.IsValid() || !scene.isLoaded) // Scene 유효성 확인
            {
                Debug.LogError("[Project Epsilon] Game Scene이 유효하지 않아 Day16 구성을 중단합니다."); // Scene 오류 출력
                return; // 구성 중단
            }

            if (scene.isDirty) // 사용자 미저장 변경 확인
            {
                Debug.LogWarning("[Project Epsilon] Game Scene에 미저장 변경이 있어 Day16 구성을 보류합니다. 먼저 Scene을 저장하세요."); // 변경 보호 경고
                return; // 자동 덮어쓰기 방지
            }

            GameObject gameplayRoot = FindRootObject(scene, "===Gameplay==="); // Gameplay Root 탐색
            GameObject uiRoot = FindRootObject(scene, "===UI==="); // UI Root 탐색
            WeaponTarget[] targets = FindTargets(scene); // Scene Enemy 대상 조회
            EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyDataPath); // Debug Enemy 데이터 로드
            WeaponData physicalWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(PhysicalWeaponPath); // 물리 Debug 무기 로드
            WeaponData fireWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(FireWeaponPath); // 화염 Debug 무기 로드
            WeaponData coldWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(ColdWeaponPath); // 냉기 Debug 무기 로드
            WeaponData electricWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(ElectricWeaponPath); // 전기 Debug 무기 로드

            if (gameplayRoot == null || uiRoot == null || enemyData == null || physicalWeapon == null || fireWeapon == null || coldWeapon == null || electricWeapon == null || targets.Length != ExpectedEnemyCount) // 필수 구성 요소 확인
            {
                Debug.LogError("[Project Epsilon] Day16 필수 Root, Debug Data 또는 정확한 6개의 WeaponTarget을 찾지 못했습니다."); // 필수 요소 오류
                return; // 구성 중단
            }

            Transform player = gameplayRoot.transform.Find("Player"); // Player 탐색
            Transform snakeBody = gameplayRoot.transform.Find("SnakeBody"); // SnakeBody 탐색
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas"); // HUD Canvas 탐색

            if (player == null || snakeBody == null || hudCanvas == null) // 핵심 Transform 확인
            {
                Debug.LogError("[Project Epsilon] Player, SnakeBody 또는 HUDCanvas를 찾지 못해 Day16 구성을 중단합니다."); // 핵심 오브젝트 오류
                return; // 구성 중단
            }

            if (!IsDay15Configured(player, snakeBody, hudCanvas, physicalWeapon, fireWeapon)) // Day15 적용 상태 확인
            {
                Debug.LogError("[Project Epsilon] Day15 Physical·Fire Scene 구성이 완전하지 않아 Day16 구성을 중단합니다."); // Day15 상속 오류
                return; // 기존 기능 보호
            }

            SnakeBodyManager bodyManager = snakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            SnakeWeaponManager weaponManager = snakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            WeaponAttributeDebugControls attributeDebug = player.GetComponent<WeaponAttributeDebugControls>(); // 속성 Debug 입력 조회

            if (!force && IsDay16Configured(player, targets, enemyData, coldWeapon, electricWeapon)) // 기존 Day16 구성 확인
            {
                if (ProjectEpsilonDay16SetupRules.CanCleanupLegacySetup(true, true, true, targets.Length, ExpectedEnemyCount)) // 기존 전체 구성 검증
                {
                    CleanupLegacySetup(); // 검증 완료된 Day15 Setup 정리
                }

                return; // 중복 구성 방지
            }

            for (int index = 0; index < targets.Length; index++) // 모든 Enemy 대상 순회
            {
                ConfigureTarget(targets[index], enemyData, player); // Enemy 이동과 상태 구성
            }

            attributeDebug.Configure(bodyManager, weaponManager, fireWeapon, physicalWeapon, coldWeapon, electricWeapon); // Day16 속성 Debug 연결
            EditorUtility.SetDirty(attributeDebug); // 속성 Debug 변경 표시
            EnemyStatusDebugControls enemyDebug = EnsureComponent<EnemyStatusDebugControls>(player.gameObject); // Enemy 상태 Debug 확보
            enemyDebug.Configure(player, 100f); // Enemy 상태 검색 기준 연결
            EditorUtility.SetDirty(enemyDebug); // Enemy Debug 변경 표시
            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 표시
            bool sceneSaved = EditorSceneManager.SaveScene(scene, GameScenePath); // Scene 저장 결과
            bool day15Valid = sceneSaved && IsDay15Configured(player, snakeBody, hudCanvas, physicalWeapon, fireWeapon); // 저장 후 Day15 재검증
            bool day16Valid = sceneSaved && IsDay16Configured(player, targets, enemyData, coldWeapon, electricWeapon); // 저장 후 Day16 재검증

            if (!ProjectEpsilonDay16SetupRules.CanCleanupLegacySetup(sceneSaved, day15Valid, day16Valid, targets.Length, ExpectedEnemyCount)) // 완료 조건 확인
            {
                Debug.LogError("[Project Epsilon] Day15 보존, Day16 참조 또는 Scene 저장 검증에 실패해 Day15 Setup을 유지합니다."); // 검증 실패 오류
                return; // 삭제와 완료 처리 중단
            }

            CleanupLegacySetup(); // 전체 검증 성공 후 Day15 Setup 삭제
            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 목록 갱신
            Debug.Log("[Project Epsilon] Day16 enemy movement, Cold and Electric setup complete."); // 완료 로그 출력
        }

        private static void ConfigureTarget(WeaponTarget target, EnemyData enemyData, Transform player) // 단일 Enemy 구성
        {
            if (target == null) // 대상 없음 확인
            {
                return; // 구성 생략
            }

            GameObject targetObject = target.gameObject; // 대상 오브젝트 조회
            WeaponTargetStatusController statuses = EnsureComponent<WeaponTargetStatusController>(targetObject); // 상태 관리자 확보
            Rigidbody2D movementBody = EnsureComponent<Rigidbody2D>(targetObject); // 이동 물리 확보
            movementBody.bodyType = RigidbodyType2D.Kinematic; // Kinematic 형식 지정
            movementBody.gravityScale = 0f; // 중력 제거
            movementBody.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정
            movementBody.simulated = true; // 물리 시뮬레이션 활성화
            SnakeContactHazard hazard = EnsureComponent<SnakeContactHazard>(targetObject); // 접촉 위험 확보
            hazard.Configure(SnakeContactHazardType.EnemyDirect); // Enemy 직접 접촉 지정
            EnemyMovementController movement = EnsureComponent<EnemyMovementController>(targetObject); // Enemy 이동 확보
            movement.Configure(enemyData, player, statuses, movementBody); // 이동 참조 연결
            EditorUtility.SetDirty(statuses); // 상태 관리자 변경 표시
            EditorUtility.SetDirty(movementBody); // 이동 물리 변경 표시
            EditorUtility.SetDirty(hazard); // 접촉 위험 변경 표시
            EditorUtility.SetDirty(movement); // Enemy 이동 변경 표시
        }

        private static bool IsDay15Configured(Transform player, Transform snakeBody, Transform hudCanvas, WeaponData physicalWeapon, WeaponData fireWeapon) // Day15 구성 검사
        {
            if (player == null || snakeBody == null || hudCanvas == null || physicalWeapon == null || fireWeapon == null) // 필수 참조 확인
            {
                return false; // 미구성 반환
            }

            Transform hud = hudCanvas.Find("AttributeSynergyHUD"); // 속성 HUD 탐색
            SnakeBodyManager bodyManager = snakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            SnakeWeaponManager weaponManager = snakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            WeaponGradeEffectHooks gradeHooks = snakeBody.GetComponent<WeaponGradeEffectHooks>(); // 등급 Hook 조회
            WeaponAttributeSynergyManager synergyManager = snakeBody.GetComponent<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            WeaponAttributeEffectHooks attributeHooks = snakeBody.GetComponent<WeaponAttributeEffectHooks>(); // 속성 Hook 조회
            WeaponAttributeCombatEffects combatEffects = snakeBody.GetComponent<WeaponAttributeCombatEffects>(); // 속성 전투 효과 조회
            WeaponAttributeDebugControls debugControls = player.GetComponent<WeaponAttributeDebugControls>(); // 속성 Debug 조회
            WeaponAttributeHUDPresenter hudPresenter = hud == null ? null : hud.GetComponent<WeaponAttributeHUDPresenter>(); // HUD 표시기 조회
            Text hudText = hud == null ? null : hud.GetComponent<Text>(); // HUD Text 조회

            if (bodyManager == null || weaponManager == null || gradeHooks == null || synergyManager == null || attributeHooks == null || combatEffects == null || debugControls == null || hudPresenter == null || hudText == null) // Day15 컴포넌트 존재 확인
            {
                return false; // Day15 미구성 반환
            }

            bool managerMatches = ProjectEpsilonDay16SetupRules.AreSameReference(weaponManager, synergyManager.WeaponManager); // 시너지 관리자 연결 검증
            bool hooksMatch = ProjectEpsilonDay16SetupRules.AreSameReference(gradeHooks, attributeHooks.GradeEffectHooks) && ProjectEpsilonDay16SetupRules.AreSameReference(synergyManager, attributeHooks.SynergyManager); // 속성 Hook 연결 검증
            bool combatMatches = ProjectEpsilonDay16SetupRules.AreSameReference(synergyManager, combatEffects.SynergyManager) && ProjectEpsilonDay16SetupRules.AreSameReference(attributeHooks, combatEffects.EffectHooks) && ProjectEpsilonDay16SetupRules.AreSameReference(combatEffects, weaponManager.AttributeCombatEffects); // 전투 효과 연결 검증
            bool hudMatches = ProjectEpsilonDay16SetupRules.AreSameReference(synergyManager, hudPresenter.SynergyManager) && ProjectEpsilonDay16SetupRules.AreSameReference(hudText, hudPresenter.AttributeText); // HUD 연결 검증
            bool debugMatches = ProjectEpsilonDay16SetupRules.AreSameReference(bodyManager, debugControls.BodyManager) && ProjectEpsilonDay16SetupRules.AreSameReference(weaponManager, debugControls.WeaponManager) && ProjectEpsilonDay16SetupRules.AreSameReference(fireWeapon, debugControls.FireWeapon) && ProjectEpsilonDay16SetupRules.AreSameReference(physicalWeapon, debugControls.FallbackWeapon); // Day15 Debug 연결 검증
            return managerMatches && hooksMatch && combatMatches && hudMatches && debugMatches; // Day15 전체 구성 결과 반환
        }

        private static bool IsDay16Configured(Transform player, WeaponTarget[] targets, EnemyData enemyData, WeaponData coldWeapon, WeaponData electricWeapon) // Day16 구성 검사
        {
            if (player == null || targets == null || targets.Length != ExpectedEnemyCount || enemyData == null || coldWeapon == null || electricWeapon == null) // Day16 공통 참조 확인
            {
                return false; // 미구성 반환
            }

            EnemyStatusDebugControls enemyDebug = player.GetComponent<EnemyStatusDebugControls>(); // Enemy Debug 입력 조회
            WeaponAttributeDebugControls attributeDebug = player.GetComponent<WeaponAttributeDebugControls>(); // 속성 Debug 입력 조회

            if (enemyDebug == null || attributeDebug == null) // Day16 Debug 컴포넌트 확인
            {
                return false; // 미구성 반환
            }

            bool enemyDebugMatches = ProjectEpsilonDay16SetupRules.AreSameReference(player, enemyDebug.SearchOrigin); // Enemy 검색 중심 검증
            bool attributeDebugMatches = ProjectEpsilonDay16SetupRules.AreSameReference(coldWeapon, attributeDebug.ColdWeapon) && ProjectEpsilonDay16SetupRules.AreSameReference(electricWeapon, attributeDebug.ElectricWeapon); // Cold·Electric Debug 연결 검증

            if (!enemyDebugMatches || !attributeDebugMatches) // Day16 Debug 참조 검사
            {
                return false; // 잘못된 구성 반환
            }

            for (int index = 0; index < targets.Length; index++) // 모든 Enemy 대상 순회
            {
                if (!IsTargetConfigured(targets[index], enemyData, player)) // 단일 대상 구성 확인
                {
                    return false; // 미구성 반환
                }
            }

            return true; // Day16 전체 구성 완료 반환
        }

        private static bool IsTargetConfigured(WeaponTarget target, EnemyData enemyData, Transform player) // 단일 Enemy 구성 검사
        {
            if (target == null) // 대상 없음 확인
            {
                return false; // 미구성 반환
            }

            WeaponTargetStatusController statuses = target.GetComponent<WeaponTargetStatusController>(); // 상태 관리자 조회
            EnemyMovementController movement = target.GetComponent<EnemyMovementController>(); // Enemy 이동 조회
            Rigidbody2D movementBody = target.GetComponent<Rigidbody2D>(); // 이동 물리 조회
            SnakeContactHazard hazard = target.GetComponent<SnakeContactHazard>(); // 접촉 위험 조회
            bool movementMatches = movement != null && ProjectEpsilonDay16SetupRules.AreSameReference(enemyData, movement.EnemyData) && ProjectEpsilonDay16SetupRules.AreSameReference(player, movement.ChaseTarget) && ProjectEpsilonDay16SetupRules.AreSameReference(statuses, movement.StatusController) && ProjectEpsilonDay16SetupRules.AreSameReference(movementBody, movement.MovementBody); // 이동 참조 검증
            bool physicsMatches = movementBody != null && movementBody.bodyType == RigidbodyType2D.Kinematic && movementBody.gravityScale == 0f && movementBody.constraints == RigidbodyConstraints2D.FreezeRotation && movementBody.simulated; // 물리 구성 검증
            bool hazardMatches = hazard != null && hazard.HazardType == SnakeContactHazardType.EnemyDirect; // 접촉 위험 검증
            return statuses != null && movementMatches && physicsMatches && hazardMatches; // 단일 구성 결과 반환
        }

        private static GameObject FindRootObject(Scene scene, string rootName) // Scene Root 탐색
        {
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // 모든 Root 순회
            {
                if (roots[index].name == rootName) // Root 이름 확인
                {
                    return roots[index]; // 일치 Root 반환
                }
            }

            return null; // Root 없음 반환
        }

        private static WeaponTarget[] FindTargets(Scene scene) // Scene 대상 조회
        {
            List<WeaponTarget> targets = new List<WeaponTarget>(); // 대상 목록 생성
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // 모든 Root 순회
            {
                targets.AddRange(roots[index].GetComponentsInChildren<WeaponTarget>(true)); // 비활성 포함 대상 추가
            }

            return targets.ToArray(); // 대상 배열 반환
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component // 컴포넌트 확보 도우미
        {
            T component = target.GetComponent<T>(); // 기존 컴포넌트 조회

            if (component == null) // 컴포넌트 없음 확인
            {
                component = target.AddComponent<T>(); // 새 컴포넌트 추가
            }

            return component; // 컴포넌트 반환
        }

        private static void CleanupLegacySetup() // 이전 Setup 정리
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(LegacySetupPath) == null) // 이전 Setup 존재 확인
            {
                return; // 삭제 생략
            }

            AssetDatabase.DeleteAsset(LegacySetupPath); // Day15 Setup 삭제
        }
    }
}
