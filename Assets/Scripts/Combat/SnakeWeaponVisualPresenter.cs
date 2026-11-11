using System.Collections.Generic; // 실제 몸통별 표시 목록
using ProjectEpsilon.Player; // 실제 몸통 구성
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Combat // 전투 영역
{
    [DefaultExecutionOrder(90)] // 실제 무기 관리자 이후 초기화
    public sealed class SnakeWeaponVisualPresenter : MonoBehaviour // 실제 몸통별 무기 표시
    {
        [SerializeField] private SnakeWeaponManager weapons; // 실제 무기 관리자
        [SerializeField] private float visualScale = 0.65f; // 몸통 기준 이미지 크기 임시값
        private readonly Dictionary<SnakeSegment, SpriteRenderer> visuals = new Dictionary<SnakeSegment, SpriteRenderer>(); // 몸통별 실제 표시기
        private readonly HashSet<SnakeSegment> active = new HashSet<SnakeSegment>(); // 현재 장착 몸통 집합
        private readonly List<SnakeSegment> removed = new List<SnakeSegment>(); // 제거할 표시 대상
        private readonly Dictionary<SnakeSegment, Quaternion> aimDirections = new Dictionary<SnakeSegment, Quaternion>(); // 몸통별 마지막 조준 방향

        private void OnEnable() // 실제 무기 변경 연결
        {
            if (weapons == null) // 관리자 연결 확인
            {
                weapons = GetComponent<SnakeWeaponManager>(); // 같은 객체 관리자 조회
            }
            if (weapons != null) // 실제 관리자 확인
            {
                weapons.SlotsChanged += Refresh; // 실제 슬롯 변경 구독
                Refresh(); // 현재 슬롯 즉시 반영
            }
        }
        private void OnDisable() // 실제 표시 연결 해제
        {
            if (weapons != null) // 실제 관리자 확인
            {
                weapons.SlotsChanged -= Refresh; // 실제 슬롯 변경 구독 해제
            }
            foreach (SpriteRenderer renderer in visuals.Values) // 기존 표시기 순회
            {
                if (renderer != null) // 실제 표시기 확인
                {
                    renderer.gameObject.SetActive(false); // 지연 파괴 전 표시 차단
                    Destroy(renderer.gameObject); // 실제 장식 제거
                }
            }
            visuals.Clear(); // 기존 표시 목록 초기화
            aimDirections.Clear(); // 이전 조준 방향 초기화
        }
        private void LateUpdate() // 몸통별 최근접 적 조준
        {
            foreach (KeyValuePair<SnakeSegment, SpriteRenderer> entry in visuals) // 실제 장착 포탑 순회
            {
                if (entry.Key == null || entry.Value == null || !entry.Value.gameObject.activeInHierarchy) // 몸통과 포탑 활성 확인
                {
                    continue; // 사라진 포탑 무시
                }
                WeaponTarget target = WeaponTarget.FindClosest(entry.Key.transform.position, float.PositiveInfinity); // 해당 몸통의 최근접 적
                if (target != null) // 실제 조준 대상 확인
                {
                    Vector3 direction = target.transform.position - entry.Key.transform.position; // 몸통에서 적으로 향하는 방향
                    if (direction.sqrMagnitude > 0.0001f) // 겹치지 않은 방향 확인
                    {
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // 위쪽 포신 기준 월드 회전
                        aimDirections[entry.Key] = Quaternion.Euler(0f, 0f, angle); // 마지막 유효 조준 방향 저장
                    }
                }
                if (aimDirections.TryGetValue(entry.Key, out Quaternion rotation)) // 저장된 조준 방향 확인
                {
                    entry.Value.transform.rotation = rotation; // 몸통 회전과 무관한 실제 포탑 회전
                }
            }
        }
        private void Refresh() // 실제 장착 상태에 맞춘 이미지 갱신
        {
            if (weapons == null) // 실제 관리자 확인
            {
                return; // 관리자 없는 갱신 방지
            }
            active.Clear(); // 이전 장착 몸통 초기화
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            {
                if (slot == null || slot.Owner == null || slot.IsEmpty || slot.Weapon.Artwork == null) // 실제 몸통과 이미지 확인
                {
                    continue; // 빈 슬롯과 미설정 이미지 제외
                }
                SnakeSegment owner = slot.Owner; // 실제 장착 몸통
                active.Add(owner); // 현재 장착 몸통 등록
                if (!visuals.TryGetValue(owner, out SpriteRenderer renderer) || renderer == null) // 기존 표시기 확인
                {
                    GameObject item = new GameObject("WeaponVisual"); // 실제 표시용 객체
                    item.transform.SetParent(owner.transform, false); // 실제 장착 몸통에 연결
                    item.transform.localPosition = Vector3.zero; // 실제 몸통 중심 위치
                    renderer = item.AddComponent<SpriteRenderer>(); // 실제 무기 표시기
                    visuals[owner] = renderer; // 몸통별 표시기 보관
                }
                renderer.sprite = slot.Weapon.Artwork; // 실제 장착 무기 이미지
                renderer.color = slot.Weapon.ArtworkTint; // 속성별 포탑 색상 표시
                renderer.sortingOrder = 30; // 몸통 위 표시 순서
                renderer.transform.localScale = Vector3.one * Mathf.Max(0.01f, visualScale); // 실제 몸통 기준 크기
                renderer.gameObject.SetActive(true); // 실제 무기 표시
            }
            removed.Clear(); // 이전 제거 목록 초기화
            foreach (KeyValuePair<SnakeSegment, SpriteRenderer> entry in visuals) // 기존 표시기 순회
            {
                if (entry.Key != null && active.Contains(entry.Key)) // 현재 장착 몸통 확인
                {
                    continue; // 유지할 표시 보존
                }
                if (entry.Value != null) // 실제 제거 표시기 확인
                {
                    entry.Value.gameObject.SetActive(false); // 같은 프레임 표시 차단
                    Destroy(entry.Value.gameObject); // 실제 장식 제거
                }
                removed.Add(entry.Key); // 실제 제거 대상 저장
            }
            foreach (SnakeSegment owner in removed) // 제거 대상 순회
            {
                visuals.Remove(owner); // 몸통별 표시 목록 제거
                aimDirections.Remove(owner); // 제거한 몸통의 조준 방향 삭제
            }
        }
    }
}
