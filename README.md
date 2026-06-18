# 턴제 카드 배틀 게임 — 버거몬스터 채용 과제

## Unity 버전

**Unity 6000.3.2f1 (Unity 6)**

---

## 구현한 기능 목록

### 필수 구현

**전투 시스템**
- 턴 진행 시스템
- 카드 선택 시스템
- 공격 및 스킬 처리
- 카드별 효과 처리
- HP 계산
- 카드 제거 처리
- 신규 카드 자동 배치
- 승리 및 패배 판정
- 간단한 상대 AI

**UI**
- 전투 화면
- 카드 선택 UI
- 현재 턴 표시 UI
- 카드 HP 표시
- 승리 및 패배 결과 화면

### 추가 구현 (가산점)

**카드 종류 확장**
- `탱커` — 공격 불가, 아군 보호 (반격 없음), 탱커 생존 시 다른 카드 타겟 불가
- `어쌔신` — 탱커 무시, 반격 없음, 순간이동 슬래시 연출

**비주얼**
- 카드별 일러스트, 프레임, 타입 아이콘 적용
- 카드 등급 (Normal / Special / Epic) 별 테두리·이름 색상
- 근접 돌진 / 원거리 파티클 투사체 / 어쌔신 순간이동 공격 연출 (DOTween)
- 무쌍 스플래시 피격 시 좌우 넉백 연출 (고무줄 탄성 이징)
- 공격·피격·힐·사망 VFX (FXPool 오브젝트 풀링)
- 카드 덱 배치 연출 (앞뒤 플립)
- 롱프레스 카드 설명 팝업 (`FloatingDesc`)
- 게임 시작 인트로 / 턴 코인·배너 전환 애니메이션
- 로비·카드 편집 패널 슬라이드 입장 연출 (화면 밖 → 원위치 + OutBack 이징)

**시스템**
- `CardManager` (DontDestroyOnLoad) — PlayerPrefs 기반 컬렉션·덱·강화 영속화
- 최초 실행 시 Normal 카드 전체 자동 지급 및 덱 자동 등록
- 상점 — 1뽑 / 10뽑, Normal 60% / Special 30% / Epic 10% 확률
- 카드 편집 화면 — 덱 슬롯 8개, 카드 DOTween 이동 배치, 등급·타입 순 정렬
- 카드 강화 — 최대 1회, HP 증가, 인게임 즉시 반영
- 카드 상세 패널 — 장착 / 강화 버튼, 조건부 비활성화
- 적 덱 — 매 게임 CardLibrary에서 랜덤 10장 (동일 확률)
- 신입 유저 튜토리얼 — 로비 / 전투 단계별 가이드 (dimOverlay + UI Raise 방식)
- 힐러 턴 시작 시 HP 즉시 반영 후 입력 잠금 → VFX 완료 후 해제

---

## 주요 코드 구조

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs        # 덱 구성, GameState 관리
│   ├── TurnManager.cs        # 턴 진행, 카드 선택 흐름, 힐러 턴시작 효과
│   ├── BattleManager.cs      # 공격 처리, AttackResult 계산, 승패 판정
│   ├── CardManager.cs        # 컬렉션·덱·강화·PlayerPrefs 관리 (Singleton)
│   ├── UIManager.cs          # 전투 필드 상태 동기화, FX 트리거
│   ├── FXPool.cs             # VFX 오브젝트 풀
│   ├── TutorialManager.cs    # 전투 튜토리얼 단계 관리
│   ├── AttackResult.cs       # 공격 결과 데이터 구조체
│   ├── Enums.cs              # CardType, CardRarity, Owner, GameState 등
│   ├── LobbyManager.cs       # 로비 버튼 연결
│   └── SceneNames.cs         # 씬 이름 상수
│
├── Card/
│   ├── Data/
│   │   ├── CardDataBase.cs          # 추상 ScriptableObject (HP, 등급, 강화)
│   │   ├── NormalCardData.cs
│   │   ├── RangedCardData.cs
│   │   ├── MusoCardData.cs
│   │   ├── HealerCardData.cs
│   │   ├── TankerCardData.cs
│   │   ├── AssassinCardData.cs
│   │   └── OwnedCardEntry.cs        # 소유 카드 단위 (데이터 + 강화 레벨)
│   ├── Effects/
│   │   ├── CardEffect.cs            # 효과 추상 클래스 (Strategy 패턴)
│   │   ├── NormalEffect.cs
│   │   ├── RangedEffect.cs
│   │   ├── MusoEffect.cs            # 스플래시 50% 피해
│   │   ├── HealerEffect.cs          # 턴 시작 시 아군 HP +1 즉시 적용
│   │   ├── TankerEffect.cs
│   │   └── AssassinEffect.cs        # IgnoresTanker, 반격 없음
│   ├── Visual/
│   │   └── CardVisualConfig.cs      # 일러스트, 프레임 색상, 타입 아이콘 매핑
│   ├── CardInstance.cs       # 런타임 카드 인스턴스 (현재 HP, 이벤트)
│   ├── CardField.cs          # 3슬롯 필드 + 대기 큐, 자동 보충 로직
│   ├── CardLibrary.cs        # 전체 카드 마스터 목록 ScriptableObject
│   ├── CardView.cs           # 인게임 카드 뷰 (연출 진입점)
│   ├── CardAttackAnimator.cs # 근접·원거리·어쌔신·넉백·스폰 DOTween 연출
│   ├── CardDeathAnimator.cs  # 카드 사망 연출
│   ├── Projectile.cs         # 원거리 투사체 이동 로직
│   └── BattleSlot.cs         # 전투 슬롯 (카드 배치, 클릭 처리, 하이라이트)
│
├── AI/
│   └── EnemyAI.cs            # 위협도 점수 기반 공격자·타겟 결정
│
└── UI/
    ├── CardUIView.cs          # 상점·결과 카드 뷰
    ├── CardEditPanel.cs       # 덱 편집 패널 (입장 슬라이드 연출 포함)
    ├── CardEditCardView.cs    # 컬렉션 카드 아이템
    ├── CardDetailPanel.cs     # 카드 상세·장착·강화 패널
    ├── DeckSlotView.cs        # 덱 슬롯
    ├── ShopManager.cs         # 상점 뽑기 로직
    ├── LobbyPanel.cs          # 공용 슬라이드 패널 (instantShow 옵션 지원)
    ├── LobbyTutorialManager.cs# 로비 튜토리얼 단계 관리
    ├── ResultPanel.cs         # 승패 결과 패널
    ├── HPText.cs              # HP 텍스트 표시
    ├── FloatingDesc.cs        # 롱프레스 카드 설명 팝업
    ├── TurnBanner.cs          # 턴 전환 배너 연출
    ├── TurnCoin.cs            # 턴 코인 애니메이션
    ├── WaitingCardCount.cs    # 대기 카드 수 표시
    └── ButtonScaleEffect.cs   # 버튼 탭 스케일 피드백
```

### 설계 핵심

- **Strategy 패턴** — `CardEffect` 추상 클래스를 상속받아 카드 타입별 효과를 독립 구현. 새 카드 추가 시 기존 코드 수정 없음
- **ScriptableObject 데이터 분리** — `CardDataBase` SO → `CardInstance` 런타임 복사본으로 데이터와 상태를 완전 분리
- **PlayerPrefs 영속화** — 소유 카드·덱·강화 레벨을 키 기반으로 저장, 앱 재시작 후 복원
- **FXPool 오브젝트 풀링** — VFX GameObject를 재사용해 GC 부하 최소화
- **DOTween 연출** — 모든 이동·공격·UI 애니메이션을 DOTween Sequence로 처리, Inspector에서 타이밍 조정 가능

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
| Epic Toon FX | Unity Asset Store |
| Damage Numbers Pro | CodeMonkeyUnity (Unity Asset Store) |
| DOTween | Demigiant (Unity Asset Store) |
| SB 어그로 폰트 | 산돌구름 |
