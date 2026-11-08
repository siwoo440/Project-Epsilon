using System.Collections.Generic; // 생존 소환 적 목록
using ProjectEpsilon.Combat; // 실제 보스 대상
using ProjectEpsilon.Core; // 게임 진행 상태
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class PrototypeBossSummoner : MonoBehaviour // 시험 보스 잡몹 소환
    { // 클래스 시작
        private readonly List<WeaponTarget> summons = new List<WeaponTarget>(); // 생존 소환 적 목록
        private EarlyEnemyWaveSpawner spawner; // 기존 일반 적 생성기
        private WeaponTarget boss; // 실제 보스 대상
        private float interval; // 소환 간격
        private int maximum; // 동시 소환 상한
        private float radius; // 보스 주변 출현 거리
        private float nextSummonAt; // 다음 소환 시각
        public int ActiveCount => summons.Count; // 실제 소환 수 조회

        public void Configure(EarlyEnemyWaveSpawner enemySpawner, WeaponTarget owner, float spawnInterval, int activeMaximum, float spawnRadius) // 실제 소환 설정
        { // 메서드 시작
            spawner = enemySpawner; // 기존 생성기 저장
            boss = owner; // 실제 보스 저장
            interval = Mathf.Max(0.1f, spawnInterval); // 안전한 소환 간격
            maximum = Mathf.Max(0, activeMaximum); // 안전한 동시 상한
            radius = Mathf.Max(0.1f, spawnRadius); // 안전한 출현 거리
            nextSummonAt = Time.time + interval; // 첫 소환 시각
            if (boss != null) // 실제 보스 확인
            { // 조건 시작
                boss.Died += HandleBossDied; // 실제 보스 사망 연결
            } // 조건 끝
        } // 메서드 끝
        private void FixedUpdate() // 실제 보스 패턴 소환
        { // 메서드 시작
            summons.RemoveAll(target => target == null || !target.IsAlive); // 처치된 소환 적 정리
            if (spawner == null || boss == null || !boss.IsAlive || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < nextSummonAt) // 실제 전투와 소환 시각 확인
            { // 조건 시작
                return; // 정지 또는 소환 대기
            } // 조건 끝
            nextSummonAt = Time.time + interval; // 다음 소환 시각
            if (summons.Count >= maximum) // 동시 소환 상한 확인
            { // 조건 시작
                return; // 상한 초과 방지
            } // 조건 끝
            Vector2 direction = Random.insideUnitCircle.normalized; // 실제 출현 방향
            if (direction.sqrMagnitude < 0.01f) // 유효 방향 확인
            { // 조건 시작
                direction = Vector2.right; // 대체 출현 방향
            } // 조건 끝
            WeaponTarget summon = spawner.SpawnBossSummon(transform.position + (Vector3)(direction * radius)); // 기존 추적자 소환
            if (summon != null) // 실제 소환 성공 확인
            { // 조건 시작
                summons.Add(summon); // 생존 소환 적 등록
            } // 조건 끝
        } // 메서드 끝
        private void HandleBossDied(WeaponTarget target) // 실제 보스 사망 처리
        { // 메서드 시작
            if (target != boss) // 실제 보스 확인
            { // 조건 시작
                return; // 다른 대상 무시
            } // 조건 끝
            foreach (WeaponTarget summon in summons) // 남은 소환 적 순회
            { // 반복 시작
                if (spawner != null) // 기존 생성기 확인
                { // 조건 시작
                    spawner.RemoveBossSummon(summon); // 생존 소환 적 무보상 제거
                } // 조건 끝
            } // 반복 끝
            summons.Clear(); // 소환 적 목록 초기화
        } // 메서드 끝
        private void OnDestroy() // 실제 보스 연결 정리
        { // 메서드 시작
            if (boss != null) // 실제 보스 확인
            { // 조건 시작
                boss.Died -= HandleBossDied; // 사망 이벤트 연결 해제
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
