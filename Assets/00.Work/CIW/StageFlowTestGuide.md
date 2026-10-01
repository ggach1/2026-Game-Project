# Stage Flow 테스트

## 열기

`Assets/00.Work/CIW/StageFlowTest/StageFlowTest.unity`를 열고 Play 합니다.
생성 에디터 스크립트는 최초 임포트 때 누락된 테스트 씬을 생성하며, 기존 씬이 있으면 덮어쓰지 않습니다.
필요하면 `CIW > Stage Flow > Create Test Assets (Only If Missing)` 메뉴로 생성할 수 있습니다.
기존 MenuScene, Build Settings는 변경하지 않습니다.

## 구성

- World.asset: A와 B가 있는 선택 지도.
- Stage_A.asset: A1(평지), A2(중앙 구덩이) 두 구간. 처음부터 해금.
- Stage_B.asset: B1 한 구간. A 전체 완료 후 해금.
- Section_*.prefab: Ground 레이어 발판, Spawn Point, ExitDoor, SectionContext.
- StageNode.prefab: 잠금/완료 표시 및 선택 버튼.
- 공용 Player: 구간 밖에 하나만 존재. 기존 Player 프리팹 재사용.
- Stage Systems: 패널과 별개로 항상 활성화. 진행/로딩/저장/UI 제어.
- StageFlowTestHUD: 진행 구간 표시 및 y < -6 낙사 처리.

텍스트는 기본 TMP 폰트의 글리프 누락을 피하기 위해 영어로 표시합니다.
스테이지 노드 프리팹의 앵커/피벗은 중앙 기준입니다.

## 지도형 선택 UI

Play하면 기존 카드 대신 붉은 배경, 섬 지형, 아치형 문으로 구성된 지도 UI가 생성됩니다.
씬/프리팹의 기존 참조는 유지하며, 편집 모드의 카드를 저장해 덮어쓰지 않습니다.

- StageSelectUI의 Map Title에서 화면 제목을 변경할 수 있습니다.
- World.asset의 Entries > Position은 문의 배치 좌표입니다. 기본 지형 안쪽(대략 x -400~400, y -80~80)에 배치하세요.
- 점선은 StageDefinition의 Unlock Stage 관계를 따라 연결됩니다. 단순한 Entries 배열 순서가 아닙니다.
- 잠긴 문은 자물쇠와 LOCKED, 완료한 문은 체크 표시, 현재 선택한 문은 밝은 테두리와 움직이는 화살표로 구분합니다.
- 문 클릭으로 입장합니다. 방향키와 Enter(기존 InputSystemUIInputModule의 Navigate/Submit)도 사용합니다.
- 선택 화면 복귀 시 해금된 미완료 문을 우선 선택합니다. 완료한 문은 다시 플레이할 수 있습니다.
- 지형/문 표현: StageMapGraphic. 화면 생성/크기 맞춤: StageMapPresentation. 기존 진행/저장 로직은 그대로입니다.
- StageFlowTestHUD의 상태/조작 텍스트는 선택 화면에서는 숨기고 플레이 중에만 표시합니다.

지도 자체는 현재 A/B 테스트 구성을 위한 고정 지형입니다. 대규모 월드, 지도 스크롤, 설정 메뉴는 이번 범위에 포함하지 않습니다.

## 확인 순서

1. 첫 실행에서 A는 선택 가능, B는 LOCKED.
2. A 선택 후 오른쪽 문에 입장하면 A2로 전환.
3. A2에서 F8 또는 구덩이 낙사: A2 프리팹 재생성 후 A2 시작 위치로 복귀.
4. A2 출구 입장 완료: 선택 화면 복귀, A CLEARED, B 선택 가능.
5. B 선택 및 완료 후 다시 선택 가능.
6. 플레이 중 STAGE SELECT 버튼: 진행 취소 후 선택 화면. 재선택은 첫 구간부터.
7. Play 종료/재실행 후 A 완료/B 해금 유지.

진행 데이터는 `CIW.StageFlowTest.v1` 키에 저장합니다. 기존 `CIW.StageProgress.v1` 기록과 분리되어 있습니다.
첫 실행 상태를 새로 확인하려면 Stage Systems의 StageProgressService Save Key를 다른 테스트용 값으로 바꾸세요.
기존 저장 기록을 자동 삭제하지 않습니다.

### 진행 정보 초기화 키

에디터 또는 Development Build의 스테이지 선택 화면에서 **F9를 1.5초 동안 누르면** 현재 저장 키의 클리어 기록을 초기화합니다.
화면 상단에 안내/진행률이 표시되며, 키를 일찍 떼면 취소됩니다. 초기화 직후 완료 표시와 해금 상태가 갱신되어 A만 입장 가능하고 B는 다시 잠깁니다.
플레이 중에는 동작하지 않으며, 일반 출시 빌드에는 이 테스트 입력과 안내가 포함되지 않습니다.
StageFlowTestHUD의 Reset Progress Key / Reset Hold Seconds에서 키와 유지 시간을 변경할 수 있습니다.
현재 테스트 씬의 `CIW.StageFlowTest.v1` 기록만 삭제하며 다른 저장 키나 설정은 지우지 않습니다. 삭제한 클리어 기록은 자동 복구되지 않습니다.

## 주의

프리팹 생성과 씬 연결만으로 런타임 검증이 완료되는 것은 아닙니다. 위 순서를 Play Mode에서 확인하세요.
씬 플레이어는 초기 활성 상태여야 Awake가 실행된 뒤 진행 관리자가 이벤트를 연결할 수 있습니다.
기믹을 추가할 때 초기화할 오브젝트는 구간 프리팹 안에 넣으세요. 전역 상태는 별도 초기화가 필요합니다.
