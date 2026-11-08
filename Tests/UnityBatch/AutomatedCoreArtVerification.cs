using System; // 예외 형식
using System.Reflection; // 실제 보석 병합 경로
using ProjectEpsilon.Combat; // 실제 무기 슬롯
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현 통제
using ProjectEpsilon.Player; // 실제 플레이어와 몸통
using ProjectEpsilon.Progression; // 실제 경험치 보석
using ProjectEpsilon.UI; // 실제 무기 선택 화면
using ProjectEpsilon.World; // 실제 청크 맵
using UnityEditor; // 배치와 에셋 조회
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 선택 버튼 이미지

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedCoreArtVerification // 실제 핵심 이미지 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonCoreArtVerification"; // 시험 진행 표식
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 범위
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedCoreArtVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 시험 결과 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for core art test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 선택 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵 조회
            if (player == null || body == null || weapons == null || map == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            try // 실제 시험 오류 감시
            { // 보호 시작
                player.enabled = false; // 실제 플레이어 이동 통제
                EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
                if (spawner != null) // 실제 출현기 확인
                { // 조건 시작
                    spawner.enabled = false; // 추가 적 출현 통제
                } // 조건 끝
                VerifyWeapons(weapons, body); // 실제 무기 이미지와 슬롯 갱신 확인
                VerifyWeaponSelection(); // 실제 선택 화면 무기 이미지 확인
                VerifyGems(player); // 실제 보석 등급과 병합 이미지 확인
                VerifyGround(player, map); // 실제 청크 바닥과 연결 확인
                Finish(0, "Actual six weapon assets, slots, gem tiers and 3x3 ground passed."); // 실제 전체 결과
            } // 보호 끝
            catch (Exception error) // 실제 시험 오류 확인
            { // 예외 시작
                Finish(1, error.ToString()); // 실제 실패 기록
            } // 예외 끝
        } // 메서드 끝
        private static void VerifyWeapons(SnakeWeaponManager weapons, SnakeBodyManager body) // 실제 무기와 몸통 표시 검증
        { // 메서드 시작
            string[] files = { "DebugBlade", "DebugNeedle", "DebugNova", "DebugCutter", "DebugBlaster", "DebugPulse" }; // 실제 시험 무기 데이터
            string[] images = { "WeaponSteelSword_Casual", "WeaponSteelSpear_Casual", "WeaponMace_Casual", "WeaponFireSword_Casual", "WeaponFireball_Casual", "WeaponFireRing_Casual" }; // 실제 무기 이미지
            PropertyInfo art = typeof(WeaponData).GetProperty("Artwork"); // 실제 무기 이미지 속성
            Require(art != null, "Actual weapon artwork property missing."); // 실제 데이터 연결 확인
            Require(weapons.Slots.Count >= 2 && body.CurrentBodyCount >= 2, "Actual weapon slots missing."); // 실제 몸통 슬롯 확인
            WeaponData first = null; // 실제 합성 무기 저장
            for (int index = 0; index < files.Length; index++) // 실제 무기 종류 순회
            { // 반복 시작
                WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/" + files[index] + ".asset"); // 실제 무기 데이터 조회
                Sprite sprite = weapon == null ? null : art.GetValue(weapon) as Sprite; // 실제 무기 이미지
                Require(sprite != null && sprite.name == images[index], "Actual weapon artwork missing: " + files[index]); // 실제 무기별 이미지 확인
                Require(weapons.TryEquipAt(0, weapon), "Actual slot equip failed."); // 실제 몸통 무기 교체
                VerifyWeaponVisual(weapons.Slots[0], sprite); // 실제 몸통 위치의 이미지 확인
                if (index == 0) // 실제 첫 무기 확인
                { // 조건 시작
                    first = weapon; // 실제 합성 무기 보관
                } // 조건 끝
            } // 반복 끝
            Require(weapons.TryEquipAt(0, first) && weapons.TryEquipAt(1, first), "Actual merge setup failed."); // 실제 동일 무기 장착
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "Actual weapon merge failed."); // 실제 무기 합성
            VerifyWeaponVisual(weapons.Slots[0], (Sprite)art.GetValue(first)); // 실제 합성 결과 이미지 확인
            Transform emptyVisual = weapons.Slots[1].Origin.Find("WeaponVisual"); // 실제 빈 슬롯 이미지 조회
            Require(emptyVisual == null || !emptyVisual.gameObject.activeSelf, "Actual merged-away weapon visual remained active."); // 실제 합성 소모 이미지 제거 확인
            Require(body.TryGainBodyFromLevelUp() && weapons.Slots.Count == 4, "Actual growth did not add a weapon slot."); // 실제 몸통 성장과 슬롯 확인
            Require(weapons.TryEquipAt(3, first), "Actual grown slot equip failed."); // 실제 성장 몸통 무기 장착
            VerifyWeaponVisual(weapons.Slots[3], (Sprite)art.GetValue(first)); // 실제 성장 몸통 이미지 확인
            Transform lostVisual = weapons.Slots[3].Origin.Find("WeaponVisual"); // 실제 손실 예정 무기 표시
            Require(body.RemoveBodies(1) == 1 && weapons.Slots.Count == 3, "Actual body loss did not remove its slot."); // 실제 몸통 손실과 슬롯 제거 확인
            Require(lostVisual == null || !lostVisual.gameObject.activeInHierarchy, "Actual removed body retained a visible weapon."); // 실제 손실 몸통 이미지 제거 확인
        } // 메서드 끝
        private static void VerifyWeaponVisual(SnakeWeaponSlot slot, Sprite expected) // 실제 장착 무기 표시 확인
        { // 메서드 시작
            Transform visual = slot.Origin == null ? null : slot.Origin.Find("WeaponVisual"); // 실제 몸통 아래 무기 표시
            SpriteRenderer renderer = visual == null ? null : visual.GetComponent<SpriteRenderer>(); // 실제 무기 표시기
            Require(renderer != null && visual.gameObject.activeSelf && renderer.sprite == expected && renderer.color == Color.white, "Actual weapon visual did not follow its slot."); // 실제 이미지와 색상 확인
            Require(visual.GetComponent<Collider2D>() == null, "Actual weapon decoration added a collider."); // 실제 공격 판정 영향 방지 확인
        } // 메서드 끝
        private static void VerifyGems(SnakeMovement player) // 실제 경험치 보석 이미지 검증
        { // 메서드 시작
            string[] images = { "ExperienceSmall_Casual", "ExperienceMedium_Casual", "ExperienceLarge_Casual" }; // 실제 등급별 보석 이미지
            int[] values = { 1, 5, 20 }; // 실제 등급별 경험치
            ExperiencePickup first = null; // 실제 병합 결과 보석
            for (int index = 0; index < values.Length; index++) // 실제 보석 등급 순회
            { // 반복 시작
                ExperiencePickup pickup = ExperiencePickup.Spawn(player.transform.position + Vector3.right * (30f + index * 4f), values[index], null, SnakeExperience.Current); // 실제 보석 생성
                SpriteRenderer renderer = pickup.GetComponent<SpriteRenderer>(); // 실제 보석 표시기
                Require(renderer.sprite != null && renderer.sprite.name == images[index] && renderer.color == Color.white, "Actual gem tier image missing: " + values[index]); // 실제 등급별 이미지 확인
                if (index == 0) // 실제 첫 보석 확인
                { // 조건 시작
                    first = pickup; // 실제 병합 대상 보관
                } // 조건 끝
            } // 반복 끝
            MethodInfo promote = typeof(ExperiencePickup).GetMethod("PromoteForMerge", Private); // 실제 보석 병합 갱신 경로
            Require(promote != null, "Actual gem merge path missing."); // 실제 병합 경로 확인
            promote.Invoke(first, new object[] { 20 }); // 실제 소형 보석의 대형 승급
            Require(first.ExperienceValue == 20 && first.GetComponent<SpriteRenderer>().sprite.name == images[2], "Actual merged gem image did not update."); // 실제 병합 뒤 이미지 확인
            ExperienceGemVisualPalette palette = UnityEngine.Object.FindFirstObjectByType<ExperienceGemVisualPalette>(); // 실제 보석 이미지 설정
            Require(palette != null, "Actual gem palette missing."); // 실제 설정 존재 확인
            palette.enabled = false; // 기존 대체 이미지 경계 통제
            Sprite fallback = player.GetComponent<SpriteRenderer>().sprite; // 실제 대체 이미지
            ExperiencePickup legacy = ExperiencePickup.Spawn(player.transform.position + Vector3.right * 50f, 1, fallback, SnakeExperience.Current); // 실제 대체 보석 생성
            Require(legacy.GetComponent<SpriteRenderer>().sprite == fallback && legacy.GetComponent<SpriteRenderer>().color != Color.white, "Actual disabled palette did not preserve fallback visuals."); // 실제 미설정 대체 표시 확인
            palette.enabled = true; // 실제 보석 이미지 설정 복구
        } // 메서드 끝
        private static void VerifyWeaponSelection() // 실제 무기 선택 화면 이미지 검증
        { // 메서드 시작
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 선택 화면
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 검 데이터
            Require(panel != null && weapon != null, "Actual weapon selection fixtures missing."); // 실제 선택 구성 확인
            FieldInfo buttonField = typeof(LevelUpPanelController).GetField("candidateButton01", Private); // 실제 첫 후보 버튼 필드
            Button button = buttonField == null ? null : buttonField.GetValue(panel) as Button; // 실제 첫 후보 버튼
            FieldInfo labelField = typeof(LevelUpPanelController).GetField("candidateLabel01", Private); // 실제 첫 후보 문구 필드
            Text label = labelField == null ? null : labelField.GetValue(panel) as Text; // 실제 첫 후보 문구
            Require(button != null && label != null, "Actual weapon selection button or label missing."); // 실제 후보 구성 확인
            panel.ShowEliteRewards(new[] { "HEAL", "REPAIR", "WEAPON" }, new[] { true, true, true }); // 실제 원래 보상 문구 영역 표시
            Vector2 originalOffset = label.rectTransform.offsetMax; // 실제 원래 문구 영역
            panel.ShowStartingWeapons(new[] { new WeaponRewardCandidate(weapon, 1) }); // 실제 시작 무기 후보 표시
            Transform icon = button == null ? null : button.transform.Find("WeaponArtwork"); // 실제 선택 무기 이미지
            Image image = icon == null ? null : icon.GetComponent<Image>(); // 실제 UI 이미지
            Require(image != null && icon.gameObject.activeSelf && image.sprite == weapon.Artwork && !image.raycastTarget, "Actual weapon selection image was missing or blocked input."); // 실제 선택 이미지와 입력 확인
            panel.ShowEliteRewards(new[] { "HEAL", "REPAIR", "WEAPON" }, new[] { true, true, true }); // 실제 엘리트 보상 화면 전환
            Require(!icon.gameObject.activeSelf && label.rectTransform.offsetMax == originalOffset, "Actual weapon image or label layout leaked into the elite reward screen."); // 실제 다른 화면 이미지와 문구 복구 확인
            panel.Hide(); // 실제 시험 화면 정리
        } // 메서드 끝
        private static void VerifyGround(SnakeMovement player, PrototypeChunkMap map) // 실제 청크 바닥 이미지 검증
        { // 메서드 시작
            Vector2Int center = new Vector2Int(Mathf.FloorToInt(player.transform.position.x / map.ChunkSize), Mathf.FloorToInt(player.transform.position.y / map.ChunkSize)); // 실제 중심 청크 좌표
            GameObject chunk = map.GetChunk(center); // 실제 중심 청크
            GameObject neighbor = map.GetChunk(center + Vector2Int.right); // 실제 인접 청크
            GameObject above = map.GetChunk(center + Vector2Int.up); // 실제 위쪽 청크
            SpriteRenderer ground = chunk == null ? null : chunk.GetComponentInChildren<SpriteRenderer>(); // 실제 바닥 표시기
            SpriteRenderer next = neighbor == null ? null : neighbor.GetComponentInChildren<SpriteRenderer>(); // 실제 인접 바닥 표시기
            SpriteRenderer upper = above == null ? null : above.GetComponentInChildren<SpriteRenderer>(); // 실제 위쪽 바닥 표시기
            Require(map.ActiveCount == 9 && ground != null && next != null && upper != null && ground.sprite != null && ground.sprite.name == "PlainsGround_Casual", "Actual 3x3 ground sprite missing."); // 실제 바닥 연결 확인
            Require(Mathf.Abs(ground.bounds.size.x - map.ChunkSize) < 0.01f && Mathf.Abs(ground.bounds.size.y - map.ChunkSize) < 0.01f, "Actual ground did not cover its chunk."); // 실제 청크 크기 확인
            Require(ground.flipX != next.flipX && ground.flipY == next.flipY, "Actual neighboring ground did not mirror horizontally."); // 실제 인접 경계 반전 확인
            Require(ground.flipY != upper.flipY && ground.flipX == upper.flipX, "Actual neighboring ground did not mirror vertically."); // 실제 위쪽 경계 반전 확인
            Require(ground.GetComponent<Collider2D>() == null && ground.sortingOrder < player.GetComponent<SpriteRenderer>().sortingOrder, "Actual ground affected collision or draw order."); // 실제 충돌과 표시 순서 확인
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 끝
        } // 메서드 끝
        private static void Finish(int code, string message) // 실제 결과 기록
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[CoreArtVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
