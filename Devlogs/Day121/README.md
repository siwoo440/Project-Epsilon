# 121일차: 전기 포탑 등급 효과 시험안

- 전기 근접은 연쇄 대상 ★3 +1·★5 +2. 전기 범위는 연쇄 대상 +1과 범위 ★3 1.15배·★5 1.3배를 시험한다.
- 전기 속사 근접은 공격속도 ★3 1.1배·★5 1.2배와 ★5 연쇄 +1. 전기 직사는 ★3 네 번째 공격 1.2배·★5 세 번째 공격 1.35배와 ★5 연쇄 +1을 시험한다.
- 전기 관통은 ★3 관통 +1·★5 +2와 ★5 연쇄 +1. 전기 파동은 ★3 세 번째 공격 1.2배·★5 두 번째 공격 1.3배와 연쇄 대상 ★3 +1·★5 +2를 시험한다.
- 등급 연쇄는 전기 시너지가 없어도 적용되며 기존 시너지 연쇄 수에 더해진다. 기존 중복 대상 방지·속성 반응·승패 경계를 유지한다. 모든 수치는 포탑별 데이터 시험값이다.

## Unity 비대화형 배치

- `AutomatedDay121ElectricTurretVerification`: 수정 전 단독 ★3 연쇄가 없어 실패했다. 수정 후 실제 등급·시너지별 연쇄 대상, 연쇄 범위 경계, 속사·관통·주기 공격과 보스 승리 이후 피해 중단이 통과했다.
- `AutomatedDay39ElectricVerification`, `AutomatedColdElectricReactionVerification`: 기존 전기 시너지·합성 후 단계와 냉기·전기 반응이 통과했다.
- `AutomatedDay105VictoryProjectileVerification.RunChain`·`.RunChainSecondary`, `AutomatedDay120ColdTurretVerification`, `AutomatedDay119FireTurretVerification`: 기존 승리 경계와 냉기·화염 시험 효과가 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 독·폭발·신성·암흑 24종의 개별 시험 효과와 전체 빌드 밸런스, 실제 화면·자연 전투 확인이 남았다.
