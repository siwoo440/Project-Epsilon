using System; // 실제 저장 예외 처리
using UnityEngine; // 실제 화면과 소리 설정

namespace ProjectEpsilon.UI // 화면 설정 영역
{
    [Serializable] // 실제 저장 형식
    public sealed class FrontendOptionsData // 실제 화면과 음량 설정
    {
        public int version = 1; // 실제 저장 버전
        public int volume = 100; // 실제 음량 백분율
        public int shake = 100; // 화면 흔들림 백분율
        public int resolution = -1; // 실제 기본 화면 해상도
        public bool fullscreen; // 실제 전체 화면 여부
    }
    public sealed class FrontendOptions // 기존 입력과 분리된 화면 설정
    {
        public const string SaveKey = "ProjectEpsilon.FrontendOptions.v1"; // 실제 독립 저장 키
        private static readonly int[] Widths = { 1280, 1600, 1920 }; // 시험 해상도 너비
        private static readonly int[] Heights = { 720, 900, 1080 }; // 시험 해상도 높이
        private FrontendOptionsData data = new FrontendOptionsData(); // 실제 설정 데이터
        public bool IsAvailable { get; private set; } = true; // 실제 저장 가능 여부
        public int Volume => data.volume; // 실제 음량 백분율
        public int Shake => data.shake; // 실제 흔들림 백분율
        public int ResolutionIndex => data.resolution; // 실제 해상도 번호
        public bool Fullscreen => data.fullscreen; // 실제 전체 화면 여부
        public string ResolutionLabel => data.resolution < 0 ? $"{Screen.width} × {Screen.height} (현재)" : $"{Widths[data.resolution]} × {Heights[data.resolution]}"; // 실제 해상도 표시
        public FrontendOptions() // 실제 설정 읽기
        {
            data.fullscreen = Screen.fullScreen; // 현재 화면 모드 기본값
            data.volume = Mathf.RoundToInt(AudioListener.volume * 100f); // 현재 음량 기본값
            try // 실제 저장 읽기
            {
                if (PlayerPrefs.HasKey(SaveKey)) // 기존 설정 확인
                {
                    string json = PlayerPrefs.GetString(SaveKey); // 실제 저장 내용
                    if (!json.Contains("\"version\"") || !json.Contains("\"volume\"") || !json.Contains("\"shake\"") || !json.Contains("\"resolution\"") || !json.Contains("\"fullscreen\"")) // 필수 저장 항목 확인
                    {
                        IsAvailable = false; // 손상 원본 보존
                        return; // 실제 설정 적용 차단
                    }
                    FrontendOptionsData loaded = JsonUtility.FromJson<FrontendOptionsData>(json); // 실제 설정 복원
                    if (loaded == null || loaded.version != 1 || loaded.volume < 0 || loaded.volume > 100 || loaded.shake < 0 || loaded.shake > 100 || loaded.resolution < -1 || loaded.resolution >= Widths.Length) // 실제 설정 검증
                    {
                        IsAvailable = false; // 손상 원본 보존
                        return; // 실제 설정 적용 차단
                    }
                    data = loaded; // 검증된 설정 적용
                    ApplyDisplay(); // 저장된 화면 모드 적용
                }
                AudioListener.volume = data.volume / 100f; // 실제 음량 적용
            }
            catch (Exception error) // 실제 설정 오류 확인
            {
                IsAvailable = false; // 기존 설정 보존
                Debug.LogError("[Project Epsilon] 화면 설정 복원 실패: " + error.Message); // 실제 오류 기록
            }
        }
        public bool Change(int volumeDelta, int shakeDelta, int resolutionDelta, bool toggleFullscreen) // 실제 설정 변경
        {
            if (!IsAvailable) // 실제 저장 상태 확인
            {
                return false; // 손상 저장 덮어쓰기 차단
            }
            FrontendOptionsData next = new FrontendOptionsData { volume = Mathf.Clamp(data.volume + volumeDelta, 0, 100), shake = Mathf.Clamp(data.shake + shakeDelta, 0, 100), resolution = resolutionDelta == 0 ? data.resolution : (data.resolution + resolutionDelta % Widths.Length + Widths.Length) % Widths.Length, fullscreen = toggleFullscreen ? !data.fullscreen : data.fullscreen }; // 실제 변경 설정
            try // 실제 설정 저장
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(next)); // 실제 독립 설정 기록
                PlayerPrefs.Save(); // 실제 설정 저장 반영
                data = next; // 저장 성공 후 설정 확정
                AudioListener.volume = data.volume / 100f; // 실제 음량 반영
                if (resolutionDelta != 0 || toggleFullscreen) // 실제 화면 변경 확인
                {
                    ApplyDisplay(); // 실제 해상도와 화면 모드 반영
                }
                return true; // 실제 설정 완료
            }
            catch (Exception error) // 실제 저장 오류 확인
            {
                Debug.LogError("[Project Epsilon] 화면 설정 저장 실패: " + error.Message); // 실제 오류 기록
                return false; // 기존 설정 유지
            }
        }
        private void ApplyDisplay() // 실제 해상도 적용
        {
            if (!Application.isBatchMode) // 비대화형 시험 보호
            {
                int width = data.resolution < 0 ? Screen.width : Widths[data.resolution]; // 실제 선택 화면 너비
                int height = data.resolution < 0 ? Screen.height : Heights[data.resolution]; // 실제 선택 화면 높이
                Screen.SetResolution(width, height, data.fullscreen); // 실제 화면 설정 적용
            }
        }
    }
}
