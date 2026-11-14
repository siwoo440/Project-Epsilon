using System; // 실제 예외 처리
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 시험 수치 설정
using ProjectEpsilon.Combat; // 기존 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 기존 적 출현기
using ProjectEpsilon.Player; // 실제 뱀과 체력
using ProjectEpsilon.Progression; // 독립 시험 맵 해금
using ProjectEpsilon.World; // 실제 맵 위험
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.UI; // 실제 예고 게이지

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay86WorldEventsPlayVerification // 실제 맵 위험 동작 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay86WorldEventsPlay"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        private static bool mapPrepared; // 실제 시험 맵 선택 완료
        static AutomatedDay86WorldEventsPlayVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            int selected = 0; // 기본 평원 시험
            string[] args = Environment.GetCommandLineArgs(); // 실제 배치 인수
            for (int index = 0; index + 1 < args.Length; index++) // 실제 인수 순회
            { // 반복 시작
                if (args[index] == "-epsilonMapIndex") // 실제 시험 맵 인수 확인
                { // 조건 시작
                    int.TryParse(args[index + 1], out selected); // 실제 맵 번호 저장
                } // 조건 종료
            } // 반복 종료
            SessionState.SetInt(Key + "Map", selected); // 도메인 갱신 간 맵 번호 보존
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started) // 실제 플레이와 중복 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            if (!mapPrepared) // 실제 시험 맵 선택 확인
            { // 조건 시작
                PrototypeMapSelection selection = UnityEngine.Object.FindFirstObjectByType<PrototypeMapSelection>(); // 실제 맵 선택기
                MetaProgressionController meta = MetaProgressionController.Instance; // 독립 시험 해금 관리자
                if (selection == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 선택 준비 확인
                { // 조건 시작
                    return; // 시작 준비 대기
                } // 조건 종료
                int selected = SessionState.GetInt(Key + "Map", 0); // 실제 선택 맵 번호
                if (selected > 0) // 추가 맵 시험 확인
                { // 조건 시작
                    if (!Application.productName.EndsWith("-Isolated", StringComparison.Ordinal) || meta == null || meta.Service == null) // 독립 저장 확인
                    { // 조건 시작
                        Finish(false, "독립 시험 저장 누락"); // 실제 저장 실패 기록
                        return; // 실제 전투 시작 중단
                    } // 조건 종료
                    if (!meta.IsMapUnlocked(1)) // 폐허 해금 상태 확인
                    { // 조건 시작
                        meta.Service.TryCompleteRun("day86-plains-unlock", 0d, 0, 0, true, 0); // 독립 저장의 평원 승리
                    } // 조건 종료
                    if (!meta.IsMapUnlocked(2)) // 습지 해금 상태 확인
                    { // 조건 시작
                        meta.Service.TryCompleteRun("day86-ruins-unlock", 0d, 0, 0, true, 1); // 독립 저장의 폐허 승리
                    } // 조건 종료
                } // 조건 종료
                if (!selection.SelectMap(selected)) // 실제 선택 맵 적용 확인
                { // 조건 시작
                    Finish(false, "실제 시험 맵 선택 실패"); // 실제 맵 선택 실패 기록
                    return; // 실제 전투 시작 중단
                } // 조건 종료
                mapPrepared = true; // 실제 맵 선택 완료
            } // 조건 종료
            if (StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            new GameObject("Day86WorldEventsRunner").AddComponent<Day86WorldEventsRunner>(); // 실제 프레임 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day86WorldEventsPlayVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료

    public sealed class Day86WorldEventsRunner : MonoBehaviour // 실제 맵 위험 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 경고와 충돌 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 위험 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 뱀 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 뱀 몸통
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 무적
            Require(events != null && map.Profile != null && player != null && body != null && health != null && invulnerability != null, "실제 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 경로 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 시험 중 몸통 이동 중지
            } // 조건 종료
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            spawner.enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 기존 지역의 시험 간섭 방지
            } // 반복 종료
            foreach (EarlyEnemyBehaviour enemy in FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 기존 적의 시험 간섭 방지
            } // 반복 종료
            SetFloat(map.Profile, "worldEventWarning", 0.6f); // 빠른 시험용 예고 시간
            SetFloat(map.Profile, "locustSpeed", 20f); // 빠른 시험용 황충 속도
            typeof(PrototypeMapProfile).GetField("locustMinimumWaves", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map.Profile, 1); // 기존 단일 대열 접촉 대조
            typeof(PrototypeMapProfile).GetField("locustMaximumWaves", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map.Profile, 1); // 기존 단일 대열 접촉 대조
            SetFloat(map.Profile, "rockfallDuration", 0.4f); // 빠른 시험용 낙석 하강
            health.ResetHealth(); // 실제 체력 복구
            invulnerability.ClearInvulnerability(); // 기존 무적 제거
            int originalHealth = health.CurrentHealth; // 실제 시작 체력
            Require(events.TryBeginLocust() && events.IsWarning && !events.IsActive, "황충떼 예고 시작 실패"); // 실제 황충 예고 확인
            Transform warningArea = events.transform.Find("WorldHazardVisual/LocustWarningArea"); // 실제 황충 경고 영역
            SpriteRenderer warningFill = warningArea == null ? null : warningArea.GetComponent<SpriteRenderer>(); // 실제 경고 영역 표시기
            Require(warningFill != null && warningFill.color.r > 0.9f && warningFill.color.g > 0.8f && warningFill.color.b > 0.2f && warningFill.color.a > 0f && warningFill.color.a < 0.4f && warningFill.sortingOrder < 25 && warningArea.GetComponent<Collider2D>() == null, "연한 노란색 비충돌 황충 경고 영역 누락"); // 색상과 전투 요소 가림 방지 확인
            LineRenderer warningOutline = warningArea.parent.GetComponent<LineRenderer>(); // 실제 황충 경고 경계선
            Require(warningOutline != null && Mathf.Abs(warningArea.localScale.x - (36.2f + 2f * map.Profile.LocustTrailLength)) < 0.01f && Mathf.Abs(warningArea.localScale.y - map.Profile.LocustWidth) < 0.01f && Mathf.Abs(Vector3.Distance(warningOutline.GetPosition(0), warningOutline.GetPosition(1)) - warningArea.localScale.x) < 0.01f, "황충 경고 채움과 전체 피해 경계 불일치"); // 실제 대열 범위와 경고 표시 일치 확인
            yield return new WaitForSeconds(0.25f); // 실제 게이지 진행 대기
            GameObject warning = GameObject.Find("WorldHazardWarning"); // 실제 예고판
            Image gauge = warning == null ? null : warning.transform.Find("WarningGauge").GetComponent<Image>(); // 실제 예고 게이지
            Require(gauge != null && gauge.fillAmount > 0f && gauge.fillAmount < 1f && health.CurrentHealth == originalHealth, "황충 경고 게이지 또는 예고 중 무피해 실패"); // 실제 게이지와 무피해 확인
            float beforePause = events.WarningProgress; // 실제 일시정지 전 예고 진행도
            GameManager.Instance.PauseGame(); // 실제 일시정지
            yield return new WaitForSecondsRealtime(0.25f); // 실제 정지 시간 경과
            Require(Mathf.Abs(events.WarningProgress - beforePause) < 0.01f, "일시정지 중 예고 진행"); // 실제 일시정지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(0.45f); // 실제 예고 완료 대기
            Transform swarm = events.transform.Find("WorldHazardVisual/LocustSwarm"); // 실제 황충 무리
            Require(events.IsActive && swarm != null && swarm.childCount == 6, "긴 황충 무리 이동 누락"); // 실제 여러 황충 무리 표시 확인
            Vector3 firstPosition = swarm.GetChild(0).position; // 첫 황충 무리 위치
            Vector3 lastPosition = swarm.GetChild(swarm.childCount - 1).position; // 마지막 황충 무리 위치
            Require(Vector2.Distance(firstPosition, lastPosition) >= 7f, "황충 무리의 긴 대열 누락"); // 실제 긴 대열 확인
            foreach (SpriteRenderer insect in swarm.GetComponentsInChildren<SpriteRenderer>()) // 실제 황충 이미지 순회
            { // 반복 시작
                Require(insect.sprite != null && insect.sprite.name == "LocustSwarm_Long", "긴 황충 이미지 누락"); // 실제 생성 이미지 확인
            } // 반복 종료
            yield return new WaitForSeconds(0.9f); // 실제 황충떼 교차 대기
            Require(health.CurrentHealth == originalHealth - map.Profile.LocustDamage, "황충떼 공용 체력 피해 오류"); // 실제 단일 피해 확인
            yield return new WaitForSeconds(1.2f); // 실제 황충 이동 종료 대기
            Require(!events.IsActive && !events.IsWarning && health.CurrentHealth == originalHealth - map.Profile.LocustDamage, "황충 이동 종료 또는 중복 피해 오류"); // 실제 종료와 단일 피해 확인
            invulnerability.ClearInvulnerability(); // 뒤쪽 대열 피해 시험 준비
            Vector3 originalPosition = player.transform.position; // 실제 머리의 원래 위치
            Collider2D[] bodyColliders = body.GetComponentsInChildren<Collider2D>(); // 실제 몸통 충돌 범위 목록
            bool[] bodyColliderStates = new bool[bodyColliders.Length]; // 기존 몸통 충돌 활성 상태
            for (int index = 0; index < bodyColliders.Length; index++) // 실제 몸통 충돌 범위 순회
            { // 반복 시작
                bodyColliderStates[index] = bodyColliders[index].enabled; // 기존 충돌 활성 상태 보관
                bodyColliders[index].enabled = false; // 뒤쪽 대열의 머리 접촉만 확인
            } // 반복 종료
            Require(events.TryBeginLocust(), "뒤쪽 대열 시험 예고 실패"); // 실제 뒤쪽 대열 위험 시작
            Vector3 travel = (Vector3)typeof(WorldEventController).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 황충 진행 방향
            player.transform.position = originalPosition + new Vector3(-travel.y, travel.x, 0f) * 40f; // 경고 영역 밖으로 머리 이동
            Physics2D.SyncTransforms(); // 실제 머리 충돌 위치 갱신
            yield return new WaitForSeconds(0.7f); // 실제 황충 출발 대기
            Transform trailingSwarm = events.transform.Find("WorldHazardVisual/LocustSwarm"); // 실제 이동 중인 황충 무리
            Require(trailingSwarm != null, "뒤쪽 대열 이동 누락"); // 실제 황충 이동 확인
            while (events.IsActive && Vector3.Dot(trailingSwarm.position - originalPosition, travel) < 4f) // 앞쪽 무리의 통과 확인
            { // 반복 시작
                yield return new WaitForFixedUpdate(); // 실제 물리 이동 대기
            } // 반복 종료
            Require(events.IsActive && health.CurrentHealth == originalHealth - map.Profile.LocustDamage, "뒤쪽 대열 시험 중 예상 밖 피해"); // 영역 밖 무피해 확인
            player.transform.position = originalPosition; // 뒤쪽 무리 영역으로 머리 진입
            Physics2D.SyncTransforms(); // 실제 머리 충돌 위치 갱신
            yield return new WaitForSeconds(0.15f); // 실제 뒤쪽 대열 접촉 판정 대기
            Require(health.CurrentHealth == originalHealth - map.Profile.LocustDamage * 2, "뒤쪽 황충 무리 접촉 피해 누락"); // 앞쪽 통과 후 뒤쪽 대열 피해 확인
            yield return new WaitForSeconds(1f); // 실제 긴 무리의 통과 완료 대기
            for (int index = 0; index < bodyColliders.Length; index++) // 실제 몸통 충돌 범위 순회
            { // 반복 시작
                bodyColliders[index].enabled = bodyColliderStates[index]; // 기존 충돌 활성 상태 복구
            } // 반복 종료
            int protectedHealth = health.CurrentHealth; // 실제 무적 전 체력
            invulnerability.StartInvulnerability(4f); // 기존 공통 무적 시작
            Require(events.TryBeginLocust(), "무적 중 황충 예고 실패"); // 실제 두 번째 황충 예고
            yield return new WaitForSeconds(3f); // 실제 황충 예고와 통과 대기
            Require(!events.IsActive && !events.IsWarning && health.CurrentHealth == protectedHealth, "황충떼가 기존 무적을 무시"); // 실제 공통 무적 보존 확인
            invulnerability.ClearInvulnerability(); // 다음 위험의 무적 해제
            int beforeRocks = map.GetComponentsInChildren<SnakeContactHazard>().Length; // 기존 장애물 수
            Require(events.TryBeginRockfall() && events.IsWarning, "안전 낙석 예고 시작 실패"); // 실제 낙석 예고 확인
            Vector3 impact = (Vector3)typeof(WorldEventController).GetField("impact", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 착탄 위치
            Camera camera = Camera.main; // 실제 전투 카메라
            Vector3 viewport = camera == null ? Vector3.zero : camera.WorldToViewportPoint(impact); // 실제 착탄 예고 화면 좌표
            Require(camera != null && viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f, "낙석 예고 중심이 전투 화면 밖에 위치"); // 실제 예고 위치의 화면 표시 확인
            yield return new WaitForSeconds(0.7f); // 실제 낙석 예고 완료 대기
            Require(events.IsActive && events.transform.Find("WorldHazardVisual/FallingRock") != null, "낙석 하강 표시 누락"); // 실제 낙석 표시 확인
            yield return new WaitForSeconds(0.55f); // 실제 착탄 완료 대기
            Require(!events.IsActive && map.GetComponentsInChildren<SnakeContactHazard>().Length >= beforeRocks + 3, "안전 위치의 낙석 잔해 세 개 누락"); // 실제 잔해 무더기 확인
            Require(!map.CanPlaceRockfallDebris(impact), "기존 잔해와 중복 생성 허용"); // 실제 잔해 중복 방지 확인
            Require(!map.CanPlaceRockfallDebris(player.transform.position), "플레이어 위치의 낙석 잔해 허용"); // 실제 머리 보호 확인
            foreach (SnakeSegment segment in body.BodySegments) // 실제 뱀 몸통 순회
            { // 반복 시작
                Require(!map.CanPlaceRockfallDebris(segment.transform.position), "몸통 위치의 낙석 잔해 허용"); // 실제 몸통 보호 확인
            } // 반복 종료
            Require(body.TailSegment == null || !map.CanPlaceRockfallDebris(body.TailSegment.transform.position), "꼬리 위치의 낙석 잔해 허용"); // 실제 꼬리 보호 확인
            Vector2Int coordinate = new Vector2Int(Mathf.FloorToInt(impact.x / map.ChunkSize), Mathf.FloorToInt(impact.y / map.ChunkSize)); // 실제 착탄 청크 좌표
            GameObject chunk = map.GetChunk(coordinate); // 실제 착탄 청크
            Require(chunk != null && !map.CanPlaceRockfallDebris(chunk.transform.position + new Vector3(map.ChunkSize * 0.5f, map.ChunkSize * 0.25f, 0f)), "중앙 안전 통로의 잔해 허용"); // 실제 연결 통로 보호 확인
            foreach (SnakeContactHazard obstacle in map.GetComponentsInChildren<SnakeContactHazard>()) // 첫 착탄의 실제 잔해 순회
            { // 반복 시작
                if (Vector2.Distance(obstacle.transform.position, impact) <= 1.5f) // 첫 시험 잔해 확인
                { // 조건 시작
                    obstacle.gameObject.SetActive(false); // 다음 시험의 안전 위치 복원
                } // 조건 종료
            } // 반복 종료
            Require(events.TryBeginRockfall(), "두 번째 안전 낙석 예고 실패"); // 실제 두 번째 낙석 예고
            Vector3 secondImpact = (Vector3)typeof(WorldEventController).GetField("impact", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 두 번째 착탄 위치
            player.transform.position = secondImpact; // 예고 후 실제 착탄 지역 진입
            Physics2D.SyncTransforms(); // 실제 머리 충돌 위치 갱신
            int beforeImpact = health.CurrentHealth; // 실제 착탄 전 체력
            yield return new WaitForSeconds(1.2f); // 실제 낙석 예고와 착탄 대기
            Require(health.CurrentHealth == beforeImpact - map.Profile.RockfallDamage, "실제 낙석 착탄 피해 오류"); // 실제 공용 체력 피해 확인
            foreach (SnakeContactHazard obstacle in map.GetComponentsInChildren<SnakeContactHazard>()) // 실제 생성 장애물 순회
            { // 반복 시작
                Require(Vector2.Distance(obstacle.transform.position, secondImpact) >= 1.3f, "착탄 시 머리 위에 잔해 생성"); // 실제 착탄 순간 머리 보호 확인
            } // 반복 종료
            Require(events.TryBeginLocust(), "보스 전환 전 황충 예고 실패"); // 실제 보스 전환 전 예고
            MethodInfo transition = typeof(EarlyEnemyWaveSpawner).GetMethod("EnterBossTransition", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 전환 경로
            Require(transition != null, "보스 전환 경로 누락"); // 실제 보스 전환 확인
            transition.Invoke(spawner, null); // 실제 보스 전환 시작
            yield return null; // 실제 보스와 경고 정리 대기
            Require(GameManager.Instance.IsFinalBossPhase && !events.IsWarning && !events.IsActive && !events.TryBeginLocust(), "보스전 중 맵 위험과 경고가 유지됨"); // 실제 보스 경고와 출구 보호
            AutomatedDay86WorldEventsPlayVerification.Finish(true, "Warning, pause, long locust swarm, invulnerability, rock damage, safe debris and boss transition passed."); // 실제 전체 결과
        } // 함수 종료
        private static void SetFloat(PrototypeMapProfile profile, string name, float value) // 독립 시험 수치 적용
        { // 함수 시작
            FieldInfo field = typeof(PrototypeMapProfile).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 수치 필드
            Require(field != null, "맵 위험 수치 누락: " + name); // 실제 데이터 확인
            field.SetValue(profile, value); // 메모리의 시험값 적용
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay86WorldEventsPlayVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
