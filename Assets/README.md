# 턴제 카드 배틀 게임 — 버거몬스터 채용 과제

## Unity 버전

**Unity 6000.3.2f1 (Unity 6)**

---

## 구현한 기능 목록

### 필수 구현

| 기능 | 내용 |
|---|---|
| 턴 진행 시스템 | 플레이어 턴 → 적 턴 교대, 턴 번호 표시 |
| 카드 선택 시스템 | 아군 카드 선택 → 적 카드 타겟 선택 순서 |
| 공격 및 효과 처리 | 카드 타입별 효과 Strategy 패턴으로 분리 |
| HP 계산 | 공격 피해, 반격 피해, 힐 계산 |
| 카드 제거 및 자동 배치 | 사망 시 대기 큐에서 자동 보충 |
| 승패 판정 | 모든 카드 제거 시 Win/Lose 처리 |
| 상대 AI | 위협도 점수 기반 공격자·타겟 선택 |
| 전투 UI | 카드 HP, 현재 턴, 승패 결과 화면 |

### 추가 구현 (가산점)

**카드 종류 확장**
- `탱커` — 공격 불가, 아군 보호 (반격 없음), 탱커 존재 시 다른 카드 타겟 불가
- `어쌔신` — 탱커 무시, 반격 피해 없음, 순간이동 연출

**비주얼**
- 카드별 일러스트, 프레임, 타입 아이콘 적용
- 근접 돌진 / 원거리 투사체 / 어쌔신 순간이동 공격 연출 (DOTween)
- 공격·피격·힐·사망 VFX
- 카드 등급(Normal/Special/Epic)별 테두리·이름 색상
- 카드 덱 배치 연출 (앞뒤 플립)
- 롱프레스 카드 설명 팝업 (`FloatingDesc`)
- 게임 시작 인트로 / 턴 텍스트 전환 애니메이션

**시스템**
- `CardManager` (DontDestroyOnLoad) — PlayerPrefs 기반 컬렉션·덱·강화 영속화
- 최초 실행 시 Normal 카드 전체 자동 지급 및 덱 등록
- 상점 — 1뽑 / 10뽑, Normal 60% / Special 30% / Epic 10% 확률
- 카드 편집 화면 — 덱 슬롯 8개, 카드 DOTween 이동 배치, 등급·타입 순 정렬
- 카드 강화 — 최대 1회, HP 증가, 인게임 즉시 반영
- 카드 상세 패널 — 장착 / 강화 버튼, 버튼 조건부 비활성화
- 적 덱 — 매 게임 CardLibrary에서 랜덤 10장, 등급 확률 Inspector에서 별도 설정 가능

**튜토리얼**
- 인게임 튜토리얼 — 최초 실행 시 로비 없이 GameScene 직행, TutorialCanvas(별도 Sort Order)로 지정 카드 하이라이트, DimOverlay + 카드 이동 가이드
- 공격 완료 후 AttributeTutoSet → HpTutoSet 순차 팝업 (Scale 연출, 클릭 쿨다운)
- 로비 튜토리얼 — 상점버튼 → chest1 → 결과패널 → 카드편집버튼 순차 가이드, 단계별 TMP 텍스트 페이드 연출
- 모든 튜토리얼 UI 요소는 단계 전환 시 원래 Canvas 위치로 자동 복원

**인게임 UX**
- 초기 딜·공격 애니메이션·교체 스폰 중 클릭 차단 (`IsInteractionLocked` 카운터 시스템)
- 상태 전환 시 `EventSystem.SetSelectedGameObject(null)` — 빈 슬롯 흰 배경 현상 방지

**최적화**
- `FXPool` — FX 프리팹 오브젝트 풀링, Instantiate/Destroy 제거로 모바일 GC 스파이크 방지
- `BattleManager` — 공격 처리 시 Dictionary/List 재사용, 매 턴 GC 할당 제거

---

## 주요 코드 구조

```
Assets/Scripts/
├── Core/
│   ├── CardManager.cs       # 컬렉션·덱·강화·PlayerPrefs 관리 (Singleton)
│   ├── GameManager.cs       # 덱 구성, GameState 관리
│   ├── TurnManager.cs       # 턴 진행, 카드 선택 흐름, 힐러 턴시작 효과
│   ├── BattleManager.cs     # 공격 처리, 승패 판정
│   ├── Enums.cs             # CardType, CardRarity, Owner, GameState 등
│   └── SceneNames.cs        # 씬 이름 상수
│
├── Card/
│   ├── Data/
│   │   ├── CardDataBase.cs          # 추상 ScriptableObject (HP, 등급, 강화)
│   │   ├── NormalCardData.cs        # 일반 카드 데이터
│   │   ├── RangedCardData.cs        # 원거리 카드 데이터
│   │   ├── MusoCardData.cs          # 무쌍 카드 데이터
│   │   ├── HealerCardData.cs        # 힐러 카드 데이터
│   │   ├── TankerCardData.cs        # 탱커 카드 데이터
│   │   ├── AssassinCardData.cs      # 어쌔신 카드 데이터
│   │   └── OwnedCardEntry.cs        # 소유 카드 단위 (데이터 + 강화 레벨)
│   ├── Effects/
│   │   ├── CardEffect.cs            # 효과 추상 클래스 (Strategy 패턴)
│   │   ├── NormalEffect.cs
│   │   ├── RangedEffect.cs
│   │   ├── MusoEffect.cs            # 스플래시 50%
│   │   ├── HealerEffect.cs          # 턴 시작 시 아군 힐
│   │   ├── TankerEffect.cs
│   │   └── AssassinEffect.cs        # IgnoresTanker, 반격 없음
│   ├── CardInstance.cs      # 런타임 카드 인스턴스 (현재 HP, 이펙트)
│   ├── CardField.cs         # 3슬롯 필드 + 대기 큐, 자동 보충
│   ├── CardLibrary.cs       # 전체 카드 마스터 목록 ScriptableObject
│   └── Visual/
│       └── CardVisualConfig.cs      # 일러스트, 프레임 색상, 아이콘 등
│
├── AI/
│   └── EnemyAI.cs           # 위협도 점수 기반 공격자·타겟 결정
│
└── UI/
    ├── CardView.cs           # 인게임 카드 뷰 (공격·피격·사망 연출)
    ├── CardUIView.cs         # 상점·상세 카드 뷰
    ├── CardEditPanel.cs      # 덱 편집 패널
    ├── CardEditCardView.cs   # 컬렉션 카드 아이템
    ├── CardDetailPanel.cs    # 카드 상세·장착·강화 패널
    ├── DeckSlotView.cs       # 덱 슬롯
    ├── ShopManager.cs        # 상점 뽑기 로직
    ├── LobbyPanel.cs         # 공용 슬라이드 패널
    ├── LobbyManager.cs       # 로비 버튼 연결, 튜토리얼 첫 진입 분기
    ├── LobbyTutorialManager.cs  # 로비 튜토리얼 단계 관리
    └── BattleSlot.cs         # 전투 슬롯 (카드 배치·하이라이트·클릭 차단)
│
├── Core/
│   └── TutorialManager.cs   # 인게임 튜토리얼 (슬롯 하이라이트, 딤, 팝업 순서 관리)
│   └── FXPool.cs            # FX 오브젝트 풀 (prefab 키 기반 Queue 관리)
```

### 설계 핵심

- **Strategy 패턴** — `CardEffect` 추상 클래스를 상속받아 카드 타입별 효과를 독립 구현. 새 카드 추가 시 기존 코드 수정 없음
- **ScriptableObject 기반 데이터** — 카드 데이터와 런타임 인스턴스 분리. `CardDataBase` SO → `CardInstance` 런타임 복사본
- **PlayerPrefs 영속화** — 소유 카드·덱·강화 레벨을 키 기반으로 저장, 앱 재시작 후 복원
- **DOTween 연출** — 모든 이동·공격·UI 애니메이션을 DOTween으로 처리, Inspector에서 타이밍 조정 가능
- **튜토리얼 Canvas 분리** — TutorialCanvas(높은 Sort Order)로 특정 UI 요소를 임시 reparent해 DimOverlay 위에 렌더링, 단계 완료 시 원래 Canvas 복원
- **인터랙션 락** — `UIManager.IsInteractionLocked` 카운터로 애니메이션 중 전체 슬롯 클릭 차단, 동시 다중 애니메이션도 안전하게 처리

---

## AI 도구 활용

| 활용 범위 | 도구 |
|---|---|
| 전체 코드 설계·작성·리팩토링·디버깅 | Claude Code (claude-sonnet-4-6) |
| 카드 일러스트 및 UI 비주얼 리소스 생성 | AI 이미지 생성 도구 |

---

## 외부 에셋

| 에셋 | 출처 |
|---|---|
| Green Card TCG kit | FantasyLoft (Unity Asset Store) |
| Epic Toon FX | MTC Games (Unity Asset Store) |
| SB 어그로 폰트 | 산돌구름 |
| DOTween | Demigiant (Unity Asset Store) |
| Damage Numbers Pro | CodeMonkeyUnity (Unity Asset Store) |
