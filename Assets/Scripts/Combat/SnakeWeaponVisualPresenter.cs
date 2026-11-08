using System.Collections.Generic; // 실제 몸통별 표시 목록
using ProjectEpsilon.Player; // 실제 몸통 구성
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Combat // 전투 영역
{ // 영역 시작
    [DefaultExecutionOrder(90)] // 실제 무기 관리자 이후 초기화
    public sealed class SnakeWeaponVisualPresenter : MonoBehaviour // 실제 몸통별 무기 표시
    { // 클래스 시작
        [SerializeField] private SnakeWeaponManager weapons; // 실제 무기 관리자
        [SerializeField] private float visualScale = 0.65f; // 몸통 기준 이미지 크기 임시값
        private readonly Dictionary<SnakeSegment, SpriteRenderer> visuals = new Dictionary<SnakeSegment, SpriteRenderer>(); // 몸통별 실제 표시기
        private readonly HashSet<SnakeSegment> active = new HashSet<SnakeSegment>(); // 현재 장착 몸통 집합
        private readonly List<SnakeSegment> removed = new List<SnakeSegment>(); // 제거할 표시 대상

        private void OnEnable() // 실제 무기 변경 연결
        { // 메서드 시작
            if (weapons == null) // 관리자 연결 확인
            { // 조건 시작
                weapons = GetComponent<SnakeWeaponManager>(); // 같은 객체 관리자 조회
            } // 조건 끝
            if (weapons != null) // 실제 관리자 확인
            { // 조건 시작
                weapons.SlotsChanged += Refresh; // 실제 슬롯 변경 구독
                Refresh(); // 현재 슬롯 즉시 반영
            } // 조건 끝
        } // 메서드 끝
        private void OnDisable() // 실제 표시 연결 해제
        { // 메서드 시작
            if (weapons != null) // 실제 관리자 확인
            { // 조건 시작
                weapons.SlotsChanged -= Refresh; // 실제 슬롯 변경 구독 해제
            } // 조건 끝
            foreach (SpriteRenderer renderer in visuals.Values) // 기존 표시기 순회
            { // 반복 시작
                if (renderer != null) // 실제 표시기 확인
                { // 조건 시작
                    renderer.gameObject.SetActive(false); // 지연 파괴 전 표시 차단
                    Destroy(renderer.gameObject); // 실제 장식 제거
                } // 조건 끝
            } // 반복 끝
            visuals.Clear(); // 기존 표시 목록 초기화
        } // 메서드 끝
        private void Refresh() // 실제 장착 상태에 맞춘 이미지 갱신
        { // 메서드 시작
            if (weapons == null) // 실제 관리자 확인
            { // 조건 시작
                return; // 관리자 없는 갱신 방지
            } // 조건 끝
            active.Clear(); // 이전 장착 몸통 초기화
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            { // 반복 시작
                if (slot == null || slot.Owner == null || slot.IsEmpty || slot.Weapon.Artwork == null) // 실제 몸통과 이미지 확인
                { // 조건 시작
                    continue; // 빈 슬롯과 미설정 이미지 제외
                } // 조건 끝
                SnakeSegment owner = slot.Owner; // 실제 장착 몸통
                active.Add(owner); // 현재 장착 몸통 등록
                if (!visuals.TryGetValue(owner, out SpriteRenderer renderer) || renderer == null) // 기존 표시기 확인
                { // 조건 시작
                    GameObject item = new GameObject("WeaponVisual"); // 실제 표시용 객체
                    item.transform.SetParent(owner.transform, false); // 실제 장착 몸통에 연결
                    item.transform.localPosition = Vector3.zero; // 실제 몸통 중심 위치
                    renderer = item.AddComponent<SpriteRenderer>(); // 실제 무기 표시기
                    visuals[owner] = renderer; // 몸통별 표시기 보관
                } // 조건 끝
                renderer.sprite = slot.Weapon.Artwork; // 실제 장착 무기 이미지
                renderer.color = Color.white; // 원본 색상 유지
                renderer.sortingOrder = 30; // 몸통 위 표시 순서
                renderer.transform.localScale = Vector3.one * Mathf.Max(0.01f, visualScale); // 실제 몸통 기준 크기
                renderer.gameObject.SetActive(true); // 실제 무기 표시
            } // 반복 끝
            removed.Clear(); // 이전 제거 목록 초기화
            foreach (KeyValuePair<SnakeSegment, SpriteRenderer> entry in visuals) // 기존 표시기 순회
            { // 반복 시작
                if (entry.Key != null && active.Contains(entry.Key)) // 현재 장착 몸통 확인
                { // 조건 시작
                    continue; // 유지할 표시 보존
                } // 조건 끝
                if (entry.Value != null) // 실제 제거 표시기 확인
                { // 조건 시작
                    entry.Value.gameObject.SetActive(false); // 같은 프레임 표시 차단
                    Destroy(entry.Value.gameObject); // 실제 장식 제거
                } // 조건 끝
                removed.Add(entry.Key); // 실제 제거 대상 저장
            } // 반복 끝
            foreach (SnakeSegment owner in removed) // 제거 대상 순회
            { // 반복 시작
                visuals.Remove(owner); // 몸통별 표시 목록 제거
            } // 반복 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
