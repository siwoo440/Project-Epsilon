using ProjectEpsilon.Core;
using ProjectEpsilon.Enemies; // 실제 전투 시간 참조
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(200)] // 출현기 시간 갱신 이후 표시
    public sealed class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private Text timerText;

        private float elapsedSeconds;
        private float displayOffset; // 표시 전용 초기화 기준
        private EarlyEnemyWaveSpawner spawner; // 실제 전투 시계

        private void Start()
        {
            spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기 조회
            Refresh();
        }

        private void LateUpdate() // 실제 전투 시간 표시
        {
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            { // 조건 시작
                elapsedSeconds = Mathf.Max(0f, spawner.ElapsedSeconds - displayOffset); // 표시 전용 초기화 반영
                Refresh(); // 실제 시간 표시 갱신
                return; // 독립 시계 증가 생략
            } // 조건 끝
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            Refresh();
        }

        public void Bind(Text nextTimerText)
        {
            timerText = nextTimerText;
            Refresh();
        }

        public void ResetTimer()
        {
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            { // 조건 시작
                displayOffset = spawner.ElapsedSeconds; // 표시만 초기화할 기준 저장
            } // 조건 끝
            elapsedSeconds = 0f;
            Refresh();
        }

        private void Refresh()
        {
            if (timerText != null)
            {
                timerText.text = HUDTextFormatter.FormatTime(elapsedSeconds);
            }
        }
    }
}
