using System.IO; // 파일 존재 검사
using ProjectEpsilon.Combat; // 전투 컴포넌트 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using ProjectEpsilon.Debugging; // 디버그 컴포넌트 사용
using ProjectEpsilon.Player; // Body 컴포넌트 사용
using ProjectEpsilon.UI; // HUD 컴포넌트 사용
using UnityEditor; // Unity 편집기 기능 사용
using UnityEditor.SceneManagement; // 장면 저장 기능 사용
using UnityEngine; // Unity 기본 기능 사용
using UnityEngine.SceneManagement; // 장면 형식 사용
using UnityEngine.UI; // UI Text 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 네임스페이스 시작
    [InitializeOnLoad] // 스크립트 로드 시 실행
    public static class ProjectEpsilonDay15Setup // Day15 자동 구성기
    { // 클래스 시작
        private const string GameScenePath = "Assets/Scenes/Game.unity"; // 게임 장면 경로
        private const string LegacySetupPath = "Assets/Editor/ProjectEpsilonDay14Setup.cs"; // 이전 Setup 경로
        private const string PhysicalWeaponPath = "Assets/Data/Weapons/Day14/DebugPhysical.asset"; // 물리 무기 경로
        private const string FireWeaponPath = "Assets/Data/Weapons/Day14/DebugFire.asset"; // 화염 무기 경로
        private const string PulseSpritePath = "Assets/Art/Sprites/DebugSnakeBody.png"; // 명중 Pulse 이미지 경로

        static ProjectEpsilonDay15Setup() // 정적 생성자
        { // 생성자 시작
            EditorApplication.delayCall += RunAutoSetup; // 자동 구성 예약
        } // 생성자 끝

        [MenuItem("Project Epsilon/Day 15/Run Setup")] // 수동 실행 메뉴
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

        private static void RunSetup(bool force) // Day15 전체 구성
        { // 메서드 시작
            if (!File.Exists(GameScenePath)) // 게임 장면 존재 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] Game Scene이 없어 Day 15 자동 구성을 건너뜁니다."); // 장면 누락 경고
                return; // 구성 중단
            } // 조건 끝

            Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single); // 게임 장면 열기

            if (!force && IsDay15Configured()) // 기존 구성 확인
            { // 조건 시작
                CleanupLegacySetup(); // 검증 완료된 이전 Setup 정리
                return; // 중복 구성 방지
            } // 조건 끝

            GameObject gameplayRoot = GameObject.Find("===Gameplay==="); // Gameplay Root 탐색
            GameObject uiRoot = GameObject.Find("===UI==="); // UI Root 탐색

            if (gameplayRoot == null || uiRoot == null) // 필수 Root 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Gameplay 또는 UI Root를 찾지 못해 Day 15 구성을 중단합니다."); // Root 누락 오류
                return; // 구성 중단
            } // 조건 끝

            Transform player = gameplayRoot.transform.Find("Player"); // Player 탐색
            Transform snakeBody = gameplayRoot.transform.Find("SnakeBody"); // SnakeBody 탐색
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas"); // HUD Canvas 탐색

            if (player == null || snakeBody == null || hudCanvas == null) // 필수 오브젝트 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Player, SnakeBody 또는 HUDCanvas를 찾지 못해 Day 15 구성을 중단합니다."); // 오브젝트 누락 오류
                return; // 구성 중단
            } // 조건 끝

            SnakeBodyManager bodyManager = snakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            SnakeWeaponManager weaponManager = snakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            WeaponGradeEffectHooks gradeHooks = EnsureComponent<WeaponGradeEffectHooks>(snakeBody.gameObject); // 등급 Hook 확보

            if (bodyManager == null || weaponManager == null) // 핵심 관리자 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Body 또는 Weapon Manager를 찾지 못해 Day 15 구성을 중단합니다."); // 관리자 누락 오류
                return; // 구성 중단
            } // 조건 끝

            WeaponAttributeSynergyManager synergyManager = EnsureComponent<WeaponAttributeSynergyManager>(snakeBody.gameObject); // 시너지 관리자 확보
            synergyManager.Configure(weaponManager); // 무기 관리자 연결

            WeaponAttributeEffectHooks attributeHooks = EnsureComponent<WeaponAttributeEffectHooks>(snakeBody.gameObject); // 속성 Hook 확보
            attributeHooks.Configure(gradeHooks, synergyManager); // 등급 Hook과 시너지 연결

            WeaponAttributeCombatEffects combatEffects = EnsureComponent<WeaponAttributeCombatEffects>(snakeBody.gameObject); // 속성 전투 효과 확보
            Sprite pulseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PulseSpritePath); // 명중 Pulse 이미지 로드
            combatEffects.Configure(synergyManager, attributeHooks, pulseSprite); // 전투 효과 참조 연결
            weaponManager.BindAttributeCombatEffects(combatEffects); // 무기 관리자 명중 효과 연결

            WeaponAttributeHUDPresenter hudPresenter = EnsureAttributeHud(hudCanvas, synergyManager); // 속성 HUD 구성
            WeaponData fireWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(FireWeaponPath); // 화염 무기 로드
            WeaponData physicalWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(PhysicalWeaponPath); // 물리 무기 로드

            if (fireWeapon == null || physicalWeapon == null) // 디버그 무기 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Day14 Debug Weapon을 찾지 못해 구성을 중단합니다."); // 무기 누락 오류
                return; // 구성 중단
            } // 조건 끝

            WeaponAttributeDebugControls debugControls = EnsureComponent<WeaponAttributeDebugControls>(player.gameObject); // 디버그 입력 확보
            debugControls.Configure(bodyManager, weaponManager, fireWeapon, physicalWeapon); // 디버그 입력 연결

            EditorUtility.SetDirty(synergyManager); // 시너지 관리자 변경 표시
            EditorUtility.SetDirty(attributeHooks); // 속성 Hook 변경 표시
            EditorUtility.SetDirty(combatEffects); // 전투 효과 변경 표시
            EditorUtility.SetDirty(weaponManager); // 무기 관리자 변경 표시
            EditorUtility.SetDirty(hudPresenter); // HUD 표시기 변경 표시
            EditorUtility.SetDirty(debugControls); // 디버그 입력 변경 표시
            EditorSceneManager.MarkSceneDirty(scene); // 장면 변경 표시
            bool sceneSaved = EditorSceneManager.SaveScene(scene, GameScenePath); // 장면 저장 결과
            bool configurationValid = sceneSaved && IsDay15Configured(); // 저장 후 연결 검증

            if (!ProjectEpsilonDay15SetupRules.CanCleanupLegacySetup(sceneSaved, configurationValid)) // 완료 조건 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] Day 15 장면 저장 또는 참조 검증에 실패해 Day14 Setup을 유지합니다."); // 검증 실패 오류
                return; // 삭제와 완료 처리 중단
            } // 조건 끝

            CleanupLegacySetup(); // 검증 성공 후 Day14 Setup 삭제
            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 목록 갱신
            Selection.activeGameObject = snakeBody.gameObject; // 구성 대상 선택
            Debug.Log("[Project Epsilon] Day 15 Physical and Fire combat effects setup complete."); // 완료 로그 출력
        } // 메서드 끝

        private static bool IsDay15Configured() // 기존 구성 검사
        { // 메서드 시작
            GameObject gameplayRoot = GameObject.Find("===Gameplay==="); // Gameplay Root 탐색
            GameObject uiRoot = GameObject.Find("===UI==="); // UI Root 탐색

            if (gameplayRoot == null || uiRoot == null) // Root 존재 확인
            { // 조건 시작
                return false; // 미구성 반환
            } // 조건 끝

            Transform player = gameplayRoot.transform.Find("Player"); // Player 탐색
            Transform snakeBody = gameplayRoot.transform.Find("SnakeBody"); // SnakeBody 탐색
            Transform hudCanvas = uiRoot.transform.Find("HUDCanvas"); // HUD Canvas 탐색

            if (player == null || snakeBody == null || hudCanvas == null) // 대상 존재 확인
            { // 조건 시작
                return false; // 미구성 반환
            } // 조건 끝

            Transform hud = hudCanvas.Find("AttributeSynergyHUD"); // 속성 HUD 탐색
            SnakeBodyManager bodyManager = snakeBody.GetComponent<SnakeBodyManager>(); // Body 관리자 조회
            SnakeWeaponManager weaponManager = snakeBody.GetComponent<SnakeWeaponManager>(); // 무기 관리자 조회
            WeaponGradeEffectHooks gradeHooks = snakeBody.GetComponent<WeaponGradeEffectHooks>(); // 등급 Hook 조회
            WeaponAttributeSynergyManager synergyManager = snakeBody.GetComponent<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            WeaponAttributeEffectHooks attributeHooks = snakeBody.GetComponent<WeaponAttributeEffectHooks>(); // 속성 Hook 조회
            WeaponAttributeCombatEffects combatEffects = snakeBody.GetComponent<WeaponAttributeCombatEffects>(); // 속성 전투 효과 조회
            WeaponAttributeDebugControls debugControls = player.GetComponent<WeaponAttributeDebugControls>(); // 디버그 입력 조회
            WeaponAttributeHUDPresenter hudPresenter = hud == null ? null : hud.GetComponent<WeaponAttributeHUDPresenter>(); // HUD 표시기 조회
            Text hudText = hud == null ? null : hud.GetComponent<Text>(); // HUD Text 조회
            WeaponData fireWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(FireWeaponPath); // 화염 무기 조회
            WeaponData physicalWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(PhysicalWeaponPath); // 물리 무기 조회
            bool managerMatches = synergyManager != null && ProjectEpsilonDay15SetupRules.AreSameReference(weaponManager, synergyManager.WeaponManager); // 무기 관리자 연결 검증
            bool hooksMatch = attributeHooks != null && ProjectEpsilonDay15SetupRules.AreSameReference(gradeHooks, attributeHooks.GradeEffectHooks) && ProjectEpsilonDay15SetupRules.AreSameReference(synergyManager, attributeHooks.SynergyManager); // Hook 연결 검증
            bool combatMatches = combatEffects != null && ProjectEpsilonDay15SetupRules.AreSameReference(synergyManager, combatEffects.SynergyManager) && ProjectEpsilonDay15SetupRules.AreSameReference(attributeHooks, combatEffects.EffectHooks) && ProjectEpsilonDay15SetupRules.AreSameReference(combatEffects, weaponManager.AttributeCombatEffects); // 전투 효과 연결 검증
            bool hudMatches = hudPresenter != null && ProjectEpsilonDay15SetupRules.AreSameReference(synergyManager, hudPresenter.SynergyManager) && ProjectEpsilonDay15SetupRules.AreSameReference(hudText, hudPresenter.AttributeText); // HUD 연결 검증
            bool debugMatches = debugControls != null && ProjectEpsilonDay15SetupRules.AreSameReference(bodyManager, debugControls.BodyManager) && ProjectEpsilonDay15SetupRules.AreSameReference(weaponManager, debugControls.WeaponManager) && ProjectEpsilonDay15SetupRules.AreSameReference(fireWeapon, debugControls.FireWeapon) && ProjectEpsilonDay15SetupRules.AreSameReference(physicalWeapon, debugControls.FallbackWeapon); // 디버그 연결 검증

            return managerMatches && hooksMatch && combatMatches && hudMatches && debugMatches; // 정확한 참조 구성 상태 반환
        } // 메서드 끝

        private static WeaponAttributeHUDPresenter EnsureAttributeHud(Transform hudCanvas, WeaponAttributeSynergyManager synergyManager) // 속성 HUD 확보
        { // 메서드 시작
            Transform existing = hudCanvas.Find("AttributeSynergyHUD"); // 기존 HUD 탐색
            GameObject hudObject; // HUD 오브젝트 변수

            if (existing == null) // 기존 HUD 없음 확인
            { // 조건 시작
                hudObject = new GameObject("AttributeSynergyHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); // HUD 오브젝트 생성
                hudObject.transform.SetParent(hudCanvas, false); // Canvas 아래 배치
            } // 조건 끝
            else // 기존 HUD 존재
            { // 대안 시작
                hudObject = existing.gameObject; // 기존 오브젝트 사용
            } // 대안 끝

            RectTransform rect = EnsureComponent<RectTransform>(hudObject); // 사각 변환 확보
            rect.anchorMin = new Vector2(1f, 1f); // 우측 상단 최소 앵커
            rect.anchorMax = new Vector2(1f, 1f); // 우측 상단 최대 앵커
            rect.pivot = new Vector2(1f, 1f); // 우측 상단 피벗
            rect.anchoredPosition = new Vector2(-24f, -150f); // HUD 위치 지정
            rect.sizeDelta = new Vector2(280f, 250f); // HUD 크기 지정

            Text text = EnsureComponent<Text>(hudObject); // Text 확보
            text.font = ResolveUIFont(hudCanvas); // UI 글꼴 지정
            text.fontSize = 18; // 글자 크기 지정
            text.alignment = TextAnchor.UpperLeft; // 왼쪽 위 정렬
            text.color = new Color(0.9f, 0.95f, 1f, 1f); // 글자색 지정
            text.raycastTarget = false; // 마우스 차단 해제

            WeaponAttributeHUDPresenter presenter = EnsureComponent<WeaponAttributeHUDPresenter>(hudObject); // HUD 표시기 확보
            presenter.Configure(synergyManager, text); // 표시기 연결
            EditorUtility.SetDirty(text); // Text 변경 표시
            EditorUtility.SetDirty(rect); // Rect 변경 표시
            return presenter; // 표시기 반환
        } // 메서드 끝

        private static Font ResolveUIFont(Transform hudCanvas) // UI 글꼴 탐색
        { // 메서드 시작
            Text existingText = hudCanvas.GetComponentInChildren<Text>(true); // 기존 Text 탐색

            if (existingText != null && existingText.font != null) // 기존 글꼴 확인
            { // 조건 시작
                return existingText.font; // 기존 글꼴 반환
            } // 조건 끝

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 기본 글꼴 반환
        } // 메서드 끝

        private static T EnsureComponent<T>(GameObject target) where T : Component // 컴포넌트 확보 도우미
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

            AssetDatabase.DeleteAsset(LegacySetupPath); // Day14 Setup 삭제
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
