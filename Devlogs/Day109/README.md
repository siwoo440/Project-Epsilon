# 109일차: 암흑 처치 효과의 승리 경계

- 암흑 공격으로 보스 승리가 확정된 뒤 체력 흡수, 처치 회복과 주변 저주 전파가 뒤늦게 적용되지 않도록 한다.
- 일반 전투 중 암흑 흡수·처치 회복·저주 전파와 기존 준비 상태의 캐릭터 패시브 호출은 유지한다. 승리 처리를 만든 보스 피해는 그대로 적용한다.

## Unity 비대화형 배치

- `AutomatedDay109DarkKillVictoryVerification.RunVictory`: 수정 전 최대 저주 보스 처치 후 암흑 흡수 또는 저주 전파가 이어지는 것을 재현하고 수정 후 통과했다.
- `AutomatedDay109DarkKillVictoryVerification.RunNormal`: 실제 일반 적 처치의 흡수와 저주 전파를 통과했다.
- 첫 수정은 준비 상태의 암흑 패시브까지 차단해 `AutomatedCharacterRuntimeVerification`이 실패했다. 조건을 승리·패배로 좁힌 뒤 여섯 캐릭터 패시브 시험을 통과했다.
- `AutomatedDay41AttributeVerification`, `AutomatedOverlappingReactionVerification`, `AutomatedDay108ShieldOutcomeVerification.RunNormal`·`.RunVictory`, `AutomatedBossOutcomeRaceVerification.RunDefeat`: 기존 속성 반응·보호막·패배 경합 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 자연 플레이에서 최종 타격과 결과 화면의 체력 표시, 나머지 처치 효과의 종료 경계는 확인하지 못했다.
