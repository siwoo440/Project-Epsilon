using ProjectEpsilon.Combat; // 실제 무기 공격 대상
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어와 공용 체력
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class PrototypeBossEncounter : MonoBehaviour // 시험용 최종 보스 출현기
    { // 클래스 시작
        [SerializeField] private float maximumHealth = 600f; // 보스 체력 임시값
        [SerializeField] private int damage = 15; // 투사체 피해 임시값
        [SerializeField] private float speedRatio = 0.75f; // 플레이어 기준 이동 속도 임시값
        [SerializeField] private float attackInterval = 2f; // 사격 간격 임시값
        [SerializeField] private float keepDistance = 4f; // 사격 유지 거리 임시값
        [SerializeField] private float projectileSpeed = 5f; // 투사체 속도 임시값
        [SerializeField] private float projectileLifetime = 3f; // 투사체 수명 임시값
        [SerializeField] private int projectileCount = 3; // 첫 단계 부채꼴 투사체 수 임시값
        [SerializeField] private int phaseTwoProjectileCount = 5; // 두 번째 단계 투사체 수 임시값
        [SerializeField] private float projectileSpreadDegrees = 40f; // 부채꼴 전체 각도 임시값
        [SerializeField] private float phaseTwoSpeedMultiplier = 1.25f; // 두 번째 단계 속도 임시 배율
        [SerializeField] private float phaseTwoIntervalMultiplier = 0.75f; // 두 번째 단계 공격 간격 임시 배율
        [SerializeField] private float spawnDistance = 8f; // 보스 출현 거리 임시값
        private EarlyEnemyWaveSpawner spawner; // 실제 20분 전환기
        private WeaponTarget boss; // 실제 공격 가능한 보스
        private EarlyEnemySettings settings; // 실제 보스 행동 설정
        private readonly PrototypeBossPhase phase = new PrototypeBossPhase(); // 보스 체력 단계 상태
        private bool spawned; // 보스 출현 완료 여부
        public WeaponTarget Boss => boss; // 실제 생성 보스 조회
        public bool IsPhaseTwo => phase.IsPhaseTwo; // 현재 보스 단계 조회

        private void OnEnable() // 실제 전환 알림 연결
        { // 메서드 시작
            spawner = GetComponent<EarlyEnemyWaveSpawner>(); // 같은 관리자 출현기 조회
            if (spawner == null) // 출현기 부재 확인
            { // 조건 시작
                spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 활성 출현기 대체 조회
            } // 조건 끝
            if (spawner != null) // 실제 출현기 연결 확인
            { // 조건 시작
                spawner.BossTransitionStarted += TrySpawnBoss; // 실제 전환 이벤트 구독
                if (spawner.HasEnteredBossTransition) // 이미 완료된 전환 확인
                { // 조건 시작
                    TrySpawnBoss(); // 늦은 활성화 시 보스 출현
                } // 조건 끝
            } // 조건 끝
        } // 메서드 끝

        private void OnDisable() // 실제 전환 알림 해제
        { // 메서드 시작
            if (spawner != null) // 출현기 연결 확인
            { // 조건 시작
                spawner.BossTransitionStarted -= TrySpawnBoss; // 중복 구독 방지
            } // 조건 끝
        } // 메서드 끝

        private void Update() // 실제 보스 체력 단계 관찰
        { // 메서드 시작
            if (boss == null || !boss.IsAlive || settings == null) // 실제 생존 보스 확인
            { // 조건 시작
                return; // 단계 갱신 생략
            } // 조건 끝
            bool wasPhaseTwo = phase.IsPhaseTwo; // 기존 단계 저장
            phase.Observe(boss.CurrentHealth, boss.MaximumHealth, boss.IsAlive); // 실제 체력 경계 반영
            if (!wasPhaseTwo && phase.IsPhaseTwo) // 두 번째 단계 최초 진입 확인
            { // 조건 시작
                settings.speedRatio = Mathf.Max(0f, speedRatio) * Mathf.Max(0f, phaseTwoSpeedMultiplier); // 임시 이동 속도 변경
                settings.attackInterval = Mathf.Max(0.1f, attackInterval * Mathf.Max(0f, phaseTwoIntervalMultiplier)); // 임시 사격 간격 변경
                settings.projectileCount = Mathf.Clamp(phaseTwoProjectileCount, 1, 9); // 두 번째 단계 임시 사격 수
            } // 조건 끝
        } // 메서드 끝

        private void OnGUI() // 시험용 보스 체력 표시
        { // 메서드 시작
            if (boss == null || !boss.IsAlive || boss.MaximumHealth <= 0f) // 생존 보스와 최대 체력 확인
            { // 조건 시작
                return; // 표시 생략
            } // 조건 끝
            float width = Mathf.Min(400f, Mathf.Max(1f, Screen.width - 40f)); // 화면 너비에 맞춘 체력 바
            Rect background = new Rect((Screen.width - width) * 0.5f, 48f, width, 22f); // 상단 중앙 표시 영역
            GUI.Box(background, GUIContent.none); // 체력 바 배경
            float ratio = Mathf.Clamp01(boss.CurrentHealth / boss.MaximumHealth); // 현재 체력 비율
            Rect fill = new Rect(background.x + 2f, background.y + 2f, Mathf.Max(0f, background.width - 4f) * ratio, background.height - 4f); // 남은 체력 영역
            Color previousColor = GUI.color; // 이전 화면 색상 보존
            GUI.color = new Color(0.85f, 0.15f, 0.25f); // 시험용 체력 색상
            GUI.DrawTexture(fill, Texture2D.whiteTexture); // 남은 체력 표시
            GUI.color = previousColor; // 다른 UI 색상 복구
            GUI.Label(background, $"BOSS {Mathf.CeilToInt(boss.CurrentHealth)} / {Mathf.CeilToInt(boss.MaximumHealth)}"); // 현재 및 최대 체력 문구
        } // 메서드 끝

        private void TrySpawnBoss() // 실제 최종 보스 출현
        { // 메서드 시작
            if (spawned || GameManager.Instance == null) // 중복 출현과 관리자 확인
            { // 조건 시작
                return; // 출현 생략
            } // 조건 끝
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동 조회
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            SnakeHealth health = FindFirstObjectByType<SnakeHealth>(); // 실제 공용 체력 조회
            if (player == null || body == null || health == null) // 필수 플레이어 구성 확인
            { // 조건 시작
                Debug.LogError("[Project Epsilon] 시험 보스 출현에 필요한 플레이어 구성이 없습니다."); // 구성 오류 기록
                return; // 잘못된 출현 방지
            } // 조건 끝
            SpriteRenderer source = player.GetComponentInChildren<SpriteRenderer>(); // 플레이어 스프라이트 조회
            Sprite sprite = source == null ? null : source.sprite; // 시험용 대체 시각 자료
            GameObject instance = new GameObject("PrototypeFinalBoss"); // 실제 보스 객체 생성
            instance.transform.position = player.transform.position + Vector3.up * Mathf.Max(1f, spawnDistance); // 시험용 출현 위치
            instance.transform.localScale = Vector3.one * 1.4f; // 시험용 보스 크기
            SpriteRenderer renderer = instance.AddComponent<SpriteRenderer>(); // 실제 보스 렌더러
            renderer.sprite = sprite; // 임시 보스 시각 자료
            renderer.color = new Color(0.7f, 0.1f, 0.9f); // 시험용 보스 구분 색상
            renderer.sortingOrder = 8; // 실제 렌더 순서
            CircleCollider2D collider = instance.AddComponent<CircleCollider2D>(); // 실제 보스 충돌체
            collider.isTrigger = true; // 실제 투사체 접촉 설정
            Rigidbody2D physicsBody = instance.AddComponent<Rigidbody2D>(); // 실제 보스 물리체
            physicsBody.bodyType = RigidbodyType2D.Kinematic; // 기존 적 이동 방식
            physicsBody.gravityScale = 0f; // 중력 제거
            physicsBody.useFullKinematicContacts = true; // 실제 투사체 접촉 허용
            WeaponTarget target = instance.AddComponent<WeaponTarget>(); // 실제 무기 공격 대상
            target.Configure(maximumHealth); // 임시 최대 체력 반영
            instance.AddComponent<WeaponTargetStatusController>(); // 기존 상태 효과 사용
            instance.AddComponent<WeaponTargetKnockbackController>().Configure(true); // 보스 넉백 면역
            settings = new EarlyEnemySettings // 기존 사수 행동의 임시 설정
            { // 설정 시작
                kind = EarlyEnemyKind.Shooter, // 시험용 사격 행동
                health = maximumHealth, // 임시 체력
                damage = damage, // 임시 투사체 피해
                speedRatio = Mathf.Max(0f, speedRatio), // 임시 이동 속도
                attackInterval = Mathf.Max(0.1f, attackInterval), // 임시 사격 간격
                projectileCount = Mathf.Clamp(projectileCount, 1, 9), // 첫 단계 임시 사격 수
                projectileSpreadDegrees = Mathf.Clamp(projectileSpreadDegrees, 0f, 180f) // 임시 부채꼴 각도
            }; // 설정 끝
            EarlyEnemyBehaviour behaviour = instance.AddComponent<EarlyEnemyBehaviour>(); // 기존 적 행동 재사용
            behaviour.Configure(settings, player, body, health, sprite, keepDistance, projectileSpeed, projectileLifetime, 1f, 0.6f, 3f); // 실제 사격과 이동 설정
            if (!GameManager.Instance.RegisterFinalBoss(target)) // 실제 보스 등록 확인
            { // 조건 시작
                instance.SetActive(false); // 등록 실패 시 공격 차단
                Destroy(instance); // 등록 실패 보스 제거
                settings = null; // 임시 행동 참조 해제
                return; // 승리 대상 없는 출현 방지
            } // 조건 끝
            boss = target; // 실제 보스 참조 저장
            spawned = true; // 중복 출현 방지
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
