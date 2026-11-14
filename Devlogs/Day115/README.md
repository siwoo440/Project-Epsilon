# 115일차: 전투 효과음 교체

- 포탑 공격, 피격, 몸통 손실, 레벨업, 합성, 엘리트와 보스 출현의 임시 합성음 참조를 CC0 공개 효과음 일곱 개로 교체했다.
- 기존 음원 파일과 개별 음량·반복 간격·이벤트 연결은 유지한다. 제작자와 원본은 `Assets/Audio/SFX/CREDITS.md`에 기록했다.

## Unity 비대화형 배치

- `AutomatedDay115LicensedEffectsVerification`: 교체 전 기존 음원 경로가 실패했고, 교체 후 서로 다른 일곱 음원의 실제 가져오기와 재생 길이 확인이 통과했다.
- `AutomatedDay76GameplaySoundVerification`: 공격 간격 제한과 실제 피격·몸통 손실·합성·레벨업·엘리트·보스 이벤트, 음량 연결이 통과했다.
- `AutomatedDay77MusicVerification`: 기존 맵·보스·메뉴 배경음 전환과 반복이 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 선택한 효과음의 실제 청취, 반복 공격 시 청각 피로와 배경음 대비 음량은 확인하지 못했다. 음원 선정과 음량은 청취 후 조정이 필요하다.
