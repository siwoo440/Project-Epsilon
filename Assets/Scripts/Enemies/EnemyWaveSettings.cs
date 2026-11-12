using System; // 구간 데이터 직렬화
using UnityEngine; // Unity 데이터 에셋

namespace ProjectEpsilon.Enemies // 적 출현 영역
{
    [Serializable] // 구간 데이터 직렬화 허용
    public sealed class EnemyWaveStage // 시간별 일반 적 출현 설정
    {
        public float startTime; // 구간 시작 시간
        public float spawnInterval = 1.3f; // 일반 적 출현 간격
        public int normalEnemyLimit = 40; // 일반 적 최대 수
        public float healthMultiplier = 1f; // 일반 적 체력 배율
        public float damageMultiplier = 1f; // 일반 적 피해 배율
    }

    [CreateAssetMenu(menuName = "Project Epsilon/Enemy Wave Settings")] // 출현 데이터 생성 메뉴
    public sealed class EnemyWaveSettings : ScriptableObject // 조정 가능한 출현 구간 데이터
    {
        [SerializeField] private EnemyWaveStage[] stages; // 시간별 출현 구간 목록

        public float ResolveMultiplier(float elapsed, bool health) // 분 단위 능력치 배율
        {
            float minuteTime = Mathf.Floor(Mathf.Max(0f, elapsed) / 60f) * 60f; // 완료된 분의 시작 시각
            EnemyWaveStage previous = Resolve(minuteTime); // 현재 시각의 이전 기준
            if (previous == null) // 기준 데이터 부재 확인
            {
                return 1f; // 기본 배율 유지
            }
            EnemyWaveStage next = null; // 다음 기준 초기화
            foreach (EnemyWaveStage stage in stages) // 전체 기준 순회
            {
                if (stage != null && stage.startTime > minuteTime && (next == null || stage.startTime < next.startTime)) // 가장 가까운 다음 기준 확인
                {
                    next = stage; // 다음 기준 저장
                }
            }
            float first = health ? previous.healthMultiplier : previous.damageMultiplier; // 현재 기준 배율
            if (next == null) // 다음 기준 부재 확인
            {
                return Mathf.Max(0f, first); // 마지막 배율 유지
            }
            float second = health ? next.healthMultiplier : next.damageMultiplier; // 다음 기준 배율
            float progress = Mathf.InverseLerp(previous.startTime, next.startTime, minuteTime); // 기준 사이 진행 비율
            return Mathf.Max(0f, Mathf.Lerp(first, second, progress)); // 분 단위 보간 배율
        }

        public EnemyWaveStage Resolve(float elapsed) // 현재 시간에 해당하는 출현 구간
        {
            EnemyWaveStage selected = null; // 선택된 구간 초기화
            if (stages == null) // 구간 목록 부재 확인
            {
                return null; // 기존 설정 사용
            }
            foreach (EnemyWaveStage stage in stages) // 출현 구간 순회
            {
                if (stage != null && stage.startTime <= elapsed && (selected == null || stage.startTime > selected.startTime)) // 현재 시간에 가장 가까운 구간 확인
                {
                    selected = stage; // 현재 구간 저장
                }
            }
            return selected; // 현재 구간 반환
        }
    }
}
