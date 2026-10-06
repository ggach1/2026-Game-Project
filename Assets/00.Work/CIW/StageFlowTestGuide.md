# Stage Flow 테스트

## 열기

`Assets/00.Work/CIW/StageFlowTest/StageFlowTest.unity`를 열고 Play 합니다.
생성 에디터 스크립트는 최초 임포트 때 누락된 테스트 씬을 생성하며, 기존 씬이 있으면 덮어쓰지 않습니다.
필요하면 `CIW > Stage Flow > Create Test Assets (Only If Missing)` 메뉴로 생성할 수 있습니다.
테스트 생성기는 기존 MenuScene, Build Settings를 변경하지 않습니다. 실제 메뉴의 씬 기반 연결은 아래 CUH 스테이지 항목을 참고하세요.

## 구성

- World.asset: A와 B가 있는 선택 지도.
- Stage_A.asset: A1(평지), A2(중앙 구덩이) 두 구간. 처음부터 해금.
- Stage_B.asset: B1 한 구간. A 전체 완료 후 해금.
- Section_*.prefab: Ground 레이어 발판, Spawn Point, ExitDoor, SectionContext.
- StageNode.prefab: 잠금/완료 표시 및 선택 버튼.
- 공용 Player: 구간 밖에 하나만 존재. 기존 Player 프리팹 재사용.
- Stage Systems: 패널과 별개로 항상 활성화. 진행/로딩/저장/UI 제어.
- StageFlowTestHUD: 진행 구간 표시와 개발용 초기화 입력.
- 각 Section 프리팹의 KillZone2D: y=-6 아래의 낙사 영역을 감지해 Fall 원인으로 사망 요청.

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

### 기믹 사망 연결

톱은 `KillContact2D → IKillable.Kill`로 사망 요청을 전달합니다. 플레이어의 실제 BodyCollider만 허용하고 발/상호작용 센서는 제외합니다. 생존 상태 검사를 통해 중복 사망 요청을 막으며, 기믹은 플레이어 오브젝트를 직접 끄지 않습니다.

`CIW/02.Prefabs/KillZone.prefab`을 가시/즉사 영역에 배치하고 BoxCollider2D 크기와 Cause를 설정하세요. 기본은 Spike이며, 낙사는 Fall로 변경합니다. 움직이는 위험물에는 이동 스크립트를 수정하는 대신 해당 위험 Collider와 같은 오브젝트에 KillZone2D를 붙일 수 있습니다. Trigger와 일반 Collision을 모두 지원하며, Unity 물리 레이어 충돌 허용 및 한쪽 Rigidbody2D가 필요합니다.

기존 Saw에는 이미 사망 판정이 있으므로 KillZone2D를 중복 부착하지 않습니다. 단순 이동/성장/튕기기/순간이동에는 자동으로 사망 판정을 추가하지 않았습니다. 압사나 미사일 본체의 즉사 조건은 별도 설계가 필요합니다.

CIW 테스트 Section_A1/A2/B1은 고정 낙사 영역을 사용합니다. KMJ Stage1~12와 CUH Map1~3에는 `Fall Boundary` 오브젝트를 연결했습니다. Play 시 해당 씬의 활성 비-Trigger Collider(플레이어/즉사 영역 제외)의 시작 경계를 계산하여 하단 5유닛 아래에 낙사 영역을 생성합니다. Inspector에서 Bottom Margin, Horizontal Margin, Depth를 조절할 수 있습니다. 시작 이후 생성되거나 더 아래로 이동하는 플레이 가능 지형, 중력 반전 맵은 별도 경계 설계가 필요합니다.

단독 맵에서는 Player 프리팹의 PlayerRespawnTest가 사망 연출 후 현재 플레이어 씬을 다시 불러옵니다. Auto Respawn과 Reload Scene On Death가 켜져 있어야 합니다. 씬을 저장하고 Play해야 하며, Play 도중에만 추가한 플레이어나 변경한 값은 씬 재로드 시 사라집니다. 플레이어도 편집 모드에서 배치하고 저장하세요. 시작 위치는 저장된 씬의 배치를 따릅니다. 에디터에서는 Build Settings 미등록 씬도 재시도할 수 있지만 빌드에서는 씬 등록이 필요합니다. 저장된 진행 정보는 지우지 않습니다.

단독 맵은 PlayerRespawnTest가 사망 연출 후 SceneRetryController에 재시작을 요청합니다. 별도 컴포넌트 부착은 필요 없습니다. 공통 관리자는 ISceneRetryCleanup 구현 객체(비활성 객체 포함)를 정리한 뒤 씬을 재로드합니다. Saw는 대여한 객체만 기존 Push로 반환하며, 씬에 직접 배치된 톱은 반환하지 않습니다. DevLib/ObjectPool은 변경하지 않습니다.

새 풀 기믹이나 씬 밖에 남는 실행 상태는 ISceneRetryCleanup.CleanupBeforeSceneRetry()에서 코루틴/트윈 등 자기 상태를 정리해야 합니다. 반복 호출에도 안전하게 구현하고, 이 함수에서 씬 로딩/리스폰은 하지 마세요. 인터페이스가 없는 풀 객체까지 자동 반환하는 기능은 아닙니다. 현재 정리 검색은 Single 모드 전체 씬 교체용이며, 동시 실행하는 여러 맵의 선택적 초기화 용도가 아닙니다.

씬에 배치된 일반 기믹은 씬 로딩으로 재생성합니다. 프리팹 구간형 StageFlowController는 자동 씬 재로드를 끄고 기존 구간 재생성 흐름을 유지합니다(공통 전체 씬 정리를 호출하지 않음). 테스트: 톱을 여러 개 발사한 뒤 F8/톱 접촉/낙사로 각각 죽고, 이전 톱이 남지 않는지 및 세 번 이상 연속 재시도가 되는지 확인하세요.

## 실제 메뉴: CUH 스테이지

`Assets/00.Work/GameScene/MenuScene.unity`에서 Play하고 STAGE 01 문을 선택합니다. 이 스테이지는 `Map 1 → Map 2 → Map 3 → Map 4` 네 씬을 순서대로 진행합니다. 각 출구의 입장 연출이 끝나면 다음 맵으로 이동하며, 마지막 맵에서만 완료 기록을 저장하고 메뉴로 복귀합니다. 메뉴에서 다시 선택하면 Map 1부터 재도전합니다. 중간 구간 번호는 실행 중에만 유지하며 앱 재실행 시 이어하기는 제공하지 않습니다.

MenuStageUI가 SceneStageFlowController를 생성하므로 맵마다 추가 컴포넌트를 붙일 필요 없습니다. 기존 PlayerRespawnTest의 사망 재로드는 유지하고, 씬 로드마다 새 ExitDoor에 이벤트를 다시 연결합니다. 맵 씬을 직접 Play하면 기존 단독 사망 테스트 모드이며 순차 진행 세션은 시작하지 않습니다.

검증 순서: 메뉴에서 4 SECTIONS 표시 확인 → Map 1 출구로 Map 2 이동 → Map 2에서 F8/낙사 후 Map 2 시작 위치와 기믹 초기화 확인 → Map 3/4 순차 진행 → 마지막 출구 후 메뉴의 CLEARED 확인 → 다시 선택해 Map 1 진입 확인. 에셋 연결 회귀 테스트는 EditMode의 SceneStageFlowTests입니다.

메뉴와 네 맵은 Build Settings에 등록했습니다. 기존 첫 씬 SampleScene의 순서는 보존했으므로 빌드 시작 화면까지 메뉴로 바꾸려면 MenuScene을 첫 씬으로 설정해야 합니다. DevLib/ObjectPool은 변경하지 않습니다.

회귀 테스트는 Unity Test Runner의 EditMode에서 `KillContact2DTests`를 실행하세요. 센서 제외, 부모 레이어 검사, 중복 사망 방지, 사망 문맥 전달과 낙사 프리팹 설정을 검사합니다.

프리팹 생성과 씬 연결만으로 런타임 검증이 완료되는 것은 아닙니다. 위 순서를 Play Mode에서 확인하세요.
씬 플레이어는 초기 활성 상태여야 Awake가 실행된 뒤 진행 관리자가 이벤트를 연결할 수 있습니다.
기믹을 추가할 때 초기화할 오브젝트는 구간 프리팹 안에 넣으세요. 전역 상태는 별도 초기화가 필요합니다.
