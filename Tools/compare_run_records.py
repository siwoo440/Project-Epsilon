import argparse  # 비교 인수 처리
import json  # 전투 기록 읽기
import sys  # 실제 종료 코드
from collections import Counter  # 피해 원인별 횟수
from pathlib import Path  # 실제 기록 파일 경로

CONDITION_FIELDS = (  # 같은 시작 조건에 필요한 항목
    "version", "unityVersion", "applicationVersion",  # 기록과 실행 버전
    "seedProvided", "seed", "randomProbe", "fixedDeltaTime", "targetFrameRate",  # 난수와 물리 조건
    "characterId", "mapIndex", "mapName", "permanentLevels",  # 실제 시작 선택
    "spawnInterval", "enemyLimit", "bossTransitionTime", "enemies",  # 실제 적과 보스 조건
    "playerMoveSpeed", "playerTurnSpeed", "maximumStamina",  # 실제 플레이어 조건
    "experienceBaseRequirement", "experienceGrowthMultiplier",  # 실제 경험치 조건
    "obstacleChance", "specialZoneChance",  # 실제 맵 출현 조건
)  # 시작 조건 목록 종료


def read_record(path):  # 실제 기록 파일 검증
    data = json.loads(Path(path).read_text(encoding="utf-8-sig"))  # 실제 JSON 파일 읽기
    if not isinstance(data, dict) or data.get("version") != 1:  # 실제 기록 형식 확인
        raise ValueError("지원하지 않는 기록 형식")  # 잘못된 기록 차단
    if not isinstance(data.get("snapshots"), list) or not data["snapshots"]:  # 실제 상태 표본 확인
        raise ValueError("상태 표본이 없는 기록")  # 불완전 기록 차단
    if not isinstance(data.get("damages"), list) or not isinstance(data.get("inputs"), list):  # 실제 사건 목록 확인
        raise ValueError("피해 또는 입력 기록 누락")  # 불완전 사건 기록 차단
    missing = [key for key in CONDITION_FIELDS if key not in data]  # 실제 시작 조건 누락 목록
    if missing:  # 실제 필수 항목 확인
        raise ValueError("시작 조건 누락: " + ", ".join(missing))  # 잘못된 조건 비교 차단
    if data["snapshots"][0].get("reason") != "start" or data["snapshots"][-1].get("reason") != "end":  # 실제 시작과 종료 표본 확인
        raise ValueError("시작 또는 종료 표본 누락")  # 미완료 기록 비교 차단
    return data  # 실제 검증된 기록 반환


def condition_differences(left, right):  # 실제 시작 조건 비교
    differences = [key for key in CONDITION_FIELDS if left[key] != right[key]]  # 실제 다른 설정 목록
    if not left["seedProvided"] or not right["seedProvided"]:  # 실제 시드 지정 확인
        differences.append("명시적 난수 시드 없음")  # 재현 불가능 조건 표시
    first_left = left["snapshots"][0]  # 첫 기록의 실제 시작 상태
    first_right = right["snapshots"][0]  # 둘째 기록의 실제 시작 상태
    for key in ("level", "currentExperience", "currentBodies", "earnedBodies", "bodyLimit", "health", "maximumHealth", "weapons"):  # 실제 초기 성장과 장비 순회
        if first_left.get(key) != first_right.get(key):  # 실제 시작 상태 차이 확인
            differences.append("start." + key)  # 다른 시작 상태 기록
    return differences  # 실제 다른 조건 목록 반환


def summarize(record):  # 실제 최종 전투 결과 요약
    final = record["snapshots"][-1]  # 실제 마지막 상태 표본
    damage_counts = Counter(item.get("origin", "Unknown") for item in record["damages"])  # 실제 원인별 피해 횟수
    return {  # 실제 비교 결과 구성
        "outcome": record.get("outcome", ""),  # 실제 승리 또는 패배
        "seconds": final.get("seconds", 0),  # 실제 전투 시간
        "level": final.get("level", 0),  # 실제 최종 레벨
        "totalExperience": final.get("totalExperience", 0),  # 실제 누적 경험치
        "kills": final.get("kills", 0),  # 실제 전체 처치
        "rewardableKills": final.get("rewardableKills", 0),  # 실제 보상 대상 처치
        "elites": final.get("elites", 0),  # 실제 엘리트 처치
        "currentBodies": final.get("currentBodies", 0),  # 실제 남은 몸통
        "health": final.get("health", 0),  # 실제 최종 체력
        "lastDamageOrigin": record.get("lastDamageOrigin", ""),  # 실제 마지막 피해 원인
        "damageCounts": dict(sorted(damage_counts.items())),  # 실제 원인별 피해 횟수
        "inputChanges": len(record["inputs"]),  # 실제 이동 입력 변화 횟수
    }  # 실제 비교 결과 종료


def main():  # 실제 비교 명령 진입
    parser = argparse.ArgumentParser(description="두 전투 기록의 시작 조건과 결과 비교")  # 실제 명령 설명
    parser.add_argument("first", help="첫 번째 JSON 기록")  # 실제 첫 파일 인수
    parser.add_argument("second", help="두 번째 JSON 기록")  # 실제 둘째 파일 인수
    args = parser.parse_args()  # 실제 명령 인수 읽기
    try:  # 실제 파일과 기록 확인
        left = read_record(args.first)  # 실제 첫 기록 읽기
        right = read_record(args.second)  # 실제 둘째 기록 읽기
        differences = condition_differences(left, right)  # 실제 시작 조건 비교
        first_result = summarize(left)  # 실제 첫 결과 요약
        second_result = summarize(right)  # 실제 둘째 결과 요약
    except (OSError, ValueError, KeyError, TypeError) as error:  # 실제 파일 또는 형식 오류
        print("기록을 비교할 수 없습니다:", error, file=sys.stderr)  # 실제 오류 안내
        return 1  # 실제 오류 종료 코드
    print("시작 조건:", "차이 있음" if differences else "일치")  # 실제 조건 비교 결과
    if differences:  # 실제 조건 차이 확인
        print("다른 조건:", ", ".join(differences))  # 실제 다른 설정 표시
    print("첫 번째 결과:", json.dumps(first_result, ensure_ascii=False, sort_keys=True))  # 실제 첫 결과 표시
    print("두 번째 결과:", json.dumps(second_result, ensure_ascii=False, sort_keys=True))  # 실제 둘째 결과 표시
    print("입력·프레임·물리 처리 순서에 따라 같은 시드에서도 전투 결과가 달라질 수 있습니다.")  # 실제 재현 한계 안내
    return 2 if differences else 0  # 실제 조건 차이에 따른 종료 코드


if __name__ == "__main__":  # 직접 실행 여부 확인
    raise SystemExit(main())  # 실제 비교 결과 반환
