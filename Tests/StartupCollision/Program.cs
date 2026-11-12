using System; // 기본 형식
using System.Reflection; // 초기화 순서 조회
using ProjectEpsilon.Player; // 실제 플레이어 코드
using ProjectEpsilon.UI; // 실제 HUD 코드
using UnityEngine; // 유니티 대체 형식
using UnityEngine.UI; // UI 문구 대체

public static class Program // 시작 충돌 검증
{
    public static int Main() // 검증 실행
    {
        GameObject head = new GameObject("Player"); // 머리 객체
        head.transform.localScale = new Vector3(0.72f, 0.72f, 1f); // 씬 머리 크기
        SnakePathRecorder recorder = head.AddComponent<SnakePathRecorder>(); // 실제 경로 기록기
        GameObject root = new GameObject("SnakeBody"); // 몸통 부모
        SnakeBodyFollower follower = root.AddComponent<SnakeBodyFollower>(); // 실제 몸통 추적기
        SnakeBodyManager bodies = root.AddComponent<SnakeBodyManager>(); // 실제 몸통 관리자
        for (int index = 0; index < 3; index++) // 씬 몸통 생성
        {
            GameObject segment = new GameObject($"Body_{index + 1:00}"); // 시작 몸통 객체
            segment.transform.SetParent(root.transform, false); // 몸통 부모 연결
            segment.transform.localScale = new Vector3(0.62f, 0.62f, 1f); // 씬 몸통 크기
            segment.AddComponent<SnakeSegment>(); // 실제 몸통 구성
        }
        SetField(bodies, "pathRecorder", recorder); // 경로 기록기 연결
        SetField(bodies, "bodyFollower", follower); // 몸통 추적기 연결
        int recorderOrder = Order(typeof(SnakePathRecorder)); // 경로 초기화 순서
        int bodyOrder = Order(typeof(SnakeBodyManager)); // 몸통 초기화 순서
        if (recorderOrder < bodyOrder) // 경로 우선 초기화 확인
        {
            Awake(recorder); // 경로 초기화
            Awake(bodies); // 몸통 초기화
        }
        else // 몸통 우선 초기화
        {
            Awake(bodies); // 몸통 초기화
            Awake(recorder); // 경로 초기화
        }
        float minimumDistance = 0.32f * 0.72f + 0.36f * 0.62f; // 씬 충돌 반경 합
        if (!BodiesClearHead(bodies, head, minimumDistance, bodyOrder, recorderOrder)) // 시작 겹침 확인
        {
            return 1; // 실패 종료
        }
        if (bodies.CurrentBodyCount != 3 || !bodies.TryGainBodyFromLevelUp() || bodies.CurrentBodyCount != 4) // 몸통 성장 확인
        {
            Console.Error.WriteLine("Starting bodies or growth failed."); // 실패 결과 출력
            return 1; // 실패 종료
        }
        if (!BodiesClearHead(bodies, head, minimumDistance, bodyOrder, recorderOrder)) // 성장 후 겹침 확인
        {
            return 1; // 실패 종료
        }
        if (!EarnedResetNotifies(bodies)) // 성장 최대 초기화 알림 확인
        {
            return 1; // 실패 종료
        }
        Console.WriteLine("Startup body placement and growth passed."); // 성공 결과 출력
        return 0; // 성공 종료
    }

    private static bool EarnedResetNotifies(SnakeBodyManager bodies) // 성장 최대 초기화 검증
    {
        if (!bodies.TryGainBodyFromLevelUp() || !bodies.TryGainBodyFromLevelUp() || bodies.RemoveBodies(3) != 3) // 현재 3칸과 성장 최대 6칸 준비
        {
            Console.Error.WriteLine("Earned reset setup failed."); // 준비 실패 출력
            return false; // 실패 반환
        }
        Application.isPlaying = true; // HUD 이벤트 구독 허용
        GameObject hudObject = new GameObject("BodyHUD"); // 시험용 HUD 객체
        HUDController hud = hudObject.AddComponent<HUDController>(); // 실제 HUD 관리자
        Text label = new Text(); // 몸통 표시 문구
        hud.Bind(null, label, null, null, null); // 몸통 문구 연결
        SnakeBodyHUDPresenter presenter = hudObject.AddComponent<SnakeBodyHUDPresenter>(); // 실제 몸통 표시기
        presenter.Bind(bodies, hud); // 몸통 이벤트 구독
        if (label.text != "몸통 3 / 6 (최대 20)") // 초기 표시 확인
        {
            Console.Error.WriteLine("Earned reset HUD setup failed."); // 준비 실패 출력
            return false; // 실패 반환
        }
        int notifications = 0; // 변경 알림 횟수
        int eventCurrent = -1; // 알림 시 현재 몸통 수
        int eventEarned = -1; // 알림 시 성장 최대 수
        int eventMaximum = -1; // 알림 시 절대 최대 수
        bodies.BodyCountChanged += (current, maximum) => // 몸통 변경 구독
        {
            notifications++; // 알림 횟수 증가
            eventCurrent = current; // 현재 수 저장
            eventEarned = bodies.EarnedBodyCount; // 성장 최대 조회
            eventMaximum = maximum; // 절대 최대 저장
        };
        bodies.InitializeEarnedBodyCountFromCurrent(); // 성장 최대 초기화
        if (notifications != 1 || eventCurrent != 3 || eventEarned != 3 || eventMaximum != 20) // 갱신 및 알림 확인
        {
            Console.Error.WriteLine($"Earned reset notification failed: count={notifications}, current={eventCurrent}, earned={eventEarned}, max={eventMaximum}"); // 실패 결과 출력
            return false; // 실패 반환
        }
        if (label.text != "몸통 3 / 3 (최대 20)") // 실제 HUD 변경 확인
        {
            Console.Error.WriteLine("Earned reset did not update the body HUD."); // 표시 실패 출력
            return false; // 실패 반환
        }
        bodies.InitializeEarnedBodyCountFromCurrent(); // 같은 값으로 재설정
        if (notifications != 1) // 불필요한 중복 알림 확인
        {
            Console.Error.WriteLine("Unchanged earned count produced a duplicate notification."); // 중복 결과 출력
            return false; // 실패 반환
        }
        return true; // 검증 성공
    }

    private static bool BodiesClearHead(SnakeBodyManager bodies, GameObject head, float minimumDistance, int bodyOrder, int recorderOrder) // 몸통 위치 확인
    {
        foreach (SnakeSegment segment in bodies.BodySegments) // 몸통 확인
        {
            float distance = Vector3.Distance(head.transform.position, segment.transform.position); // 머리와 몸통 거리
            if (distance <= minimumDistance) // 초기 겹침 확인
            {
                Console.Error.WriteLine($"Initial body overlap: order {bodyOrder}/{recorderOrder}, distance {distance:0.000}"); // 재현 결과 출력
                return false; // 겹침 발견
            }
        }
        return true; // 겹침 없음
    }

    private static int Order(Type type) // 실행 순서 조회
    {
        return type.GetCustomAttribute<DefaultExecutionOrder>()?.Order ?? 0; // 설정 순서 반환
    }

    private static void Awake(object value) // 실제 초기화 호출
    {
        value.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(value, null); // 비공개 초기화 실행
    }

    private static void SetField(object value, string name, object reference) // 씬 직렬화 연결
    {
        value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(value, reference); // 참조 설정
    }
}
