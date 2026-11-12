import argparse  # 명령행 인수 처리
import json  # 실제 전투 기록 해석
import math  # 시간 값 유효성 확인
import re  # 실제 실행 시간 판독
import sys  # 실패 종료 코드
from pathlib import Path  # 실제 로그 경로

RECORD_MARKER = "[NaturalCombatVerification] RECORD "  # 실제 전투 기록 표식
SUCCESS_MARKER = "[NaturalCombatVerification] PASS boss victory phaseTwo=True"  # 실제 2단계 승리 표식
MILESTONES = (("five_minutes", 300), ("ten_minutes", 600), ("fifteen_minutes", 900), ("twenty_minutes", 1200), ("end", 1200))  # 실제 필수 생존 경계


def validate_full_run(text):  # 실제 배치 기록의 전체 승리 조건 확인
    lines = text.splitlines()  # 실제 로그 행 분리
    if "-epsilonFightBoss" not in lines or "-epsilonRecord" not in lines:  # 실제 보스전과 기록 옵션 확인
        raise ValueError("보스전 또는 기록 실행 옵션이 없음")  # 실행 조건 누락
    if any("[SurvivalMilestonesVerification]" in line or "AutomatedSurvivalMilestonesVerification.Run" in line for line in lines):  # 알려진 강제 시간 통제 시험 확인
        raise ValueError("시간과 보스 피해를 통제한 시험은 자연 전투가 아님")  # 통제 결과 제외
    successes = [line for line in lines if line.startswith(SUCCESS_MARKER)]  # 실제 결과 화면까지의 성공 기록
    if len(successes) != 1:  # 실제 성공 기록 개수 확인
        raise ValueError("보스 2단계와 승리 화면 성공 기록이 정확히 하나가 아님")  # 미완료 또는 중복 기록 제외
    wall = re.search(r"\bwall=([0-9]+(?:\.[0-9]+)?)s\b", successes[0])  # 실제 경과 시간 조회
    if wall is None or float(wall.group(1)) < 1199:  # 정상 속도 전체 생존의 최소 실제 시간 확인
        raise ValueError("전체 전투의 실제 경과 시간이 부족함")  # 시간 가속 또는 불완전 기록 제외
    entries = [line.partition(RECORD_MARKER)[2] for line in lines if line.startswith(RECORD_MARKER)]  # 실제 완료 기록 검색
    if len(entries) != 1:  # 실제 기록 개수 확인
        raise ValueError("전체 전투 기록이 정확히 하나가 아님")  # 누락 또는 중복 기록 제외
    record = json.loads(entries[0])  # 실제 전투 기록 복원
    if not isinstance(record, dict) or record.get("outcome") != "Victory" or record.get("bossTransitionTime") != 1200:  # 실제 승리와 보스 시각 확인
        raise ValueError("승리 결과 또는 20분 보스 전환 설정이 다름")  # 잘못된 승리 조건 제외
    snapshots = record.get("snapshots")  # 실제 시점별 전투 표본
    if not isinstance(snapshots, list):  # 실제 표본 목록 확인
        raise ValueError("전투 표본 목록이 없음")  # 잘못된 기록 제외
    previous = 0.0  # 이전 표본 시간 초기화
    final = None  # 마지막 종료 표본 초기화
    for reason, threshold in MILESTONES:  # 실제 다섯 표본 순회
        found = [sample for sample in snapshots if isinstance(sample, dict) and sample.get("reason") == reason]  # 실제 해당 표본 검색
        if len(found) != 1:  # 실제 표본 수 확인
            raise ValueError(f"{reason} 표본이 정확히 하나가 아님")  # 누락 또는 중복 제외
        sample = found[0]  # 실제 단일 표본
        seconds = sample.get("seconds")  # 실제 게임 시간
        if isinstance(seconds, bool) or not isinstance(seconds, (int, float)) or not math.isfinite(seconds) or seconds < threshold or seconds < previous:  # 실제 시간과 순서 확인
            raise ValueError(f"{reason} 표본의 시간 또는 순서가 잘못됨")  # 조기 또는 역전 표본 제외
        previous = seconds  # 다음 표본의 이전 시간
        final = sample  # 실제 마지막 표본 저장
    if type(final.get("bossKills")) is not int or final["bossKills"] != 1 or type(final.get("currentBodies")) is not int or final["currentBodies"] <= 0:  # 실제 보스 처치와 플레이어 생존 확인
        raise ValueError("보스 처치 또는 플레이어 생존 기록이 없음")  # 실제 승리 증거 누락
    return record  # 검증된 실제 기록 반환


def main():  # 명령행 진입점
    parser = argparse.ArgumentParser(description="20분 생존 이후 보스 승리 기록 확인")  # 실제 검사 설명
    parser.add_argument("log", help="완료된 Unity 자연 전투 배치 로그")  # 실제 로그 경로
    args = parser.parse_args()  # 실제 인수 조회
    try:  # 실제 파일과 기록 오류 처리
        record = validate_full_run(Path(args.log).read_text(encoding="utf-8-sig", errors="replace"))  # 실제 로그 검증
    except (OSError, ValueError, TypeError, json.JSONDecodeError) as error:  # 실제 파일 또는 기록 오류
        print(f"검증 실패: {error}", file=sys.stderr)  # 실제 실패 사유 출력
        return 1  # 실제 실패 종료 코드
    final = next(sample for sample in record["snapshots"] if sample["reason"] == "end")  # 실제 종료 표본 조회
    print(f"전체 전투 기록 확인: 승리, 게임 시간 {final['seconds']:.1f}초, 보스 처치 {final['bossKills']}회, 몸통 {final['currentBodies']}칸")  # 실제 검증 결과 요약
    print("알려진 통제 표식과 기록 일관성을 검사했으며 자동 조종의 실제 조작과 환경은 로그를 별도로 확인해야 합니다.")  # 실제 검사의 증거 범위
    return 0  # 실제 성공 종료 코드


if __name__ == "__main__":  # 직접 실행 확인
    sys.exit(main())  # 실제 명령행 종료
