import sys  # 실제 도구 경로 연결
import unittest  # 독립 검증 실행
from pathlib import Path  # 프로젝트 경로 조회

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Tools"))  # 실제 요약 도구 경로 추가
from summarize_early_run import gem_collection_summary  # 실제 보석 수집률 계산


class ExperienceAccountingTests(unittest.TestCase):  # 보석 수집률 경계 검증
    def test_legacy_record_has_no_rate(self):  # 기존 기록의 집계 누락 확인
        self.assertIsNone(gem_collection_summary({}, {}))  # 미기록 값을 영으로 추정하지 않음

    def test_empty_drop_has_no_rate(self):  # 생성된 보석이 없는 경우 확인
        result = gem_collection_summary({"gemAccountingVersion": 1}, {"droppedGemExperience": 0, "collectedGemExperience": 0})  # 실제 빈 집계 대체
        self.assertEqual(result, (0, 0, None))  # 수집률 계산 불가 확인

    def test_raw_collection_ratio(self):  # 원본 보석 경험치 수집률 확인
        result = gem_collection_summary({"gemAccountingVersion": 1}, {"droppedGemExperience": 25, "collectedGemExperience": 5})  # 실제 생성·수집량 대체
        self.assertEqual(result, (25, 5, 0.2))  # 원본 경험치 기준 20퍼센트 확인

    def test_invalid_accounting_is_rejected(self):  # 누락·음수·초과 집계 확인
        for sample in ({"droppedGemExperience": 5}, {"droppedGemExperience": -1, "collectedGemExperience": 0}, {"droppedGemExperience": 5, "collectedGemExperience": 6}):  # 실제 오류별 표본 순회
            with self.subTest(sample=sample):  # 실제 오류별 결과 구분
                with self.assertRaises(ValueError):  # 잘못된 수집률 거부 확인
                    gem_collection_summary({"gemAccountingVersion": 1}, sample)  # 실제 오류 표본 판별


if __name__ == "__main__":  # 직접 실행 확인
    unittest.main()  # 독립 시험 실행
