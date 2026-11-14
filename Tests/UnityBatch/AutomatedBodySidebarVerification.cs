using System; // 실제 예외 형식
using System.Collections; // 실제 프레임 대기
using System.Collections.Generic; // 실제 UI 광선 검사 결과
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
using UnityEngine.EventSystems; // 실제 마우스 클릭 전달

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
            Transform mounted = weapons.Slots[0].Origin.Find("WeaponVisual"); // 실제 몸통 포탑 그림
            Require(mounted != null && mounted.GetComponent<SpriteRenderer>().sprite == blade.Artwork, "Mounted turret image differs from its data."); // 몸통과 포탑 데이터 일치 확인
            Require(Icon("SidebarWeapon_1") == blade.Icon && Icon("SidebarWeapon_2") == blade.Icon && TextValue("SidebarLabel_1").Contains(blade.DisplayName) && TextValue("SidebarAttribute_0").Contains("2단계"), "Turret image, order or synergy did not update."); // 실제 아이콘과 물리 시너지 확인
            Click("SidebarAttributeButton_0"); // 실제 물리 속성 클릭
            yield return null; // 실제 상세 화면의 렌더 등록 대기
            Require(Visible("SidebarSynergyDetail") && !Visible("SidebarRow_1") && TextValue("SidebarInfoTitle").Contains("물리"), "물리 시너지 상세 전환 실패"); // 실제 상세 화면 전환 확인
            Require(TextComponent("SidebarSynergyStage_2").color == Color.white && TextComponent("SidebarSynergyStage_4").color != Color.white, "활성 단계와 미활성 단계 색상 오류"); // 실제 단계별 글자 색상 확인
            for (int index = 0; index < WeaponAttributeSynergyRules.AttributeCount; index++) // 실제 여덟 속성 순회
            { // 반복 시작
                Click("SidebarAttributeButton_" + index); // 실제 속성별 클릭
                Require(TextValue("SidebarInfoTitle").Contains(WeaponDisplayNames.Attribute((WeaponAttribute)index)), "속성 상세 제목 오류: " + index); // 실제 속성별 상세 전환 확인
            } // 반복 종료
            Click("SidebarAttributeButton_0"); // 실제 물리 상세 재선택
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "Could not merge test turrets."); // 실제 같은 포탑 합성
            Require(TextComponent("SidebarSynergyStage_2").color != Color.white && TextValue("SidebarInfoStatus").Contains("미활성"), "합성 후 상세 시너지 미활성 갱신 실패"); // 실제 합성 후 상세 단계 갱신 확인
            Click("SidebarInfoCloseButton"); // 실제 몸통 목록 복귀
            Text rowLabel = GameObject.Find("SidebarLabel_1").GetComponent<Text>(); // 실제 첫 포탑 이름 글자
            Require(rowLabel.alignment == TextAnchor.MiddleCenter && rowLabel.fontSize >= 22 && !rowLabel.text.Contains("\n") && !rowLabel.text.Contains("기본 공격"), "Turret name is not centered or still shows base attack details."); // 실제 가운데 정렬과 공격 상세 제거 확인
            Require(TextValue("SidebarLabel_1").Contains("★2") && Icon("SidebarWeapon_2") == null && !TextValue("SidebarAttribute_0").Contains("2단계"), "Merge grade or synergy did not update."); // 실제 합성 등급과 단계 갱신 확인
            Require(weapons.TryEquipAt(1, fire) && Icon("SidebarWeapon_2") == fire.Icon && TextValue("SidebarLabel_2").Contains("화염"), "Replacement turret image or attribute mismatch."); // 실제 교체 아이콘과 속성 확인
            WeaponAttributeCombatEffects effects = weapons.AttributeCombatEffects; // 실제 전투 속성 효과 설정
            Require(effects != null, "속성 조합 전투 설정 누락"); // 실제 반응 수치 구성 확인
            SetReactionFloat(effects, "corrosiveBurnDuration", 4.5f); // 실제 지속 피해 시간 시험값
            SetReactionFloat(effects, "corrosiveBurnDamagePerSecond", 2.25f); // 실제 초당 피해 시험값
            SetReactionFloat(effects, "coolingDischargeDamage", 7f); // 실제 냉기 전기 피해 시험값
            SetReactionFloat(effects, "coolingDischargeSlowDuration", 2f); // 실제 감속 시간 시험값
            SetReactionFloat(effects, "coolingDischargeSlowMultiplier", 0.65f); // 실제 감속 배율 시험값
            SetReactionFloat(effects, "shatteringImpactRadius", 3.5f); // 실제 파쇄 반경 시험값
            SetReactionFloat(effects, "shatteringImpactDamage", 9f); // 실제 파쇄 피해 시험값
            SetReactionFloat(effects, "shatteringImpactKnockbackDistance", 1.2f); // 실제 밀치기 거리 시험값
            SetReactionFloat(effects, "shatteringImpactKnockbackDuration", 0.4f); // 실제 밀치기 시간 시험값
            SetReactionFloat(effects, "sacredExplosionRadius", 4f); // 실제 성화 반경 시험값
            SetReactionFloat(effects, "sacredExplosionDamage", 11f); // 실제 성화 피해 시험값
            SetReactionFloat(effects, "lightDarkDamage", 12f); // 실제 명암 피해 시험값
            SetReactionFloat(effects, "corrosiveCurseDuration", 6f); // 실제 공격 약화 시간 시험값
            SetReactionFloat(effects, "corrosiveCurseOutgoingMultiplier", 0.7f); // 실제 주는 피해 배율 시험값
            SetReactionFloat(effects, "corrosiveCurseIncomingMultiplier", 1.4f); // 실제 받는 피해 배율 시험값
            typeof(WeaponAttributeCombatEffects).GetField("lightDarkHealing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(effects, 13); // 실제 공용 체력 회복 시험값
            Click("SidebarReactionsButton"); // 실제 속성 조합 패널 선택
            yield return null; // 실제 조합 화면의 렌더 등록 대기
            Require(Visible("SidebarReactionPanel") && !Visible("SidebarRow_1"), "속성 조합 패널 전환 실패"); // 실제 조합 화면 전환 확인
            WeaponAttribute[] expectedLeft = { WeaponAttribute.Fire, WeaponAttribute.Cold, WeaponAttribute.Physical, WeaponAttribute.Holy, WeaponAttribute.Dark, WeaponAttribute.Holy }; // 실제 첫 조합 속성 순서
            WeaponAttribute[] expectedRight = { WeaponAttribute.Poison, WeaponAttribute.Electric, WeaponAttribute.Explosion, WeaponAttribute.Fire, WeaponAttribute.Poison, WeaponAttribute.Dark }; // 실제 둘째 조합 속성 순서
            for (int index = 0; index < 6; index++) // 실제 여섯 가지 조합 순회
            { // 반복 시작
                Require(Icon("SidebarReactionLeft_" + index) == ProjectUiSkin.AttributeIcon((int)expectedLeft[index]) && Icon("SidebarReactionRight_" + index) == ProjectUiSkin.AttributeIcon((int)expectedRight[index]) && WeaponAttributeReactionRules.ResolvePair(expectedLeft[index], expectedRight[index]) != WeaponAttributeReaction.None && TextValue("SidebarReactionPlus_" + index) == "+" && TextValue("SidebarReactionEquals_" + index) == "=" && !string.IsNullOrEmpty(TextValue("SidebarReactionText_" + index)), "속성 조합 아이콘과 수식 누락: " + index); // 실제 조합 순서와 수식 확인
            } // 반복 종료
            Require(TextValue("SidebarReactionText_0").Contains("4.5초") && TextValue("SidebarReactionText_0").Contains("초당 피해 2.25"), "화염 독 지속 피해 수치 오류"); // 실제 지속 피해 설정 반영 확인
            Require(TextValue("SidebarReactionText_1").Contains("피해 7") && TextValue("SidebarReactionText_1").Contains("2초") && TextValue("SidebarReactionText_1").Contains("35%"), "냉기 전기 감속 수치 오류"); // 실제 추가 피해와 감속 설정 반영 확인
            Require(TextValue("SidebarReactionText_2").Contains("반경 3.5") && TextValue("SidebarReactionText_2").Contains("피해 9") && TextValue("SidebarReactionText_2").Contains("1.2") && TextValue("SidebarReactionText_2").Contains("0.4초"), "물리 폭발 밀치기 수치 오류"); // 실제 범위 피해와 밀치기 설정 반영 확인
            Require(TextValue("SidebarReactionText_3").Contains("반경 4") && TextValue("SidebarReactionText_3").Contains("피해 11"), "신성 화염 범위 피해 수치 오류"); // 실제 범위 피해 설정 반영 확인
            Require(TextValue("SidebarReactionText_4").Contains("6초") && TextValue("SidebarReactionText_4").Contains("-30%") && TextValue("SidebarReactionText_4").Contains("+40%"), "암흑 독 피해 약화 수치 오류"); // 실제 피해 배율 설정 반영 확인
            Require(TextValue("SidebarReactionText_5").Contains("피해 12") && TextValue("SidebarReactionText_5").Contains("13 회복"), "신성 암흑 피해 회복 수치 오류"); // 실제 피해와 회복 설정 반영 확인
            Canvas.ForceUpdateCanvases(); // 실제 설명 글자 배치 갱신
            for (int index = 0; index < 6; index++) // 실제 여섯 설명 글자 순회
            { // 반복 시작
                Text description = TextComponent("SidebarReactionText_" + index); // 실제 조합 설명 글자
                int expectedCharacters = description.text.Replace("\n", "").Length; // 줄바꿈을 제외한 전체 글자 수
                Require(description.cachedTextGenerator.characterCountVisible >= expectedCharacters, "속성 조합 설명 잘림: " + index); // 실제 표시 가능한 글자 수 확인
            } // 반복 종료
            foreach (string name in new[] { "부식성 연소", "냉각 방전", "파쇄 충격", "성화 폭발", "부식 저주", "명암 상쇄" }) // 기존 반응 명칭 순회
            { // 반복 시작
                for (int index = 0; index < 6; index++) // 실제 여섯 조합 설명 순회
                { // 반복 시작
                    Require(!TextValue("SidebarReactionText_" + index).Contains(name), "삭제한 속성 조합 명칭 노출: " + name); // 실제 명칭 제거 확인
                } // 반복 종료
            } // 반복 종료
            Require(TextComponent("SidebarReactionText_2").color != Color.white, "미보유 조합 글자 색상 오류"); // 실제 미보유 조합 색상 확인
            WeaponData explosion = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Day14/DebugExplosion.asset"); // 실제 폭발 포탑 데이터
            Require(explosion != null && weapons.TryEquipAt(2, explosion) && TextComponent("SidebarReactionText_2").color == Color.white, "속성 보유 후 조합 글자 갱신 실패"); // 실제 포탑 변경에 따른 조합 갱신 확인
            Click("SidebarInfoCloseButton"); // 실제 몸통 목록 복귀
            Require(Visible("SidebarRow_1") && !Visible("SidebarReactionPanel"), "몸통 목록 복귀 실패"); // 실제 정보 패널 닫기 확인
            Require(body.TryGainBodyFromLevelUp() && Visible("SidebarRow_4"), "Growth row did not appear."); // 실제 몸통 성장 행 확인
            Require(weapons.TryEquipAt(3, fire) && Icon("SidebarWeapon_4") == fire.Icon, "Tail turret image mismatch."); // 실제 꼬리 쪽 포탑 확인
            Require(body.TryRemoveBody() && !Visible("SidebarRow_4"), "Lost body row remained visible."); // 실제 몸통 손실 행 제거
            Require(body.TryRepairBody() && Visible("SidebarRow_4") && Icon("SidebarWeapon_4") == null, "Repaired body slot was not empty."); // 실제 복구 행과 빈 슬롯 확인
            while (body.CurrentBodyCount < 20) // 실제 절대 몸통 상한까지 성장
            {
                Require(body.TryGainBodyFromLevelUp(), "Could not grow to maximum body count."); // 실제 성장 성공 확인
            }
            Require(Visible("SidebarRow_20") && weapons.TryEquipAt(19, fire) && Icon("SidebarWeapon_20") == fire.Icon, "Twentieth body turret was not displayed."); // 실제 20번 몸통 아이콘 확인
            RectTransform lastRow = (RectTransform)GameObject.Find("SidebarRow_20").transform; // 실제 마지막 몸통 행
            Require(lastRow.anchorMin.y >= 0f && lastRow.anchorMax.y <= 1f, "Last body row exceeds sidebar bounds."); // 실제 마지막 행 표시 영역 확인
            Require(body.TryRemoveBody() && !Visible("SidebarRow_20") && body.TryRepairBody() && Visible("SidebarRow_20") && Icon("SidebarWeapon_20") == null, "Twentieth body loss or repair failed."); // 실제 마지막 몸통 손실과 빈 복구 확인
            for (int index = 3; index < 10; index++) // 실제 물리 여덟 개 장착 준비
            { // 반복 시작
                Require(weapons.TryEquipAt(index, blade), "고단계 물리 포탑 장착 실패: " + index); // 실제 물리 속성 수 증가
            } // 반복 종료
            Click("SidebarAttributeButton_0"); // 실제 물리 고단계 선택
            yield return null; // 실제 상세 화면의 렌더 등록 대기
            Require(synergy.GetStage(WeaponAttribute.Physical) == 8 && TextComponent("SidebarSynergyStage_2").color == Color.white && TextComponent("SidebarSynergyStage_4").color == Color.white && TextComponent("SidebarSynergyStage_6").color != Color.white && TextComponent("SidebarSynergyStage_8").color == Color.white, "고단계 활성 효과 또는 미구현 효과 색상 오류"); // 실제 활성과 미구현 단계 구분 확인
            Click("SidebarInfoCloseButton"); // 실제 몸통 목록 복귀
            AutomatedBodySidebarVerification.Finish(0, "Viewport, body order, images, replacement, merge, synergy, growth, loss and repair passed."); // 실제 전체 시험 성공
        }
        private static void SetReactionFloat(WeaponAttributeCombatEffects effects, string name, float value) // 격리된 실제 반응 수치 설정
        { // 함수 시작
            FieldInfo field = typeof(WeaponAttributeCombatEffects).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); // 실제 반응 설정 필드
            Require(field != null, "속성 반응 설정 누락: " + name); // 실제 설정 필드 확인
            field.SetValue(effects, value); // 격리된 메모리 시험값 적용
        } // 함수 종료
        private static Text TextComponent(string name) // 실제 패널 글자 조회
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 실제 글자 객체 조회
            return item == null ? null : item.GetComponent<Text>(); // 실제 글자 반환
        } // 함수 종료
        private static void Click(string name) // 실제 마우스 위치와 클릭 확인
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 실제 버튼 객체 조회
            Button button = item == null ? null : item.GetComponent<Button>(); // 실제 선택 버튼
            Require(button != null && button.interactable && EventSystem.current != null, "클릭 버튼 누락: " + name); // 실제 버튼과 입력 시스템 확인
            Canvas.ForceUpdateCanvases(); // 실제 화면 위치 갱신
            Vector3[] corners = new Vector3[4]; // 실제 버튼 화면 모서리
            button.GetComponent<RectTransform>().GetWorldCorners(corners); // 실제 버튼 위치 조회
            PointerEventData pointer = new PointerEventData(EventSystem.current); // 실제 마우스 포인터 정보
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) * 0.5f); // 실제 버튼 중앙 위치
            pointer.button = PointerEventData.InputButton.Left; // 실제 왼쪽 마우스 버튼
            List<RaycastResult> hits = new List<RaycastResult>(); // 실제 화면 입력 대상 목록
            EventSystem.current.RaycastAll(pointer, hits); // 실제 포인터 대상 검색
            Require(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == item, "마우스 클릭 대상 가림: " + name + " / " + (hits.Count == 0 ? "없음" : hits[0].gameObject.name) + " / " + pointer.position + " / " + Screen.width + "x" + Screen.height); // 실제 입력 대상 확인
            ExecuteEvents.Execute(item, pointer, ExecuteEvents.pointerClickHandler); // 실제 마우스 클릭 전달
        } // 함수 종료
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
