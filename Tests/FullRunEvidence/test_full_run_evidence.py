import json  # 기록 자료 생성
import sys  # 검사 도구 경로 연결
import unittest  # 독립 검증 실행
from pathlib import Path  # 프로젝트 경로 조회

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Tools"))  # 실제 도구 경로 추가
from check_full_run import validate_full_run  # 실제 전체 전투 판별 도구


class FullRunEvidenceTests(unittest.TestCase):  # 전체 전투 기록 경계 검증
    def setUp(self):  # 독립 시험 자료 준비
        self.record = {"outcome": "Victory", "bossTransitionTime": 1200, "snapshots": []}  # 실제 종료 필드 대체
        for reason, seconds in (("five_minutes", 300), ("ten_minutes", 600), ("fifteen_minutes", 900), ("twenty_minutes", 1200), ("end", 1201)):  # 실제 시간 경계 순회
            self.record["snapshots"].append({"reason": reason, "seconds": seconds, "bossKills": int(reason == "end"), "currentBodies": 3})  # 실제 시점별 표본 대체
        self.prefix = "\n-epsilonFightBoss\n-epsilonRecord\n[NaturalCombatVerification] PASS boss victory phaseTwo=True wall=1201.0s\n"  # 자연 보스전 로그 표식 대체

    def log(self):  # 실제 배치 로그 형태 생성
        return self.prefix + "[NaturalCombatVerification] RECORD " + json.dumps(self.record) + "\n"  # 결과와 기록 조합

    def test_complete_victory(self):  # 정상 전체 승리 기록 확인
        self.assertEqual(validate_full_run(self.log())["outcome"], "Victory")  # 실제 승리 자료 반환 확인

    def test_controlled_execution_is_not_natural(self):  # 강제 통제 시험 구별
        self.prefix += "[SurvivalMilestonesVerification] Controlled clock jumps\n"  # 실제 시간 통제 표식 추가
        with self.assertRaises(ValueError):  # 자연 전투 결과 거부 확인
            validate_full_run(self.log())  # 통제 기록 판별

    def test_boss_kill_is_required(self):  # 실제 보스 처치 필요 확인
        self.record["snapshots"][-1]["bossKills"] = 0  # 보스 처치 기록 제거
        with self.assertRaises(ValueError):  # 처치 없는 승리 거부 확인
            validate_full_run(self.log())  # 불완전 기록 판별

    def test_short_real_time_is_rejected(self):  # 강제 시계 이동 가능성 확인
        self.prefix = self.prefix.replace("wall=1201.0s", "wall=0.2s")  # 비정상적으로 짧은 실제 시간 대체
        with self.assertRaises(ValueError):  # 조기 종료 기록 거부 확인
            validate_full_run(self.log())  # 짧은 실행 기록 판별

    def test_dead_player_and_duplicate_record_are_rejected(self):  # 종료 생존과 기록 개수 확인
        self.record["snapshots"][-1]["currentBodies"] = 0  # 종료 시 몸통 소진 대체
        with self.assertRaises(ValueError):  # 생존하지 않은 결과 거부 확인
            validate_full_run(self.log())  # 몸통 소진 기록 판별
        self.setUp()  # 독립 자료 복원
        with self.assertRaises(ValueError):  # 중복 기록 거부 확인
            validate_full_run(self.log() + "[NaturalCombatVerification] RECORD " + json.dumps(self.record) + "\n")  # 실제 기록 중복 판별

    def test_missing_boss_option_is_rejected(self):  # 실제 보스전 실행 인수 확인
        self.prefix = self.prefix.replace("-epsilonFightBoss\n", "")  # 실제 보스전 실행 인수 제거
        with self.assertRaises(ValueError):  # 인수 누락 결과 거부 확인
            validate_full_run(self.log())  # 보스전 인수 누락 판별

    def test_missing_or_duplicate_milestones_are_rejected(self):  # 표본 누락과 중복 확인
        self.record["snapshots"] = [sample for sample in self.record["snapshots"] if sample["reason"] != "fifteen_minutes"]  # 15분 표본 제거
        with self.assertRaises(ValueError):  # 누락 기록 거부 확인
            validate_full_run(self.log())  # 불완전 기록 판별
        self.setUp()  # 독립 자료 복원
        self.record["snapshots"].append(dict(self.record["snapshots"][0]))  # 5분 표본 중복
        with self.assertRaises(ValueError):  # 중복 기록 거부 확인
            validate_full_run(self.log())  # 중복 기록 판별

    def test_early_or_reversed_timing_is_rejected(self):  # 조기 종료와 시간 역전 확인
        self.record["snapshots"][3]["seconds"] = 1199  # 보스 전환 전 표본
        with self.assertRaises(ValueError):  # 조기 표본 거부 확인
            validate_full_run(self.log())  # 조기 기록 판별
        self.setUp()  # 독립 자료 복원
        self.record["snapshots"][-1]["seconds"] = 1190  # 종료 시각 역전
        with self.assertRaises(ValueError):  # 역전 기록 거부 확인
            validate_full_run(self.log())  # 역전 기록 판별

    def test_defeat_and_missing_success_are_rejected(self):  # 패배와 성공 표식 누락 확인
        self.record["outcome"] = "Defeated"  # 실제 패배 결과 대체
        with self.assertRaises(ValueError):  # 패배 결과 거부 확인
            validate_full_run(self.log())  # 패배 기록 판별
        self.setUp()  # 독립 자료 복원
        self.prefix = self.prefix.replace("PASS boss", "FAIL boss")  # 성공 표식 제거
        with self.assertRaises(ValueError):  # 성공 누락 거부 확인
            validate_full_run(self.log())  # 미완료 기록 판별


if __name__ == "__main__":  # 직접 실행 확인
    unittest.main()  # 독립 시험 실행
