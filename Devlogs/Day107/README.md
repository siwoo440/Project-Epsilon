# 107일차: 범위 속성 반응의 승리 경계

- 신성·화염의 성화 폭발과 물리·폭발의 파쇄 충격이 보스를 처치하면, 실제 승리가 확정된 뒤의 후속 대상 피해와 넉백을 중단한다.
- 승리 전 처리된 피해와 일반 전투 중 반응의 범위 피해·넉백은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay107ReactionVictoryVerification.RunSacred`, `.RunShattering`: 수정 전 두 실제 범위 반응의 승리 후 피해를 재현하고 수정 후 통과했다.
- `AutomatedDay107ReactionVictoryVerification.RunSacredNormal`, `.RunShatteringNormal`: 일반 전투의 두 범위 반응 피해를 통과했다.
- `AutomatedRemainingReactionVerification`, `AutomatedPhysicalExplosionReactionVerification`, `AutomatedColdElectricReactionVerification`: 기존 속성 반응과 보스 넉백 저항 회귀를 통과했다.
- `AutomatedDay105VictoryProjectileVerification.RunExplosion`, `AutomatedBossOutcomeRaceVerification.RunVictory`: 기존 폭발 공격 및 보스 승리 경계 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 자연 플레이에서 속성 반응과 승리 직후 화면 연출은 확인하지 못했다. 다른 처치·보호막 연쇄 효과의 승리 경계는 별도 확인이 필요하다.
