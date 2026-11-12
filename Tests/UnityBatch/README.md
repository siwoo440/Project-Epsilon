---

# Unity 비대화형 통합 검증

이 폴더에는 실제로 실행한 Unity 에디터 배치 검증 스크립트를 보관한다. 제품 씬이나 런타임 스크립트가 아니며, 반드시 **별도 프로젝트 사본**에서 실행한다. 테스트는 끝나면 편집기 프로세스를 종료한다.

27일차 시작 무기 선택을 추가하면서 기존 배치에 공용 선택 헬퍼를 연결했다. 아래 과거 결과는 변경 전 스크립트와 해당 일차 제품의 실행 결과이며, 헬퍼를 연결한 현재 버전의 전체 Unity 회귀는 아직 완료되지 않았다.

---

## 준비와 실행

1. `Assets`, `Packages`, `ProjectSettings`를 포함한 Unity 프로젝트 사본을 별도 폴더에 준비한다. 원본 프로젝트나 다른 작업 중인 프로젝트를 사용하지 않는다.
2. 선택한 검증 스크립트와 `StartingWeaponBatchHelper.cs`를 사본의 `Assets/Editor`에 복사한다. 공용 헬퍼는 실제 시작 무기 첫 후보 버튼을 선택한 뒤 각 검증을 진행한다. 시작 무기 선택 자체를 검사하는 전용 스크립트에는 헬퍼를 적용하지 않는다. 원본 프로젝트의 `Assets`에는 복사하지 않는다.
3. 프로젝트 버전인 Unity **6000.3.21f1**의 `Unity.exe`를 사용한다. `-batchmode`, `-nographics`, 사본을 가리키는 `-projectPath`, 아래 표의 `-executeMethod`, 별도 `-logFile`을 지정한다. `-quit`은 플레이 모드 진입 전에 종료될 수 있으므로 사용하지 않는다.
4. 프로세스가 종료된 뒤 로그의 테스트 접두사와 종료 코드를 확인한다. Unity 초기 에셋 가져오기와 컴파일 시간은 플레이 시간과 별개다.

게임 씬을 직접 여는 기존 배치 검증에는 `-epsilonDirectGame`을 명시한다. 이 옵션은 비대화형 배치에서만 직접 전투 시험을 허용한다. 일반 실행과 화면 분리 검증은 이 옵션 없이 메인 메뉴·출격 준비·전투·결과 씬을 순서대로 사용한다.

38일차 `AutomatedSizeAndHitVerification.cs`는 `NaturalSpearFiveMinute/AutomatedNaturalCombatVerification.cs`와 `StartingWeaponBatchHelper.cs`를 함께 사본에 복사해야 한다. 같은 이름의 다른 자연 전투 스크립트는 동시에 복사하지 않는다. 시험은 실제 원형 충돌체 크기를 시험 조종기가 참조하는지와 플레이어·적·보스·보석 크기, 경험치 보존 및 실제 피격 색상 복구를 확인한다.

38일차 확대 이후 자연 전투는 정상 속도·자연 적 출현·가상 A/D·Space 입력·자동 보상 선택을 사용했다. 최초 조종기는 게임 시간 33.3초에 레벨 1로 패배했고, 실제 몸통 간격과 충돌 반경을 읽도록 조종기를 보정한 재시험도 36.7초에 레벨 1로 패배했다. 두 시험 모두 마지막 몸통 손실의 호출 경로는 적 직접 충돌이었고 Unity 종료 코드는 1이었다. 시험의 몸통 변경 이벤트는 손실 시 호출 경로를 기록한다. 무작위 조건이 다른 두 번의 결과만으로 확대와 패배 사이의 인과관계를 확정할 수 없다.

39일차 24종 보상 풀의 같은 강철 창 자연 전투는 정상 속도, 자연 출현, 가상 입력과 자동 보상 선택 조건에서 게임 시간 76.8초에 적 직접 충돌로 패배해 종료 코드 1이었다. 60초 시점에 자연 처치 14회, 미획득 보석 14개, 레벨 1과 경험치 0을 관찰했으며 자연 레벨업과 5분 생존은 확인하지 못했다. 사용자 수동 플레이와는 구분한다.

40일차 사거리 제한 제거와 36종 보상 풀 적용 후 같은 자동 조종 자연 전투는 게임 시간 67.7초에 적 직접 충돌로 패배해 종료 코드 1이었다. 60초 시점에 18처치와 경험치 5, 레벨 1을 기록했다. 무작위 출현 조건이 다른 이전 시험과 결과만으로 변경의 영향을 단독 판단할 수 없다.

41일차 실제 씬 배치에서 48종 시험 무기의 몸통 공격·보상 풀, 신성 보호막·암흑 저주, 독·폭발, 최근접 공격·체력·경험치·스태미나 막대, 보상 카드 배치, 일반 적·엘리트·보스 이동 속도 절반, 레벨업 합성·엘리트 보상·사수·냉기·기존 아트가 각각 종료 코드 0이었다. 보스 돌진 시험은 최초 연속 시험 간 이전 물리 접촉이 남아 실패했으며 보스 위치 분리와 한 물리 스텝 이후 재시험에서 실제 머리·몸통 접촉, 절단 및 무적이 종료 코드 0이었다. 사용자의 화면 가독성이나 수동 플레이는 확인하지 않았다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

적 속도 조정 후 자동 조종 자연 전투는 129.0초에 패배해 종료 코드 1이었다. 120초 시점에 자연 처치 25회·경험치 3·레벨 1을 관찰했고 자연 레벨업, 4분 엘리트와 5분 생존은 확인하지 못했다.

42일차 3등급 시험은 수정 전 범위 설정 부재로 종료 코드 1, 수정 후 48종 데이터 및 각 무기의 실제 3등급 두 표적 공격, 대표 근접·직선·범위 무기의 2·3등급 비교로 종료 코드 0이었다. 화염탄·1등급 48종·HUD·보상 합성·신성·암흑 회귀도 각각 종료 코드 0이었다. 화염탄 기존 시험은 다른 무기의 전달 금지를 가정해 최초 실패했으며, 전달을 비활성화한 데이터 사본을 사용하도록 조정해 기존 비활성 경계와 화염탄 전달을 검증했다. 실제 자연 전투는 이 일차에 다시 실행하지 않았다.

43일차 5등급 시험은 수정 전 설정 부재로 종료 코드 1, 수정 후 48종 데이터와 무기별 실제 공격 및 대표 무기의 4·5등급 차이로 종료 코드 0이었다. 48종 3등급과 화염탄·HUD·보상 합성·신성·암흑 회귀도 각각 종료 코드 0이었고, 실제 5등급 투사체의 발사 후 데이터 변경에 대한 발사 시 배율 유지도 확인했다. 자연 전투는 이 일차에 다시 실행하지 않았다.

44일차 Unity 배치에서 여섯 캐릭터 데이터·실제 시작 능력치·패시브·후보 가중치, 실제 방향키 선택·설정 우선 및 캐릭터별 무기 시작이 종료 코드 0이었다. 초기 방향키 시험은 입력 설정 정리 중 이전 설정이 소멸해 중단됐고, 기존 설정이 유효할 때만 복원하는 수정 후 여섯 캐릭터 순환과 실제 시작을 확인했다. 포탑별 최근접 조준, 속성별 색상, 몸통·꼬리 및 머리·몸통 실제 충돌 범위 축소와 시작 무기·보상·충돌·보스·48종 무기 회귀도 각각 종료 코드 0이었다.

캐릭터 이동 보정에 적 속도가 따라간 선행 시험은 기동형 적 속도 1.32와 기대 1.2의 차이로 종료 코드 1이었다. 적 속도를 보정 전 플레이어 기본 속도에 연결한 재시험에서 기동형·중장형 적 속도와 일반 적·엘리트·보스의 절반 속도는 각각 종료 코드 0이었다.

포탑·몸통 변경 후 캐릭터 기능 적용 전 자동 조종 자연 전투는 339.3초·레벨 10·197처치·엘리트 보상 선택 1회로 종료 코드 0이었다. 합성 재료 화면에서 자동 조종기가 이전 버튼을 반복한 뒤 화면이 닫혀 시험이 완료됐다. 재료 확정 버튼을 선택하도록 조종기를 고친 별도 실제 합성 시험은 종료 코드 0이었다. 캐릭터 기능 적용 후 자연 전투는 121.9초·레벨 1에서 패배해 종료 코드 1, 적 속도 보완 후 마지막 자연 전투도 86.6초·레벨 1에서 패배해 종료 코드 1이었다. 무작위 조건이 다른 자동 조종 시험이며 사용자 수동 플레이는 아니다.

34일차 `AutomatedBossTransitionVerification.cs` SHA-256 `F2517686492B0736C4D8600EC5C9756FE9866795EF76AB756374659AA1836EDE`는 실제 씬에서 32배 누적 1,200.003초에 단일 전환과 관리 적 40마리 무보상 정리, XP·HP 회복 보존, 몸통 복구와 적 투사체 제거, 미확정 합성 재료 보존과 재개, 보스 50% 단계 및 강제 처치 승리를 확인해 종료 코드 0이었다. 이동·충돌과 적 행동을 통제하고 보스를 직접 치명 피해로 처치한 시험이다. 기존 33일차 사본의 선행 배치는 전환 API 부재로 종료 코드 1이었다.

`AutomatedRunClockVerification.cs` SHA-256 `CF25E310166934019854757488242C128912D13A46640EE702D9E149DAB4BDE6`는 실제 씬에서 같은 프레임 일시정지 전 경과 0.000178초를 보존하고 HUD와 실제 전투 시간이 같으며, 정지 중 시간 유지·표시 전용 초기화·재개가 동작해 종료 코드 0이었다. 게임 관리자 API로 일시정지한 통제 시험이다.

34일차 초반 웨이브 회귀의 첫 실행은 레벨업 직후 같은 프레임에 보석 객체가 조회돼 종료 코드 1이었다. 보강 스크립트 SHA-256 `22F63CD799B90DFBF200DB3B50CC50BA87C0F416B0C27FD354F86F802A54E1B1`은 실제 8배 누적 240.011초, 강제 엘리트 사망·20 XP 보석·연속 레벨업과 엘리트 보상 UI에서 종료 코드 0이었다. 보강 후 같은 프레임 분기는 재발하지 않아 첫 실패의 원인은 확정하지 않았다. 각 Unity 실행에서 에디터 Search 시작 인덱싱 예외가 별도로 발생했다.

35일차 `AutomatedBossFanVerification.cs` SHA-256 `D46C2D133CDCD6EAB0B58869A210AC51D69535897098B9EDAB5B1284E1644D23`은 34일차 사본에서 보스가 한 발만 발사해 종료 코드 1, 수정 사본에서는 일반 사수 1발과 보스 1단계 3발·2단계 5발의 대칭 각도 및 실제 몸통 접촉 HP 15 피해를 확인해 종료 코드 0이었다. 보스 출현과 사격은 강제로 호출했고 자동 출현과 이동을 통제했다. 기존 실제 사수·출현 회귀도 종료 코드 0이었다. 두 수정본 Unity 실행에서 에디터 Search 시작 인덱싱 예외가 별도로 발생했다.

36일차 돌진 선행 스크립트 SHA-256 `493FAEBB1510A891B344FD140889B2AD2ACBB3E8141C73FD79165AB7903AA4B4`는 35일차 사본에서 돌진 행동이 없어 종료 코드 1, 수정 사본의 실제 예고·1단계 HP 피해·2단계 절단에서 종료 코드 0이었다. 보강 스크립트 SHA-256 `0B398950085F38821528C814D7281736A068427F2724D7747892FE146FE838C1`은 실제 예고 중 정지·재개, HP 100→85, 몸통 3→2와 HP 100 유지, 전체 무적 중 HP·몸통 유지까지 확인해 종료 코드 0이었다. 보스 생성과 공격 시각, 예고·이동 수치는 통제했다. 기존 실제 보스 부채꼴 사격과 32배 가속 0→1,200.004초 전환·승리 회귀도 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외는 각 실행에서 별도로 발생했다.

36일차 최종 돌진 스크립트 SHA-256 `2698DDDB32F81EA3971ABB0EC53346710D2D99AAF13A2CB23AACE841033FCBCC`는 같은 통제 조건에서 실제 물리 콜백의 몸통 선행 진입 3회·머리 진입 1회와 몸통 손실 1칸을 함께 확인해 종료 코드 0이었다. 접촉 수는 하나의 실행에서 관측된 값이다.

37일차 `AutomatedPoisonShotVerification.cs` SHA-256 `80583B4D2D7337C65AA2980CFDC4C15234D9AAF8D0A819D82E9228A5B5C39DD1`은 36일차 사본에서 독 2단계 사수의 실제 피해가 21로 유지돼 종료 코드 1이었다. 수정 사본의 실제 몸통 접촉에서는 독 미적용 21, 발사 전 독 적용 20, 발사 후 독 제거 20, 발사 후 독 적용 21, 기본 피해 8의 독 적용 8을 확인해 종료 코드 0이었다. 사수 생성과 발사, 이동·자연 출현은 통제했다. 발사 시점 피해 고정과 정수 올림은 임시 정책이며 기획 확정값이 아니다. 실제 보스 부채꼴 사격 회귀도 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외는 각 실행에서 별도로 발생했다.

37일차 `AutomatedBossRepairVerification.cs` SHA-256 `D35926B83EE2D548722B76DD124741A2BB8C34A2D35FEF93E5657CC9EC744D9C`는 보스전에도 별도 몸통 복구 API가 허용돼 수정 전 종료 코드 1, 수정 후 보스 전 복구 허용·보스전 API/아이템/엘리트 복구 보상 제한·실제 레벨업의 현재 몸 2→3과 성장 최대 3→4를 확인해 종료 코드 0이었다. 전환 시간을 1,199.99초로 주입하고 이동·충돌·공격은 통제했다. 기존 실제 몸통 성장·연속 레벨업과 4분 엘리트 무기 보상 회귀도 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외가 각 실행에서 별도로 발생했다.

37일차 냉기 선행 스크립트 SHA-256 `D5166463B656000C0367C51E7A743F379D2B8A6B46ABB5FE8A51B9867C7EC33A`는 일반 적의 최대 누적 이후 추가 명중에 빙결이 없어 종료 코드 1이었다. 최종 스크립트 SHA-256 `72F8F6738C2E7131F11128917969C560E949B0DDE039D4E26FBBEE25F732BC44`는 실제 속성 명중을 직접 호출하고 보스를 강제 생성한 조건에서 낮은 단계 감속, 고단계 일반 적 빙결과 보스 강한 감속·실제 이동, 조정 가능한 추가 명중·지속 시간, 일시정지와 효과 종료를 확인했다. 기존 실제 화염탄·화상과 보스 돌진 회귀도 종료 코드 0이었다. 추가 명중 조건, 빙결 시간과 보스 감속 배율은 기획 미정 임시값이다. Unity Editor Search 시작 인덱싱 예외가 각 실행에서 별도로 발생했다.

37일차 보스 밀림 선행 스크립트 SHA-256 `EA7F7D3504F9541B0AAD1756F7A602F1FE01C4735F8A6D54083F44C60B2B05D8`은 밀림·몸통별 유예 부재로 종료 코드 1이었다. 수정 중 지속 겹침이 유예 후 처리되지 않은 사례에서는 머리 2D 물리체의 절전 상태와 지속 접촉 콜백 중단을 관찰했다. 최종 스크립트 SHA-256 `2212AD8E8F74785E76AEC5AE30D1E1FE5C0650CDC6E012529369D95BB0D734B7`은 비활성 추적기의 밀림·유예 차단, 실제 밀림·경로 복귀, 같은 스텝 유예, 다른 몸통 충돌, 정지, 절전 중 지속 겹침과 예약 뒤 같은 스텝 분리를 확인해 종료 코드 0이었다. 보스는 강제 생성하고 이동·공격과 몸통 위치를 통제했다. 실제 동시 충돌 우선순위·비활성 충돌·보스 돌진 회귀도 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외가 각 실행에서 별도로 발생했다.

37일차 위험 구역 스크립트 SHA-256 `75FB576DC7F382062D37B97B614938A7B778FB4490B6F57BCCA3427000B0B885`는 구현 전 위험 구역 설정 부재로 종료 코드 1이었다. 수정 사본에서는 실제 예고와 정지·회피·만료, 머리와 몸통 동시 접촉의 주기당 단일 피해 및 보스 사망 시 정리를 확인해 종료 코드 0이었다. 보스와 구역 생성 시각, 플레이어와 몸통 위치를 통제했다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

37일차 보스 소환 선행 시험은 설정 부재로 종료 코드 1이었다. 수정 후 첫 시험은 처치 직후 물리 스텝의 목록 갱신을 기다리지 않아 실패했다. 보강 스크립트 SHA-256 `64AC48134E15756736F4373BA2A1C54C7D3734A702CA5C9CFF7FBA449F0C4F5D`는 실제 소환 상한·무보상·처치 후 재소환·일시정지·승리 정리를 확인해 종료 코드 0이었다. 보스 전환과 소환 간격, 자동 공격·이동을 통제했다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

37일차 청크 스크립트 SHA-256 `87B2FB981B0084E6A58EE9FCA6839FD327C92C9F8AA96B4CFE9A6513513E1A4E`는 구현 전 실제 씬 구성 부재로 종료 코드 1, 수정 후 3×3 생성·유지·이탈 제거, 정지, 음수 좌표와 먼 거리 이동 확인으로 종료 코드 0이었다. 플레이어 위치를 직접 변경한 시험이며 빈 청크의 생명주기만 검사한다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

37일차 냉기 누적 선행 시험은 시너지 없는 기본 냉기 명중에서 감속이 없어 종료 코드 1이었다. 최종 스크립트 SHA-256 `7880C726E921D3697D9ED4C805CA16ACE86ED64E354ECDFCFC567EDA0E49422A`는 실제 속성 명중을 직접 호출해 기본 10%·20%·30% 감속과 최대 누적, ×2의 감속량 1.15배 임시 해석, 낮은 단계 빙결 방지 및 기존 일반 적·보스 효과를 확인해 종료 코드 0이었다. 초반 시험 무기와 보스 돌진 회귀도 각각 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

37일차 이미지 적용 전 수정본에서 자연 전투 스크립트 SHA-256 `6ECD5A56ADEC76058440F3BC819BFE3964A7D67F62A2638A689D39BB87818957`을 정상 속도 300초 실행해 자연 처치 153회, 레벨 9, 잔여 경험치 18, 선택 11회, 몸통 3칸과 HP 100으로 종료 코드 0이었다. 가상 이동과 선택 UI 자동 선택을 사용한 결과이며 이미지 적용 후 시험이 아니다.

37일차 플레이어 이미지 스크립트 SHA-256 `4608F861965EFB051E4F8628B699BED649DBF3AF8ADC34664EC3D154C369028F`는 적용 전 머리 이미지 부재로 종료 코드 1, 수정 후 실제 씬의 머리·몸통·꼬리, 성장 후 이미지·충돌체와 적·보스 이미지 분리를 확인해 종료 코드 0이었다. 기존 몸통 성장·보스 밀림·시험 무기 회귀도 종료 코드 0이었다. 화면의 시각적 품질은 검사하지 않았다.

37일차 일반 적 이미지 시험은 종류별 이미지 부재로 종료 코드 1, 이미지 연결 후 추가한 방향 시험은 기존 위쪽 방향이 유지돼 종료 코드 1이었다. 최종 스크립트 SHA-256 `41397813724E20D9B88B1EF0EC0A35B28D996932A3D6A607B7949F7420E18A99`는 실제 일반 적 3종의 이미지·목표 방향·충돌체와 기존 보상 이미지 유지로 종료 코드 0이었다. 적 출현 위치와 플레이어 이동을 통제했다. 시험 무기·사수 출현·보스 소환·충돌 우선순위 회귀도 각각 종료 코드 0이었다.

37일차 엘리트·보스 이미지 시험은 엘리트 이미지 부재로 종료 코드 1이었다. 확장 스크립트 SHA-256 `1F048D129449E0E6119F60ACD94D374556AAB040EDDFC592D9DE2C40FE69F88E`는 일반 적·엘리트·보스 이미지, 엘리트 고정 돌진과 보스 예고 방향, 기존 보상·보스 투사체 이미지 유지로 종료 코드 0이었다. 출현 위치와 돌진 방향·시각을 통제했다. 기존 엘리트 보상·보스 돌진·부채꼴 사격·몸통 밀림 회귀도 각각 종료 코드 0이었다.

37일차 무기·보석·바닥 이미지 선행 시험은 무기 이미지 데이터 부재로 종료 코드 1, 후보 UI 보강 전 시험은 후보 이미지 부재로 종료 코드 1이었다. 최종 스크립트 SHA-256 `6A5A38DA5F8AB0C7CA921C79812E3D770CF1D53D7101FFA56179C506FDAA76E0`은 무기 6종의 장착·합성·성장·손실 표시, 선택 UI와 다른 화면 복구, 보석 3단계·병합·대체 이미지, 바닥 3×3과 인접 반전을 확인해 종료 코드 0이었다. 적 출현·이동과 시험 데이터를 통제했으며 실제 화면 품질은 검사하지 않았다. 기존 보석 병합·부분 병합, 몸통 성장, 시험 무기, 청크, 플레이어 이미지, 시작 무기, 레벨 보상 합성, 설정 입력 우선순위, 엘리트 보상과 가속 보스 전환 회귀도 각각 종료 코드 0이었다. Unity Editor Search 시작 인덱싱 예외가 별도로 발생했다.

| 스크립트 | 실행할 메서드 | 실제로 실행한 조건 |
| --- | --- | --- |
| `AutomatedPilotVerification.cs` | `ProjectEpsilon.Editor.AutomatedPilotVerification.Run` | 정상 속도 300초, 가상 A/D·Space 회피 입력, 레벨업·보상 UI 자동 선택. 무적·능력치 변경 없음. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedEliteRewardVerification.cs` | `ProjectEpsilon.Editor.AutomatedEliteRewardVerification.Run` | 출현기 시간을 240초로 설정하고 엘리트를 강제 처치. XP 보석과 보상 일시정지, 실제 UI 버튼 이벤트, 장착·재개 확인 |
| `AutomatedEarlyWaveVerification.cs` | `ProjectEpsilon.Editor.AutomatedEarlyWaveVerification.Run` | 실제 출현 시간 누적을 8배로 가속하고 일반 적을 관측 후 제거. 4분 자동 엘리트를 강제 처치하고 실제 XP 보석·연속 레벨업·엘리트 보상 UI 순서 확인 |
| `AutomatedBossTransitionVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossTransitionVerification.Run` | 실제 씬의 출현 시간을 0초부터 32배로 누적하고 적 이동·공격과 플레이어 이동·충돌을 통제. 20분 전환 시 적·아이템 정리, 합성 보류, 시험 보스 단계와 강제 처치 승리 확인 |
| `AutomatedBossFanVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossFanVerification.Run` | 보스 생성·사격을 강제하고 이동·자연 출현을 통제. 실제 일반 사수 단발, 보스 1·2단계 부채꼴 각도와 실제 2D 투사체 접촉 피해 확인 |
| `AutomatedBossChargeVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossChargeVerification.Run` | 보스 생성·돌진 시각과 예고·속도를 통제. 실제 2D 접촉의 1단계 HP 피해, 2단계 몸통 절단, 예고 중 정지와 전체 무적 확인 |
| `AutomatedPoisonShotVerification.cs` | `ProjectEpsilon.Editor.AutomatedPoisonShotVerification.Run` | 사수 생성과 발사를 통제하고 실제 독 시너지의 발사 전후 적용·제거와 2D 투사체 접촉 피해 확인 |
| `AutomatedBossRepairVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossRepairVerification.Run` | 전환 시간을 1,199.99초로 주입하고 이동·충돌·공격을 통제. 보스 전후 별도 복구, 엘리트 후보와 실제 레벨업 성장 확인 |
| `AutomatedColdFreezeVerification.cs` | `ProjectEpsilon.Editor.AutomatedColdFreezeVerification.Run` | 실제 속성 명중을 직접 호출하고 보스를 강제 생성. 일반 적 빙결, 보스 강한 감속·물리 이동, 설정·정지·효과 종료 확인 |
| `AutomatedBossPushVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossPushVerification.Run` | 보스를 강제 생성하고 이동·공격과 몸통 위치를 통제. 실제 몸통 밀림·경로 복귀, 몸통별 유예, 같은 스텝 분리와 절전 중 재판정 확인 |
| `AutomatedBossHazardVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossHazardVerification.Run` | 보스와 구역 생성 시각·위치를 통제. 예고·정지·회피·주기 피해와 승리 시 구역 제거 확인 |
| `AutomatedBossSummonVerification.cs` | `ProjectEpsilon.Editor.AutomatedBossSummonVerification.Run` | 보스 전환과 소환 간격을 통제. 소환 상한·무보상·재소환·일시정지와 승리 정리 확인 |
| `AutomatedChunkMapVerification.cs` | `ProjectEpsilon.Editor.AutomatedChunkMapVerification.Run` | 플레이어 위치를 직접 변경. 3×3 청크 생성·유지·제거, 일시정지와 음수 좌표 확인 |
| `AutomatedPlayerArtVerification.cs` | `ProjectEpsilon.Editor.AutomatedPlayerArtVerification.Run` | 실제 씬의 플레이어 머리·몸통·꼬리와 성장, 충돌체, 적·보스 이미지 분리 확인 |
| `AutomatedEnemyArtVerification.cs` | `ProjectEpsilon.Editor.AutomatedEnemyArtVerification.Run` | 적 생성 위치·플레이어 이동과 돌진 방향을 통제. 일반 적·엘리트·보스 이미지, 목표와 돌진 방향, 충돌체와 보상·투사체 이미지 확인 |
| `AutomatedCoreArtVerification.cs` | `ProjectEpsilon.Editor.AutomatedCoreArtVerification.Run` | 무기 데이터와 슬롯·UI를 통제. 무기 6종, 보석 3단계·병합, 3×3 바닥 이미지와 경계 반전 확인 |
| `AutomatedRunClockVerification.cs` | `ProjectEpsilon.Editor.AutomatedRunClockVerification.Run` | 실제 씬에서 같은 프레임의 일시정지 전 게임 시간, HUD 동기화, 정지 유지, 표시 전용 초기화와 재개를 확인 |
| `AutomatedEliteScheduleVerification.cs` | `ProjectEpsilon.Editor.AutomatedEliteScheduleVerification.Run` / `RunExternalDestroy` | 실제 출현 시간을 32배로 가속하고 일반 적 만석을 유지. 엘리트 FIFO 대기·공존·보상과 별도의 외부 파괴 경계 확인 |
| `AutomatedShooterSpawnVerification.cs` | `ProjectEpsilon.Editor.AutomatedShooterSpawnVerification.Run` | 실제 사수의 물리 발사·머리 기준 거리 유지 및 32:9 이동 카메라의 일반·엘리트 출현 위치와 정리, 렌더 직전 뷰포트 확인 |
| `AutomatedBodyHudVerification.cs` | `ProjectEpsilon.Editor.AutomatedBodyHudVerification.Run` | 초기 물리 단계 뒤 게임을 일시정지하고 성장·손실·복구 API를 직접 호출. 세 수치, 이벤트, HUD 텍스트와 너비 확인 |
| `AutomatedCombatProgressionVerification.cs` | `ProjectEpsilon.Editor.AutomatedCombatProgressionVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체 없는 정지 표적을 배치. 실제 몸통 무기 공격, 보석, 레벨업과 UI 버튼 이벤트 확인 |
| `AutomatedEarlyWeaponVerification.cs` | `ProjectEpsilon.Editor.AutomatedEarlyWeaponVerification.Run` | 이동·몸통 추적·자연 스폰을 끄고 세 번째 몸통과 정지 표적을 배치. 시험 무기 36종의 자동 공격·냉기와 보상 풀·이미지, 통제 생성·강제 처치한 일반 적 3종의 경험치 확인 |
| `AutomatedGlobalAttackHudVerification.cs` | `ProjectEpsilon.Editor.AutomatedGlobalAttackHudVerification.Run` | 사거리 밖 최근접 공격·대상 변경·쿨타임·투사체 도달·범위 폭발과 실제 체력·경험치 막대 확인 |
| `AutomatedDay40StatusVerification.cs` | `ProjectEpsilon.Editor.AutomatedDay40StatusVerification.Run` | 실제 독 최대 5중첩과 폭발 ×4 넉백 증가, 보스 넉백 면역 확인 |
| `AutomatedDay39ElectricVerification.cs` | `ProjectEpsilon.Editor.AutomatedDay39ElectricVerification.Run` | 실제 속성 명중으로 전기 ×0·×2·×4의 연쇄 대상과 범위 확인. 실제 전기 무기 4개의 합성 후 ×2 단계 갱신 확인 |
| `AutomatedSteelSwordGradeVerification.cs` | `ProjectEpsilon.Editor.AutomatedSteelSwordGradeVerification.Run` | 실제 시작 버튼을 자동 선택하고 이동·몸통 추적·자연 스폰을 끈 뒤 실제 세 번째 몸통과 정지 표적을 배치. 강철 검 등급별 자동 공격, 실제 사망 콜백의 대상 변경과 물리 시너지 확인 |
| `AutomatedFireballGradeVerification.cs` | `ProjectEpsilon.Editor.AutomatedFireballGradeVerification.Run` | 이동·몸통 추적·자연 스폰을 끄고 실제 세 번째 몸통과 정지 2D 표적을 배치. 화염탄 등급별 자동 발사·전달·화상·시너지와 실제 물리 접촉 경계 확인 |
| `AutomatedExperienceMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedExperienceMergeVerification.Run` | 실제 씬에서 플레이어 이동과 자연 출현을 끄고 보석과 검사 설정을 통제. 자동 병합과 경험치 보존, 우선순위, 일시정지, 활성 상태 및 실제 수집 확인 |
| `AutomatedBodyGrowthVerification.cs` | `ProjectEpsilon.Editor.AutomatedBodyGrowthVerification.Run` | 실제 씬의 통제된 몸통·체력 상태에서 보석을 수집하고 실제 레벨업 보상 버튼을 선택. 세 몸통 수, 공용 HP, 연속 보상과 HUD 확인 |
| `AutomatedExperienceMergePartialVerification.cs` | `ProjectEpsilon.Editor.AutomatedExperienceMergePartialVerification.Run` | 실제 씬에서 이동·자연 출현을 끄고 보석·검사 예산을 통제. 다중 프레임 부분 그룹, 동적 삽입, 선택 재료 이동·수집과 경험치 보존 확인 |
| `AutomatedStartingWeaponVerification.cs` | `ProjectEpsilon.Editor.AutomatedStartingWeaponVerification.Run` | 공용 헬퍼 없이 세 번의 새 플레이에서 실제 시작 후보 UI, 가상 숫자 입력, 시작 전 정지와 첫 몸통 장착 확인. `RunDisabledBeforeStart`는 선택기 비활성·재구성 경계를 별도로 확인 |
| `AutomatedSettingsPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedSettingsPriorityVerification.Run` | XP 주입과 만석 슬롯을 강제하고 가상 ESC·숫자 입력 및 실제 UI 버튼 이벤트 사용. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedCollisionPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedCollisionPriorityVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체를 제어. 실제 2D 물리 접촉과 별도의 인위 콜백 순서·수명주기 시험 사용 |
| `AutomatedDisabledCollisionVerification.cs` | `ProjectEpsilon.Editor.AutomatedDisabledCollisionVerification.Run` | 실제 2D 물리 접촉에서 비활성 충돌기·적 행동·적 투사체와 재활성 후 재접촉, 동일 스텝 자기 충돌 예약 및 보호막 우선순위 확인 |
| `AutomatedMergeSelectionVerification.cs` | `ProjectEpsilon.Editor.AutomatedMergeSelectionVerification.Run` | 실제 게임 씬에서 자연 출현과 충돌을 끄고 몸통 20칸과 무기를 강제 구성. 가상 E·ESC·이동 입력과 실제 UI 버튼 이벤트로 합성 선택, 시너지 미리보기와 우선순위 확인 |
| `AutomatedRewardMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedRewardMergeVerification.Run` | 실제 게임 씬에서 이동과 자연 출현을 끄고 장착 무기·강화와 미장착 보상 값을 제어. UI 없이 실제 무기 관리자의 보상 합성 API와 시너지 확인 |
| `AutomatedLevelRewardMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedLevelRewardMergeVerification.RunAll` | 실제 게임 씬에서 자연 출현과 자기 충돌을 끄고 경험치·장착 무기·엘리트 후보를 제어. 가상 E·ESC·숫자 입력, 실제 UI 버튼 이벤트와 통제된 2D 적 접촉 사용 |
| `AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 정상 속도, 자연 적 출현·무기 공격·보석 획득과 가상 A/D·Space 조종. 능력치와 경험치 주입 없음, 선택 화면은 자동 선택 |
| `NaturalTrajectory/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 정상 속도와 자연 출현을 유지하고, 조종기가 가속·회전 경로와 몸통 접촉 위험을 비교한 세 번째 시험 |
| `NaturalBalanced/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 24일차 합성 구현 사본에서 공격 거리 선호를 높이고 실제 무기 거리와 적 피해를 기록한 네 번째 시험 |
| `NaturalFocus/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 자연 적 한 명의 체력·거리를 추적하면서 연속 공격과 안전 이동을 시험한 다섯 번째 시험 |
| `NaturalDiagnostics/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 동일 조종 정책에서 실제 위치 변화·추적 적 속도·조향 후보 점수를 기록하는 최대 60초 진단 |
| `NaturalCurve/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 적을 향한 곡선 재접근 후보를 비교한 자연 전투 시험 |
| `NaturalHold/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 적 재접근을 위한 단시간 회전 유지와 근사 접촉 위험 검사를 시험한 자연 전투 시험 |
| `NaturalHoldAll/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 여부와 관계없이 단시간 회전 유지와 위험 시 해제를 시험한 자연 전투 시험 |
| `NaturalDirection/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 같은 방향을 최대 4초 유지하며 위험 시 해제하는 자연 전투 시험 |
| `NaturalResponsive/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 실제 피격된 가까운 적으로 추적 대상을 전환하는 자연 전투 시험 |
| `NaturalGemAware/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 보석 거리 평가와 실제 보석 좌표 기록을 추가한 자연 전투 시험 |
| `NaturalSpear/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 시작 후보 2번 강철 창을 선택하는 자연 전투 성장 시험 |
| `NaturalSpearFiveMinute/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 강철 창 자연 성장 이후 300초까지 지속하는 생존 시험 |

자연 전투 스크립트는 같은 클래스 이름을 사용하므로 실행할 버전 하나만 사본의 `Assets/Editor/AutomatedNaturalCombatVerification.cs`로 복사한다. 여러 파일을 동시에 복사하면 중복 클래스로 컴파일할 수 없다.

예를 들어 자동 조종 검증은 다음과 같이 실행한다. 경로는 설치된 Unity와 별도 프로젝트 사본에 맞게 지정한다.

```powershell
& "C:/Program Files/Unity/Hub/Editor/6000.3.21f1/Editor/Unity.exe" -batchmode -nographics -projectPath "<검증 사본 경로>" -executeMethod ProjectEpsilon.Editor.AutomatedPilotVerification.Run -logFile "<검증 로그 경로>" # 자동 생존 검증
```

---

## 결과 해석

- 자동 조종은 사람의 수동 플레이가 아니다. 300초 생존 여부와 경험치·레벨업 발생 여부를 따로 확인한다.
- 37일차 무기·보석·바닥 이미지 적용 후 `NaturalSpearFiveMinute`를 정상 속도로 다시 실행한 결과 240초에 자연 처치 122회와 레벨 7을 기록했으나 266.3초에 몸통 0칸으로 패배해 종료 코드 1이었다. 마지막 충돌의 종류는 로그로 확인되지 않는다. 무작위 전투이므로 이 한 번의 결과만으로 이미지 변경의 영향을 판단할 수 없고, 현재 버전의 300초 생존도 통과로 기록하지 않는다.
- 강철 창 5분 자연 전투는 31일차 `d515b65` 사본에 32일차 적 행동·출현기·씬 세 파일을 반영해 실행했다. 세 파일의 SHA-256은 각각 `4ACD9CBFABBAED00D902A32CB8EA1AB32263BFD16F6ED7BBC90ABE79691C33E7`, `CFF1BDEAA647A8535D4104ACD678B569E79122E8F24BF8FDD7694FCFC9661AE3`, `7943692B4319CDF87B04B919143FAC14F3CBAC9142627EC95CC1329FFE7248E2`이며 해당 파일만 식별한다. 실제 시작 화면의 두 번째 버튼으로 강철 창을 장착하고 자연 출현, 정상 속도, 가상 A/D·Space 입력을 사용했다. 무적·능력치·경험치·적 출현을 조작하지 않았고 레벨업·엘리트 보상 UI는 자동으로 선택했다.
- 강철 창 5분 시험은 자연 처치·경험치 수집·레벨업 후 게임 시간 300.0초까지 생존해 Unity 종료 코드 0이었다. 실제 플레이 시간은 425.9초이고 마지막 상태는 181처치, 레벨 8, XP 19, 몸통 5칸, HP 80, 보석 126개였다. 로그의 레벨 선택 10과 엘리트 선택 1은 선택 상태에서 버튼 호출을 시도한 횟수이며 각각의 성공 횟수를 뜻하지 않는다. 지속 피해에 따른 미세 체력 변화마다 스택 추적이 기록돼 로그가 약 272MB로 늘었으며 계측이 실행 성능에 영향을 주었을 가능성은 있으나 실제 지연의 원인은 확인되지 않았다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다. 재현 코드는 `NaturalSpearFiveMinute`에 있으며 기존 자연 전투 스크립트와 동시에 사본에 복사하지 않는다.
- 강철 창 자연 전투는 31일차 `d515b65` 제품·설정 추적 파일 437개를 확인한 사본에서 실행했다. 이 중 411개는 바이트가 일치했고 26개는 줄바꿈만 달랐다. 실제 시작 화면의 두 번째 버튼으로 1성 강철 창을 장착했으며 실제 슬롯의 사거리는 7.00이었다. 정상 속도, 자연 출현과 가상 A/D·Space 입력을 사용하고 무적·능력치·경험치·적 출현을 조작하지 않았다. 레벨업 보상 UI는 자동 선택했다. 조종기의 공격 거리 점수는 강철 검 기준 고정값이 남아 있어 창에 맞춘 최적 조종이 아니다.
- 강철 창 시험은 자연 처치 15회와 경험치 수집 후 레벨 2·보상 버튼 선택 1회·몸통 4칸·HP 100으로 플레이를 재개해 Unity 종료 코드 0이었다. 게임 시간 67.0초, 실제 플레이 시간 67.4초에 성장 조건을 충족하자마자 종료했으므로 300초 생존이나 4분 엘리트 보상 검증은 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다. 재현 코드는 `NaturalSpear`에 있으며 기존 자연 전투 스크립트와 동시에 사본에 복사하지 않는다. 다른 시험과 시작 무기·무작위 출현이 달라 결과만으로 조종 성능을 비교할 수 없다.
- 보석 추적 자연 전투는 30일차 `f34132a` 기준에 31일차 충돌 관련 `SnakeExternalCollision.cs`, `SnakeSelfCollision.cs`, `EarlyEnemyBehaviour.cs`, `EnemyTestProjectile.cs` 네 파일을 반영한 사본에서 실행했다. 실제 시작 화면의 첫 버튼으로 강철 검을 고르고 정상 속도, 자연 출현, 가상 A/D·Space 입력을 사용했다. 무적·능력치·경험치·적 출현을 조작하지 않았고 선택 UI는 자동화했다. 조종기의 보석 접근 점수는 적과 몸통의 위험 점수와 함께 평가하며, 다섯 초마다 보석·머리 좌표, 최단 적 거리와 키 입력을 기록한다.
- 보석 추적 시험은 게임 시간 300.1초, 실제 플레이 시간 303.9초에 몸통 2칸·HP 35로 생존했다. 자연 처치 6회, 획득 XP 6/10, 미수집 보석 0개, 레벨 1, 레벨업 선택 0회로 자연 성장 통과 조건을 충족하지 못해 Unity 종료 코드 1이었다. 275.46초에 기록된 여섯 번째 보석은 머리와 2.40 거리였고 276.15초에 XP를 획득했다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다. 재현 코드는 `NaturalGemAware`에 있으며 기존 자연 전투 스크립트와 동시에 사본에 복사하지 않는다. 다른 시험과 무작위 출현 및 제품 사본이 달라 이 결과만으로 조종 성능을 비교할 수 없다.
- 반응형 자연 전투는 29일차 `fa68b92` 제품·설정 추적 파일 437개를 확인한 사본에서 실행했다. 이 중 411개는 바이트가 일치했고 26개는 줄바꿈만 달랐다. 30일차 변경은 반영하지 않았다. 실제 시작 화면의 첫 버튼으로 강철 검을 고르고 정상 속도, 자연 출현, 가상 A/D·Space 입력을 사용했다. 무적·능력치·경험치·적 출현을 조작하지 않았고 선택 UI는 자동화했다. 이전 추적 적이 무기 사거리 밖일 때 사거리 안에서 피격된 적으로 추적을 바꾸는 조종 방식이다.
- 반응형 시험은 게임 시간 300.0초, 실제 플레이 시간 302.8초에 몸통 3칸·HP 3으로 생존했다. 자연 처치 5회, 획득 XP 3/10, 미수집 보석 2개, 레벨 1, 레벨업 선택 0회로 자연 성장 통과 조건을 충족하지 못해 Unity 종료 코드 1이었다. 회전·가속 입력과 실제 이동을 관찰했다. 재현 코드는 `NaturalResponsive`에 있으며 기존 자연 전투 스크립트와 동시에 사본에 복사하지 않는다. 무작위 출현 조건이 서로 다른 다른 시험과 이 결과만으로 조종 성능을 비교할 수 없다.
- 자연 전투 최초 시도는 조종 입력 확인을 위해 초기에 A·D·Space를 순서대로 입력했다. Unity 6000.3.21f1, 정상 속도, 무적·능력치·경험치·적 출현 조작 없이 6처치와 보석 XP 6/10을 얻었고, 게임 시간 57.2초에 레벨 1로 패배해 종료 코드 1이었다. 가상 입력 및 실제 회전·가속은 확인됐으나 자연 레벨업은 확인되지 않았다. 보관한 자연 전투 스크립트는 이 시도에 해당한다.
- 자연 전투 두 번째 시도는 초기 고정 입력을 제거하고 머리와 몸통의 예상 접촉 위험을 회피하는 시험 조종기를 사용했다. 같은 게임 조건에서 1처치와 보석 XP 1/10을 얻었고, 게임 시간 12.6초에 레벨 1로 패배해 종료 코드 1이었다. 두 시도 모두 자동 조종 결과이며 게임의 난이도 문제를 단독으로 입증하지 않는다.
- 자연 전투 세 번째 시도는 23일차 제품 사본, 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종 경로를 비교했다. 게임 시간 300.0초, 실제 플레이 시간 300.5초에 몸통 3칸·HP 92로 생존했고 A/D·Space 입력과 실제 회전·가속을 확인했다. 다만 적 처치 0, 경험치 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. 재현 코드는 `NaturalTrajectory` 하위 폴더에 있다.
- 자연 전투 네 번째 시도는 24일차 합성 구현이 포함된 커밋 전 사본을 사용했다. 사본의 `SnakeWeaponManager.cs` SHA-256은 `1B14F30D04A9B87FD50D90A0C1FCF0DF3EA74A6C02FFA30D83AD209819E96760`, `SnakeWeaponMergeController.cs` SHA-256은 `6B25697BEE51C38A68EB20473EA94031DC2EFCF2642D76FFC95CFD0B6A8AB746`였다. 두 해시는 해당 파일만 식별하며 사본 전체의 일치를 증명하지 않는다.
- 네 번째 시도는 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종하고 UI 선택을 자동화했다. 게임 시간 300.0초, 실제 플레이 시간 302.9초에 몸통 3칸·HP 64로 생존했고 A/D·Space 입력과 실제 회전·가속, 네 차례의 적 체력 감소를 기록했다. 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalBalanced` 하위 폴더에 있다.
- 자연 전투 다섯 번째 시도는 24일차 `8cafb51`의 `Assets`, `Packages`, `ProjectSettings` 추적 파일 427개에 25일차 `SnakeWeaponManager.cs`만 추가 반영한 사본에서 실행했다. 해당 무기 관리자 SHA-256은 `9E731853B3AAE04710DD1935761C7FE4CE117C8EC3756003B9F25029D593E2F1`이다. 정상 속도·자연 출현·가상 입력을 사용하고 무적·능력치·XP·적 출현을 조작하지 않았다. UI 선택은 자동화했다.
- 다섯 번째 시도는 두 추적자의 체력이 각각 25→15→5로 감소했지만 처치와 XP는 없었다. 게임 시간 300.0초, 실제 플레이 시간 300.1초에 몸통 3칸·HP 100으로 생존했고 실제 A/D·Space 입력과 회전·가속을 확인했다. 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalFocus` 하위 폴더에 있다.
- 별도의 무작위 출현 실행에서 단일 적 추적 조종의 이동 진단을 진행하다 26일차 Unity 검증을 위해 중단했다. 마지막 로그는 게임 시간 18.03초다. 13.02초의 0.5초 표본에서 플레이어 평균 이동 속도는 3.00, 적은 2.40이었고, 적은 전진 방향 뒤쪽에 있었다. 당시 직진 후보 점수는 5.93, 우회전 6.73, 좌회전 169.42였으며 직진이 선택됐다. 10.52초 가속 중 플레이어 평균 속도는 4.50, 적은 2.40이었다. 이는 관찰 구간에서 거리 증가에 기여할 수 있는 근거이나 다섯 번째 시도와는 다른 무작위 실행이므로 그 시험의 거리 정체 원인을 확정하지 못한다. 실행 셸은 중단되어 코드 1로 끝났으며 예정된 60초 진단은 완료되지 않았다. 재현 코드는 `NaturalDiagnostics` 하위 폴더에 있다.
- 곡선 재접근 시도는 25일차 `d8a7b8c` 기준에 26일차 `SnakeLevelUpController.cs`, `SnakeWeaponMergeController.cs`, `LevelUpPanelController.cs`, `WeaponMergePanelController.cs` 네 파일을 동기화한 사본을 사용했다. 인계 후 제품 파일은 추가 동기화하지 않았다. 네 파일의 SHA-256은 순서대로 `0D1F929775CC5EBB33BD627D7781A92192368F2FD0F09D17BA1251DBE93685E6`, `3506A88FF4F4DCDB3366F0E820C7174A05355E888BC3705BFE1A5DF9AB3AB35A`, `3913E859FD46006A1663A360DC12C36E7F7A615BA91C581B6CB9472168F141D5`, `2F7CA9D705A18388308F9275819785E0F0DE0A6B261B1FEECB15CA4449BF2592`이다. 이 해시는 해당 파일만 식별한다.
- 곡선 재접근 시도는 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종하고 UI 선택을 자동화했다. 게임 시간 300.0초, 실제 플레이 시간 300.3초에 몸통 3칸·HP 100으로 생존했고 실제 A/D·Space 입력과 회전·가속을 확인했다. 추적자 한 명에게 25→15, 다른 한 명에게 25→15→5의 피해를 주었으나 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalCurve` 하위 폴더에 있다.
- 회전 유지 시도는 27일차 `2fa479c1f10f5e2aa38153c8fc450df76c715d12` 제품 스냅샷과 실질 내용이 일치하는 사본에서 진행했다. 확인한 제품 파일 433개 중 406개는 SHA-256이 바이트 단위로 일치했고, 26개는 줄바꿈, 후보 자산 메타 파일 1개는 빈 필드의 끝 공백만 달랐다. 인계 후 제품 파일은 동기화하지 않았다.
- 회전 유지 시도에서는 시작 화면이 게임 시간을 정지한 상태에서 실제 첫 후보 버튼 이벤트를 실행해 강철 검 ★1 하나가 첫 몸통에 장착된 것을 확인했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·적 출현·시간 배율은 시험 코드가 변경하지 않았으며 UI 선택은 자동화했다. 마지막 기록은 게임 시간 120.0초·실제 플레이 시간 119.9초, 몸통 3칸·HP 100, 처치 0·XP 0·레벨 1이었다. 부상 적 조건을 충족하지 못해 회전 유지 정책은 한 번도 발동하지 않았다. 28일차 시험을 위해 실행 셸에 중단 신호를 보냈으며 셸 종료 코드는 1이다. 300초 생존, 회전 정책 효과와 자연 레벨업은 이 미완료 실행에서 검증되지 않았다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalHold` 하위 폴더에 있다.
- 부상 조건 없는 회전 유지 시도는 27일차 `2fa479c`의 제품 파일 433개를 확인한 사본에 28일차 `WeaponData.cs`, `WeaponTarget.cs`, `SnakeWeaponManager.cs`, `DebugBlade.asset`만 동기화한 커밋 전 제품 스냅샷에서 실행했다. 네 파일의 SHA-256은 순서대로 `8DFC662A1932123340A4254CC1237F074C83DB21E0E87E11950F8CAFBF4077B5`, `3B57D558300F096D4B8702ACB4195F6EAE387A961A1A2293883732FCDCDEE364`, `733A92CD3FC7E2E703298987E5CB26A736DD1A006698853D62516ACCA542CA0B`, `A2905BDC6CB90D2A7DC47B2867EFBAB083894483632AD69B13CEF4852198BEC1`이다. 이 해시는 해당 파일만 식별한다.
- 부상 조건 없는 회전 유지 시도에서는 시간 정지 상태에서 실제 시작 첫 후보 버튼 이벤트로 강철 검 ★1 하나를 장착했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·출현·시간 배율을 조작하지 않았으며 UI 선택은 자동화했다. 게임 시간 300.0초·실제 플레이 시간 313.8초에 몸통 3칸·HP 100으로 생존하고 A/D·Space 입력 및 실제 회전·가속을 확인했다. 유지 회전 273회 중 연속 회전 방향이 바뀐 경우는 215회, 예측 위험 조기 해제는 2회였다. 실제 처치·XP·보상 선택은 0이었고 자연 성장 조건에 실패해 Unity 종료 코드는 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalHoldAll` 하위 폴더에 있다.
- 최대 4초 방향 유지 시도는 바로 앞 시험과 같은 27일차 `2fa479c` 기준 및 28일차 제품 네 파일 동기화 사본에서 진행했다. 시험 시작 전 같은 사본의 Unity 프로세스가 없었고, 네 제품 파일과 시작 선택기·씬의 SHA-256이 앞 시험과 동일함을 확인했다. 인계 후 제품 파일은 동기화하지 않았다.
- 최대 4초 방향 유지 시도는 시간 정지 상태에서 실제 시작 첫 후보 버튼 이벤트로 강철 검 ★1 하나를 장착했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·출현·시간 배율을 조작하지 않았으며 UI 선택은 자동화했다. 게임 시간과 실제 플레이 시간 모두 300.0초에 몸통 2칸·HP 30으로 생존하고 A/D·Space 입력 및 실제 회전·가속을 확인했다. 자연 적 2처치·보석 XP 2/10·레벨 1·보상 선택 0으로 자연 성장 조건에 실패해 Unity 종료 코드는 1이었다. 방향 고정 86회와 예측 위험 관련 해제 39회를 기록했다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalDirection` 하위 폴더에 있다.
- 강제 출현·처치와 통제 표적 시험은 자연 출현 순서, 적 충돌, 난이도 또는 5분 생존의 대체 근거가 아니다.
- 26일차 `11f4a25`의 추적 자산·설정 427개 해시가 일치하는 사본에서 시험 무기 6종의 공격 원점·직접 피해·공격 주기를 확인한 배치는 종료 코드 0이었다. 실제 세 번째 몸통을 머리에서 멀리 고정해 공격했고, 강철 창의 2명 관통, 철퇴의 3명과 화염 고리의 2명 범위 적중 및 화염 무기의 기본 화상도 확인했다. 실제 출현기 설정으로 일반 적을 통제 생성·강제 처치한 뒤 추적자·포식자·사수의 소형 보석 1/2/2개와 흡수 경험치 1/2/2를 확인했다. 자연 전투나 사람이 조작한 플레이 결과는 아니다.
- 강철 검 통제 배치는 27일차 `2fa479c` 기준에서 ★3의 첫 공격이 한 명만 적중해 종료 코드 1을 기록했다. 28일차 네 제품 파일 반영 후 동일 실제 씬 배치는 종료 코드 0이었다. ★1·2는 가까운 적 1명, ★3·4·5는 가까운 적 2명에게 각각 10·12.5·15·18·22 직접 피해를 주고 0.8초 공격 주기를 유지했다. 화염 검 ★3·5의 단일 공격, 첫 대상 사망 중 다른 대상 비활성·재등록 및 신규 대상 생성, 물리 시너지 ×2에서 두 대상에 각각 16.5 피해도 확인했다. 표적은 정지 상태이며 사망 콜백의 대상 변경은 시험 코드가 유발했다.
- 화염탄 통제 배치는 29일차 `fa68b92`에서 실제 ★1·2 발사가 각각 1명에게 7·8.75 피해를 주고 ★3의 첫 발사에서 추가 명중이 없어 종료 코드 1이었다. 같은 스크립트 SHA-256 `DF21C12687B7816C2B6AF7278AE44E32D340B67289CB88B58320A31E9D768A4F`와 30일차 제품 세 파일을 반영한 실제 씬 배치는 종료 코드 0이었다. ★3·4·5는 첫 표적과 그 주변 최근접 표적에게 각각 10.5·12.6·15.4 피해를 줬으며, 임시 거리 2.0에서 거리 1.99는 포함하고 2.01은 제외했다. 강철 창의 전달 미적용, 화염 시너지 ×2에서 두 대상의 초당 화상 피해 1.2, 첫 표적 사망 및 전달 대상의 사망·비활성, 투사체 컴포넌트 비활성 상태의 실제 접촉과 재활성 후 실제 재접촉을 확인했다. 에셋의 메모리 사본으로 관통 횟수 2를 설정해 전달 대상 중복 명중을 검사하고, 발사 후 사본의 전달 설정을 바꿔 발사 시점 설정 보존도 확인했다. 전달 거리 2.0과 피해 배율 1.0은 기획 미정 임시값이며 관통 2는 경계 시험용 설정이다. 정지 표적과 사망 콜백은 통제된 시험 조건이다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 보석 병합 통제 배치는 28일차 `b573b98` 사본에서 관리자가 없어 종료 코드 1을 기록했다. 같은 스크립트 SHA-256 `1A222F01ABC1125176DA9480B1A327B25FFA030D485B57AB9215DE4FDFA96223`을 사용해 29일차 제품 파일과 씬 연결을 반영한 실제 게임 씬 배치는 종료 코드 0이었다. 1×5→5와 5×4→20의 경험치 보존, 최초 검사에서 화면 밖·원거리 우선, 병합 예산 1, 일시정지와 재개, 비활성화·재활성화와 외부 파괴 완료 후 제외, 실제 근거리 수집 경험치 5를 확인했다. 최초 검사의 인접 후보 검사 5회, 예산에 포함된 거리 계산 13회, 별도 머리 거리 계산 15회를 기록했다. 이 조합과 검사 설정은 기획 미정인 구현 가정이며 보석 값 1·5·20은 기획 기준이다. 보석 배치와 경험치 요구량은 시험에서 통제했으므로 자연 전투 결과가 아니다. 외부에서 임의로 `Destroy`한 같은 프레임의 후보 제외는 검증하거나 보장하지 않는다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 초반 출현·엘리트 연결 통제 배치는 31일차 `d515b65` 제품·설정 파일 437개 중 411개가 바이트 일치하고 26개는 줄바꿈만 다른 사본에서 Unity 종료 코드 0이었다. 실행 스크립트 SHA-256은 `CC111BFFBEE0139FBF4279DC51A45FE5D76BEDD08569EC9F24132FB414F2C581`이다. 실제 씬의 출현 간격 1.5초와 해금 0·60·120·240초를 확인하고 출현기 시간을 직접 변경하지 않았다. 시작 무기 버튼 선택 후 관측을 시작한 출현기 시간은 0.350초였으며 시간 배율 8, 무작위 시드 3191, 29.95실초 통제로 경과 시간 240.009초까지 진행했다. 실제 최초 관측 시각은 추적자 0.350, 포식자 60.039, 사수 123.056, 엘리트 240.000초였고 해당 시점까지 관측 수는 82·50·28·1이었다. 자동 출현한 엘리트를 강제 처치해 실제 사망 이벤트 1회, XP 20 보석 1개와 엘리트 보상 대기 1개를 확인했다. 실제 보석 수집 후 레벨 2·3 보상을 각각 실제 버튼으로 완료하는 동안 엘리트 보상이 대기했고, 세 가지 서로 다른 엘리트 후보 중 실제 버튼으로 복구 보상 하나를 선택한 뒤 대기열 0과 게임 재개를 확인했다. XP 기본 요구량 10·배율 1.0, 엘리트의 무작위 회복·몸통 드롭 확률 0, 플레이어 이동·자동 공격 정지 및 관측한 일반 적의 보상 없는 제거는 모두 시험 통제다. 시간 가속과 강제 처치를 사용했으므로 자연 속도 5분 생존이나 자연 엘리트 처치의 근거가 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 위 초반 출현·엘리트 연결 배치를 32일차 두 적 스크립트와 씬을 반영한 사본에서 같은 시험 조건으로 재실행해 Unity 종료 코드 0을 확인했다. 최초 관측 시각은 추적자 0.350, 포식자 60.044, 사수 123.064, 엘리트 240.001초였고, 실제 엘리트 강제 처치 뒤 XP 20 보석·연속 레벨업·엘리트 보상 선택과 재개도 유지됐다. 시간 8배, 일반 적 제거와 강제 처치는 앞선 시험과 동일한 통제이며 Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 엘리트 일정 통제 배치는 32일차 `5c453e7` 사본에서 첫 엘리트 처치 후 대기된 8분 회차가 생성되지 않아 Unity 종료 코드 1이었다. 동일 스크립트 SHA-256 `6CD432CBC6241D69CAD92807357D2AA5876A8B95F65B514ED8BC7953B9CF0A7C`과 33일차 출현기·씬을 반영한 실제 Game 씬 배치는 종료 코드 0이었다. 출현기 시간을 직접 주입하지 않고 시간 배율 32로 진행했으며 4분 회차 240.001초, 기존 엘리트 사망 후 8분 대기 회차 481.019초, 8분 엘리트와 공존한 12분 회차 720.007초, 두 마리 중 한 마리 사망 후 16분 대기 회차 961.017초를 관측했다. 전체 적 만석 40에서 가장 먼 일반 적이 보상 없이 제거됐고, 비활성 직후 같은 프레임의 직접 치명 피해에도 XP·회복 보상이 생기지 않았다. 실제 엘리트 네 마리를 강제 처치해 사망 이벤트 4회, XP 20 보석 4개, 선택 보상 4개가 각각 한 번씩 생성됐다. 실제 보석 하나를 수집해 연속 레벨업 두 번을 먼저 완료하고 실제 UI로 엘리트 보상 4개를 차례로 소비해 게임이 재개됐다. 미처치 엘리트 때문에 정시에 등장하지 못한 회차의 FIFO 대기는 기획 미정인 임시 가정이다. 플레이어 이동·충돌·무기 공격 및 적 행동 정지, 강제 처치, XP 요구량 10·배율 1.0, 임의 회복·복구 드롭 확률 0과 보석 위치 이동은 시험 통제다. 이는 자연 16분 전투가 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 같은 엘리트 일정 스크립트의 외부 파괴 진입점은 32일차에서 대기 회차 미재개로 Unity 종료 코드 1, 33일차에서 종료 코드 0이었다. 실제 `Destroy` 완료 후 Unity fake-null이 된 엘리트는 사망 이벤트·XP·선택 보상 없이 활성 수에서 정리됐고, 481.041초에 대기 중인 다음 엘리트가 나타나 전체 적 수 40을 유지했다. 같은 32배 가속과 행동 정지 통제를 사용했으며 Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 33일차 출현기 내부 필드 변경에 맞춘 사수·출현 배치 SHA-256 `C9A533F66F58311C97825FD8858B4C53B6B9FFB5CFE75EB9126DF2F9F8912CFF`도 실제 Game 씬에서 Unity 종료 코드 0으로 재통과했다. 기존 엘리트 보상 편집기 검증 `ProjectEpsilonEliteRewardVerification.Run`도 종료 코드 0이었으며, 이 편집 모드 검증의 콜백 직접 호출은 실제 물리 접촉과 구분한다.
- 사수·출현 통제 배치는 31일차 `d515b65`에서 실제 사수가 몸통 대신 머리로 발사하고 초광폭 화면 안에 일반 적과 엘리트가 생성돼 Unity 종료 코드 1이었다. 같은 스크립트 SHA-256 `443812CA3E5259F822CC8292BDD439D8BED95F4011EA65C4BEA1B9CDD2FF2DAA`과 32일차 적 스크립트 두 개·씬을 반영한 실제 Game 씬 배치는 종료 코드 0이었다. 실제 몸통 거리 제곱 `2=2` 동률의 첫 몸통과 `2>1.25`인 더 가까운 몸통을 향한 사격, 몸통 비활성 시 머리 대체, 머리 거리별 접근·유지·후퇴를 확인했다. 카메라 위치를 옮긴 32:9·직교 크기 12에서 실제 일반 적 16개 모두 생성 프레임에 화면 여유 0.5 밖이었고, 그중 기존 정리 거리 22 밖 4개를 포함한 16개 모두 다음 정리 후 남았다. 강제 240초 엘리트도 화면 밖이었으며, 카메라 부재 시 생성 거리는 9.000이었다. 별도 프레임에서 실제 카메라가 15.593 이동한 뒤 렌더 직전 출현 위치가 화면 여유 밖임을 확인했다. 플레이어·사수·카메라 위치, 화면 비율, 엘리트 출현 시간과 일반 적 관찰 중 8배 시간 가속은 시험 통제이며 엘리트 4분 자연 출현을 증명하지 않는다. 생성 프레임과 그 다음 물리 스텝의 적 이동을 구분하기 위해 표본 적을 생성 직후 정지했다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 몸통 성장 통제 배치는 29일차 `fa68b92` 기준 사본에서 Unity 종료 코드 0이었다. 추적 제품·설정 437개 중 411개는 바이트 일치, 26개는 줄바꿈만 달랐다. 테스트 스크립트 SHA-256은 `28FBE311334EE1DB4A35A77D312569CE602AE7913AD56D3582F6894A3FDBE20E`다. 실제 시작 무기 버튼을 누르고 이동과 자연 출현을 끈 뒤 실제 성장 API와 초과 피해로 몸통·체력 상태를 구성했다. 이후 실제 보석 수집과 레벨업 보상 버튼을 사용해 현재/성장 최대/절대 상한 `4/6/20` HP 77에서 `5/7/20` HP 77로의 성장, `19/20/20`에서 `20/20/20`으로의 복구 시 HP 77 유지, 다음 레벨의 HP 100 전회복을 확인했다. 연속 레벨업은 시험 중 요구량을 기본 10·배율 1.0으로 고정해 매 레벨 10으로 만들었으며, 실제 30 XP 보석을 수집한 뒤 세 차례 보상과 잔여 XP 20·10·0, HP 70→100→100 및 게임 재개를 확인했다. 이 보석 값과 요구량은 시험 통제값이며 자연 전투나 기본 성장 밸런스 결과가 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 저예산 부분 그룹 보강 배치는 같은 29일차 동결 사본에서 Unity 종료 코드 0으로 통과했다. 실제 시작 버튼을 자동 선택하고 이동·자연 출현을 끄며 레벨업 요구량을 임시 10000으로 설정했다. 기준 1개·그룹 1개·검사 예산 17에서 실제 서로 다른 프레임에 부분 재료 3개가 보존된 뒤 새 보석 삽입, 선택 재료 이동 후 복귀, 선택 재료의 실제 수집 후 대체 보석 추가를 각각 검증했다. 각 검사와 지연 파괴 경계에서 수집 경험치와 미소비 보석값의 합이 보존됐고, 유효 그룹은 이후 병합됐다. 관찰된 재료 거리 계산 최대는 15회였고 별도로 계산되는 머리 거리 검사는 최대 25회였다. 따라서 전체 보석 순회와 정렬까지 17회로 제한한 결과는 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 충돌 시험의 실제 동시 접촉 순서는 로그에서 따로 확인한다. 인위적으로 호출한 양쪽 콜백 순서와 수명주기 초기화는 실제 물리 콜백이나 씬 전환을 재현한 결과와 구분한다.
- 비활성 충돌 통제 배치는 30일차 `f34132a` 기준에서 Unity 종료 코드 1이었다. 실제 2D 콜백이 비활성 컴포넌트에도 전달되어 외부 충돌 HP 75·무적, 자기 충돌의 동일 스텝 재활성 전 예약과 몸통 3→1, 적 근접 행동 HP 90, 적 투사체 HP 92·소멸을 각각 재현했다. 같은 스크립트 SHA-256 `AA86A486697E1A144EB4940A1308143D362B8FFFEE7FAC9670A3CDE07D91D4B4`과 31일차 네 제품 파일을 반영한 실제 씬 배치는 종료 코드 0이었다. 네 비활성 접촉 모두 피해가 없었고, 자기 충돌은 같은 물리 스텝 재활성 전에 예약이 없었다. 외부 충돌과 근접 적의 재접촉은 정상 피해를 주었으며, 비활성 접촉 뒤 살아남은 같은 적 투사체도 재활성 후 정상 피해를 줬다. 별도의 실제 장애물·투사체 동시 접촉에서는 HP 100, 보호막 25와 전체 무적을 확인했다. 플레이어 이동·자연 출현을 중지하고 충돌체 위치와 적 공격 시각을 통제한 시험이며, Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 합성 시험은 24일차 제품 코드에서 Unity 6000.3.21f1 배치 종료 코드 0으로 통과했다. 네 단계 ESC 취소, 20번 몸통 선택, 양쪽 강화 계승, 속성 수와 단계 즉시 갱신, 몸통 손실·레벨업·엘리트 보상 우선순위를 확인했다. 실제 시간 0.400초에 게임 시간 0.280초가 진행됐으며, 회전과 가속 입력 중 방향을 유지하고 직진했다. 이 결과는 통제된 장비와 가상 입력에 대한 검증이다.
- 같은 합성 배치에서 시너지 미리보기 추가 후 속성 무기 수 2~9의 단계 하락 경계와 유지 구간을 네 선택 화면에서 확인하고, 다른 속성으로 선택을 옮겼을 때의 표시, ESC 취소 시 보존, 4→3 합성 후 실제 단계와 예상의 일치도 확인했다. 대표 상태 문구의 필요 크기는 최대 462.46×15.59, 영역 크기는 620×32였으며 20번 몸통과 강화 선택 버튼 문구의 높이도 확인했다. Unity 배치 종료 코드는 0이다.
- 미장착 보상 합성의 같은 실제 씬 배치는 24일차 `8cafb51` 기준에서 해당 API가 없어 종료 코드 1을 기록했고, 25일차 API 반영 후 종료 코드 0으로 통과했다. 빈 슬롯과 만석, 두 강화 계승, 부적합 요청 시 무변경, 무기 수·시너지 유지, 기존 장착 무기 2→1 합성을 확인했다. 조작한 장비와 강화를 사용하는 낮은 수준 API 시험이며 보상 UI 검증은 포함하지 않는다.
- 레벨업 보상 합성의 26일차 초기 Unity 배치는 실제 2D 접촉 후 직접 주입한 초과 피해로 패배할 때 보상 패널이 남아 종료 코드 1을 기록했다. 별도 경계 시험에서는 숫자 2 중복 적용, 제어기 비활성 후 화면·후보 복구, 마지막 호환 몸통 손실 후 합성 버튼 상태를 각각 확인해 실패를 기록했다. 수정 후 `RunAll`은 가상 입력, 실제 버튼 이벤트, 통제된 충돌과 직접 치명 피해로 이 경계와 빈 슬롯·만석·연속 레벨업·엘리트 대기 흐름을 확인해 종료 코드 0으로 통과했다. 시험 사례 사이에 테스트 코드가 상태를 초기화하므로 자연 전투 결과와 구분한다.
- 시작 무기 선택의 비활성 `Configure` 수정 직전 실제 씬 배치는 새 플레이 세 번에서 임시 후보 강철 검·강철 창·화염 고리의 첫 몸통 장착, 버튼과 가상 숫자 입력, 선택 전 시간·이동·출현 정지 및 선택 후 재개를 확인해 종료 코드 0이었다. 이후 비활성 선택기의 `Configure` 화면 복구 문제를 수정하고 별도 실제 씬 배치에서 비활성 유지와 첫 후보 선택을 확인해 종료 코드 0이었다. 최종 수정본으로 세 후보와 공용 헬퍼가 적용된 기존 배치를 다시 실행하려 했으나 Unity 초기화가 정체해 중단했고, 독립 사본의 캐시 복사도 미완료이므로 최종 전체 회귀는 확인되지 않았다.
- HUD 시험의 텍스트 너비 비교는 배치 환경에서 측정 가능한 경우에만 유효하다. 실제 화면에서의 가독성은 별도로 확인한다.
- 실행 환경에 따라 Unity Editor의 SearchDatabase 시작 인덱싱 예외가 발생할 수 있다. 게임 스크립트 예외와 구분하고 로그에서 실제 발생 여부를 확인한다.

---

## 45일차 초반 전투와 경험치 보석

`Day45NaturalCombat/AutomatedNaturalCombatVerification.cs`를 사본의 `Assets/Editor/AutomatedNaturalCombatVerification.cs`로 복사하고 `StartingWeaponBatchHelper.cs`를 함께 둔다. `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run`에 `-epsilonSeed 4501`을 추가하면 시험 조종기의 난수 초기값을 지정한다. 다른 자연 전투 조종기와 같은 클래스 이름이므로 동시에 복사하지 않는다. 300초 생존·자연 레벨업·4분 엘리트 관찰을 확인하며 수동 플레이나 모든 프레임의 결정적 재현을 뜻하지 않는다.

`AutomatedDay45PilotVerification.Run`은 같은 조종기 파일과 공용 헬퍼가 필요하며 뒤의 적 재접근 대신 앞쪽 보석으로 향하는 판단을 검사한다. `AutomatedDay45PickupVerification.Run`은 실제 움직이는 머리의 보석 수집과 일시정지를 검사한다. `AutomatedDay45PickupSettingsVerification.Run`은 실제 씬의 설정 에셋 연결, 조정한 새 보석의 이동, 경험치 보존, 설정 부재 시 기존 기본값을 검사한다. 각 테스트는 `ProjectEpsilon.Editor.` 접두사를 붙여 실행한다.

변경 전과 후의 자연 전투 결과 및 수동 검증 한계는 `Devlogs/Day45/README.md`에 기록한다.

---

## 46일차 맵과 특수 지역

`AutomatedDay46MapsVerification.cs`와 `StartingWeaponBatchHelper.cs`를 별도 Unity 사본의 `Assets/Editor`에 복사한다. `ProjectEpsilon.Editor.AutomatedDay46MapsVerification.Run`에 `-epsilonMapIndex 0`, `1`, `2`를 각각 지정해 세 맵을 검사한다. 실제 시작 전 맵 선택, 3×3 바닥, 확률을 100%로 통제한 장애물·특수 지역, 보스 체력·두 단계 사격과 단계 전환을 확인한다. 보스 생성과 플레이어 위치는 직접 통제한다.

`AutomatedDay46GenerationVerification.Run`은 실제 씬에서 생성 확률 0%·100%, 장애물 충돌체와 특수 지역 최소 거리를 확인한다. `AutomatedDay46ZonesVerification.Run`은 실제 감속, 경험치 소수 누적, 위험 피해, 회복, 일시정지와 지역 이탈을 통제 조건에서 검사한다. 각 진입점 앞에는 `ProjectEpsilon.Editor.`를 붙인다.

`AutomatedCharacterInputVerification.Run`은 시작 화면의 맵 위·아래 키, 설정 화면 입력 우선과 전투 중 변경 금지를 함께 검사한다. 실행 결과와 미완료 사항은 `Devlogs/Day46/README.md`에 기록한다.

---

## 47일차 포탑 고유 효과와 탄 표시

`AutomatedDay47UniqueEffectsVerification.Run`은 실제 강철 포탑의 4회 주기·범위·재장착 초기화와 화염탄의 화상 처치·만료·중복·즉사·연쇄 차단·지속 피해 처치·실제 탄 발사 후 폭발을 통제 조건에서 검사한다. `AutomatedPlayerArtVerification.Run`은 실제 머리와 몸통의 최대 표시 크기 각 1 및 기존 충돌체를 확인한다. `AutomatedGlobalAttackHudVerification.Run`은 근접·범위의 시각 탄 이동·일시정지와 실제 직선 탄의 이미지·충돌체·원거리 명중을 함께 확인한다. 각 진입점 앞에는 `ProjectEpsilon.Editor.`를 붙이며, 스크립트와 `StartingWeaponBatchHelper.cs`는 별도 Unity 사본에만 복사한다.

48종 ★5 시험은 강철 근접의 주기 강타와 화염탄의 화상 폭발을 공통 효과와 구분한다. 등급별 시험 결과와 수동 확인 한계는 `Devlogs/Day47/README.md`를 참조한다.

---

## 48일차 영구 성장과 저장

`AutomatedDay48MetaVerification.Run`, `AutomatedDay48EffectsVerification.Run`, `AutomatedDay48StartStatsVerification.Run`은 `StartingWeaponBatchHelper.cs`와 함께 별도 Unity 사본의 `Assets/Editor`에서 실행한다. 메타 시험은 저장 초기화 전 제품 이름이 `Project-Epsilon-Day48-Isolated`인지 검사한다. 저장 시험 직후 `AutomatedDay48FreshProcessVerification.Run`을 새 Unity 프로세스에서 실행해 실제 파일 재로딩을 확인한다. 기존 모든 대상 해금을 전제로 한 회귀 검사는 격리 사본에서만 `AutomatedDay48RegressionSeed.Run`을 실행한 뒤 수행한다. 각 진입점 앞에는 `ProjectEpsilon.Editor.`를 붙인다.

성과·해금 대상의 임시값, 실제 테스트 범위와 사용자 화면에서 확인할 내용은 `Devlogs/Day48/README.md`에 기록한다.

---

## 49일차 전용 화면

`AutomatedDay49FrontendVerification.Run`은 `StartingWeaponBatchHelper.cs` 및 `AutomatedDay48MetaVerification.cs`의 메모리 저장소와 함께 격리 사본에서 실행한다. 제품 이름이 `Project-Epsilon-Day48-Isolated`인지 검사한 후에만 전용 메타·화면 설정 키를 초기화한다. 실제 버튼 이벤트와 가상 키로 메뉴·선택·설정·합성 우선·정지·결과·저장 실패·재도전을 검증한다. 화면 그림과 입력 위치는 직접 확인해야 한다.

기존 시작 포탑 회귀 도우미는 실제 주 메뉴와 캐릭터·맵 단계를 거쳐 시작한다. 검사 결과와 별도 프로필 사용 범위는 `Devlogs/Day49/README.md`에 기록한다.

---

## 50일차 전투 기록과 표시 이름

`AutomatedDay50RecordingVerification.Run`은 `-epsilonRecord -epsilonSeed 13579`와 공용 시작 도우미를 사용해 격리된 제품 프로필에서 실행한다. 실제 승패 시에만 기록 파일이 생성된다. `AutomatedDay50NoRecordingVerification.Run`은 기록 인수 없이 실행해 기본 기록 비활성을 확인한다. `AutomatedDay50NamingVerification.Run`은 포탑 에셋과 실제 보상 문구를 검사하고 `AutomatedDay50FontVerification.Run`은 관련 텍스트의 공용 글꼴 연결을 검사한다. 각 진입점 앞에는 `ProjectEpsilon.Editor.`를 붙인다.

`python Tools/compare_run_records.py 첫번째.json 두번째.json`은 기록된 시작 조건과 결과를 비교한다. 같은 시드로 완전히 동일한 전투가 재생된다는 뜻은 아니다. 통제된 시험 결과와 실제 화면에서 남은 확인은 `Devlogs/Day50/README.md`에 기록한다.

---

## 51일차 초반 전투와 데이터 ID

기존 `Day45NaturalCombat/AutomatedNaturalCombatVerification.cs`는 실제 메뉴·캐릭터·맵 단계를 공용 도우미로 통과하고 두 번째 시작 포탑을 선택한다. `-epsilonRecord -epsilonSeed 5101` 등으로 실행하면 300초 생존 시 로그에 실제 기록을 남긴다. `python Tools/summarize_early_run.py 로그경로`는 완료된 로그만 요약한다. 세 시드의 결과는 `Devlogs/Day51/README.md`를 참조한다.

`AutomatedDataIdentifierVerification.Run`은 실제 캐릭터·포탑 ID의 접두어와 중복, 여덟 포탑과 캐릭터의 해금 참조 및 기존 비트 저장 복원을 검사한다. 게임 실행 중 ID 참조와 실제 저장은 격리 사본의 48·49·50일차 회귀로 확인한다.

---

## 51-A 화면별 씬

`AutomatedSceneAssetsVerification.Run`은 실제 네 씬과 활성 빌드 순서를 검사한다. `AutomatedSceneFlowVerification.Run`은 제품 이름이 `Project-Epsilon-SceneFlow-Isolated`인 별도 Unity 사본에서만 실행하며 해당 시험 프로필의 메타 저장 키를 초기화한다. 실제 버튼 이벤트로 선택·전투·결과·저장 실패·재도전·승리·메뉴 복귀를 검사한다. `ConfigureSceneFlow.Run`은 씬 에셋을 생성하는 편집기 전용 보조 도구이며 실제 사용자 프로젝트에 실행하기 전 기존 씬과 빌드 설정을 확인해야 한다.

---

## 51-B 몸통·포탑 우측 패널

`AutomatedBodySidebarVerification.Run`은 실제 전투 카메라와 UI의 픽셀 경계, 20칸까지 포탑 이미지·순서·등급, 빈 슬롯과 가운데 정렬, 중복 HUD 제거, 설정 접근, 합성·시너지·성장·손실·복구를 검사한다. `AutomatedRewardLayoutVerification.Run`은 실제 화면 픽셀로 보상 카드 약 70%와 우측 패널 비중복을 확인한다. 자연 전투와 회귀 결과는 `Devlogs/BodySidebar/README.md`에 기록한다.

---

## 51-C 화면 한국어 정리

`AutomatedKoreanUiVerification.Run`은 `StartingWeaponBatchHelper.cs`와 함께 제품 이름이 `Project-Epsilon-KoreanUI-Isolated`인 별도 Unity 사본에서 실행한다. 실제 메뉴와 전투 화면을 진행하며 활성 UI 문구의 영문과 글꼴 연결을 검사한다. 키 문자는 허용하며 글꼴의 화면상 렌더링은 직접 확인해야 한다. 상세 결과는 `Devlogs/KoreanUI/README.md`를 참조한다.

---

## 52일차 엘리트 전투

`AutomatedEarlyWaveVerification.Run`은 실제 출현기를 가속해 4분 엘리트의 예고·방향 고정·돌진 이동을 확인한 후 강제 처치와 연속 레벨·엘리트 보상을 검증한다. 테스트 중 플레이어와 일반 적의 이동·충돌, 보상 전 경험치 설정을 통제한다. `AutomatedEliteRewardVerification.Run`은 시각을 강제로 240초로 설정하고 포탑 보상 선택과 장착을 검사한다. 실제 정상 속도 자연 전투 및 제한 사항은 `Devlogs/Day52/README.md`를 참조한다.

---

## 53일차 5~10분 성장

`Day45NaturalCombat/AutomatedNaturalCombatVerification.cs`는 기본적으로 300초 관찰을 유지한다. `-epsilonSurvivalSeconds 600 -epsilonRecord`를 지정하면 정상 게임 속도로 600초까지 진행하고 5분·10분 표본을 기록한다. 시험은 가상 입력과 보상 자동 선택을 사용하며 실제 수동 플레이를 대신하지 않는다. `python Tools/summarize_early_run.py 로그경로`는 완료 기록의 5~10분 성장 변화를 출력한다. 사용자의 저장과 격리된 별도 Unity 사본에서 실행한다.

---

## 54일차 20칸과 장애물 이탈

`AutomatedCollisionPriorityVerification.Run`은 통제된 20칸 몸통에서 자기 충돌·다수 적 접촉·좁은 통로와 장애물 무적 종료 후 안전 위치 및 재접촉 유예를 검사한다. `AutomatedLevelRewardMergeVerification.RunAll`은 합성 중 기존 무적의 적 접촉 차단과 피해·패배 처리를 확인한다. 시험은 몸통 위치와 충돌체를 통제한다. 상세 결과와 복수 장애물 위치 탐색의 한계는 `Devlogs/Day54/README.md`를 참조한다.

---

## 55일차 10분 통합 검증

기존 자연 조종기에 `-epsilonAutoMerge`를 지정하면 실제 장착 포탑 중 합성 가능한 쌍을 찾아 합성 화면 API로 진입하고 후보 버튼을 자동 선택한다. 합성 완료·취소를 기록하며, 옵션이 없을 때는 기존 조종 방식을 유지한다. E 키와 합성 화면 입력은 `AutomatedMergeSelectionVerification.Run`으로 따로 확인한다. 자연 전투 세 회의 개별 조건과 결과는 `Devlogs/Day55/README.md`를 참조한다.
