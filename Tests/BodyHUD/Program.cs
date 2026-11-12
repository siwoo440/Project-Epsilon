using System; // 기본 자료형
using System.Reflection; // 생명 주기 호출
using ProjectEpsilon.Player; // 몸통 상태
using ProjectEpsilon.UI; // 실제 표시 코드
using UnityEngine.UI; // 텍스트 대체

public static class Program // 몸통 HUD 검증기
{
    public static int Main() // 검증 시작
    {
        try // 실패 처리
        {
            Text label = new Text(); // 몸통 표시 문구
            HUDController hud = new HUDController(); // 실제 HUD 관리자
            hud.Bind(null, label, null, null, null); // 몸통 문구 연결
            SnakeBodyManager body = new SnakeBodyManager(); // 시험용 몸통 상태
            SnakeBodyHUDPresenter presenter = new SnakeBodyHUDPresenter(); // 실제 표시 제어기
            presenter.Bind(body, hud); // 몸통과 HUD 연결
            Require(label.text == "몸통 3 / 3 (최대 20)", "초기 현재·성장·절대 최대 표시 오류"); // 초기 표시 확인
            body.Change(2, 3); // 몸통 손실
            Require(label.text == "몸통 2 / 3 (최대 20)", "몸통 손실 표시 오류"); // 손실 시 상한 유지 확인
            body.Change(3, 4); // 손실 후 레벨 성장
            Require(label.text == "몸통 3 / 4 (최대 20)", "레벨 성장 표시 오류"); // 성장 시 두 값 증가 확인
            body.Change(4, 4); // 몸통 복구
            Require(label.text == "몸통 4 / 4 (최대 20)", "몸통 복구 표시 오류"); // 복구 시 성장 최대 유지 확인
            body.Change(19, 20); // 절대 상한에서 몸통 손실
            Require(label.text == "몸통 19 / 20 (최대 20)", "절대 상한 미복구 표시 오류"); // 두 자리 상한 표시 확인
            body.Change(20, 20); // 절대 상한까지 복구
            Require(label.text == "몸통 20 / 20 (최대 20)", "절대 상한 표시 오류"); // 두 자리 최대 표시 확인
            Invoke(presenter, "OnDisable"); // 표시 제어기 비활성화
            body.Change(18, 20); // 비활성 중 몸통 손실
            Invoke(presenter, "OnEnable"); // 표시 제어기 재활성화
            Require(label.text == "몸통 18 / 20 (최대 20)", "재활성화 시 현재 몸통 미반영"); // 재구독 시 상태 갱신 확인
            hud.SetBodyCount(3, 20); // 기존 두 인자 표시 호출
            Require(label.text == "몸통 3 / 20", "기존 HUD API 호환 오류"); // 기존 API 확인
            Require(HUDTextFormatter.FormatBodyCount(3, 20) == "몸통 3 / 20", "기존 포매터 API 호환 오류"); // 기존 포매터 확인
            Console.WriteLine("몸통 HUD growth and recovery passed."); // 성공 기록
            return 0; // 성공 종료
        }
        catch (Exception exception) // 실패 처리
        {
            Console.Error.WriteLine(exception.GetBaseException().Message); // 실패 내용 출력
            return 1; // 실패 종료
        }
    }
    private static void Invoke(SnakeBodyHUDPresenter presenter, string method) // 생명 주기 실행
    {
        typeof(SnakeBodyHUDPresenter).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(presenter, null); // 실제 메서드 호출
    }
    private static void Require(bool condition, string message) // 검증 조건
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 보고
        }
    }
}
