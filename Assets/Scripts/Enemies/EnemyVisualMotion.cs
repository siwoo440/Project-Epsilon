using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using UnityEngine; // Unity 표시와 시간
namespace ProjectEpsilon.Enemies // 적 시각 영역
{ // 영역 시작
    [RequireComponent(typeof(WeaponTarget), typeof(SpriteRenderer))] // 기존 전투 대상과 표시기 필요
    public sealed class EnemyVisualMotion : MonoBehaviour // 충돌체와 분리된 적 임시 모션
    { // 모션 시작
        [SerializeField, Min(0f)] private float movementAmount = 0.035f; // 이동 표시 크기 시험값
        [SerializeField, Min(0f)] private float movementFrequency = 8f; // 이동 표시 속도 시험값
        [SerializeField, Min(0f)] private float attackAmount = 0.1f; // 공격 표시 크기 시험값
        [SerializeField, Min(0.01f)] private float attackDuration = 0.2f; // 공격 표시 시간 시험값
        [SerializeField, Min(0f)] private float movementTilt = 2f; // 이동 시 이미지 기울기 시험값
        private WeaponTarget target; // 실제 적 전투 대상
        private SpriteRenderer original; // 기존 색상과 이미지 기준
        private SpriteRenderer artwork; // 충돌체와 분리된 실제 표시
        private Vector3 previousPosition; // 이전 위치
        private float movingUntil; // 이동 표시 유지 시각
        private float attackUntil; // 공격 표시 종료 시각
        private bool originalVisible; // 기존 표시 활성 상태
        public void ConfigureMotion(EarlyEnemyKind kind, ElitePattern elitePattern) // 실제 행동별 표시 시험값
        { // 함수 시작
            movementAmount = 0.035f; // 기본 이동 진폭
            movementFrequency = 8f; // 기본 이동 속도
            movementTilt = 2f; // 기본 이동 기울기
            attackAmount = 0.1f; // 기본 공격 진폭
            attackDuration = 0.2f; // 기본 공격 표시 시간
            switch (kind) // 실제 적 행동 유형
            { // 분기 시작
                case EarlyEnemyKind.Flyer: // 비행 적 움직임
                    movementAmount = 0.06f; // 빠른 날갯짓 표시 진폭
                    movementFrequency = 14f; // 빠른 날갯짓 표시 속도
                    movementTilt = 4f; // 비행 방향 흔들림
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Armored: // 장갑 적 움직임
                    movementAmount = 0.025f; // 무거운 이동 표시 진폭
                    movementFrequency = 4f; // 느린 이동 표시 속도
                    movementTilt = 1f; // 작은 이동 기울기
                    attackAmount = 0.14f; // 무거운 공격 표시 진폭
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Charger: // 일반 돌진 적 움직임
                    movementFrequency = 10f; // 빠른 이동 표시 속도
                    attackAmount = 0.18f; // 돌진 시작 표시 진폭
                    attackDuration = 0.3f; // 돌진 시작 표시 시간
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Shooter: // 단일 사격 적 움직임
                case EarlyEnemyKind.Turret: // 부채꼴 사격 적 움직임
                    movementAmount = 0.025f; // 사격 적 이동 진폭
                    movementFrequency = 6f; // 사격 적 이동 속도
                    attackAmount = 0.14f; // 사격 반동 표시 진폭
                    attackDuration = 0.15f; // 사격 반동 표시 시간
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Predator: // 포식 적 움직임
                    movementFrequency = 10f; // 민첩한 이동 표시 속도
                    movementTilt = 3f; // 포식 적 이동 기울기
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Disruptor: // 감속 지대 적 움직임
                    movementFrequency = 6f; // 완만한 이동 표시 속도
                    movementTilt = 3f; // 감속 지대 적 기울기
                    attackDuration = 0.3f; // 지대 생성 표시 시간
                    break; // 유형 설정 종료
                case EarlyEnemyKind.Elite: // 전용 엘리트 움직임
                    movementFrequency = elitePattern == ElitePattern.Ranged ? 6f : 7f; // 사격 또는 근접 엘리트 속도
                    attackAmount = 0.16f; // 엘리트 공격 표시 진폭
                    attackDuration = 0.3f; // 엘리트 공격 표시 시간
                    break; // 유형 설정 종료
            } // 분기 종료
        } // 함수 종료
        public void ConfigureBossMotion() // 실제 보스 표시 시험값
        { // 함수 시작
            movementAmount = 0.02f; // 보스 이동 진폭
            movementFrequency = 4f; // 보스 이동 표시 속도
            movementTilt = 1f; // 보스 이동 기울기
            attackAmount = 0.16f; // 보스 공격 표시 진폭
            attackDuration = 0.35f; // 보스 공격 표시 시간
        } // 함수 종료
        private void Awake() // 실제 표시 객체 준비
        { // 함수 시작
            target = GetComponent<WeaponTarget>(); // 실제 적 전투 대상
            original = GetComponent<SpriteRenderer>(); // 기존 적 표시기
            originalVisible = original.enabled; // 기존 표시 활성 상태
            GameObject child = new GameObject("EnemyArtwork"); // 충돌체와 분리된 표시 객체
            child.transform.SetParent(transform, false); // 실제 적 위치와 회전 상속
            artwork = child.AddComponent<SpriteRenderer>(); // 실제 이미지 표시기
            artwork.sharedMaterial = original.sharedMaterial; // 기존 이미지 재질 재사용
            artwork.sortingLayerID = original.sortingLayerID; // 기존 표시 레이어 유지
            artwork.sortingOrder = original.sortingOrder; // 기존 표시 순서 유지
            artwork.maskInteraction = original.maskInteraction; // 기존 마스크 설정 유지
            previousPosition = transform.position; // 최초 실제 위치
            SyncArtwork(); // 기존 이미지와 색상 연결
        } // 함수 종료
        private void OnEnable() // 실제 모션과 사망 이벤트 연결
        { // 함수 시작
            if (target != null) // 실제 전투 대상 확인
            { // 조건 시작
                target.Died += OnDied; // 실제 처치 시각 효과 구독
            } // 조건 종료
            if (original != null && artwork != null) // 실제 표시기 확인
            { // 조건 시작
                original.enabled = false; // 기존 표시의 중복 렌더 방지
                artwork.enabled = originalVisible; // 기존 표시 상태 유지
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 실제 모션과 이벤트 해제
        { // 함수 시작
            if (target != null) // 실제 전투 대상 확인
            { // 조건 시작
                target.Died -= OnDied; // 실제 사망 이벤트 해제
            } // 조건 종료
            if (original != null) // 기존 표시기 확인
            { // 조건 시작
                original.enabled = originalVisible; // 기존 표시 상태 복구
            } // 조건 종료
            if (artwork != null) // 실제 표시 객체 확인
            { // 조건 시작
                artwork.enabled = false; // 비활성 모션 표시 중지
            } // 조건 종료
        } // 함수 종료
        public void PlayAttack() // 실제 공격 모션 시작
        { // 함수 시작
            attackUntil = Time.time + Mathf.Max(0.01f, attackDuration); // 실제 공격 표시 종료 시각
        } // 함수 종료
        private void LateUpdate() // 실제 이미지와 임시 모션 갱신
        { // 함수 시작
            if (original == null || artwork == null) // 실제 표시기 확인
            { // 조건 시작
                return; // 표시기 부재 시 갱신 생략
            } // 조건 종료
            SyncArtwork(); // 기존 피격과 화상 색상 반영
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying && (transform.position - previousPosition).sqrMagnitude > 0.000001f) // 실제 전투 중 위치 변경 확인
            { // 조건 시작
                movingUntil = Time.time + 0.15f; // 이동 표시 유지 시각
            } // 조건 종료
            previousPosition = transform.position; // 다음 이동 확인 위치
            float movement = Time.time < movingUntil ? Mathf.Sin(Time.time * movementFrequency) * movementAmount : 0f; // 실제 이동 표시 진폭
            float attack = Time.time < attackUntil ? Mathf.Clamp01((attackUntil - Time.time) / Mathf.Max(0.01f, attackDuration)) * attackAmount : 0f; // 실제 공격 표시 진폭
            artwork.transform.localScale = new Vector3(1f + movement - attack * 0.5f, 1f - movement + attack, 1f); // 충돌체와 독립된 이미지 크기
            float tilt = Time.time < movingUntil ? Mathf.Sin(Time.time * movementFrequency) * movementTilt : 0f; // 실제 유형별 이동 기울기
            artwork.transform.localRotation = Quaternion.Euler(0f, 0f, tilt); // 충돌체와 독립된 이미지 방향
        } // 함수 종료
        private void SyncArtwork() // 기존 표시 상태 동기화
        { // 함수 시작
            artwork.sprite = original.sprite; // 기존 적 이미지
            artwork.color = original.color; // 기존 피격과 화상 색상
            artwork.flipX = original.flipX; // 기존 좌우 반전
            artwork.flipY = original.flipY; // 기존 상하 반전
            artwork.sortingLayerID = original.sortingLayerID; // 기존 표시 레이어
            artwork.sortingOrder = original.sortingOrder; // 기존 표시 순서
        } // 함수 종료
        private void OnDied(WeaponTarget dead) // 실제 적 처치 표시
        { // 함수 시작
            if (dead != target || artwork == null || original == null || !originalVisible) // 실제 표시 대상 확인
            { // 조건 시작
                return; // 무효 사망 표시 생략
            } // 조건 종료
            GameObject ghost = new GameObject("EnemyDeathVisual"); // 충돌이 없는 사망 표시 객체
            ghost.transform.position = artwork.transform.position; // 실제 사망 위치
            ghost.transform.rotation = artwork.transform.rotation; // 실제 사망 방향
            ghost.transform.localScale = artwork.transform.lossyScale; // 실제 표시 크기
            SpriteRenderer image = ghost.AddComponent<SpriteRenderer>(); // 실제 사망 이미지
            image.sprite = original.sprite; // 기존 적 이미지
            image.color = original.color; // 실제 사망 시점 색상
            image.sharedMaterial = original.sharedMaterial; // 기존 표시 재질
            image.sortingLayerID = original.sortingLayerID; // 기존 표시 레이어
            image.sortingOrder = original.sortingOrder; // 기존 표시 순서
            image.flipX = original.flipX; // 기존 좌우 방향
            image.flipY = original.flipY; // 기존 상하 방향
            ghost.AddComponent<EnemyDeathVisual>().Configure(image, 0.25f); // 무충돌 임시 사망 모션
        } // 함수 종료
    } // 모션 종료
} // 영역 종료
