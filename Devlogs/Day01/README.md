# 프로젝트 ε 개발 일지 — Day01

## 개발 목표

프로젝트 ε의 본격적인 게임 시스템 개발에 앞서 Unity 프로젝트의 기본 구조를 구축하고,
이후 플레이어·무기·적·UI 시스템을 확장할 수 있는 개발 기반을 준비한다.

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 저장소 | siwoo440/Project-Epsilon |
| 기준 커밋 | `b0eb88b9e391e1d12d61e8748b8ed7224e141025` |
| 기준 커밋 제목 | `1일차 : 프로젝트 기반 구축` |

---

## 진행 내용

### 1. Unity 프로젝트 기본 구조 구축

프로젝트에서 사용할 공통 폴더 구조를 생성했다.

주요 분류는 다음과 같다.

- `Assets/Art`
- `Assets/Audio`
- `Assets/Data`
- `Assets/Prefabs`
- `Assets/Scenes`
- `Assets/Scripts`
- `Assets/Settings`

스크립트는 역할에 따라 `Core`, `Player`, `Combat`, `Enemies`, `Weapons`, `UI`, `Data`로 분리하여
이후 기능이 증가하더라도 관리하기 쉽도록 구성했다.

### 2. Game Scene 구성

`Assets/Scenes/Game.unity`를 생성하고 기본 Hierarchy를 구성했다.

```text
===Managers===
└─ GameManager

===Gameplay===
===Environment===
===UI===
Main Camera
```

`GameManager`는 `===Managers===` 하위에 배치했으며,
Main Camera는 2D 플레이를 위한 Orthographic 카메라로 설정했다.

### 3. GameManager 기본 구조 구현

게임 전체 상태를 관리하기 위한 `GameManager`를 구현했다.

현재 지원 상태:

- `Starting`
- `Playing`
- `Paused`

주요 기능:

- Singleton 기반 전역 접근
- 중복 GameManager 제거
- Scene 전환 이후 유지
- 게임 시작 상태 전환
- 일시정지 / 재개
- 게임 상태에 따른 `Time.timeScale` 처리

### 4. 기본 데이터 구조 구축

향후 콘텐츠를 코드와 분리하여 관리할 수 있도록 기본 데이터 구조를 생성했다.

구성:

- `WeaponData`
- `EnemyData`
- `CharacterData`
- `WeaponAttribute`
- `WeaponAttackType`

무기 속성은 다음 8종을 기준으로 구성했다.

- Physical
- Fire
- Cold
- Electric
- Poison
- Explosion
- Holy
- Dark

공격 형태는 다음 공통 분류를 사용할 수 있도록 준비했다.

- Melee
- StraightProjectile
- HomingProjectile
- Area
- Persistent
- Special

### 5. Git 프로젝트 설정

Unity에서 자동 생성되는 캐시·임시 파일이 저장소에 포함되지 않도록 `.gitignore`를 구성했다.

주요 제외 항목:

- `Library`
- `Temp`
- `Obj`
- `Build`
- `Builds`
- `Logs`
- `UserSettings`
- IDE 임시 파일

반대로 `Assets`, `Packages`, `ProjectSettings`는 프로젝트 재현에 필요한 파일이므로 저장소에서 관리한다.

---

## 확인 결과

GitHub 최신 커밋을 기준으로 프로젝트 구조를 검토했다.

- Unity 버전이 `6000.3.21f1`로 설정되어 있음
- `Game.unity` 존재 확인
- `GameManager`가 Scene에 연결되어 있음
- `GameManager` 기본 상태 관리 코드 확인
- 데이터 스크립트 구조 존재 확인
- Unity용 `.gitignore` 적용 확인
- 1일차 개발 진행을 막는 구조적 문제는 확인되지 않음

단, 현재 저장소에는 GitHub Actions 등의 CI가 없으므로
**Unity Editor에서의 실제 스크립트 컴파일 및 Console Error 0 여부는 로컬 Unity 실행으로 최종 확인해야 한다.**

---

## 1일차 완료 기준

- [x] Unity 프로젝트 생성
- [x] Git 저장소 연결
- [x] 기본 폴더 구조 구성
- [x] `Game` Scene 생성
- [x] 기본 Hierarchy 구성
- [x] `GameManager` 구현
- [x] `WeaponData` 기본 구조 구현
- [x] `EnemyData` 기본 구조 구현
- [x] `CharacterData` 기본 구조 구현
- [x] 무기 속성·공격 형태 Enum 구성
- [x] Unity용 `.gitignore` 적용
- [ ] 로컬 Unity Console Error 0 최종 확인

---

## 다음 개발 방향

Day02에서는 다음 기능을 한 번에 구축한다.

**Input 시스템 → 테스트 플레이어 → 카메라 추적 → 기본 HUD**

확인 목표:

- 좌우 회전 입력 인식
- 가속 입력 인식
- 테스트 플레이어 조작 기반 생성
- 카메라가 플레이어를 추적
- HP / 몸 길이 / XP / 시간 UI 기본 표시
