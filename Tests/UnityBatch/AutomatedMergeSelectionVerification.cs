using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 버튼 조회
using ProjectEpsilon.Combat; // 무기와 시너지
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 시험 출현 제어
using ProjectEpsilon.Player; // 플레이어와 몸통
using ProjectEpsilon.Progression; // 합성과 보상
using ProjectEpsilon.UI; // 합성 화면
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 실제 버튼

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMergeSelectionVerification // 합성 실제 씬 시험
    {
        private const string ActiveKey = "EpsilonMergeSelectionVerification"; // 배치 진행 표식
        private static double started; // 시작 시각
        private static bool injected; // 시험 실행기 여부
        private static bool finished; // 종료 중복 방지

        static AutomatedMergeSelectionVerification() // 재시작 연결
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 여부
            {
                EditorApplication.update += Tick; // 시작 감시 연결
            }
        }

        public static void Run() // 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 기록
            EditorApplication.update -= Tick; // 중복 연결 제거
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 시작 및 제한 시간 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            {
                return; // 전투 시작 대기
            }
            if (started <= 0d) // 최초 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 제한 시간
            {
                Finish(1, "Timed out before all merge checks completed."); // 시간 초과 기록
                return; // 감시 종료
            }
            if (injected) // 실행기 존재 확인
            {
                return; // 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || Time.time < 0.35f) // 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 중복 실행 방지
            player.gameObject.AddComponent<MergeSelectionVerificationRunner>(); // 실제 씬 시험 시작
        }

        public static void Finish(int code, string message) // 결과 기록 및 종료
        {
            if (finished) // 종료 중복 확인
            {
                return; // 중복 종료 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 제거
            Debug.Log($"[MergeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }

    public sealed class MergeSelectionVerificationRunner : MonoBehaviour // 실제 UI 합성 시험
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 필드 검색
        private SnakeMovement movement; // 실제 자동 이동
        private SnakeStamina stamina; // 실제 가속 상태
        private PlayerInputReader input; // 실제 이동 입력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private WeaponAttributeSynergyManager synergy; // 실제 속성 집계
        private SnakeWeaponMergeController merge; // 실제 합성 처리기
        private WeaponMergePanelController panel; // 실제 합성 UI
        private EliteRewardController elite; // 실제 엘리트 보상
        private WeaponData weapon; // 시험 대상 무기
        private Keyboard keyboard; // 시험용 키보드
        private InputSettings originalSettings; // 기존 입력 설정
        private InputSettings testSettings; // 메모리 입력 설정
        private bool originalBackground; // 기존 백그라운드 설정

        private IEnumerator Start() // 실제 씬 시험 순서
        {
            Prepare(); // 씬과 가상 입력 준비
            yield return null; // 입력 장치 갱신 대기
            yield return VerifyMovement(); // 합성 중 직진과 시간 배율
            yield return VerifyCancellation(); // 각 선택 단계 ESC 취소
            yield return VerifySynergyPreview(); // 시너지 임계치 표시와 취소 보존
            yield return VerifyMixedAttributes(); // 다른 속성 전환과 긴 문구 표시
            VerifyInvalidPairs(); // 불일치와 최대 등급 차단
            PrepareMaterials(); // 스무 슬롯 재료 준비
            yield return OpenByKey(); // 실제 E 입력으로 합성 열기
            Click(0); // 마지막 재료로 역순 이동
            Require(Label(1).Contains("몸통 20:"), "Twentieth body was not reachable from the first slot."); // 스무 번째 슬롯 표시
            CheckMergeLabelsFit(); // 스무 번째 몸통 문구 높이 확인
            Click(1); // 스무 번째 재료 확정
            Require(Label(1).Contains("몸통 1:"), "Second material did not start at the first body."); // 둘째 재료 표시
            Click(1); // 첫 번째 몸통 재료 확정
            CheckMergeLabelsFit(); // 두 재료 강화 내역 높이 확인
            Require(weapons.OccupiedSlotCount == 2, "Materials were consumed before inheritance selection."); // 미확정 재료 유지
            Click(1); // 둘째 재료 강화 계승
            Require(weapons.OccupiedSlotCount == 2, "Materials were consumed before common upgrade selection."); // 강화 전 재료 유지
            Click(2); // 공통 범위 강화 선택
            Require(!merge.IsMergeOpen && weapons.OccupiedSlotCount == 1, "Merge did not consume exactly one weapon."); // 합성 완료와 무기 수
            Require(weapons.Slots[0].Weapon == weapon && weapons.Slots[0].Grade == 2, "Merged weapon or grade mismatch."); // 결과 무기와 등급
            Require(weapons.Slots[0].DamageUpgrades == 2 && weapons.Slots[0].SpeedUpgrades == 0 && weapons.Slots[0].RangeUpgrades == 1, "Second material inheritance or range upgrade mismatch."); // 둘째 강화 계승과 신규 강화
            Require(synergy.GetCount(weapon.Attribute) == 1 && synergy.GetStage(weapon.Attribute) == 0, "Synergy was not updated immediately after merging."); // 즉시 시너지 갱신
            Require(Mathf.Approximately(Time.timeScale, 1f) && !movement.IsMergeMovementMode && !stamina.IsBoostBlocked, "Movement state did not recover after merge."); // 이동 상태 복원
            Debug.Log("[MergeVerification] Actual buttons selected body 20 and inherited body 1; synergy 2->1 was immediate."); // 실제 버튼 결과 기록
            PrepareMaterials(); // 몸통 손실 시험 재료 준비
            yield return OpenByKey(); // 합성 열기
            Click(1); // 첫 재료 선택
            Require(body.TryRemoveBody(), "Could not remove a body for the external change test."); // 실제 몸통 손실
            Require(!merge.IsMergeOpen && Mathf.Approximately(Time.timeScale, 1f), "Body loss did not cancel merge and restore speed."); // 외부 변경 취소
            Require(body.TryRepairBody(), "Could not restore the lost body."); // 몸통 복구
            PrepareMaterials(); // 레벨업 끼어들기 재료 준비
            yield return OpenByKey(); // 합성 열기
            VerifyLevelUpInterruption(); // 레벨업 우선과 속도 복구
            PrepareMaterials(); // 엘리트 보상 시험 준비
            yield return VerifyEliteReward(); // 보상 취소와 성공 처리
            Cleanup(); // 시험 입력 정리
            AutomatedMergeSelectionVerification.Finish(0, "Actual scene E/ESC, UI buttons, four cancellation stages, twenty-slot selection, inheritance, upgrade, synergy, movement, body loss, and elite reward."); // 전체 성공 기록
        }

        private void Prepare() // 실제 씬과 시험 재료 준비
        {
            movement = GetComponent<SnakeMovement>(); // 플레이어 이동 조회
            stamina = GetComponent<SnakeStamina>(); // 가속 구성 조회
            input = GetComponent<PlayerInputReader>(); // 입력 읽기 구성
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 무기 관리자 조회
            synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 시너지 관리자 조회
            merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 합성 처리기 조회
            panel = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 합성 화면 조회
            elite = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 엘리트 보상 조회
            Require(movement != null && stamina != null && input != null && body != null && weapons != null && synergy != null && merge != null && panel != null && elite != null, "Required scene components are missing."); // 씬 연결 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 출현기 존재 확인
            {
                spawner.enabled = false; // 통제 시험 중 적 출현 중지
            }
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                enemy.gameObject.SetActive(false); // 통제 시험 중 기존 적 비활성화
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 자기 충돌 조회
            if (self != null) // 충돌 구성 확인
            {
                self.enabled = false; // 시험용 대량 성장 중 충돌 방지
            }
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 외부 충돌 조회
            if (external != null) // 충돌 구성 확인
            {
                external.enabled = false; // 통제 시험 중 외부 피해 방지
            }
            weapon = weapons.Slots[0].Weapon; // 씬 기본 무기 조회
            Require(weapon != null && weapon.MaxGrade >= 2, "Scene starting weapon cannot be merged."); // 시험 무기 확인
            while (body.CurrentBodyCount < 20) // 최대 슬롯 준비
            {
                Require(body.TryGainBodyFromLevelUp(), "Could not prepare twenty body slots."); // 직접 성장 적용
            }
            Require(weapons.SlotCount == 20, "Weapon slots did not follow body growth."); // 슬롯 동기화 확인
            originalSettings = InputSystem.settings; // 기존 입력 설정 저장
            testSettings = UnityEngine.Object.Instantiate(originalSettings); // 입력 설정 복사
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 설정 파일 저장 방지
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 메모리 설정 적용
            originalBackground = Application.runInBackground; // 기존 백그라운드 상태 저장
            Application.runInBackground = true; // 비활성 입력 허용
            keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
            PrepareMaterials(); // 시험 재료 장착
            Debug.Log("[MergeVerification] Actual Game scene; enemy spawning and collisions disabled; body growth and weapons controlled; virtual keyboard and real UI button events."); // 시험 조건 기록
        }

        private void PrepareMaterials() // 첫째와 스무 번째 몸통 무기 준비
        {
            for (int index = 0; index < weapons.SlotCount; index++) // 모든 슬롯 순회
            {
                weapons.Slots[index].Clear(); // 이전 무기 제거
            }
            Require(weapons.TryEquipAt(0, weapon, 1) && weapons.TryEquipAt(19, weapon, 1), "Could not equip controlled materials."); // 실제 장착 경로 사용
            weapons.Slots[0].Equip(weapon, 1, 2, 0, 0); // 첫 몸통 공격 강화 준비
            weapons.Slots[19].Equip(weapon, 1, 0, 3, 0); // 끝 몸통 속도 강화 준비
            synergy.Recalculate(); // 직접 구성 후 속성 집계
            Require(weapons.OccupiedSlotCount == 2 && synergy.GetCount(weapon.Attribute) == 2 && synergy.GetStage(weapon.Attribute) == 2, "Controlled pair or initial synergy mismatch."); // 초기 무기와 시너지 확인
        }

        private IEnumerator VerifyMovement() // 실제 입력과 합성 중 이동 확인
        {
            yield return OpenByKey(); // 실제 E로 합성 열기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A, Key.Space)); // 회전과 가속 입력 유지
            yield return null; // 입력 시스템 갱신
            yield return null; // 게임 입력 처리
            Require(input.TurnInput < -0.5f && input.BoostPressed, "Virtual rotation and boost did not reach gameplay input."); // 입력 전달 확인
            Vector3 start = movement.transform.position; // 이동 시작 위치
            Vector3 direction = movement.transform.up; // 현재 진행 방향
            Quaternion rotation = movement.transform.rotation; // 회전 시작 각도
            float scaledStart = Time.time; // 시작 게임 시간
            float realStart = Time.realtimeSinceStartup; // 시작 실제 시간
            yield return new WaitForSecondsRealtime(0.4f); // 실제 시간 이동 대기
            float realDelta = Time.realtimeSinceStartup - realStart; // 경과 실제 시간
            float scaledDelta = Time.time - scaledStart; // 경과 게임 시간
            Vector3 delta = movement.transform.position - start; // 실제 이동량
            Debug.Log($"[MergeVerification] movement: real={realDelta:F3}, scaled={scaledDelta:F3}, distance={delta.magnitude:F3}, turn={input.TurnInput:F1}, boost={input.BoostPressed}"); // 실제 측정값 기록
            Require(Mathf.Approximately(Time.timeScale, 0.7f) && realDelta > 0f && Mathf.Abs(scaledDelta / realDelta - 0.7f) < 0.12f, "Merge time scale was not approximately 70 percent."); // 실제 시간 배율 확인
            Require(Quaternion.Angle(rotation, movement.transform.rotation) < 0.1f && Vector3.Dot(delta, direction) > 0.1f && Vector3.Cross(delta, direction).magnitude < 0.03f, "Merge movement did not remain straight."); // 방향 유지와 직진 확인
            Require(stamina.IsBoostBlocked && !stamina.IsBoosting, "Boost remained active during merging."); // 가속 차단 확인
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 이동 입력 해제
            yield return null; // 입력 해제 대기
            yield return KeyTap(Key.Escape); // ESC 취소 입력
            Require(!merge.IsMergeOpen && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "ESC did not cancel merge before pause."); // 취소 우선과 속도 복원
        }

        private IEnumerator VerifyCancellation() // 모든 선택 단계의 취소 보존
        {
            for (int stage = 0; stage < 4; stage++) // 선택 단계 순회
            {
                PrepareMaterials(); // 재료 초기화
                yield return OpenByKey(); // 실제 E 입력
                if (stage >= 1) // 첫 재료 확정 단계
                {
                    Click(1); // 첫 재료 선택
                }
                if (stage >= 2) // 둘째 재료 확정 단계
                {
                    Click(1); // 둘째 재료 선택
                }
                if (stage >= 3) // 계승 확정 단계
                {
                    Click(0); // 첫 재료 강화 계승
                }
                yield return KeyTap(Key.Escape); // 실제 ESC 입력
                Require(!merge.IsMergeOpen && GameManager.Instance.IsPlaying && weapons.OccupiedSlotCount == 2 && weapons.Slots[0].DamageUpgrades == 2 && weapons.Slots[19].SpeedUpgrades == 3, "Cancellation changed materials or paused the game at stage " + stage); // 무기 보존과 일시정지 방지
            }
        }

        private void VerifyInvalidPairs() // 같은 무기 및 등급 제한 확인
        {
            PrepareMaterials(); // 재료 초기화
            weapons.TryEquipAt(19, weapon, 2); // 다른 등급 설정
            Require(!merge.HasMergeCandidates(), "Different grades were accepted as a pair."); // 등급 불일치 차단
            WeaponData other = UnityEngine.Object.Instantiate(weapon); // 다른 무기 객체 생성
            weapons.TryEquipAt(19, other, 1); // 다른 무기 설정
            Require(!merge.HasMergeCandidates(), "Different weapons were accepted as a pair."); // 무기 불일치 차단
            weapons.TryEquipAt(0, weapon, weapon.MaxGrade); // 첫 무기 최대 등급
            weapons.TryEquipAt(19, weapon, weapon.MaxGrade); // 둘째 무기 최대 등급
            Require(!merge.HasMergeCandidates(), "Maximum grade pair was accepted."); // 최대 등급 차단
            UnityEngine.Object.Destroy(other); // 시험 무기 제거
        }

        private IEnumerator VerifySynergyPreview() // 시너지 표시와 취소 검증
        {
            int[] beforeStages = { 2, 2, 4, 4, 6, 6, 8, 8 }; // 개수별 현재 단계
            int[] afterStages = { 0, 2, 2, 4, 4, 6, 6, 8 }; // 개수별 예상 단계
            for (int count = 2; count <= 9; count++) // 단계 경계와 유지 구간 순회
            {
                int before = beforeStages[count - 2]; // 현재 기대 단계
                int after = afterStages[count - 2]; // 합성 후 기대 단계
                PrepareAttributeCount(count, before); // 속성 무기 준비
                yield return OpenByKey(); // 합성 화면 열기
                VerifyDisplayedSynergy(count, before, after); // 첫 재료 화면 표시
                Click(1); // 첫 재료 확정
                VerifyDisplayedSynergy(count, before, after); // 둘째 재료 화면 표시
                Click(1); // 둘째 재료 확정
                VerifyDisplayedSynergy(count, before, after); // 강화 계승 화면 표시
                Click(0); // 첫 재료 강화 계승
                VerifyDisplayedSynergy(count, before, after); // 공통 강화 화면 표시
                Require(synergy.GetCount(weapon.Attribute) == count && synergy.GetStage(weapon.Attribute) == before, "Preview changed live synergy before confirmation."); // 미확정 시너지 불변
                yield return KeyTap(Key.Escape); // 합성 취소 입력
                Require(!merge.IsMergeOpen && weapons.OccupiedSlotCount == count && synergy.GetCount(weapon.Attribute) == count && synergy.GetStage(weapon.Attribute) == before, "Cancellation changed equipment or synergy at count " + count); // 무기와 시너지 유지
            }
            PrepareAttributeCount(4, 4); // 4단계 경계 재구성
            yield return OpenByKey(); // 합성 화면 열기
            Click(1); // 첫 재료 확정
            Click(1); // 둘째 재료 확정
            Click(0); // 첫 재료 강화 계승
            VerifyDisplayedSynergy(4, 4, 2); // 최종 예상 단계 확인
            Click(0); // 실제 합성 확정
            Require(!merge.IsMergeOpen && synergy.GetCount(weapon.Attribute) == 3 && synergy.GetStage(weapon.Attribute) == 2, "Actual post-merge synergy did not match its preview."); // 실제 단계와 예상 비교
            Debug.Log("[MergeVerification] Synergy preview verified for counts 2-9 across all selection stages; cancel preserved state; 4->3 confirmed."); // 임계치 검증 기록
        }

        private void PrepareAttributeCount(int count, int stage) // 같은 속성 무기 개수 준비
        {
            for (int index = 0; index < weapons.SlotCount; index++) // 모든 슬롯 순회
            {
                weapons.Slots[index].Clear(); // 이전 무기 제거
            }
            for (int index = 0; index < count; index++) // 시험 무기 개수 순회
            {
                Require(weapons.TryEquipAt(index, weapon, 1), "Could not equip preview boundary material."); // 실제 장착 경로 사용
            }
            synergy.Recalculate(); // 직접 초기화 후 속성 집계
            Require(synergy.GetCount(weapon.Attribute) == count && synergy.GetStage(weapon.Attribute) == stage, "Initial preview boundary mismatch: " + count); // 경계 상태 확인
        }

        private void VerifyDisplayedSynergy(int count, int before, int after) // 실제 상태 문구 검증
        {
            Text state = (Text)typeof(WeaponMergePanelController).GetField("stateText", PrivateInstance).GetValue(panel); // 실제 UI 문구 조회
            string currentStage = before == 0 ? "미활성" : before + "단계"; // 현재 단계 문구
            string nextStage = after == 0 ? "미활성" : after + "단계"; // 예상 단계 문구
            string expected = $"{WeaponDisplayNames.Attribute(weapon.Attribute)}: {count} → {count - 1} | {currentStage} → {nextStage}"; // 실제 표시 기대값
            Require(state != null && state.text.Contains(expected), $"Incorrect synergy preview: expected '{expected}', actual '{(state == null ? "<null>" : state.text)}'."); // 표시 속성 및 수치 확인
            Canvas.ForceUpdateCanvases(); // 실제 UI 크기 갱신
            float preferredWidth = state.preferredWidth; // 전체 문구 필요 너비
            float availableWidth = state.rectTransform.rect.width; // 상태 문구 영역 너비
            float preferredHeight = state.preferredHeight; // 전체 문구 필요 높이
            float availableHeight = state.rectTransform.rect.height; // 상태 문구 영역 높이
            if (count == 9) // 대표 경계 문구 확인
            {
                Debug.Log($"[MergeVerification] preview text bounds: needed={preferredWidth:F2}x{preferredHeight:F2}, available={availableWidth:F2}x{availableHeight:F2}"); // 실제 UI 크기 기록
            }
            if (preferredWidth > 0.01f) // 너비 측정 가능 확인
            {
                Require(preferredWidth <= availableWidth + 0.5f, $"Synergy preview may wrap or be clipped: preferred={preferredWidth:F2}, available={availableWidth:F2}."); // 한 줄 너비 확인
            }
            if (preferredHeight > 0.01f && state.verticalOverflow == VerticalWrapMode.Truncate) // 측정 가능 및 잘림 설정
            {
                Require(preferredHeight <= availableHeight + 0.5f, $"Synergy preview may be vertically clipped: preferred={preferredHeight:F2}, available={availableHeight:F2}."); // 전체 문구 높이 확인
            }
            CheckMergeLabelsFit(); // 현재 선택 버튼 문구 높이 확인
        }

        private void CheckMergeLabelsFit() // 실제 버튼 문구 높이 확인
        {
            string[] names = { "candidateLabel01", "candidateLabel02", "candidateLabel03" }; // 버튼 문구 필드
            foreach (string name in names) // 모든 버튼 문구 순회
            {
                Text label = (Text)typeof(WeaponMergePanelController).GetField(name, PrivateInstance).GetValue(panel); // 실제 문구 조회
                if (label == null || !label.gameObject.activeInHierarchy) // 표시 중인 문구 확인
                {
                    continue; // 숨긴 문구 제외
                }
                Canvas.ForceUpdateCanvases(); // 실제 UI 크기 갱신
                float needed = label.preferredHeight; // 문구 필요 높이
                float available = label.rectTransform.rect.height; // 실제 문구 높이
                if (needed > 0.01f && label.verticalOverflow == VerticalWrapMode.Truncate) // 측정 가능 및 잘림 설정
                {
                    Require(needed <= available + 0.5f, $"Merge button text may be clipped: {name}, preferred={needed:F2}, available={available:F2}."); // 문구 높이 비교
                }
            }
        }

        private IEnumerator VerifyMixedAttributes() // 서로 다른 속성 선택과 표시 갱신
        {
            WeaponData original = weapon; // 기존 시험 무기 보존
            WeaponData other = UnityEngine.Object.Instantiate(original); // 서로 다른 시험 무기 생성
            WeaponAttribute otherAttribute = original.Attribute == WeaponAttribute.Explosion ? WeaponAttribute.Physical : WeaponAttribute.Explosion; // 다른 속성 선택
            typeof(WeaponData).GetField("attribute", PrivateInstance).SetValue(other, otherAttribute); // 복사 무기 속성 설정
            for (int index = 0; index < weapons.SlotCount; index++) // 모든 슬롯 순회
            {
                weapons.Slots[index].Clear(); // 기존 무기 초기화
            }
            Require(weapons.TryEquipAt(0, original) && weapons.TryEquipAt(1, original), "Original attribute pair could not be equipped."); // 기존 속성 재료 장착
            Require(weapons.TryEquipAt(2, other) && weapons.TryEquipAt(3, other) && weapons.TryEquipAt(4, other), "Other attribute pair could not be equipped."); // 다른 속성 재료 장착
            synergy.Recalculate(); // 모든 속성 집계
            yield return OpenByKey(); // 합성 화면 열기
            VerifyDisplayedSynergy(2, 2, 0); // 첫 속성 표시 확인
            Click(2); // 같은 속성 다음 재료 이동
            Click(2); // 다른 속성 재료 이동
            weapon = other; // 표시 확인용 대상 전환
            VerifyDisplayedSynergy(3, 2, 2); // 새 속성 수와 단계 표시
            Click(1); // 새 속성 첫 재료 선택
            VerifyDisplayedSynergy(3, 2, 2); // 둘째 재료 화면 표시
            Click(1); // 새 속성 둘째 재료 선택
            VerifyDisplayedSynergy(3, 2, 2); // 강화 계승 화면 표시
            Click(0); // 첫 재료 강화 계승
            VerifyDisplayedSynergy(3, 2, 2); // 공통 강화 화면 표시
            yield return KeyTap(Key.Escape); // 합성 취소 입력
            Require(synergy.GetCount(original.Attribute) == 2 && synergy.GetCount(otherAttribute) == 3, "Attribute switching or cancellation changed actual counts."); // 두 속성 보존 확인
            weapon = original; // 원래 시험 무기 복원
            PrepareMaterials(); // 복사 무기 참조 제거
            UnityEngine.Object.Destroy(other); // 임시 무기 객체 제거
        }

        private IEnumerator VerifyEliteReward() // 엘리트 합성 보상 취소와 소비
        {
            elite.QueueReward(); // 시험 보상 한 개 주입
            typeof(EliteRewardController).GetField("candidates", PrivateInstance).SetValue(elite, new int[] { 3, 2, 0 }); // 합성 후보 고정
            Require(elite.ProcessPendingReward() && elite.TrySelectCandidate(0), "Could not start controlled elite merge reward."); // 실제 보상 합성 진입
            Require(elite.IsAwaitingMerge && merge.IsMergeOpen && elite.PendingRewardCount == 1, "Elite merge state mismatch."); // 보상 대기 확인
            yield return KeyTap(Key.Escape); // 합성 취소
            Require(!elite.IsAwaitingMerge && elite.PendingRewardCount == 1, "Cancelled elite merge consumed its reward."); // 보상 보존 확인
            if (!elite.IsPresentingReward) // 자동 재표시 여부
            {
                Require(elite.ProcessPendingReward(), "Cancelled reward was not available again."); // 보상 재표시
            }
            int index = -1; // 합성 후보 위치
            for (int candidate = 0; candidate < elite.Candidates.Count; candidate++) // 실제 후보 순회
            {
                if (elite.Candidates[candidate] == 3) // 합성 후보 확인
                {
                    index = candidate; // 합성 위치 기록
                }
            }
            Require(index >= 0 && elite.TrySelectCandidate(index), "Could not restart elite merge reward."); // 보상 합성 재진입
            Click(1); // 첫 재료 선택
            Click(1); // 둘째 재료 선택
            Click(0); // 첫 강화 내역 계승
            Click(0); // 공격력 강화 선택
            Require(!elite.IsAwaitingMerge && elite.PendingRewardCount == 0 && !merge.IsMergeOpen, "Successful elite merge did not consume its reward."); // 성공 보상 소비 확인
            Require(weapons.Slots[0].Weapon == weapon && weapons.Slots[0].Grade == 2 && weapons.Slots[0].DamageUpgrades == 3 && weapons.Slots[0].SpeedUpgrades == 0, "First material inheritance or damage upgrade mismatch."); // 첫 강화 계승과 신규 공격 강화
        }

        private void VerifyLevelUpInterruption() // 실제 경험치 레벨업과 합성 중단
        {
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 조회
            SnakeLevelUpController level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기 조회
            LevelUpPanelController reward = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 보상 화면 조회
            Require(experience != null && level != null && reward != null, "Level-up components are missing."); // 레벨업 구성 확인
            experience.AddExperience(Mathf.Max(1, experience.RequiredExperience - experience.CurrentExperience)); // 시험 경험치 주입
            Require(!merge.IsMergeOpen && level.IsPresentingLevelUp && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Level-up did not cancel merge and take pause priority."); // 합성 취소와 정지 우선 확인
            Button button = (Button)typeof(LevelUpPanelController).GetField("candidateButton01", PrivateInstance).GetValue(reward); // 실제 보상 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Level-up reward button is unavailable."); // 선택 가능 확인
            button.onClick.Invoke(); // 실제 보상 버튼 이벤트
            Require(!level.IsPresentingLevelUp && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "Level-up did not restore normal speed after selection."); // 보상 후 정상 속도 복원
        }

        private IEnumerator OpenByKey() // 실제 E 입력 합성 열기
        {
            yield return KeyTap(Key.E); // 실제 합성 키 입력
            Require(merge.IsMergeOpen && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 0.7f), "E did not open merge at 70 percent speed."); // 합성 시작 확인
        }

        private IEnumerator KeyTap(Key key) // 입력 장치 키 누름과 해제
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 키 누름 전달
            yield return null; // 게임 입력 대기
            yield return null; // 입력 처리 대기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 키 해제 전달
            yield return null; // 키 해제 대기
            yield return null; // 후속 프레임 대기
        }

        private void Click(int index) // 실제 합성 UI 버튼 이벤트
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 실제 버튼 필드
            Button button = (Button)typeof(WeaponMergePanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 직렬화 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Merge button unavailable: " + index); // 사용 가능 확인
            button.onClick.Invoke(); // 실제 버튼 이벤트 실행
        }

        private string Label(int index) // 실제 합성 버튼 문구 조회
        {
            string[] names = { "candidateLabel01", "candidateLabel02", "candidateLabel03" }; // 실제 문구 필드
            Text label = (Text)typeof(WeaponMergePanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 문구 조회
            Require(label != null, "Merge label missing: " + index); // 문구 연결 확인
            return label.text; // 현재 표시 문구 반환
        }

        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedMergeSelectionVerification.Finish(1, message); // 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }

        private void Cleanup() // 시험 입력 설정 복구
        {
            InputSystem.RemoveDevice(keyboard); // 가상 키보드 제거
            if (originalSettings != null) // 원래 설정 존재 확인
            {
                InputSystem.settings = originalSettings; // 기존 입력 설정 복원
            }
            Application.runInBackground = originalBackground; // 기존 백그라운드 상태 복원
            if (testSettings != null && InputSystem.settings != testSettings) // 현재 설정과 사본 구분
            {
                UnityEngine.Object.Destroy(testSettings); // 사용을 마친 설정 사본 제거
            }
        }
    }
}
