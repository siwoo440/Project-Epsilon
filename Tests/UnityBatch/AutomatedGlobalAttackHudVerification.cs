using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Combat; // 무기와 표적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치
using ProjectEpsilon.UI; // 실제 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 HUD 텍스트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedGlobalAttackHudVerification // 실제 원거리 자동 공격과 HUD 시험
    { // 클래스 시작
        private const string Key = "EpsilonGlobalAttackHudVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedGlobalAttackHudVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<GlobalAttackHudRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[GlobalAttackHudVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class GlobalAttackHudRunner : MonoBehaviour // 실제 원거리 공격과 HUD 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 게임 시험
        { // 코루틴 시작
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            follower.enabled = false; // 몸통 시험 위치 고정
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            weapons.enabled = false; // 표적 준비 중 자동 공격 중지
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 기존 무기 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 기존 무기 제거
            } // 반복 끝
            foreach (WeaponTarget target in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 전투 대상 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 시험 외 대상 비활성화
            } // 반복 끝
            Vector3 origin = new Vector3(50f, 50f, 0f); // 실제 머리와 떨어진 몸통 위치
            weapons.Slots[2].Origin.position = origin; // 실제 세 번째 몸통 위치
            WeaponTarget near = CreateTarget("GlobalNear", origin + Vector3.right * 10f); // 사거리 밖 가까운 적
            WeaponTarget far = CreateTarget("GlobalFar", origin + Vector3.right * 15f); // 더 먼 적
            WeaponData blade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 기존 근접 무기
            Require(weapons.TryEquipAt(2, blade), "Could not equip melee."); // 실제 몸통 장착
            weapons.enabled = true; // 실제 자동 공격 시작
            yield return new WaitForSeconds(0.2f); // 첫 공격 대기
            Require(near.CurrentHealth == 90f && far.CurrentHealth == 100f, "Out-of-range melee did not attack the nearest target."); // 사거리 밖 최근접 공격 확인
            Type tracerType = typeof(SnakeWeaponManager).Assembly.GetType("ProjectEpsilon.Combat.TurretAttackTracer"); // 실제 비행 연출 형식
            Component tracer = tracerType == null ? null : UnityEngine.Object.FindFirstObjectByType(tracerType) as Component; // 실제 근접 발사 표시
            Require(tracer != null && tracer.GetComponent<SpriteRenderer>() != null && tracer.GetComponent<SpriteRenderer>().sprite != null && tracer.GetComponent<SpriteRenderer>().sprite.name == "TurretProjectile_Prototype" && tracer.GetComponent<Collider2D>() == null, "Actual melee projectile visual is missing or affects collisions."); // 실제 탄 이미지와 시각 전용 확인
            Require(tracer.transform.position.x > origin.x && tracer.transform.position.x < near.transform.position.x, "Actual melee projectile visual did not travel from the body toward the target."); // 실제 비행 방향 확인
            Vector3 pausedTracer = tracer.transform.position; // 정지 전 실제 탄 위치
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.15f); // 실제 정지 시간 대기
            Require(tracer != null && Vector3.Distance(tracer.transform.position, pausedTracer) < 0.001f, "Visual tracer moved while paused."); // 정지 중 탄 비행 차단
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(0.25f); // 공격 주기 이전 대기
            Require(near.CurrentHealth == 90f, "Melee ignored attack cooldown."); // 쿨타임 중 재공격 방지 확인
            near.transform.position = origin + Vector3.right * 16f; // 기존 대상의 거리 증가
            far.transform.position = origin + Vector3.right * 10f; // 다른 적을 새로운 최근접 대상으로 이동
            Physics2D.SyncTransforms(); // 실제 충돌 위치 동기화
            yield return new WaitForSeconds(0.55f); // 다음 공격 주기 대기
            Require(near.CurrentHealth == 90f && far.CurrentHealth == 90f, "Melee did not switch to the nearest target after cooldown."); // 쿨타임 후 실제 최근접 대상 변경 확인
            weapons.enabled = false; // 투사체 시험 준비
            near.transform.position = origin + Vector3.right * 30f; // 기존 수명 밖 실제 목표 위치
            far.transform.position = origin + Vector3.right * 50f; // 더 먼 실제 표적 위치
            near.Configure(100f); // 가까운 표적 체력 복구
            far.Configure(100f); // 먼 표적 체력 복구
            Physics2D.SyncTransforms(); // 실제 충돌 위치 동기화
            WeaponData spear = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 기존 직선 투사체 무기
            Require(weapons.TryEquipAt(2, spear), "Could not equip projectile."); // 실제 몸통 장착
            weapons.enabled = true; // 실제 투사체 발사
            yield return new WaitForSeconds(0.15f); // 첫 발사 대기
            weapons.enabled = false; // 중복 발사 중지
            StraightProjectile actualShot = UnityEngine.Object.FindFirstObjectByType<StraightProjectile>(); // 실제 공격 투사체
            SpriteRenderer actualArt = actualShot == null ? null : actualShot.transform.Find("ProjectileArtwork")?.GetComponent<SpriteRenderer>(); // 실제 투사체 자식 이미지
            Require(actualShot != null && actualArt != null && actualArt.sprite != null && actualArt.sprite.name == "TurretProjectile_Prototype" && Mathf.Approximately(actualShot.GetComponent<CircleCollider2D>().radius, 0.3f) && Mathf.Approximately(actualShot.transform.localScale.x, 0.18f), "Actual projectile artwork or original hitbox is missing."); // 실제 탄 이미지와 기존 충돌체 확인
            Require(actualShot.transform.position.x > origin.x, "Actual damaging projectile did not travel from the equipped body."); // 실제 피해 탄 이동 확인
            yield return new WaitForSeconds(3.2f); // 기존 수명 밖 표적 도달 대기
            Require(near.CurrentHealth == 92f && far.CurrentHealth == 100f, "Projectile did not reach the nearest out-of-range target."); // 원거리 최근접 실제 명중 확인
            foreach (StraightProjectile projectile in UnityEngine.Object.FindObjectsByType<StraightProjectile>(FindObjectsSortMode.None)) // 남은 시험 투사체 순회
            { // 반복 시작
                projectile.gameObject.SetActive(false); // 이전 투사체 접촉 중지
                Destroy(projectile.gameObject); // 이전 투사체 제거
            } // 반복 끝
            near.transform.position = origin + Vector3.right * 12f; // 범위 밖 가까운 표적
            far.transform.position = origin + Vector3.right * 12.6f; // 가까운 표적 주변 적
            near.Configure(100f); // 가까운 표적 체력 복구
            far.Configure(100f); // 주변 표적 체력 복구
            Physics2D.SyncTransforms(); // 실제 충돌 위치 동기화
            WeaponData area = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNova.asset"); // 기존 범위 무기
            Require(weapons.TryEquipAt(2, area), "Could not equip area weapon."); // 실제 몸통 장착
            weapons.enabled = true; // 실제 범위 공격 시작
            yield return new WaitForSeconds(0.2f); // 실제 범위 공격 대기
            weapons.enabled = false; // 시험 공격 중지
            Require(near.CurrentHealth == 93f && far.CurrentHealth == 93f, "Out-of-range area attack did not hit the nearest target and neighbors."); // 먼 표적과 주변 범위 피해 확인
            Component areaTracer = UnityEngine.Object.FindFirstObjectByType(tracerType) as Component; // 실제 범위 공격 탄 표시
            Require(areaTracer != null && areaTracer.transform.position.x > origin.x && areaTracer.transform.position.x < near.transform.position.x, "Area attack projectile visual did not travel toward the target."); // 실제 범위 탄 비행 확인
            near.Configure(100f); // 가까운 표적 체력 복구
            far.Configure(100f); // 주변 표적 체력 복구
            WeaponData explosion = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeExplosionAura.asset"); // 실제 폭발 범위 무기
            Require(explosion != null && weapons.TryEquipAt(2, explosion), "Could not equip explosion weapon."); // 실제 폭발 무기 장착
            weapons.enabled = true; // 실제 폭발 공격 시작
            yield return new WaitForSeconds(0.2f); // 실제 폭발 공격 대기
            weapons.enabled = false; // 시험 공격 중지
            Require(near.CurrentHealth < far.CurrentHealth && far.CurrentHealth < 100f, "Out-of-range explosion did not use the nearest target as its center."); // 원거리 폭발 중심과 주변 피해 확인
            HUDController hud = UnityEngine.Object.FindFirstObjectByType<HUDController>(); // 실제 HUD 조회
            FieldInfo healthField = typeof(HUDController).GetField("healthText", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 체력 글자 필드
            FieldInfo experienceField = typeof(HUDController).GetField("experienceText", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 경험치 글자 필드
            Text healthText = (Text)healthField.GetValue(hud); // 실제 체력 글자
            Text experienceText = (Text)experienceField.GetValue(hud); // 실제 경험치 글자
            Text levelText = (Text)typeof(HUDController).GetField("levelText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hud); // 실제 레벨 글자
            Text staminaText = (Text)typeof(HUDController).GetField("staminaText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hud); // 실제 스태미나 글자
            GameTimerUI timer = UnityEngine.Object.FindFirstObjectByType<GameTimerUI>(); // 실제 전투 타이머
            Text timerText = (Text)typeof(GameTimerUI).GetField("timerText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(timer); // 실제 타이머 글자
            Require(experienceText.rectTransform.anchorMin == new Vector2(0.5f, 1f) && levelText.rectTransform.anchorMin == new Vector2(0.5f, 1f) && experienceText.rectTransform.anchoredPosition.y < timerText.rectTransform.anchoredPosition.y, "Level and XP are not below the timer."); // 실제 타이머 아래 레벨과 경험치 확인
            Require(levelText.alignment == TextAnchor.UpperLeft && experienceText.alignment == TextAnchor.UpperRight, "Level and XP text alignment mismatch."); // 레벨 왼쪽과 경험치 오른쪽 확인
            Require(staminaText.rectTransform.anchorMin == new Vector2(0f, 1f) && Vector2.Distance(staminaText.rectTransform.anchoredPosition, new Vector2(30f, -150f)) < 0.001f, "Stamina did not use the previous XP position."); // 기존 경험치 위치의 스태미나 확인
            Transform healthBar = healthText.transform.Find("HealthBar"); // 실제 체력 막대
            Transform experienceBar = experienceText.transform.Find("ExperienceBar"); // 실제 경험치 막대
            Transform staminaBar = staminaText.transform.Find("StaminaBar"); // 실제 스태미나 막대
            Require(healthBar != null && experienceBar != null && staminaBar != null, "Health, experience or stamina bar is missing."); // 실제 세 막대 확인
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치
            health.TakeDamage(25); // 실제 체력 피해
            experience.AddExperience(5); // 실제 경험치 획득
            RectTransform healthFill = (RectTransform)healthBar.Find("Fill"); // 실제 체력 채움 영역
            RectTransform experienceFill = (RectTransform)experienceBar.Find("Fill"); // 실제 경험치 채움 영역
            RectTransform staminaFill = (RectTransform)staminaBar.Find("Fill"); // 실제 스태미나 채움 영역
            Require(healthFill != null && Mathf.Abs(healthFill.anchorMax.x - 0.75f) < 0.001f, "Health bar did not reflect real damage."); // 실제 체력 비율 확인
            Require(experienceFill != null && Mathf.Abs(experienceFill.anchorMax.x - 0.5f) < 0.001f, "Experience bar did not reflect real XP."); // 실제 경험치 비율 확인
            SnakeStamina stamina = GetComponent<SnakeStamina>(); // 실제 스태미나 구성
            typeof(SnakeStamina).GetField("currentStamina", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(stamina, 50f); // 시험용 스태미나 상태 설정
            typeof(SnakeStamina).GetMethod("NotifyStaminaChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stamina, null); // 실제 스태미나 이벤트 전달
            Require(staminaFill != null && Mathf.Abs(staminaFill.anchorMax.x - 0.5f) < 0.001f, "Stamina bar did not reflect the real event."); // 실제 스태미나 이벤트 비율 확인
            stamina.ResetStamina(); // 실제 스태미나 회복 이벤트
            Require(Mathf.Abs(staminaFill.anchorMax.x - 1f) < 0.001f, "Stamina bar did not recover."); // 실제 스태미나 회복 비율 확인
            Require(!healthBar.GetComponent<Image>().raycastTarget && !experienceBar.GetComponent<Image>().raycastTarget && !healthFill.GetComponent<Image>().raycastTarget && !experienceFill.GetComponent<Image>().raycastTarget, "HUD bar blocked UI input."); // 막대 입력 차단 방지 확인
            experience.AddExperience(experience.RequiredExperience - experience.CurrentExperience); // 실제 레벨업 경험치 획득
            Require(experience.CurrentLevel == 2 && Mathf.Abs(experienceFill.anchorMax.x) < 0.001f && !experienceFill.gameObject.activeSelf, "Experience bar did not reset after leveling."); // 실제 레벨업 뒤 경험치 막대 초기화 확인
            AutomatedGlobalAttackHudVerification.Finish(0, "Nearest out-of-range melee, cooldown, projectile, area and real health/XP bars."); // 전체 성공 기록
        } // 코루틴 끝
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 표적 생성
        { // 메서드 시작
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 실제 표적 위치
            CircleCollider2D collider = item.AddComponent<CircleCollider2D>(); // 실제 표적 충돌체
            collider.radius = 0.25f; // 표적 반경
            collider.isTrigger = true; // 투사체 트리거 설정
            Rigidbody2D physicsBody = item.AddComponent<Rigidbody2D>(); // 실제 표적 물리체
            physicsBody.bodyType = RigidbodyType2D.Kinematic; // 고정 표적 물리 방식
            physicsBody.useFullKinematicContacts = true; // 운동학 접촉 허용
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 무기 피해 대상
            target.Configure(100f); // 표적 체력
            Physics2D.SyncTransforms(); // 실제 충돌 위치 동기화
            return target; // 실제 표적 반환
        } // 메서드 끝
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedGlobalAttackHudVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
