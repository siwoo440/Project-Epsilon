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
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedTurretBodyVerification // 실제 포탑과 몸통 시험
    {
        private const string Key = "EpsilonTurretBodyVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedTurretBodyVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<TurretBodyVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[TurretBodyVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class TurretBodyVerificationRunner : MonoBehaviour // 실제 포탑과 충돌 크기 실행기
    {
        private IEnumerator Start() // 실제 씬 검증 시작
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 머리
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            player.enabled = false; // 플레이어 이동 중지
            follower.enabled = false; // 몸통 시험 위치 고정
            weapons.enabled = false; // 시험 중 자동 피해 중지
            Require(Mathf.Abs(player.transform.lossyScale.x - 1.44f) < 0.001f, "Player head visual size changed."); // 머리 표시 크기 유지 확인
            Require(Mathf.Abs(body.BodySegments[0].transform.lossyScale.x - 0.62f) < 0.001f && Mathf.Abs(body.TailSegment.transform.lossyScale.x - 0.48f) < 0.001f, "Body or tail visual was not halved."); // 실제 몸통과 꼬리 절반 표시 확인
            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            CircleCollider2D bodyCollider = body.BodySegments[0].GetComponent<CircleCollider2D>(); // 실제 몸통 충돌체
            Require(Mathf.Abs(headCollider.radius * Mathf.Abs(headCollider.transform.lossyScale.x) - 0.2304f) < 0.001f, "Head hitbox was not halved in world space."); // 실제 머리 월드 충돌 반경 확인
            Require(Mathf.Abs(bodyCollider.radius * Mathf.Abs(bodyCollider.transform.lossyScale.x) - 0.2232f) < 0.001f, "Body hitbox was not halved in world space."); // 실제 몸통 월드 충돌 반경 확인
            Require(Mathf.Abs(follower.SegmentSpacing - 0.58f) < 0.001f, "Body spacing does not match smaller segments."); // 축소 몸통 간격 확인
            foreach (WeaponTarget existing in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            {
                existing.gameObject.SetActive(false); // 시험 외 표적 비활성화
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 기존 슬롯 순회
            {
                slot.Clear(); // 기존 무기 제거
            }
            WeaponData steel = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 근접 포탑
            WeaponData fire = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화염탄 포탑
            Require(steel != null && fire != null && steel.DisplayName == "강철 근접 포탑" && fire.DisplayName == "화염탄 포탑", "Turret names are not synchronized."); // 실제 시험용 포탑 이름 확인
            Require(steel.Artwork != null && steel.Artwork.name == "WeaponTurret_Casual" && fire.Artwork == steel.Artwork, "Generated turret artwork missing."); // 실제 공용 포탑 이미지 확인
            weapons.Slots[0].Origin.position = new Vector3(0f, 50f, 0f); // 첫 몸통 시험 위치
            weapons.Slots[1].Origin.position = new Vector3(50f, 50f, 0f); // 둘째 몸통 시험 위치
            Require(weapons.TryEquipAt(0, steel) && weapons.TryEquipAt(1, fire), "Could not equip turrets."); // 실제 두 몸통 무기 장착
            WeaponTarget first = CreateTarget("TurretNearFirst", new Vector3(10f, 50f, 0f)); // 첫 포탑 최근접 적
            WeaponTarget second = CreateTarget("TurretNearSecond", new Vector3(50f, 60f, 0f)); // 둘째 포탑 최근접 적
            yield return null; // 실제 포탑 조준 갱신 대기
            yield return null; // 실제 프레임 갱신 확인
            Transform firstVisual = weapons.Slots[0].Origin.Find("WeaponVisual"); // 첫 몸통 실제 포탑 표시
            Transform secondVisual = weapons.Slots[1].Origin.Find("WeaponVisual"); // 둘째 몸통 실제 포탑 표시
            Require(firstVisual != null && secondVisual != null, "Mounted turret visuals are missing."); // 실제 포탑 표시 확인
            Require(Vector2.Dot(firstVisual.up, Vector2.right) > 0.99f && Vector2.Dot(secondVisual.up, Vector2.up) > 0.99f, "Turrets did not aim at their own nearest targets."); // 몸통별 최근접 조준 확인
            first.transform.position = new Vector3(0f, 60f, 0f); // 첫 대상 이동
            second.transform.position = new Vector3(60f, 50f, 0f); // 둘째 대상 이동
            weapons.Slots[0].Origin.rotation = Quaternion.Euler(0f, 0f, 63f); // 실제 몸통 회전
            yield return null; // 대상과 몸통 회전 후 포탑 갱신
            yield return null; // 실제 조준 반영 대기
            Require(Vector2.Dot(firstVisual.up, Vector2.up) > 0.99f && Vector2.Dot(secondVisual.up, Vector2.right) > 0.99f, "Turret aiming did not follow moving targets independently of body rotation."); // 대상 이동과 몸통 회전 독립 확인
            Quaternion held = firstVisual.rotation; // 마지막 실제 조준 방향
            first.gameObject.SetActive(false); // 첫 대상 제거
            second.gameObject.SetActive(false); // 둘째 대상 제거
            yield return null; // 대상 제거 후 갱신
            yield return null; // 실제 마지막 방향 유지 대기
            Require(Quaternion.Angle(held, firstVisual.rotation) < 0.1f, "Turret did not retain the last direction without targets."); // 대상 없는 마지막 방향 유지 확인
            Require(firstVisual.GetComponent<SpriteRenderer>().color != secondVisual.GetComponent<SpriteRenderer>().color, "Turret attributes have no color distinction."); // 속성별 포탑 색상 구분 확인
            AutomatedTurretBodyVerification.Finish(0, "Halved actual player hitboxes/body and per-body moving-target turret aiming."); // 전체 성공 기록
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 전투 표적 생성
        {
            GameObject item = new GameObject(name); // 표적 객체 생성
            item.transform.position = position; // 실제 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 무기 대상
            target.Configure(100f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedTurretBodyVerification.Finish(1, message); // 실패 원인 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
