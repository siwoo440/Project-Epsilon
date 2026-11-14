# 125일차: 암흑 포탑 등급 효과 시험안

- 암흑 근접은 저주된 대상에게 ★3 1.2배·★5 1.35배 피해를 시험한다.
- 암흑 범위는 저주 지속시간 ★3 1.2배·★5 1.4배를 시험한다.
- 암흑 속사는 공격속도 ★3 1.1배·★5 1.2배, 암흑 직사는 ★3 네 번째 공격 1.2배·★5 세 번째 공격 1.35배를 시험한다.
- 암흑 관통은 ★3 관통 +1·★5 +2와 저주 대상 피해 1.1배를 시험한다. 암흑 파동은 저주 시간 ★3 1.1배·★5 1.2배와 ★3 세 번째 공격 1.2배·★5 두 번째 공격 1.35배를 시험한다.
- 저주는 기존 시너지 활성 조건에서만 적용한다. 기존 저주 중첩·흡수·복합 반응과 승리 후 차단을 유지한다. 수치는 포탑 데이터의 시험안이다.

## Unity 비대화형 배치

- `AutomatedDay125DarkTurretVerification`: 저주 대상 피해, 저주 시간·중첩, 첫 명중, 속사·주기 공격과 관통 시험 통과.
- `AutomatedRemainingReactionVerification`: 신성·화염, 독·암흑, 신성·암흑 반응 양쪽 순서 시험 통과.
- `AutomatedDay109DarkKillVictoryVerification.RunVictory`, `AutomatedDay111LightDarkOutcomeVerification.RunVictory`: 승리 후 암흑 처치·명암 반응 차단 시험 통과.
- `AutomatedDay123ExplosionTurretVerification`, `AutomatedDay124HolyTurretVerification`: 폭발·신성 포탑 회귀 시험 통과.
- `AutomatedGradeFiveVerification`: 최종 코드의 48종 실제 ★5 공격과 설정 및 ★4·★5 근접·투사체·범위 공격 비교 통과.
- 고정 시드 4501, 가상 입력과 일반 속도의 자연 전투 한 회에서 300초 생존, 202킬, 레벨 10, 몸통 12, HP 100, 엘리트 보상 선택 1회를 기록했다. 실제 사람의 조작 시험은 아니다.
- 실제 화면과 자연 전투 밸런스는 확인이 필요하다. Unity 검색 데이터베이스 예외 원인도 확인이 필요하다.

## 남은 사항

- 48종의 효과는 시험안이며 실제 플레이 기반 밸런스 조정이 필요하다. 실제 시청각 확인과 개별 프레임 애니메이션 보완이 남았다.
