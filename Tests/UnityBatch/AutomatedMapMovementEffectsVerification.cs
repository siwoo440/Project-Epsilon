using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 경험치 보석
using ProjectEpsilon.World; // 실제 맵과 특수 지역
using System.Reflection; // 실제 범위 조회
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMapMovementEffectsVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonMapMovementEffectsVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedMapMovementEffectsVerification() // 도메인 재시작 처리
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
        private static void Tick() // 실제 초기화 감시
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
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<MapMovementEffectsRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[MapMovementEffectsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class MapMovementEffectsRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 맵 이동·수집·회복 효과 시험
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치 수신자
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            Require(player != null && health != null && experience != null && map != null && map.Profile != null, "Actual map movement test dependencies missing."); // 실제 구성 확인
            PrototypeZoneKind wind = Parse("Wind"); // 바람길 지역 종류
            PrototypeZoneKind pollen = Parse("Pollen"); // 꽃가루 지역 종류
            PrototypeZoneKind flower = Parse("Flower"); // 회복 꽃밭 종류
            PrototypeZoneKind mud = Parse("Mud"); // 진흙 지역 종류
            PrototypeZoneKind bridge = Parse("Bridge"); // 수초 다리 종류
            PrototypeZoneKind purify = Parse("Purify"); // 정화 꽃 종류
            player.enabled = false; // 시험 중 직접 이동 중지
            map.enabled = false; // 시험 중 자연 청크 갱신 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 포탑 공격 중지
            foreach (PrototypeSpecialZone existing in UnityEngine.Object.FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 시험 지역 순회
            {
                existing.gameObject.SetActive(false); // 다른 지역 효과 차단
            }
            Vector3 point = new Vector3(100f, 100f, 0f); // 격리된 시험 위치
            player.transform.position = point; // 실제 머리 위치
            float baseSpeed = player.CurrentMoveSpeed; // 기존 이동 속도
            PrototypeSpecialZone windZone = PrototypeSpecialZone.Create(map.transform, point, wind, map.Profile); // 실제 바람길 생성
            yield return new WaitForFixedUpdate(); // 실제 머리 진입 판정
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1.1f) < 0.001f, "Wind did not grant ten percent speed."); // 실제 바람길 이동 배율 확인
            player.transform.position = point + Vector3.right * 10f; // 실제 지역 이탈
            yield return new WaitForSeconds(2.2f); // 바람길 효과 종료 대기
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1f) < 0.001f, "Wind speed did not expire."); // 실제 바람길 종료 확인
            player.transform.position = point; // 재사용 대기 중 재진입
            yield return new WaitForFixedUpdate(); // 실제 재진입 판정
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1f) < 0.001f, "Wind retriggered before cooldown."); // 여덟 초 이전 재발동 차단
            player.transform.position = point + Vector3.right * 10f; // 재진입 준비
            yield return new WaitForSeconds(6f); // 실제 여덟 초 경과
            player.transform.position = point; // 재사용 가능한 바람길 진입
            yield return new WaitForFixedUpdate(); // 실제 재발동 판정
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1.1f) < 0.001f, "Wind did not retrigger after cooldown."); // 실제 재발동 확인
            windZone.gameObject.SetActive(false); // 후속 시험의 바람길 제거
            yield return new WaitForSeconds(2.1f); // 실제 바람 효과 종료
            ExperiencePickup pickup = ExperiencePickup.Spawn(point + Vector3.right * 30f, 1, null, experience); // 실제 경험치 보석 생성
            pickup.enabled = false; // 보석 이동 중지
            PropertyInfo range = typeof(ExperiencePickup).GetProperty("MergeAttractionRange", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 흡수 범위 조회
            Require(range != null, "Actual pickup range property missing."); // 실제 범위 경로 확인
            float baseRange = (float)range.GetValue(pickup); // 기존 보석 흡수 범위
            PrototypeSpecialZone pollenZone = PrototypeSpecialZone.Create(map.transform, point, pollen, map.Profile); // 실제 꽃가루 구역 생성
            yield return new WaitForFixedUpdate(); // 실제 꽃가루 진입 판정
            Require(Mathf.Abs((float)range.GetValue(pickup) / baseRange - 1.2f) < 0.001f, "Pollen did not increase pickup range by twenty percent."); // 실제 획득 범위 증가 확인
            int gained = 0; // 실제 획득 경험치 초기화
            experience.ExperienceGained += value => gained += value; // 실제 획득량 기록
            experience.AddExperience(1); // 실제 경험치 한 개 추가
            Require(gained == 1, "Pollen changed experience amount instead of only pickup range."); // 경험치 지급량 유지 확인
            pollenZone.gameObject.SetActive(false); // 재진입 없는 지속 시간 시험
            yield return new WaitForSeconds(6.1f); // 실제 꽃가루 효과 만료
            Require(Mathf.Abs((float)range.GetValue(pickup) / baseRange - 1f) < 0.001f, "Pollen pickup range did not expire."); // 실제 수집 범위 복구 확인
            PrototypeSpecialZone mudOne = PrototypeSpecialZone.Create(map.transform, point, mud, map.Profile); // 첫 진흙 구역
            PrototypeSpecialZone mudTwo = PrototypeSpecialZone.Create(map.transform, point, mud, map.Profile); // 겹친 두 번째 진흙 구역
            yield return new WaitForFixedUpdate(); // 실제 지형 상태 반영
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 0.85f) < 0.001f, "Overlapping mud did not apply a single fifteen percent slow."); // 중복 없는 진흙 감속 확인
            PrototypeSpecialZone bridgeZone = PrototypeSpecialZone.Create(map.transform, point, bridge, map.Profile); // 실제 수초 다리 생성
            yield return new WaitForFixedUpdate(); // 실제 다리 진입 판정
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1f) < 0.001f, "Bridge did not ignore terrain slow."); // 지형 감속 무시 확인
            bridgeZone.gameObject.SetActive(false); // 재진입 없는 면역 시험
            yield return new WaitForSeconds(5.1f); // 실제 다리 효과 만료
            Require(Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 0.85f) < 0.001f, "Terrain slow was not restored after bridge duration."); // 지형 감속 복구 확인
            Require(health.TakeDamage(20), "Could not prepare healing fixture."); // 회복량 확인용 실제 피해
            int before = health.CurrentHealth; // 회복 전 체력
            PrototypeSpecialZone purifyZone = PrototypeSpecialZone.Create(map.transform, point, purify, map.Profile); // 실제 정화 꽃 생성
            yield return new WaitForFixedUpdate(); // 실제 꽃 접촉
            Require(health.CurrentHealth == before + 15 && Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 1f) < 0.001f, "Purify did not heal fifteen and clear terrain slow."); // 정화 회복과 감속 해제 확인
            purifyZone.gameObject.SetActive(false); // 청크 제거에 따른 꽃 비활성
            player.transform.position = point + Vector3.right * 10f; // 진흙 이탈
            yield return new WaitForFixedUpdate(); // 이탈 상태 반영
            player.transform.position = point; // 같은 위치 재방문
            PrototypeSpecialZone.Create(map.transform, point, purify, map.Profile); // 동일 구역 재생성
            int after = health.CurrentHealth; // 재방문 전 체력
            yield return new WaitForFixedUpdate(); // 재생성 접촉 판정
            Require(health.CurrentHealth == after && Mathf.Abs(player.CurrentMoveSpeed / baseSpeed - 0.85f) < 0.001f, $"Recreated purification flower repeated or terrain slow failed to return: health={health.CurrentHealth}/{after}, multiplier={player.CurrentMoveSpeed / baseSpeed:F3}."); // 런당 일회와 재진입 감속 확인
            mudOne.gameObject.SetActive(false); // 첫 시험 진흙 종료
            mudTwo.gameObject.SetActive(false); // 두 번째 시험 진흙 종료
            Require(health.TakeDamage(20), "Could not prepare healing flower fixture."); // 회복 꽃밭 시험 피해
            before = health.CurrentHealth; // 회복 꽃밭 접촉 전 체력
            PrototypeSpecialZone.Create(map.transform, point, flower, map.Profile); // 실제 회복 꽃밭 생성
            yield return new WaitForFixedUpdate(); // 실제 꽃밭 접촉
            Require(health.CurrentHealth == before + 15, "Healing flower did not restore fifteen health."); // 실제 회복 꽃밭 확인
            Vector3 rewardPoint = new Vector3(300f, 300f, 0f); // 별도의 보상 시험 위치
            player.transform.position = rewardPoint; // 실제 보상 지역 머리 진입
            int gemsBefore = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 보석 수
            PrototypeSpecialZone.Create(map.transform, rewardPoint, PrototypeZoneKind.GemTrail, map.Profile); // 실제 평원 보석 꽃길
            yield return new WaitForFixedUpdate(); // 실제 보상 접촉 판정
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 생성된 활성 보석
            Require(gems.Length == gemsBefore + map.Profile.MapGemCount, "Gem trail did not create its configured gems."); // 실제 보석 생성 수 확인
            foreach (ExperiencePickup gem in gems) // 시험 보석 순회
            {
                gem.enabled = false; // 후속 시험 중 자동 수집 방지
            }
            PrototypeSpecialZone repeatedTrail = PrototypeSpecialZone.Create(map.transform, rewardPoint, PrototypeZoneKind.GemTrail, map.Profile); // 같은 보상 위치 재생성
            Require(!repeatedTrail.gameObject.activeSelf, "Claimed gem trail remained visible after regeneration."); // 획득한 보상 표시 숨김 확인
            yield return new WaitForFixedUpdate(); // 재방문 판정
            Require(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == gems.Length, "Recreated gem trail repeated its reward."); // 런당 일회 보상 확인
            Vector3 supplyPoint = new Vector3(400f, 400f, 0f); // 별도의 보급 시험 위치
            player.transform.position = supplyPoint; // 실제 보급 지역 머리 진입
            Require(health.TakeDamage(10), "Could not prepare supply healing fixture."); // 실제 보급 회복 시험 피해
            before = health.CurrentHealth; // 보급 전 체력
            PrototypeSpecialZone.Create(map.transform, supplyPoint, PrototypeZoneKind.Supply, map.Profile); // 실제 폐허 보급 창고
            yield return new WaitForFixedUpdate(); // 실제 보급 접촉 판정
            Require(health.CurrentHealth == before + map.Profile.SupplyHealing && UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == gems.Length + map.Profile.MapGemCount, "Supply did not grant configured healing and gems."); // 실제 보급 회복과 보석 확인
            int supplyHealth = health.CurrentHealth; // 보급 사용 직후 체력
            int supplyGems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 보급 사용 직후 보석 수
            PrototypeSpecialZone repeatedSupply = PrototypeSpecialZone.Create(map.transform, supplyPoint, PrototypeZoneKind.Supply, map.Profile); // 같은 보급 위치 재생성
            Require(!repeatedSupply.gameObject.activeSelf, "Claimed supply remained visible after regeneration."); // 사용한 보급 표시 숨김 확인
            yield return new WaitForFixedUpdate(); // 보급 재방문 판정
            Require(health.CurrentHealth == supplyHealth && UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == supplyGems, "Recreated supply repeated its reward."); // 보급 런당 일회 확인
            AutomatedMapMovementEffectsVerification.Finish(0, "Actual wind, pollen, mud, bridge, healing and single-use map rewards passed."); // 실제 검증 성공 기록
        }

        private static PrototypeZoneKind Parse(string name) // 실제 지역 종류 조회
        {
            Require(Enum.TryParse(name, out PrototypeZoneKind kind), "Map feature kind missing: " + name); // 실제 종류 존재 확인
            return kind; // 실제 지역 종류 반환
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedMapMovementEffectsVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
