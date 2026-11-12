using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Player; // 실제 다음 런 캐릭터
using ProjectEpsilon.Progression; // 실제 해금 상태
using ProjectEpsilon.World; // 실제 맵 데이터
using UnityEngine; // Unity 씬 구성
using UnityEngine.SceneManagement; // 실제 씬 전환

namespace ProjectEpsilon.UI // 실제 화면 흐름 영역
{
    public enum SceneFrontendKind // 분리된 화면 종류
    {
        MainMenu, // 실제 메인 메뉴
        Preparation, // 실제 출격 준비
        Results // 실제 전투 결과
    }
    public sealed class SceneFrontendContext : MonoBehaviour // 분리 씬의 선택 데이터와 이동
    {
        public const string MainMenuPath = "Assets/Scenes/MainMenu.unity"; // 실제 메인 메뉴 경로
        public const string PreparationPath = "Assets/Scenes/Preparation.unity"; // 실제 출격 준비 경로
        public const string GamePath = "Assets/Scenes/Game.unity"; // 실제 전투 경로
        public const string ResultsPath = "Assets/Scenes/Results.unity"; // 실제 결과 경로
        [SerializeField] private SceneFrontendKind kind; // 실제 씬 역할
        [SerializeField] private CharacterData[] characters; // 실제 선택 캐릭터
        [SerializeField] private PrototypeMapProfile[] maps; // 실제 선택 맵
        [SerializeField] private Sprite headSprite; // 실제 머리 미리보기
        [SerializeField] private Sprite bodySprite; // 실제 몸통 미리보기
        private static bool pendingStart; // 실제 전투 시작 예약
        private static bool separateFlow; // 분리 씬 출격 여부
        private static bool retryWeapons; // 재도전 포탑 선택 여부
        private static int chosenMap; // 실제 선택한 맵 번호
        private static int chosenWeapon; // 실제 선택한 시작 포탑 번호
        private CharacterData selectedCharacter; // 실제 선택한 캐릭터
        public SceneFrontendKind Kind => kind; // 실제 현재 씬 역할
        public int CharacterCount => characters == null ? 0 : characters.Length; // 실제 캐릭터 후보 수
        public int MapCount => maps == null ? 0 : maps.Length; // 실제 맵 후보 수
        public int SelectedMap => chosenMap; // 실제 선택한 맵 번호
        public Sprite HeadSprite => headSprite; // 실제 머리 이미지
        public Sprite BodySprite => bodySprite; // 실제 몸통 이미지
        public static bool UsesSeparateScenes => separateFlow; // 실제 분리 씬 출격 여부
        public CharacterData SelectedCharacter => selectedCharacter; // 실제 선택한 캐릭터 조회
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 실제 플레이 세션 시작 시 초기화
        private static void InitializeSession() // 이전 플레이 세션의 정적 상태 정리
        {
            ResetFlow(); // 실제 화면 흐름 기본값 복원
        }
        private void Awake() // 실제 씬 초기화
        {
            if (kind != SceneFrontendKind.Results) // 실제 메뉴 또는 준비 화면 확인
            {
                Time.timeScale = 1f; // 이전 런의 정지 상태 해제
            }
            if (kind == SceneFrontendKind.Preparation) // 실제 출격 준비 화면 확인
            {
                CharacterData previous = CharacterRunInitializer.SelectedCharacter; // 이전에 선택한 캐릭터
                selectedCharacter = IsCharacterAvailable(previous) ? previous : GetCharacter(0); // 해금된 이전 선택 또는 기본 캐릭터
                if (!IsMapAvailable(chosenMap)) // 이전 맵 해금 확인
                {
                    chosenMap = 0; // 기본 맵으로 복구
                }
            }
        }
        public CharacterData GetCharacter(int index) // 실제 캐릭터 후보 조회
        {
            return characters != null && index >= 0 && index < characters.Length ? characters[index] : null; // 유효한 후보 반환
        }
        public PrototypeMapProfile GetMap(int index) // 실제 맵 후보 조회
        {
            return maps != null && index >= 0 && index < maps.Length ? maps[index] : null; // 유효한 맵 반환
        }
        public bool IsCharacterAvailable(CharacterData character) // 실제 캐릭터 해금 확인
        {
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 저장 관리자
            return character != null && (meta == null || meta.IsCharacterUnlocked(character.Id)); // 실제 해금 상태 반환
        }
        public bool IsMapAvailable(int index) // 실제 맵 해금 확인
        {
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 저장 관리자
            return GetMap(index) != null && (meta == null || meta.IsMapUnlocked(index)); // 실제 해금된 맵 반환
        }
        public bool SelectCharacter(int index) // 실제 준비 캐릭터 선택
        {
            CharacterData candidate = GetCharacter(index); // 실제 캐릭터 후보
            if (!IsCharacterAvailable(candidate)) // 실제 해금 상태 확인
            {
                return false; // 잠긴 캐릭터 선택 차단
            }
            selectedCharacter = candidate; // 실제 캐릭터 선택 보관
            CharacterRunInitializer.SelectForNextRun(candidate); // 실제 전투 초기화에 전달
            return true; // 실제 선택 완료
        }
        public bool SelectMap(int index) // 실제 준비 맵 선택
        {
            if (!IsMapAvailable(index)) // 실제 맵 해금 확인
            {
                return false; // 잠긴 맵 선택 차단
            }
            chosenMap = index; // 실제 맵 선택 보관
            return true; // 실제 선택 완료
        }
        public bool StartRun(int index) // 실제 포탑 선택과 출격
        {
            WeaponData[] weapons = SelectedCharacter == null || SelectedCharacter.StartingWeapons == null ? null : SelectedCharacter.StartingWeapons.Weapons; // 실제 시작 포탑 후보
            if (!IsCharacterAvailable(SelectedCharacter) || !IsMapAvailable(chosenMap) || weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null) // 실제 선택과 해금 확인
            {
                return false; // 잘못된 출격 차단
            }
            CharacterRunInitializer.SelectForNextRun(SelectedCharacter); // 실제 선택 캐릭터 전달
            chosenWeapon = index; // 실제 시작 포탑 보관
            pendingStart = true; // 실제 게임 씬 적용 예약
            separateFlow = true; // 분리 씬 흐름 기록
            retryWeapons = false; // 이전 재도전 예약 해제
            SceneManager.LoadScene(GamePath); // 실제 전투 씬 이동
            return true; // 실제 출격 요청 완료
        }
        public static bool TakeStart(out int map, out int weapon) // 전투 씬의 시작 선택 수신
        {
            map = chosenMap; // 실제 선택한 맵 전달
            weapon = chosenWeapon; // 실제 선택한 포탑 전달
            if (!pendingStart) // 실제 출격 예약 확인
            {
                return false; // 직접 게임 씬 실행 유지
            }
            pendingStart = false; // 실제 예약 중복 소비 방지
            return true; // 실제 선택 전달 완료
        }
        public static void PrepareRetry() // 실제 재도전 준비
        {
            retryWeapons = true; // 이전 포탑 선택 화면 예약
        }
        public static bool TakeRetry() // 재도전 화면 예약 소비
        {
            bool requested = retryWeapons; // 실제 재도전 예약 조회
            retryWeapons = false; // 중복 재도전 화면 방지
            return requested; // 실제 예약 반환
        }
        public static void ResetFlow() // 실제 메인 메뉴 복귀
        {
            pendingStart = false; // 이전 출격 예약 제거
            separateFlow = false; // 이전 분리 흐름 종료
            retryWeapons = false; // 이전 재도전 예약 제거
            chosenMap = 0; // 기본 맵 선택 복원
        }
    }
}
