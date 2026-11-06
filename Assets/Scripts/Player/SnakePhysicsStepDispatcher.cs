using System; // 스텝 완료 이벤트
using System.Collections.Generic; // 실행 순서 목록
using UnityEngine; // 물리 설정과 오류 기록
using UnityEngine.LowLevel; // 실행 순서 제어
using UnityEngine.PlayerLoop; // 물리 단계 형식

namespace ProjectEpsilon.Player // 플레이어 영역
{ // 영역 시작
    internal static class SnakePhysicsStepDispatcher // 물리 스텝 완료 알림
    { // 클래스 시작
        private sealed class AfterPhysics2D // 삽입 단계 표식
        { // 클래스 시작
        } // 클래스 끝
        private static readonly List<Action> listeners = new List<Action>(); // 스텝 완료 수신자
        private static bool reportedFailure; // 설치 실패 기록 여부
        public static int Generation { get; private set; } // 플레이 시작 등록 세대
        public static bool CanQueue => Physics2D.simulationMode == SimulationMode2D.FixedUpdate; // 지원되는 자동 물리 방식

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 플레이 시작 초기화
        private static void ResetState() // 도메인 재사용 대비 초기화
        { // 메서드 시작
            listeners.Clear(); // 이전 수신자 제거
            reportedFailure = false; // 오류 기록 초기화
            Generation = unchecked(Generation + 1); // 리로드 생략 시 재등록 구분
        } // 메서드 끝

        public static bool Register(Action listener) // 물리 스텝 완료 구독
        { // 메서드 시작
            if (listener == null || !Install()) // 구독자와 물리 단계 확인
            { // 조건 시작
                return false; // 설치 실패 반환
            } // 조건 끝
            listeners.Remove(listener); // 중복 구독 제거
            listeners.Add(listener); // 완료 수신자 등록
            return true; // 구독 성공 반환
        } // 메서드 끝

        public static void Unregister(Action listener) // 물리 스텝 구독 해제
        { // 메서드 시작
            listeners.Remove(listener); // 완료 수신자 제거
        } // 메서드 끝

        private static bool Install() // 물리 직후 실행 단계 설치
        { // 메서드 시작
            if (Physics2D.simulationMode != SimulationMode2D.FixedUpdate) // 프로젝트 물리 방식 확인
            { // 조건 시작
                ReportFailure("Physics2D가 FixedUpdate 모드가 아닙니다."); // 지원되지 않는 설정 기록
                return false; // 설치 불가 반환
            } // 조건 끝
            PlayerLoopSystem root = PlayerLoop.GetCurrentPlayerLoop(); // 현재 실행 순서 조회
            if (!InsertAfterPhysics(ref root)) // 2D 물리 단계 검색
            { // 조건 시작
                ReportFailure("Physics2DFixedUpdate 단계를 찾지 못했습니다."); // 누락 단계 기록
                return false; // 설치 불가 반환
            } // 조건 끝
            PlayerLoop.SetPlayerLoop(root); // 기존 순서에 새 단계 반영
            return true; // 설치 성공 반환
        } // 메서드 끝

        private static bool InsertAfterPhysics(ref PlayerLoopSystem node) // 실행 순서 트리 검색
        { // 메서드 시작
            if (node.subSystemList == null) // 하위 단계 부재 확인
            { // 조건 시작
                return false; // 검색 실패 반환
            } // 조건 끝
            List<PlayerLoopSystem> updated = new List<PlayerLoopSystem>(node.subSystemList.Length + 1); // 갱신 단계 목록
            bool found = false; // 물리 단계 검색 결과
            foreach (PlayerLoopSystem original in node.subSystemList) // 하위 실행 단계 순회
            { // 반복 시작
                if (original.type == typeof(AfterPhysics2D)) // 이전 삽입 단계 확인
                { // 조건 시작
                    continue; // 이전 단계 제거
                } // 조건 끝
                PlayerLoopSystem child = original; // 하위 단계 복사
                found |= InsertAfterPhysics(ref child); // 중첩 단계 검색
                updated.Add(child); // 기존 단계 유지
                if (child.type == typeof(FixedUpdate.Physics2DFixedUpdate)) // 2D 물리 단계 확인
                { // 조건 시작
                    PlayerLoopSystem marker = new PlayerLoopSystem(); // 완료 단계 생성
                    marker.type = typeof(AfterPhysics2D); // 중복 방지 표식
                    marker.updateDelegate = Dispatch; // 스텝 완료 처리 연결
                    updated.Add(marker); // 물리 직후 단계 추가
                    found = true; // 물리 단계 발견
                } // 조건 끝
            } // 반복 끝
            node.subSystemList = updated.ToArray(); // 갱신 단계 저장
            return found; // 검색 결과 반환
        } // 메서드 끝

        private static void Dispatch() // 한 물리 스텝 충돌 해결
        { // 메서드 시작
            if (listeners.Count == 0) // 수신자 부재 확인
            { // 조건 시작
                return; // 처리 생략
            } // 조건 끝
            if (listeners.Count == 1) // 단일 플레이어 확인
            { // 반복 시작
                InvokeSafely(listeners[0]); // 할당 없는 단일 처리
                return; // 중복 처리 방지
            } // 조건 끝
            Action[] snapshot = listeners.ToArray(); // 복수 플레이어 수신자 보관
            foreach (Action listener in snapshot) // 수신자 순회
            { // 반복 시작
                InvokeSafely(listener); // 개별 충돌 처리
            } // 반복 끝
        } // 메서드 끝

        private static void InvokeSafely(Action listener) // 개별 수신자 오류 격리
        { // 메서드 시작
            try // 다른 플레이어 처리 보호
            { // 보호 시작
                listener(); // 수집한 충돌 처리
            } // 보호 끝
            catch (Exception exception) // 개별 처리 오류
            { // 예외 시작
                Debug.LogException(exception); // 오류 기록
            } // 예외 끝
        } // 메서드 끝

        private static void ReportFailure(string reason) // 설치 실패 기록
        { // 메서드 시작
            if (reportedFailure) // 중복 오류 확인
            { // 조건 시작
                return; // 반복 기록 방지
            } // 조건 끝
            reportedFailure = true; // 오류 기록 상태 저장
            Debug.LogError("[Project Epsilon] 물리 스텝 충돌 우선순위를 적용할 수 없어 즉시 판정으로 대체합니다. " + reason); // 처리 한계 알림
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
