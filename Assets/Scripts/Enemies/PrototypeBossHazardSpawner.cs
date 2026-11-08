using ProjectEpsilon.Combat; // 보스 전투 상태
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 구성
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class PrototypeBossHazardSpawner : MonoBehaviour // 시험 보스 위험 구역 출현기
    { // 클래스 시작
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeBodyManager body; // 실제 플레이어 몸통
        private SnakeHealth health; // 실제 공용 체력
        private WeaponTarget boss; // 실제 보스 상태
        private float interval; // 출현 간격
        private float warningDuration; // 예고 시간
        private float duration; // 위험 지속 시간
        private float damageInterval; // 피해 간격
        private float radius; // 위험 반경
        private int damage; // 기본 피해
        private float nextHazardAt; // 다음 출현 시각
        private PrototypeBossHazard currentHazard; // 현재 위험 구역
        public PrototypeBossHazard CurrentHazard => currentHazard; // 실제 구역 조회

        public void Configure(SnakeMovement head, SnakeBodyManager segments, SnakeHealth sharedHealth, WeaponTarget owner, float spawnInterval, float warningTime, float activeTime, float hitInterval, float range, int hitDamage) // 실제 출현 설정
        { // 메서드 시작
            player = head; // 실제 머리 저장
            body = segments; // 실제 몸통 저장
            health = sharedHealth; // 실제 체력 저장
            boss = owner; // 실제 보스 저장
            interval = Mathf.Max(0.1f, spawnInterval); // 안전한 출현 간격
            warningDuration = Mathf.Max(0.1f, warningTime); // 안전한 예고 시간
            duration = Mathf.Max(0.1f, activeTime); // 안전한 위험 시간
            damageInterval = Mathf.Max(0.1f, hitInterval); // 안전한 피해 간격
            radius = Mathf.Max(0.1f, range); // 안전한 위험 반경
            damage = Mathf.Max(0, hitDamage); // 안전한 기본 피해
            nextHazardAt = Time.time + interval; // 첫 출현 시각
        } // 메서드 끝

        private void FixedUpdate() // 실제 위험 구역 출현
        { // 메서드 시작
            if (player == null || body == null || health == null || boss == null || !boss.IsAlive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 가능 상태
            { // 조건 시작
                return; // 전투 중단 시 출현 방지
            } // 조건 끝
            if (currentHazard != null || Time.time < nextHazardAt) // 기존 구역과 출현 시각 확인
            { // 조건 시작
                return; // 다음 구역 대기
            } // 조건 끝
            GameObject instance = new GameObject("PrototypeBossHazard"); // 실제 위험 구역 생성
            instance.transform.position = player.transform.position; // 예고 시점 머리 위치
            currentHazard = instance.AddComponent<PrototypeBossHazard>(); // 실제 위험 구역 구성
            int actualDamage = Mathf.CeilToInt(damage * boss.OutgoingDamageMultiplier); // 출현 시 보스 피해 배율 확정
            currentHazard.Configure(player, body, health, boss, warningDuration, duration, damageInterval, radius, actualDamage); // 실제 예고와 피해 설정
            nextHazardAt = Time.time + interval; // 다음 출현 시각
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
