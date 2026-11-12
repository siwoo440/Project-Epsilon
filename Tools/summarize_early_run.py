import argparse  # 실행 인수 처리
import json  # 전투 기록 읽기
import sys  # 실패 종료 상태
from collections import Counter  # 피해 원인 집계
from pathlib import Path  # 로그 파일 경로

MARKER = "[NaturalCombatVerification] RECORD "  # 실제 5분 기록 구분자


def summarize(path):  # 자연 전투 로그 요약
    lines = Path(path).read_text(encoding="utf-8-sig", errors="replace").splitlines()  # 실제 로그 읽기
    entries = [line.partition(MARKER)[2] for line in lines if MARKER in line]  # 완료된 5분 기록 검색
    if len(entries) != 1:  # 실제 완료 기록 개수 확인
        raise ValueError("완료된 5분 기록이 정확히 하나가 아님")  # 미완료 로그 차단
    record = json.loads(entries[0])  # 실제 기록 복원
    samples = record.get("snapshots", [])  # 실제 상태 표본
    five = [sample for sample in samples if sample.get("reason") == "five_minutes"]  # 실제 5분 표본
    if len(five) != 1 or five[0].get("seconds", 0) < 300:  # 실제 5분 진행 확인
        raise ValueError("유효한 5분 상태 표본이 없음")  # 불완전 표본 차단
    last = five[0]  # 실제 최종 상태
    print(f"{path}: seed={record.get('seed')} 시간={last['seconds']:.1f}초 레벨={last['level']} 획득XP={last['totalExperience']} 처치={last['kills']} 보상처치={last['rewardableKills']} 몸통={last['currentBodies']} 체력={last['health']}")  # 실제 결과 출력
    intervals = [sample for sample in samples if sample.get("reason") == "interval"]  # 실제 정기 표본
    for seconds in range(30, 301, 30):  # 30초별 상태 비교
        available = [sample for sample in intervals if abs(sample["seconds"] - seconds) < 1.0]  # 유효 시간 표본 선택
        if available:  # 실제 표본 존재 확인
            sample = min(available, key=lambda item: abs(item["seconds"] - seconds))  # 가장 가까운 표본
            print(f"  {sample['seconds']:.1f}초: 레벨={sample['level']} 누적XP={sample['totalExperience']} 처치={sample['kills']} 몸통={sample['currentBodies']} 체력={sample['health']}")  # 실제 시점 출력
    damages = record.get("damages", [])  # 실제 피해 목록
    causes = Counter(item.get("origin", "Unknown") for item in damages)  # 원인별 피해 횟수
    print(f"  피해 원인={dict(causes)} 체력피해={sum(item.get('healthDamage', 0) for item in damages)} 보호막흡수={sum(item.get('shieldDamage', 0) for item in damages)} 직접몸통손실={sum(item.get('bodyLoss', 0) for item in damages)}")  # 실제 피해 합계 출력


def main():  # 명령행 진입점
    parser = argparse.ArgumentParser(description="5분 자연 전투 기록 요약")  # 인수 설명
    parser.add_argument("logs", nargs="+", help="완료된 Unity 배치 로그")  # 실제 로그 경로
    args = parser.parse_args()  # 명령행 인수 읽기
    for path in args.logs:  # 요청한 로그 순회
        try:  # 파일과 기록 오류 처리
            summarize(path)  # 실제 5분 결과 출력
        except (OSError, ValueError, KeyError, TypeError, json.JSONDecodeError) as error:  # 불완전 로그 처리
            print(f"{path}: {error}", file=sys.stderr)  # 실제 오류 출력
            return 1  # 분석 실패 반환
    return 0  # 모든 로그 분석 성공


if __name__ == "__main__":  # 직접 실행 확인
    sys.exit(main())  # 실제 종료 코드 반환
