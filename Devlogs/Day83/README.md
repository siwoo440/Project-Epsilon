# 83일차: 속성·반응과 플레이어 전투 표시

- 승인된 화풍을 참고해 투명 4×4 효과 이미지 아틀라스를 생성했다. 여덟 속성 명중, 여섯 속성 반응, 몸통 손실과 무적에 각 칸을 사용한다. 피격 표시는 물리 이미지를 적색으로 사용한다.
- 기존 속성 명중과 반응 판정에 표시만 연결했다. 시너지 단계가 없는 기본 명중도 표시하며 피해·상태·반응 규칙은 유지한다.
- 기존 공용 피해, 몸통 제거 위치, 무적 시작·종료를 재사용한다. 공용 피해 이벤트에는 실제 피격점이 없어 피격 표시는 머리 위치에 표시하고, 몸통 손실 표시는 실제 제거된 몸통 위치에 생성한다.
- 일반 명중 표시 최대 48개와 반응·플레이어 표시 최대 32개를 따로 제한하며, 표시 순서는 공격 경고선보다 낮다.
- 기획서 15번 탭의 표에 생성 이미지와 적용 상태를 추가했다. 81일차 강화형 25% 시험안은 이후 사용자 답변을 반영했다.

## Unity 비대화형 배치

- `AutomatedDay83VfxVerification`: 이미지 셀 위치, 기본 8속성 명중, 실제 6반응, 피격·몸통 손실 위치·무적 시작과 종료, 표시 상한·슬롯 반환·우선 표시·이벤트 해제 통과.
- `AutomatedFirePoisonReactionVerification`, `AutomatedRemainingReactionVerification`, `AutomatedOverlappingReactionVerification`, `AutomatedPhysicalExplosionReactionVerification`: 기존 여섯 반응과 중첩·우선순위 회귀 통과.
- `AutomatedCollisionPriorityVerification`, `AutomatedDay82MotionVerification`, `AutomatedDay76GameplaySoundVerification`: 기존 충돌 우선순위·적 모션·효과음 회귀 통과.
- `AutomatedSceneSeparationVerification`: 이전 격리 저장에 남은 맵 해금 기록 때문에 첫 실행이 실패했고, 별도 새 저장 공간에서 재실행해 통과했다.
- 플레이 모드 시작 시 기존 Unity 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 시험은 해당 로그가 있는 상태로 완료됐으며 원인은 별도 확인이 필요하다.
- 기획서 표의 텍스트·이미지 삽입은 문서 구조에서 확인했다. PDF 내보내기가 403 권한 오류로 실패해 실제 인쇄 배치는 확인하지 못했다.

## 남은 사항

- 실제 화면에서 16칸 이미지의 잘림·크기·겹침과 무적 표시의 시인성, 다수 적 환경의 성능 확인.
- 개별 상태 지속·가속·경고 VFX와 실제 피격 부위별 위치 전달은 아직 미완료다.
- 84일차 맵별 보스와 환경 요소.
