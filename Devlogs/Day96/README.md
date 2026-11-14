# 96일차: 부식 연소 지속 표시

- 화염·독 반응으로 발생하는 부식 연소가 유지되는 적에 기존 반응 이미지를 표시한다. 기존 냉기·독·암흑·부식 저주 표시와 48마리 상한을 공유한다.
- 실제 상태 시작, 일시정지, 개별 만료와 적 제거에 따라 표시를 관리한다. 반응 피해량과 지속 시간은 변경하지 않는다.

## Unity 비대화형 배치

- `AutomatedDay96CorrosiveBurnIndicatorVerification`: 수정 전 양쪽 공격 순서에서 지속 표시 누락을 확인했다. 수정 후 화염→독과 독→화염의 실제 반응, 일시정지와 개별 만료를 통과했다.
- `AutomatedDay95CurseIndicatorVerification`, `AutomatedDay93StatusIndicatorVerification`: 기존 상태 표시 전환과 48마리 상한·자리 재사용 회귀를 통과했다.
- `AutomatedDay94BurnFlashVerification`, `AutomatedFirePoisonReactionVerification`, `AutomatedDay83VfxVerification`: 화상·피격 표시, 기존 화염·독 반응과 전투 효과 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 화면에서 표시 크기·배치·위험 경고와의 가독성, 다수 적의 성능은 확인하지 못했다.
