using System; // 실제 예외 형식
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 보스 체력 표시 영역 조회
using ProjectEpsilon.Combat; // 실제 포탑과 시너지
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 실제 보스 화면
using ProjectEpsilon.Player; // 실제 몸통
using ProjectEpsilon.UI; // 실제 설정 화면
using UnityEditor; // 실제 배치 제어
using UnityEditor.SceneManagement; // 실제 전투 씬 열기
using UnityEngine; // Unity 구성
using UnityEngine.UI; // 실제 패널과 이미지

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 실제 플레이 모드 재연결
    public static class AutomatedBodySidebarVerification // 실제 우측 몸통 패널 시험
    {
        private const string Key = "EpsilonBodySidebarVerification"; // 실제 시험 진행 표식
        private static bool started; // 실제 시험 실행 여부
        static AutomatedBodySidebarVerification() // 실제 도메인 재시작 연결
        {
            if (SessionState.GetBool(Key, false)) // 실제 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시 연결
            }
        }
        public static void Run() // 실제 Unity 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 포탑 선택과 시험 시작
        {
            if (!EditorApplication.isPlaying || started) // 실제 시작과 중복 확인
            {
                return; // 실제 플레이 시작 대기
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 포탑 선택
            {
                return; // 실제 전투 시작 대기
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 상태 확인
            {
                return; // 실제 전투 시작 대기
            }
            started = true; // 실제 시험 중복 방지
            new GameObject("BodySidebarVerification").AddComponent<BodySidebarVerificationRunner>(); // 실제 패널 시험 실행
        }
        public static void Finish(int code, string message) // 실제 배치 종료
        {
            if (code == 0) // 실제 시험 성공 확인
            {
                Debug.Log("[BodySidebarVerification] PASS: " + message); // 실제 성공 결과
            }
            else // 실제 시험 실패 확인
            {
                Debug.LogError("[BodySidebarVerification] FAIL: " + message); // 실제 실패 결과
            }
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 연결 해제
            EditorApplication.Exit(code); // 실제 시험 종료 코드
        }
    }
    public sealed class BodySidebarVerificationRunner : MonoBehaviour // 실제 패널 시험 실행기
    {
        private IEnumerator Start() // 실제 예외를 포함한 시험 실행
        {
            IEnumerator steps = RunSteps(); // 실제 패널 시험 단계
            while (true) // 실제 시험 완료까지 반복
            {
                bool moved; // 실제 다음 단계 여부
                try // 실제 시험 단계 실행
                {
                    moved = steps.MoveNext(); // 실제 다음 단계 진행
                }
                catch (Exception error) // 실제 시험 오류 확인
                {
                    AutomatedBodySidebarVerification.Finish(1, error.ToString()); // 실제 오류와 실패 종료
                    yield break; // 추가 시험 중단
                }
                if (!moved) // 실제 시험 완료 확인
                {
                    yield break; // 실제 실행 종료
                }
                yield return steps.Current; // 실제 다음 프레임 대기
            }
        }
        private IEnumerator RunSteps() // 실제 위치와 몸통별 포탑 확인
        {
            yield return null; // 실제 패널 초기화 대기
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            WeaponAttributeSynergyManager synergy = FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 관리자
            Require(body != null && weapons != null && synergy != null && Camera.main != null, "Combat components missing."); // 실제 전투 구성 확인
            SnakeMovement movement = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            if (movement != null) // 실제 이동 확인
            {
                movement.enabled = false; // 통제 시험 중 자동 이동 정지
            }
            GameObject area = GameObject.Find("GameplayViewport"); // 실제 왼쪽 HUD 영역
            GameObject panel = GameObject.Find("BodySidebar"); // 실제 오른쪽 몸통 패널
            Require(area != null && panel != null, "Separated gameplay area or sidebar missing."); // 실제 분리 영역 확인
            Require(!Visible("SettingsButton") && !Visible("AttributeSynergyHUD"), "Legacy settings button or attribute list is still visible."); // 실제 기존 중복 화면 제거 확인
            SettingsMenuController settings = FindFirstObjectByType<SettingsMenuController>(); // 실제 기존 키 설정
            Require(settings != null, "Input settings controller missing."); // 실제 설정 제어기 확인
            settings.OpenSettings(); // 실제 메뉴에서 사용하는 설정 열기
            Require(settings.IsOpen, "Input settings became inaccessible."); // 실제 설정 기능 유지 확인
            settings.CloseSettings(); // 실제 키 설정 닫기
            RectTransform gameRect = (RectTransform)area.transform; // 실제 전투 HUD 영역
            RectTransform sideRect = (RectTransform)panel.transform; // 실제 우측 패널 영역
            Require(Mathf.Approximately(Camera.main.rect.width, 0.76f) && Mathf.Approximately(gameRect.anchorMax.x, 0.76f) && Mathf.Approximately(sideRect.anchorMin.x, 0.76f), "Camera and HUD regions overlap."); // 실제 카메라와 패널 경계 확인
            Canvas.ForceUpdateCanvases(); // 실제 화면 영역 계산
            Vector3[] gameCorners = new Vector3[4]; // 실제 전투 화면 모서리
            Vector3[] sideCorners = new Vector3[4]; // 실제 패널 화면 모서리
            gameRect.GetWorldCorners(gameCorners); // 실제 전투 영역 좌표
            sideRect.GetWorldCorners(sideCorners); // 실제 패널 영역 좌표
            float gameRight = RectTransformUtility.WorldToScreenPoint(null, gameCorners[2]).x; // 실제 전투 화면 오른쪽
            float sideLeft = RectTransformUtility.WorldToScreenPoint(null, sideCorners[0]).x; // 실제 패널 왼쪽
            Require(Mathf.Abs(gameRight - sideLeft) < 2f && Mathf.Abs(gameRight - Camera.main.pixelRect.xMax) < 2f, $"Actual pixel boundary mismatch: HUD={gameRight}, panel={sideLeft}, camera={Camera.main.pixelRect.xMax}."); // 실제 픽셀 경계 일치 확인
            MethodInfo bossLayout = typeof(PrototypeBossEncounter).GetMethod("ResolveHealthBarRect", BindingFlags.NonPublic | BindingFlags.Static); // 실제 보스 체력 영역 계산기
            Require(bossLayout != null, "Boss health bar does not use the gameplay viewport."); // 실제 보스 체력 위치 계산 확인
            object[] bossArguments = // 실제 보스 체력 표시 인수
            {
                Camera.main // 실제 전투 카메라
            };
            Rect bossRect = (Rect)bossLayout.Invoke(null, bossArguments); // 실제 보스 체력 표시 영역
            Require(bossRect.xMin >= Camera.main.pixelRect.xMin && bossRect.xMax <= Camera.main.pixelRect.xMax, "Boss health bar overlaps the sidebar."); // 실제 보스 체력과 우측 패널 분리 확인
            Require(Visible("SidebarRow_1") && Visible("SidebarRow_2") && Visible("SidebarRow_3") && !Visible("SidebarRow_4"), "Initial body row count incorrect."); // 실제 몸통별 행 수 확인
            Require(TextValue("SidebarLabel_2") == "-", "Empty body slot does not show a dash."); // 실제 빈 몸통 표기 확인
            WeaponData blade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 물리 포탑
            WeaponData fire = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeFireBrand.asset"); // 실제 화염 포탑
            Require(blade != null && fire != null && weapons.TryEquipAt(0, blade) && weapons.TryEquipAt(1, blade), "Could not equip test turrets."); // 실제 포탑 두 개 장착
            Require(Icon("SidebarWeapon_1") == blade.Artwork && Icon("SidebarWeapon_2") == blade.Artwork && TextValue("SidebarLabel_1").Contains(blade.DisplayName) && TextValue("SidebarAttribute_0").Contains("2단계"), "Turret image, order or synergy did not update."); // 실제 이미지와 물리 시너지 확인
            Text rowLabel = GameObject.Find("SidebarLabel_1").GetComponent<Text>(); // 실제 첫 포탑 이름 글자
            Require(rowLabel.alignment == TextAnchor.MiddleCenter && rowLabel.fontSize >= 22 && !rowLabel.text.Contains("\n") && !rowLabel.text.Contains("기본 공격"), "Turret name is not centered or still shows base attack details."); // 실제 가운데 정렬과 공격 상세 제거 확인
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "Could not merge test turrets."); // 실제 같은 포탑 합성
            Require(TextValue("SidebarLabel_1").Contains("★2") && Icon("SidebarWeapon_2") == null && !TextValue("SidebarAttribute_0").Contains("2단계"), "Merge grade or synergy did not update."); // 실제 합성 등급과 단계 갱신 확인
            Require(weapons.TryEquipAt(1, fire) && Icon("SidebarWeapon_2") == fire.Artwork && TextValue("SidebarLabel_2").Contains("화염"), "Replacement turret image or attribute mismatch."); // 실제 교체 이미지와 속성 확인
            Require(body.TryGainBodyFromLevelUp() && Visible("SidebarRow_4"), "Growth row did not appear."); // 실제 몸통 성장 행 확인
            Require(weapons.TryEquipAt(3, fire) && Icon("SidebarWeapon_4") == fire.Artwork, "Tail turret image mismatch."); // 실제 꼬리 쪽 포탑 확인
            Require(body.TryRemoveBody() && !Visible("SidebarRow_4"), "Lost body row remained visible."); // 실제 몸통 손실 행 제거
            Require(body.TryRepairBody() && Visible("SidebarRow_4") && Icon("SidebarWeapon_4") == null, "Repaired body slot was not empty."); // 실제 복구 행과 빈 슬롯 확인
            while (body.CurrentBodyCount < 20) // 실제 절대 몸통 상한까지 성장
            {
                Require(body.TryGainBodyFromLevelUp(), "Could not grow to maximum body count."); // 실제 성장 성공 확인
            }
            Require(Visible("SidebarRow_20") && weapons.TryEquipAt(19, fire) && Icon("SidebarWeapon_20") == fire.Artwork, "Twentieth body turret was not displayed."); // 실제 20번 몸통 이미지 확인
            RectTransform lastRow = (RectTransform)GameObject.Find("SidebarRow_20").transform; // 실제 마지막 몸통 행
            Require(lastRow.anchorMin.y >= 0f && lastRow.anchorMax.y <= 1f, "Last body row exceeds sidebar bounds."); // 실제 마지막 행 표시 영역 확인
            Require(body.TryRemoveBody() && !Visible("SidebarRow_20") && body.TryRepairBody() && Visible("SidebarRow_20") && Icon("SidebarWeapon_20") == null, "Twentieth body loss or repair failed."); // 실제 마지막 몸통 손실과 빈 복구 확인
            AutomatedBodySidebarVerification.Finish(0, "Viewport, body order, images, replacement, merge, synergy, growth, loss and repair passed."); // 실제 전체 시험 성공
        }
        private static bool Visible(string name) // 실제 활성 행 확인
        {
            return GameObject.Find(name) != null; // 실제 활성 객체 여부 반환
        }
        private static Sprite Icon(string name) // 실제 포탑 이미지 조회
        {
            GameObject item = GameObject.Find(name); // 실제 이미지 객체 조회
            Image image = item == null ? null : item.GetComponent<Image>(); // 실제 포탑 이미지
            return image == null || !image.enabled ? null : image.sprite; // 실제 표시 스프라이트 반환
        }
        private static string TextValue(string name) // 실제 패널 문구 조회
        {
            GameObject item = GameObject.Find(name); // 실제 문구 객체 조회
            Text text = item == null ? null : item.GetComponent<Text>(); // 실제 UI 글자
            return text == null ? "" : text.text; // 안전한 문구 반환
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
    }
}
