# 95일차: 암흑 저주·부식 저주 지속 표시

- 실제 암흑 저주와 독·암흑 반응의 부식 저주가 유지되는 동안 기존 속성·반응 이미지를 적 위에 표시한다. 냉기·독과 겹쳐도 위치를 분리하고 상태별 해제와 만료를 독립적으로 반영한다.
- 93일차의 최대 48마리 표시 제한과 자리 반환을 공유하며 실제 활성 상태에 필요한 이미지 객체만 생성한다. 피해, 저주와 반응 수치는 변경하지 않는다.

## Unity 비대화형 배치

- `AutomatedDay95CurseIndicatorVerification`: 수정 전 중첩 상태의 저주 표시 누락을 확인했다. 수정 후 네 상태의 표시와 독립 해제·만료, 실제 암흑→독 공격에서 부식 저주로의 표시 전환을 통과했다.
- `AutomatedDay93StatusIndicatorVerification`: 기존 냉기·독 표시, 일시정지, 48마리 상한과 표시 자리 재사용 회귀를 통과했다.
- `AutomatedRemainingReactionVerification`, `AutomatedFirePoisonReactionVerification`, `AutomatedDay83VfxVerification`: 기존 속성 반응과 전투 효과 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 게임 화면에서 네 표시의 크기·겹침·경고선 가독성과 다수 적의 성능은 확인하지 못했다. 다른 지속 상태 전용 표시는 아직 별도다.
