using System; // 예외와 형식
using System.Reflection; // 실제 시각 효과 경로 조회
using ProjectEpsilon.Combat; // 실제 속성 공격과 반응
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Data; // 실제 포탑 속성
using ProjectEpsilon.Player; // 실제 체력과 몸통
using ProjectEpsilon.Recording; // 실제 적 공격 피해 원인
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 기능
namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay83VfxVerification // 속성과 플레이어 시각 효과 시험
    { // 시험 시작
        private const string Key = "EpsilonDay83Vfx"; // 시험 진행 표식
        private static bool started; // 중복 실행 방지
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 조회
        static AutomatedDay83VfxVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 시각 효과 시험
            { // 시험 시작
                Verify(); // 실제 속성·반응·플레이어 표시 확인
                Finish(0, "Eight attribute hits, six reactions, player damage, body loss, invulnerability and effect limits passed."); // 성공 결과 기록
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 시각 효과 확인
        { // 함수 시작
            Type visuals = Type.GetType("ProjectEpsilon.Combat.CombatVfx, Assembly-CSharp"); // 실제 공통 속성 시각
            Type playerVisuals = Type.GetType("ProjectEpsilon.Player.PlayerCombatVfx, Assembly-CSharp"); // 실제 플레이어 시각
            Require(visuals != null && playerVisuals != null, "속성 또는 플레이어 시각 효과 누락"); // 실제 구현 확인
            MethodInfo attributeSprite = visuals.GetMethod("GetAttributeSprite"); // 실제 속성 이미지 조회
            MethodInfo reactionSprite = visuals.GetMethod("GetReactionSprite"); // 실제 반응 이미지 조회
            Require(attributeSprite != null && reactionSprite != null, "속성 또는 반응 이미지 조회 누락"); // 실제 이미지 경로 확인
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 전투 효과 관리자
            Require(effects != null, "실제 속성 전투 관리자 누락"); // 실제 전투 구성 확인
            for (int index = 0; index < 8; index++) // 실제 여덟 속성 확인
            { // 반복 시작
                WeaponAttribute attribute = (WeaponAttribute)index; // 실제 속성 종류
                Sprite expected = (Sprite)attributeSprite.Invoke(null, new object[] { attribute }); // 실제 속성 이미지
                Require(expected != null, "속성 이미지 누락: " + attribute); // 실제 속성 이미지 확인
                Require(CellMatches(expected, index), "속성 이미지 셀 위치 오류: " + attribute); // 실제 아틀라스 셀 순서 확인
                WeaponTarget target = NewTarget(index * 3f); // 실제 속성 명중 대상
                effects.ApplyHit(new WeaponAttributeAttackSnapshot(null, attribute, 0, 0, 1, Vector3.zero, 1f), target, target.transform.position); // 시너지 없는 실제 속성 명중
                GameObject pulse = GameObject.Find(attribute + "_HitPulse"); // 실제 명중 표시 객체
                Require(pulse != null && pulse.GetComponent<SpriteRenderer>().sprite == expected && pulse.GetComponent<SpriteRenderer>().sortingOrder < 40, "기본 속성 명중 표시 오류: " + attribute); // 실제 이미지와 경고 순서 확인
            } // 반복 종료
            WeaponAttribute[,] pairs = { { WeaponAttribute.Fire, WeaponAttribute.Poison }, { WeaponAttribute.Cold, WeaponAttribute.Electric }, { WeaponAttribute.Physical, WeaponAttribute.Explosion }, { WeaponAttribute.Holy, WeaponAttribute.Fire }, { WeaponAttribute.Dark, WeaponAttribute.Poison }, { WeaponAttribute.Holy, WeaponAttribute.Dark } }; // 실제 여섯 속성 반응
            for (int index = 0; index < pairs.GetLength(0); index++) // 실제 여섯 반응 확인
            { // 반복 시작
                WeaponAttributeReaction reaction = (WeaponAttributeReaction)(index + 1); // 실제 반응 종류
                Sprite expected = (Sprite)reactionSprite.Invoke(null, new object[] { reaction }); // 실제 반응 이미지
                Require(expected != null, "반응 이미지 누락: " + reaction); // 실제 반응 이미지 확인
                Require(CellMatches(expected, index + 8), "반응 이미지 셀 위치 오류: " + reaction); // 실제 아틀라스 셀 순서 확인
                WeaponTarget target = NewTarget(40f + index * 3f); // 실제 반응 대상
                effects.ApplyHit(new WeaponAttributeAttackSnapshot(null, pairs[index, 0], 2, 2, 1, Vector3.zero, 1f), target, target.transform.position); // 첫 실제 반응 재료
                effects.ApplyHit(new WeaponAttributeAttackSnapshot(null, pairs[index, 1], 2, 2, 1, Vector3.zero, 1f), target, target.transform.position); // 두 번째 실제 반응 재료
                GameObject pulse = GameObject.Find("Reaction_" + reaction); // 실제 반응 표시 객체
                Require(pulse != null && pulse.GetComponent<SpriteRenderer>().sprite == expected && pulse.GetComponent<SpriteRenderer>().sortingOrder < 40, "실제 반응 표시 오류: " + reaction); // 실제 반응 이미지와 경고 순서 확인
            } // 반복 종료
            Component playerVfx = GameManager.Instance.GetComponent(playerVisuals); // 실제 전투 관리자 플레이어 시각
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 머리
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 무적 처리
            Require(playerVfx != null && body != null && health != null && invulnerability != null, "실제 플레이어 시각 구성 누락"); // 실제 플레이어 구성 확인
            SpriteRenderer headVisual = player.GetComponent<SpriteRenderer>(); // 실제 머리 이미지
            SpriteRenderer bodyVisual = body.BodySegments[0].GetComponent<SpriteRenderer>(); // 실제 첫 몸통 이미지
            Color originalHead = headVisual.color; // 실제 머리 원래 색상
            Color originalBody = bodyVisual.color; // 실제 몸통 원래 색상
            Require(health.TakeDamage(1, DamageOrigin.EnemyProjectile) && GameObject.Find("PlayerDamageVfx") == null && headVisual.color.a < originalHead.a && bodyVisual.color.a < originalBody.a, "적 공격 피격 이미지 제거 또는 뱀 점멸 실패"); // 실제 적 투사체 피해의 몸통 점멸 확인
            playerVisuals.GetMethod("RestoreBlink", Private).Invoke(playerVfx, null); // 실제 투사체 점멸 종료 후 색상 복구
            Vector3 lastPosition = body.BodySegments[body.BodySegments.Count - 1].transform.position; // 실제 마지막 몸통 위치
            GameObject removedBody = body.BodySegments[body.BodySegments.Count - 1].gameObject; // 실제 제거 대상 몸통
            Require(body.TryRemoveBody(), "실제 몸통 제거 실패"); // 실제 몸통 손실 적용
            Require(headVisual.color.a < originalHead.a, "적 직접 충돌의 몸통 손실 점멸 누락"); // 실제 몸통 손실의 별도 점멸 확인
            Require(!removedBody.activeInHierarchy, "제거된 몸통의 이미지 또는 충돌 판정 잔류"); // 실제 몸통 제거 즉시 비활성 확인
            GameObject bodyEffect = GameObject.Find("PlayerBodyBreakVfx"); // 실제 몸통 손실 표시
            Require(bodyEffect != null && bodyEffect.GetComponent<SpriteRenderer>().sprite.name == "VFX_BodyBreak" && Vector3.Distance(bodyEffect.transform.position, lastPosition) < 0.01f, "실제 몸통 손실 위치 표시 오류"); // 실제 손실 위치와 이미지 확인
            invulnerability.StartInvulnerability(2f); // 실제 무적 시작
            Transform ring = player.transform.Find("PlayerInvulnerabilityVfx"); // 실제 무적 표시
            Require(ring != null && ring.gameObject.activeSelf && ring.GetComponent<SpriteRenderer>().sortingOrder < 40, "실제 무적 표시 누락"); // 실제 무적과 경고 표시 순서 확인
            invulnerability.ClearInvulnerability(); // 실제 무적 종료
            playerVisuals.GetMethod("Update", Private).Invoke(playerVfx, null); // 실제 무적 표시 갱신
            Require(!ring.gameObject.activeSelf, "무적 종료 후 표시 잔류"); // 실제 무적 종료 확인
            MethodInfo spawnHit = visuals.GetMethod("SpawnAttribute"); // 실제 속성 표시 생성
            for (int index = 0; index < 100; index++) // 다수 명중 표시 제한 확인
            { // 반복 시작
                spawnHit.Invoke(null, new object[] { WeaponAttribute.Fire, 0, Vector3.zero }); // 실제 기본 명중 표시 요청
            } // 반복 종료
            int active = (int)visuals.GetProperty("ActiveHitCount").GetValue(null); // 실제 활성 명중 표시 수
            Require(active == 48, "명중 표시 동시 상한 오류"); // 실제 시각 효과 상한 확인
            GameObject priority = (GameObject)visuals.GetMethod("SpawnReaction").Invoke(null, new object[] { WeaponAttributeReaction.CoolingDischarge, Vector3.zero }); // 일반 표시 상한 중 우선 반응 요청
            Require(priority != null && priority.GetComponent<Collider2D>() == null, "명중 표시 상한이 반응 표시를 차단"); // 실제 우선 표시와 무충돌 확인
            GameObject limited = GameObject.Find("Fire_HitPulse"); // 실제 제한 대상 표시
            limited.SetActive(false); // 실제 명중 표시 비활성화
            Require((int)visuals.GetProperty("ActiveHitCount").GetValue(null) == 47, "명중 표시 제거 후 상한 반환 실패"); // 실제 표시 수 감소 확인
            spawnHit.Invoke(null, new object[] { WeaponAttribute.Fire, 0, Vector3.zero }); // 실제 반환된 표시 슬롯 사용
            Require((int)visuals.GetProperty("ActiveHitCount").GetValue(null) == 48, "반환된 명중 표시 슬롯 재사용 실패"); // 실제 상한 슬롯 재사용 확인
            int previousDamage = Count("PlayerDamageVfx"); // 기존 피격 표시 수
            ((Behaviour)playerVfx).enabled = false; // 실제 플레이어 표시 비활성화
            Require(headVisual.color == originalHead && bodyVisual.color == originalBody, "표시 비활성 시 원래 뱀 색상 복구 실패"); // 실제 점멸 색상 복구 확인
            Require(health.TakeDamage(1) && Count("PlayerDamageVfx") == previousDamage && headVisual.color == originalHead && bodyVisual.color == originalBody, "비활성화된 플레이어 표시의 이벤트 구독 잔류"); // 실제 구독 해제 확인
        } // 함수 종료
        private static int Count(string name) // 실제 이름별 시각 효과 수
        { // 함수 시작
            int count = 0; // 실제 표시 수
            foreach (WeaponAttackPulse pulse in UnityEngine.Object.FindObjectsByType<WeaponAttackPulse>(FindObjectsSortMode.None)) // 실제 활성 표시 순회
            { // 반복 시작
                if (pulse.gameObject.name == name) // 실제 표시 이름 확인
                { // 조건 시작
                    count++; // 실제 표시 수 증가
                } // 조건 종료
            } // 반복 종료
            return count; // 실제 표시 수 반환
        } // 함수 종료
        private static bool CellMatches(Sprite sprite, int index) // 실제 아틀라스 셀 위치 확인
        { // 함수 시작
            float centerX = (index % 4 + 0.5f) * sprite.texture.width / 4f; // 실제 셀 열 중심
            float centerY = (3 - index / 4 + 0.5f) * sprite.texture.height / 4f; // 실제 셀 행 중심
            return Mathf.Abs(sprite.rect.center.x - centerX) <= 1f && Mathf.Abs(sprite.rect.center.y - centerY) <= 1f; // 실제 셀 중심 일치 여부
        } // 함수 종료
        private static WeaponTarget NewTarget(float x) // 독립된 실제 반응 대상
        { // 함수 시작
            GameObject instance = new GameObject("Day83VfxTarget"); // 실제 전투 대상 객체
            instance.transform.position = new Vector3(x, 0f, 0f); // 실제 대상 위치
            WeaponTarget target = instance.AddComponent<WeaponTarget>(); // 실제 전투 대상
            target.Configure(10000f); // 실제 생존 체력
            return target; // 실제 대상 반환
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
            Debug.Log("[Day83Vfx] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
