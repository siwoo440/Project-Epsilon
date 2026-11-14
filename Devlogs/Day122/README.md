# 122일차: 독 포탑 등급 효과 시험안

- 독 근접은 독 약화 대상 직접 피해 ★3 1.2배·★5 1.35배, 독 범위는 독 지속시간 ★3 1.2배·★5 1.4배를 시험한다.
- 독 속사 근접은 공격속도 ★3 1.1배·★5 1.2배, 독 직사는 ★3 네 번째 공격 1.2배·★5 세 번째 공격 1.35배를 시험한다.
- 독 관통은 ★3 관통 +1·★5 +2와 독 약화 대상 피해 1.1배를 시험한다. 독 파동은 독 지속시간 ★3 1.1배·★5 1.2배와 ★3 세 번째 공격 1.2배·★5 두 번째 공격 1.35배를 시험한다.
- 기존 공통 등급 효과와 독 시너지 활성 조건·중첩·약화를 유지한다. 독 대상 추가 피해는 명중 전 실제 독 상태에만 적용하며 수치는 포탑 데이터에서 조정한다.

## Unity 비대화형 배치

- `AutomatedDay122PoisonTurretVerification`: 수정 전 독 대상 등급 피해가 없어 실패했다. 수정 후 첫 명중, 독 대상 피해·지속시간, 시너지 결합, 관통과 속사·주기 공격이 통과했다.
- `AutomatedFirePoisonReactionVerification`, `AutomatedPoisonShotVerification`, `AutomatedOverlappingReactionVerification`, `AutomatedRemainingReactionVerification`: 기존 독·화염 반응, 적 사격 약화, 반응 우선순위와 독·암흑 반응이 통과했다.
- `AutomatedDay120ColdTurretVerification`, `AutomatedDay119FireTurretVerification`, `AutomatedDay121ElectricTurretVerification`: 기존 냉기·화염·전기 시험 효과가 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 폭발·신성·암흑 18종의 개별 시험 효과와 전체 빌드 밸런스, 실제 화면·자연 전투 확인이 남았다.
