import argparse  # 실행 인수 처리
import json  # 전투 기록 읽기
import re  # 실제 상태 로그 해석
import statistics  # 실제 표본 평균
import sys  # 실패 종료 상태
from collections import Counter  # 피해 원인 집계
from pathlib import Path  # 로그 파일 경로

MARKER = "[NaturalCombatVerification] RECORD "  # 실제 5분 기록 구분자
STATE = re.compile(r"^\[NaturalCombatVerification\] time=([\d.]+) .*?body=(\d+) hp=(\d+) .*?gems=(\d+) enemies=(\d+)")  # 주기 상태 구분자와 관찰값


def gem_collection_summary(record, sample):  # 실제 보석 원본 경험치 수집률 계산
    version = record.get("gemAccountingVersion")  # 실제 보석 집계 형식
    if version is None:  # 기존 집계 없는 기록 확인
        return None  # 기존 수집률 미기록 반환
    if version != 1:  # 지원하는 집계 형식 확인
        raise ValueError("지원하지 않는 보석 집계 형식")  # 알 수 없는 집계 제외
    dropped = sample.get("droppedGemExperience")  # 실제 생성 보석 원본량
    collected = sample.get("collectedGemExperience")  # 실제 수집 보석 원본량
    if type(dropped) is not int or type(collected) is not int or dropped < 0 or collected < 0 or collected > dropped:  # 실제 집계 범위 확인
        raise ValueError("보석 생성·수집 경험치 집계가 유효하지 않음")  # 누락 또는 잘못된 집계 제외
    return dropped, collected, None if dropped == 0 else collected / dropped  # 실제 원본량과 수집률 반환


def print_gem_collection(record, sample, label):  # 실제 표본의 보석 수집률 출력
    summary = gem_collection_summary(record, sample)  # 실제 보석 집계 계산
    if summary is None:  # 기존 기록의 집계 누락 확인
        print(f"  {label} 보석 수집률: 기록 없음")  # 미기록 상태 명시
        return  # 수집률 추정 방지
    dropped, collected, rate = summary  # 실제 원본 집계 분리
    formatted = "계산 불가" if rate is None else f"{rate:.1%}"  # 실제 수집률 또는 분모 없음 표시
    print(f"  {label} 보석 원본: 생성={dropped}, 수집={collected}, 수집률={formatted}")  # 실제 보너스 전 보석 집계 출력


def summarize(path):  # 자연 전투 로그 요약
    entries = []  # 완료된 전투의 JSON 기록
    observed = []  # 주기적으로 출력한 동시 적 상태
    with Path(path).open(encoding="utf-8-sig", errors="replace") as log:  # 대용량 배치 로그를 한 줄씩 읽기
        for line in log:  # 실제 로그 순회
            if line.startswith(MARKER):  # 완료된 전투의 JSON 줄 확인
                entries.append(line[len(MARKER):])  # 실제 JSON 기록 보존
            elif line.startswith("[NaturalCombatVerification] time="):  # 전투 주기 상태 줄 확인
                state = STATE.search(line)  # 시간과 동시 적 수 파싱
                if state:  # 유효한 상태 표본 확인
                    observed.append(state)  # 통계에 사용할 표본만 보존
    if len(entries) != 1:  # 실제 완료 기록 개수 확인
        raise ValueError("완료된 5분 기록이 정확히 하나가 아님")  # 미완료 로그 차단
    record = json.loads(entries[0])  # 실제 기록 복원
    samples = record.get("snapshots", [])  # 실제 상태 표본
    five = [sample for sample in samples if sample.get("reason") == "five_minutes"]  # 실제 5분 표본
    if len(five) != 1 or five[0].get("seconds", 0) < 300:  # 실제 5분 진행 확인
        raise ValueError("유효한 5분 상태 표본이 없음")  # 불완전 표본 차단
    last = five[0]  # 실제 최종 상태
    print(f"{path}: seed={record.get('seed')} 시간={last['seconds']:.1f}초 레벨={last['level']} 획득XP={last['totalExperience']} 처치={last['kills']} 보상처치={last['rewardableKills']} 몸통={last['currentBodies']} 체력={last['health']}")  # 실제 결과 출력
    print_gem_collection(record, last, "5분")  # 실제 5분 원본 보석 수집률
    ten = [sample for sample in samples if sample.get("reason") == "ten_minutes"]  # 실제 10분 표본 검색
    if len(ten) > 1 or (ten and ten[0].get("seconds", 0) < 600):  # 실제 10분 표본 유효성 확인
        raise ValueError("유효한 10분 상태 표본이 아님")  # 중복 또는 조기 표본 차단
    if ten:  # 실제 10분 표본 존재 확인
        later = ten[0]  # 실제 10분 최종 상태
        equipped = sum(1 for weapon in later.get("weapons", []) if weapon.get("weaponId"))  # 실제 장착 포탑 수
        print(f"  10분: 시간={later['seconds']:.1f}초 레벨={later['level']} 획득XP={later['totalExperience']} 처치={later['kills']} 몸통={later['currentBodies']} 체력={later['health']} 포탑={equipped}")  # 실제 10분 상태 출력
        print(f"  5~10분 변화: 레벨={later['level'] - last['level']:+} 획득XP={later['totalExperience'] - last['totalExperience']:+} 처치={later['kills'] - last['kills']:+} 몸통={later['currentBodies'] - last['currentBodies']:+}")  # 실제 구간 변화 출력
        print_gem_collection(record, later, "10분")  # 실제 10분 원본 보석 수집률
    fifteen = [sample for sample in samples if sample.get("reason") == "fifteen_minutes"]  # 실제 15분 표본 검색
    if len(fifteen) > 1 or (fifteen and (fifteen[0].get("seconds", 0) < 900 or not ten)):  # 실제 15분 표본과 이전 기준 확인
        raise ValueError("유효한 10분·15분 상태 표본이 아님")  # 중복 또는 누락 표본 차단
    if fifteen:  # 실제 15분 표본 존재 확인
        final = fifteen[0]  # 실제 15분 최종 상태
        equipped = sum(1 for weapon in final.get("weapons", []) if weapon.get("weaponId"))  # 실제 장착 포탑 수
        print(f"  15분: 시간={final['seconds']:.1f}초 레벨={final['level']} 획득XP={final['totalExperience']} 처치={final['kills']} 몸통={final['currentBodies']} 체력={final['health']} 포탑={equipped}")  # 실제 15분 상태 출력
        print(f"  10~15분 변화: 레벨={final['level'] - later['level']:+} 획득XP={final['totalExperience'] - later['totalExperience']:+} 처치={final['kills'] - later['kills']:+} 몸통={final['currentBodies'] - later['currentBodies']:+}")  # 실제 구간 변화 출력
        print_gem_collection(record, final, "15분")  # 실제 15분 원본 보석 수집률
        window = [match for match in observed if 600 <= float(match.group(1)) <= 901]  # 실제 10~15분 상태 표본
        if window:  # 실제 상태 표본 존재 확인
            enemies = [int(match.group(5)) for match in window]  # 실제 동시 적 수
            gems = [int(match.group(4)) for match in window]  # 실제 씬 전체 미수집 보석 수
            print(f"  10~15분 상태 {len(window)}회: 동시 적 최소/평균/최대={min(enemies)}/{statistics.mean(enemies):.1f}/{max(enemies)}, 미수집 보석 최소/최대={min(gems)}/{max(gems)}")  # 실제 출현과 보석 표본 출력
        retained = [sample['currentBodies'] / sample['earnedBodies'] for sample in samples if sample.get('reason') == 'interval' and 600 <= sample.get('seconds', 0) <= 901 and sample.get('earnedBodies', 0) > 0]  # 실제 성장 최대 대비 몸통 비율
        if retained:  # 실제 몸통 표본 존재 확인
            print(f"  10~15분 몸통 유지율 {len(retained)}회: 최소={min(retained):.1%}, 평균={statistics.mean(retained):.1%}")  # 실제 몸통 유지 상태 출력
    twenty = [sample for sample in samples if sample.get("reason") == "twenty_minutes"]  # 실제 20분 표본 검색
    if len(twenty) > 1 or (twenty and (twenty[0].get("seconds", 0) < 1200 or not fifteen or twenty[0].get("seconds", 0) < fifteen[0].get("seconds", 0))):  # 실제 20분과 이전 표본 순서 확인
        raise ValueError("유효한 15분·20분 상태 표본이 아님")  # 중복 또는 누락 표본 차단
    if twenty:  # 실제 20분 표본 존재 확인
        end = twenty[0]  # 실제 20분 최종 상태
        equipped = sum(1 for weapon in end.get("weapons", []) if weapon.get("weaponId"))  # 실제 장착 포탑 수
        print(f"  20분: 시간={end['seconds']:.1f}초 레벨={end['level']} 획득XP={end['totalExperience']} 처치={end['kills']} 몸통={end['currentBodies']} 체력={end['health']} 포탑={equipped}")  # 실제 20분 상태 출력
        print(f"  15~20분 변화: 레벨={end['level'] - final['level']:+} 획득XP={end['totalExperience'] - final['totalExperience']:+} 처치={end['kills'] - final['kills']:+} 몸통={end['currentBodies'] - final['currentBodies']:+}")  # 실제 구간 변화 출력
        print(f"  15~20분 엘리트 처치={end.get('elites', 0) - final.get('elites', 0)}")  # 실제 구간 엘리트 처치 수 출력
        print_gem_collection(record, end, "20분")  # 실제 20분 원본 보석 수집률
        window = [match for match in observed if 900 <= float(match.group(1)) <= 1201]  # 실제 15~20분 상태 표본
        if window:  # 실제 상태 표본 존재 확인
            enemies = [int(match.group(5)) for match in window]  # 실제 동시 적 수
            gems = [int(match.group(4)) for match in window]  # 실제 씬 전체 미수집 보석 수
            print(f"  15~20분 상태 {len(window)}회: 동시 적 최소/평균/최대={min(enemies)}/{statistics.mean(enemies):.1f}/{max(enemies)}, 미수집 보석 최소/최대={min(gems)}/{max(gems)}")  # 실제 출현과 보석 표본 출력
        retained = [sample['currentBodies'] / sample['earnedBodies'] for sample in samples if sample.get('reason') == 'interval' and 900 <= sample.get('seconds', 0) <= 1201 and sample.get('earnedBodies', 0) > 0]  # 실제 성장 최대 대비 몸통 비율
        if retained:  # 실제 몸통 표본 존재 확인
            print(f"  15~20분 몸통 유지율 {len(retained)}회: 최소={min(retained):.1%}, 평균={statistics.mean(retained):.1%}")  # 실제 몸통 유지 상태 출력
    intervals = [sample for sample in samples if sample.get("reason") == "interval"]  # 실제 정기 표본
    for seconds in range(30, 301, 30):  # 30초별 상태 비교
        available = [sample for sample in intervals if abs(sample["seconds"] - seconds) < 1.0]  # 유효 시간 표본 선택
        if available:  # 실제 표본 존재 확인
            sample = min(available, key=lambda item: abs(item["seconds"] - seconds))  # 가장 가까운 표본
            print(f"  {sample['seconds']:.1f}초: 레벨={sample['level']} 누적XP={sample['totalExperience']} 처치={sample['kills']} 몸통={sample['currentBodies']} 체력={sample['health']}")  # 실제 시점 출력
    for sample in (last, ten[0] if ten else None, fifteen[0] if fifteen else None, twenty[0] if twenty else None):  # 도달한 구간의 장비 비교
        if sample is None:  # 아직 도달하지 않은 구간 확인
            continue  # 누락 구간 생략
        weapons = [weapon for weapon in sample.get("weapons", []) if weapon.get("weaponId")]  # 실제 장착 포탑만 수집
        grades = Counter(weapon.get("grade") or 0 for weapon in weapons)  # 등급별 장착 수
        attributes = Counter(weapon.get("attribute") or "미기록" for weapon in weapons)  # 속성별 장착 수
        elapsed = sample["seconds"] / 60  # 표본 시각의 분 단위 표시
        print(f"  {elapsed:.1f}분 장착 등급={dict(sorted(grades.items()))} 속성={dict(sorted(attributes.items()))}")  # 합성과 속성 비교용 분포
    grade_five = next((sample for sample in samples if any(weapon.get("weaponId") and weapon.get("grade") == 5 for weapon in sample.get("weapons", []))), None)  # 처음 기록한 5등급 표본
    merge_count = sum(sample.get("reason") == "automatic_merge" for sample in samples)  # 실제 완료된 자동 합성 기록 개수
    first_five = f"{grade_five['seconds']:.1f}초" if grade_five else "관찰 안 됨"  # 5등급 첫 관찰 결과
    print(f"  자동 합성 완료 {merge_count}회, 첫 ★5 장착: {first_five}")  # 실제 합성 비교 결과
    damages = record.get("damages", [])  # 실제 피해 목록
    causes = Counter(item.get("origin", "Unknown") for item in damages)  # 원인별 피해 횟수
    print(f"  피해 원인={dict(causes)} 체력피해={sum(item.get('healthDamage', 0) for item in damages)} 보호막흡수={sum(item.get('shieldDamage', 0) for item in damages)} 직접몸통손실={sum(item.get('bodyLoss', 0) for item in damages)}")  # 실제 피해 합계 출력
    for start, end in ((0, 300), (300, 600), (600, 900), (900, 1200)):  # 다섯 분 단위의 피해 구간
        if samples[-1]["seconds"] < end:  # 도달하지 못한 구간 확인
            break  # 불완전 구간 제외
        segment_health = Counter()  # 현재 구간의 원인별 체력 피해
        segment_body_loss = 0  # 현재 구간의 직접 몸통 손실
        for event in damages:  # 실제 피해의 발생 시각 분류
            if start <= event.get("seconds", -1) < end:  # 현재 구간에 포함된 피해 확인
                segment_health[event.get("origin", "Unknown")] += event.get("healthDamage", 0)  # 원인별 공용 체력 피해 합산
                segment_body_loss += event.get("bodyLoss", 0)  # 몸통 직접 손실 합산
        print(f"  {start // 60}~{end // 60}분 피해: {dict(segment_health.most_common())}, 직접 몸통 손실={segment_body_loss}")  # 시간대별 피해 집중 지점 출력


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
