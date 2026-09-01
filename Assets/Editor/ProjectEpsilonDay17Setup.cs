using System.Collections.Generic; // 목록 기능 사용
using System.IO; // 파일 존재 검사
using ProjectEpsilon.Combat; // 전투 컴포넌트 사용
using ProjectEpsilon.Data; // 데이터 에셋 사용
using ProjectEpsilon.Debugging; // 디버그 컴포넌트 사용
using ProjectEpsilon.Enemies; // Enemy 이동 사용
using ProjectEpsilon.Player; // 플레이어 컴포넌트 사용
using UnityEditor; // Unity 편집기 기능 사용
using UnityEditor.SceneManagement; // Scene 저장 기능 사용
using UnityEngine; // Unity 기본 기능 사용
using UnityEngine.SceneManagement; // Scene 형식 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 스크립트 로드 시 실행
    public static class ProjectEpsilonDay17Setup // Day17 자동 구성기
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity"; // 게임 Scene 경로
        private const string LegacySetupPath = "Assets/Editor/ProjectEpsilonDay16Setup.cs"; // 이전 Setup 경로
        private const string EnemyDataPath = "Assets/Data/Enemies/Day16/DebugChaser.asset"; // Debug Enemy 데이터 경로
        private const string PhysicalWeaponPath = "Assets/Data/Weapons/Day14/DebugPhysical.asset"; // 물리 Debug 무기 경로
        private const string FireWeaponPath = "Assets/Data/Weapons/Day14/DebugFire.asset"; // 화염 Debug 무기 경로
        private const string ColdWeaponPath = "Assets/Data/Weapons/Day14/DebugCold.asset"; // 냉기 Debug 무기 경로
        private const string ElectricWeaponPath = "Assets/Data/Weapons/Day14/DebugElectric.asset"; // 전기 Debug 무기 경로
        private const string PoisonWeaponPath = "Assets/Data/Weapons/Day14/DebugPoison.asset"; // 독 Debug 무기 경로
        private const string ExplosionWeaponPath = "Assets/Data/Weapons/Day14/DebugExplosion.asset"; // 폭발 Debug 무기 경로
        private const int ExpectedEnemyCount = 6; // 예상 Enemy 수

        static ProjectEpsilonDay17Setup() // 정적 생성자
        {
            EditorApplication.delayCall += RunAutoSetup; // 자동 구성 예약
        }

        [MenuItem("Project Epsilon/Day 17/Run Setup")] // 수동 실행 메뉴
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

        private static void RunSetup(bool force) // Day17 전체 구성
        {
            if (!File.Exists(GameScenePath)) // 게임 Scene 존재 확인
            {
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day17 구성을 건너뜁니다."); // Scene 누락 경고
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
                Debug.LogError("[Project Epsilon] Game Scene이 유효하지 않아 Day17 구성을 중단합니다."); // Scene 오류 출력
                return; // 구성 중단
            }

            if (scene.isDirty) // 사용자 미저장 변경 확인
            {
                Debug.LogWarning("[Project Epsilon] Game Scene에 미저장 변경이 있어 Day17 구성을 보류합니다. 먼저 Scene을 저장하세요."); // 변경 보호 경고
                return; // 자동 덮어쓰기 방지
            }

            GameObject gameplayRoot = FindRootObject(scene, "===Gameplay==="); // Gameplay Root 탐색
            WeaponTarget[] targets = FindTargets(scene); // Scene Enemy 대상 조회
            EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyDataPath); // Enemy 데이터 로드
            WeaponData physicalWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(PhysicalWeaponPath); // 물리 무기 로드
            WeaponData fireWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(FireWeaponPath); // 화염 무기 로드
            WeaponData coldWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(ColdWeaponPath); // 냉기 무기 로드
            WeaponData electricWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(ElectricWeaponPath); // 전기 무기 로드
            WeaponData poisonWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(PoisonWeaponPath); // 독 무기 로드
            WeaponData explosionWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(ExplosionWeaponPath); // 폭발 무기 로드

            if (gameplayRoot == null || enemyData == null || physicalWeapon == null || fireWeapon == null || coldWeapon == null || electricWeapon == null || poisonWeapon == null || explosionWeapon == null || targets.Length != ExpectedEnemyCount) // 필수 요소 확인
            {
                Debug.LogError("[Project Epsilon] Day17 필수 Root, Debug Data 또는 정확한 6개의 WeaponTarget을 찾지 못했습니다."); // 필수 요소 오류
                return; // 구성 중단
            }

            Transform player = gameplayRoot.transform.Find("Player"); // Player 탐색
            Transform snakeBody = gameplayRoot.transform.Find("SnakeBody"); // SnakeBody 탐색

            if (player == null || snakeBody == null) // 핵심 Transform 확인
            {
                Debug.LogError("[Project Epsilon] Player 또는 SnakeBody를 찾지 못해 Day17 구성을 중단합니다."); // 핵심 오브젝트 오류
                return; // 구성 중단
            }

            SnakeBodyManager bodyManager = snakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            SnakeWeaponManager weaponManager = snakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            WeaponAttributeSynergyManager synergyManager = snakeBody.GetComponent<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            WeaponAttributeEffectHooks effectHooks = snakeBody.GetComponent<WeaponAttributeEffectHooks>(); // 속성 Hook 조회
            WeaponAttributeCombatEffects combatEffects = snakeBody.GetComponent<WeaponAttributeCombatEffects>(); // 속성 전투 효과 조회
            WeaponAttributeDebugControls attributeDebug = player.GetComponent<WeaponAttributeDebugControls>(); // 속성 Debug 조회
            EnemyStatusDebugControls enemyDebug = player.GetComponent<EnemyStatusDebugControls>(); // Enemy Debug 조회

            if (bodyManager == null || weaponManager == null || synergyManager == null || effectHooks == null || combatEffects == null || attributeDebug == null || enemyDebug == null) // Day16 기반 확인
            {
                Debug.LogError("[Project Epsilon] Day16 기반 컴포넌트가 완전하지 않아 Day17 구성을 중단합니다."); // 기반 누락 오류
                return; // 기존 기능 보호
            }

            if (!IsBaselineConfigured(player, targets, enemyData, weaponManager, synergyManager, effectHooks, combatEffects, attributeDebug, enemyDebug, coldWeapon, electricWeapon)) // Day16 적용 상태 확인
            {
                Debug.LogError("[Project Epsilon] Day16 Enemy·Cold·Electric 구성이 완전하지 않아 Day17 구성을 중단합니다."); // Day16 상속 오류
                return; // 기존 기능 보호
            }

            if (!force && IsDay17Configured(player, targets, enemyData, weaponManager, synergyManager, effectHooks, combatEffects, attributeDebug, enemyDebug, coldWeapon, electricWeapon, poisonWeapon, explosionWeapon)) // 기존 Day17 구성 확인
            {
                CleanupLegacySetup(); // 검증 완료된 Day16 Setup 정리
                return; // 중복 구성 방지
            }

            for (int index = 0; index < targets.Length; index++) // 모든 Enemy 대상 순회
            {
                ConfigureTarget(targets[index], enemyData, player); // 넉백 이동 구성
            }

            attributeDebug.Configure(bodyManager, weaponManager, fireWeapon, physicalWeapon, coldWeapon, electricWeapon, poisonWeapon, explosionWeapon); // Day17 속성 Debug 연결
            EditorUtility.SetDirty(attributeDebug); // 속성 Debug 변경 표시
            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 표시
            bool sceneSaved = EditorSceneManager.SaveScene(scene, GameScenePath); // Scene 저장 결과
            bool baselineValid = sceneSaved && IsBaselineConfigured(player, targets, enemyData, weaponManager, synergyManager, effectHooks, combatEffects, attributeDebug, enemyDebug, coldWeapon, electricWeapon); // 저장 후 기반 재검증
            bool day17Valid = sceneSaved && IsDay17Configured(player, targets, enemyData, weaponManager, synergyManager, effectHooks, combatEffects, attributeDebug, enemyDebug, coldWeapon, electricWeapon, poisonWeapon, explosionWeapon); // 저장 후 Day17 재검증

            if (!ProjectEpsilonDay17SetupRules.CanCleanupLegacySetup(sceneSaved, baselineValid, day17Valid, targets.Length, ExpectedEnemyCount)) // 완료 조건 확인
            {
                Debug.LogError("[Project Epsilon] Day16 보존, Day17 참조 또는 Scene 저장 검증에 실패해 Day16 Setup을 유지합니다."); // 검증 실패 오류
                return; // 삭제와 완료 처리 중단
            }

            CleanupLegacySetup(); // 전체 검증 성공 후 Day16 Setup 삭제
            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 목록 갱신
            Debug.Log("[Project Epsilon] Day17 Poison and Explosion setup complete."); // 완료 로그 출력
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
            WeaponTargetKnockbackController knockback = EnsureComponent<WeaponTargetKnockbackController>(targetObject); // 넉백 관리자 확보
            knockback.Configure(false); // Debug 일반 적 넉백 허용
            SnakeContactHazard hazard = EnsureComponent<SnakeContactHazard>(targetObject); // 접촉 위험 확보
            hazard.Configure(SnakeContactHazardType.EnemyDirect); // Enemy 직접 접촉 지정
            EnemyMovementController movement = EnsureComponent<EnemyMovementController>(targetObject); // Enemy 이동 확보
            movement.Configure(enemyData, player, statuses, movementBody, knockback); // 넉백 포함 이동 참조 연결
            EditorUtility.SetDirty(knockback); // 넉백 관리자 변경 표시
            EditorUtility.SetDirty(movement); // 이동 관리자 변경 표시
        }

        private static bool IsBaselineConfigured(Transform player, WeaponTarget[] targets, EnemyData enemyData, SnakeWeaponManager weaponManager, WeaponAttributeSynergyManager synergyManager, WeaponAttributeEffectHooks effectHooks, WeaponAttributeCombatEffects combatEffects, WeaponAttributeDebugControls attributeDebug, EnemyStatusDebugControls enemyDebug, WeaponData coldWeapon, WeaponData electricWeapon) // Day16 기반 검사
        {
            if (player == null || targets == null || targets.Length != ExpectedEnemyCount || enemyData == null || weaponManager == null || synergyManager == null || effectHooks == null || combatEffects == null || attributeDebug == null || enemyDebug == null) // 공통 참조 확인
            {
                return false; // 기반 미구성 반환
            }

            bool combatMatches = ProjectEpsilonDay17SetupRules.AreSameReference(synergyManager, combatEffects.SynergyManager) && ProjectEpsilonDay17SetupRules.AreSameReference(effectHooks, combatEffects.EffectHooks) && ProjectEpsilonDay17SetupRules.AreSameReference(combatEffects, weaponManager.AttributeCombatEffects); // 전투 참조 검증
            bool debugMatches = attributeDebug.IsDay16Configured && ProjectEpsilonDay17SetupRules.AreSameReference(coldWeapon, attributeDebug.ColdWeapon) && ProjectEpsilonDay17SetupRules.AreSameReference(electricWeapon, attributeDebug.ElectricWeapon); // Day16 속성 Debug 검증
            bool enemyDebugMatches = ProjectEpsilonDay17SetupRules.AreSameReference(player, enemyDebug.SearchOrigin); // Enemy 검색 중심 검증

            if (!combatMatches || !debugMatches || !enemyDebugMatches) // 공통 기반 검증 결과 확인
            {
                return false; // 기반 미구성 반환
            }

            for (int index = 0; index < targets.Length; index++) // 모든 대상 순회
            {
                if (!IsBaselineTargetConfigured(targets[index], enemyData, player)) // 단일 대상 기반 검사
                {
                    return false; // 기반 미구성 반환
                }
            }

            return true; // Day16 기반 구성 완료 반환
        }

        private static bool IsDay17Configured(Transform player, WeaponTarget[] targets, EnemyData enemyData, SnakeWeaponManager weaponManager, WeaponAttributeSynergyManager synergyManager, WeaponAttributeEffectHooks effectHooks, WeaponAttributeCombatEffects combatEffects, WeaponAttributeDebugControls attributeDebug, EnemyStatusDebugControls enemyDebug, WeaponData coldWeapon, WeaponData electricWeapon, WeaponData poisonWeapon, WeaponData explosionWeapon) // Day17 구성 검사
        {
            if (!IsBaselineConfigured(player, targets, enemyData, weaponManager, synergyManager, effectHooks, combatEffects, attributeDebug, enemyDebug, coldWeapon, electricWeapon)) // Day16 기반 확인
            {
                return false; // Day17 미구성 반환
            }

            bool debugMatches = attributeDebug.IsDay17Configured && ProjectEpsilonDay17SetupRules.AreSameReference(poisonWeapon, attributeDebug.PoisonWeapon) && ProjectEpsilonDay17SetupRules.AreSameReference(explosionWeapon, attributeDebug.ExplosionWeapon); // Day17 속성 Debug 검증

            if (!debugMatches) // Day17 Debug 확인
            {
                return false; // Day17 미구성 반환
            }

            for (int index = 0; index < targets.Length; index++) // 모든 대상 순회
            {
                EnemyMovementController movement = targets[index] == null ? null : targets[index].GetComponent<EnemyMovementController>(); // 이동 관리자 조회
                WeaponTargetKnockbackController knockback = targets[index] == null ? null : targets[index].GetComponent<WeaponTargetKnockbackController>(); // 넉백 관리자 조회

                if (movement == null || knockback == null || knockback.KnockbackImmune || !ProjectEpsilonDay17SetupRules.AreSameReference(knockback, movement.KnockbackController)) // 넉백 구성 확인
                {
                    return false; // Day17 미구성 반환
                }
            }

            return true; // Day17 전체 구성 완료 반환
        }

        private static bool IsBaselineTargetConfigured(WeaponTarget target, EnemyData enemyData, Transform player) // Day16 단일 대상 검사
        {
            if (target == null) // 대상 없음 확인
            {
                return false; // 미구성 반환
            }

            WeaponTargetStatusController statuses = target.GetComponent<WeaponTargetStatusController>(); // 상태 관리자 조회
            Rigidbody2D movementBody = target.GetComponent<Rigidbody2D>(); // 이동 물리 조회
            SnakeContactHazard hazard = target.GetComponent<SnakeContactHazard>(); // 접촉 위험 조회
            EnemyMovementController movement = target.GetComponent<EnemyMovementController>(); // Enemy 이동 조회
            bool movementMatches = movement != null && ProjectEpsilonDay17SetupRules.AreSameReference(enemyData, movement.EnemyData) && ProjectEpsilonDay17SetupRules.AreSameReference(player, movement.ChaseTarget) && ProjectEpsilonDay17SetupRules.AreSameReference(statuses, movement.StatusController) && ProjectEpsilonDay17SetupRules.AreSameReference(movementBody, movement.MovementBody); // 이동 참조 검증
            bool physicsMatches = movementBody != null && movementBody.bodyType == RigidbodyType2D.Kinematic && movementBody.gravityScale == 0f && movementBody.constraints == RigidbodyConstraints2D.FreezeRotation && movementBody.simulated; // 물리 구성 검증
            bool hazardMatches = hazard != null && hazard.HazardType == SnakeContactHazardType.EnemyDirect; // 접촉 위험 검증
            return statuses != null && movementMatches && physicsMatches && hazardMatches; // 단일 대상 결과 반환
        }

        private static GameObject FindRootObject(Scene scene, string rootName) // Scene Root 탐색
        {
            GameObject[] roots = scene.GetRootGameObjects(); // Scene Root 목록 조회

            for (int index = 0; index < roots.Length; index++) // Root 순회
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

            for (int index = 0; index < roots.Length; index++) // Root 순회
            {
                targets.AddRange(roots[index].GetComponentsInChildren<WeaponTarget>(true)); // 비활성 포함 대상 추가
            }

            return targets.ToArray(); // 대상 배열 반환
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component // 컴포넌트 확보
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

            AssetDatabase.DeleteAsset(LegacySetupPath); // Day16 Setup 삭제
        }
    }
}
