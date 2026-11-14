# 119일차: 화염 포탑 등급 효과 시험안

- 화염탄의 기존 ★3 전달·★5 화상 처치 폭발을 유지하면서 다른 화염 포탑 다섯 종에 역할별 시험 효과를 추가했다. 기존 공통 등급 효과도 유지한다.
- 화염 근접: 화상 대상 직접 피해 ★3 1.2배·★5 1.35배. 화염 범위: 화상 지속시간 ★3 1.2배·★5 1.4배. 화염 중근접: ★3 세 번째 공격 1.2배·★5 두 번째 공격 1.3배.
- 화염 관통: ★3 관통 +1, ★5 +2와 화상 대상 피해 1.1배. 화염 파동: 화상 초당 피해 ★3 1.15배·★5 1.3배. 수치는 개별 데이터에서 조정한다.
- 화상 대상 추가 피해는 명중 전에 화상이 활성화된 경우에만 적용한다. 등급 화상 효과와 기존 속성 시너지는 함께 적용하며 상위 등급이 하위 등급의 배율을 잃지 않는다.

## Unity 비대화형 배치

- `AutomatedDay119FireTurretVerification`: 수정 전 화상 대상 등급 피해가 없어 실패했다. 수정 후 실제 화상 없는 첫 명중, 화상 대상 피해, 화상 시간·피해, 시너지 결합, 관통과 주기 공격이 통과했다.
- `AutomatedDay118PhysicalTurretVerification`, `AutomatedDay47UniqueEffectsVerification`: 기존 물리 포탑 시험 효과와 강철 강타·화염탄 폭발이 통과했다.
- `AutomatedRemainingReactionVerification`, `AutomatedDay111LightDarkOutcomeVerification.RunVictory`, `AutomatedDay94BurnFlashVerification`: 기존 속성 반응과 보스 승리 경계·화상 피격 표시가 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 냉기·전기·독·폭발·신성·암흑 36종의 개별 시험 효과와 전체 빌드 밸런스, 실제 화면·자연 전투 확인이 남았다.
