# 진행 현황 (Progress)

> 마지막 업데이트: 2026-06-16

---

## Done

### 코어 로직
- [x] `Card/Data/CardDataBase.cs` (abstract SO) + `NormalCardData`, `RangedCardData`, `MusoCardData`, `HealerCardData`, `TankerCardData`
- [x] `Card/Visual/CardVisualConfig.cs` (`illustration`, `typeIcon`, `frameColor` 등)
- [x] `Card/Effects/CardEffect.cs` (Strategy) + `NormalEffect`, `RangedEffect`, `MusoEffect`, `HealerEffect`, `TankerEffect`
- [x] `Card/CardInstance.cs`, `Card/CardField.cs` (3슬롯 필드 + 대기 카드 큐, 자동 보충/사망 처리)
- [x] `Core/Enums.cs` (`Owner`, `GameState`, `GameResult`, `CardType`)
- [x] `Core/GameManager.cs` (덱 보관, GameState 관리, `OnStateChanged` 이벤트)
- [x] `Core/TurnManager.cs` (턴 진행, 카드/대상 선택 흐름, 힐러 턴시작 효과, `TurnNumber` 프로퍼티)
- [x] `Core/BattleManager.cs` (공격 처리, 승패 판정, `TotalKills` 프로퍼티)
- [x] `AI/EnemyAI.cs` (공격자=HP최대, 대상=HP최소·동점시 슬롯낮은순)

### 카드 타입 시스템
- [x] `CardType` enum 추가 (`Normal`, `Ranged`, `Muso`, `Healer`, `Tanker`)
- [x] `CardDataBase.CardType` abstract 프로퍼티, 각 데이터 클래스에 override 구현
- [x] `CardDataBase`에 `feature` / `description` 필드 추가
- [x] 내부 타입 아이콘 (`innerTypeIconImage`) — `UIManager.GetInnerTypeIcon(CardType)`으로 캐싱, CardView에서 Bind 시 적용

### 탱커 카드
- [x] `TankerCardData` / `TankerEffect` 추가 (공격 불가, 반격 피해 없음)
- [x] 탱커가 필드에 있으면 탱커만 공격 가능 (`CardField.HasActiveTanker()`) — 플레이어/적 AI 양쪽 적용
- [x] 내 탱커 클릭 시 빨간 테두리 + 흔들림 연출 (`CardView.PlayReject`)
- [x] 상대 탱커 있을 때 다른 카드 클릭 시 상대 탱커에 빨간 테두리 + 흔들림 (`UIManager.PlayTankerBlockReject`)
- [x] `RejectBorder` — `SelectionBorder.shader` 기반 빨간 머티리얼, CardPrefab에 자식 오브젝트로 추가
- [x] 탱커 SO 에셋 생성 (`TankerCardData` 타입), playerDeck/enemyDeck 배정

### 전투 연출
- [x] 근접 공격 lunge 애니메이션 + 히트 리액션 (DOTween)
- [x] 원거리 공격 투사체 연출 (`Projectile.cs`, `CardView.PlayProjectile`)
- [x] 무쌍 스플래시 히트 VFX / 데미지 표시 (`AttackResult.SplashHits` / `UIManager.PlaySplashHits`)
- [x] 공격/피격/사망 순서 동기화 (애니메이션 완료 콜백 체인)
- [x] HP 갱신 타이밍 — 피격 순간(`onImpact`/`onArrive`)에만 `RefreshHP()` 호출
- [x] 힐 연출 — `QueueHeal` / `ApplyHeal` 분리로 VFX 타이밍에 HP 적용, 힐 텍스트 `+N` 표시
- [x] 덱에서 카드 배치 연출 (`PlaySpawnFromDeck`, 앞뒤 플립 포함)
- [x] 피격 수신 VFX (`receivedHitFXPrefab` / `receivedHitFXOffset` / `receivedHitFXRotation`) — config에서 설정, 탱커 등 카드별 적용
- [x] 사망 VFX (`deathFXPrefab`) — `CardView.SpawnDeathFX`, sortingOrder 100으로 카드 위에 렌더링

### 카드 선택 UI
- [x] 선택 하이라이트 — 흰색 테두리+글로우 쉐이더 (`UI/SelectionBorder.shader`)
- [x] 빈 슬롯 시각화 — 카드 있어도 `emptyVisual` 항상 카드 뒤에 표시

### 카드 시각
- [x] `frameOutline` 이미지 — 게임 플레이 중 Z축 무한 회전 (DOTween Loop)
- [x] `cardDescText` — `feature` 필드 표시
- [x] `FloatingDesc` 스크립트 작성 — 마우스 호버 시 feature + description 표시, sortingOrder 999

### 무쌍 반격 피해
- [x] `MusoEffect.Execute`에 반격 피해 로직 추가 (`primaryTarget.effect.DealsCounterDamage` 체크)

### WIN/LOSE 연출
- [x] Win — 흰색 플래시 → winGO 활성화 → W·I·N 글자 OutBack 팝인 스태거
- [x] Lose — 검정 오버레이 + 비네트 → loseGO 활성화 → L·O·S·E 글자 페이드인 스태거
- [x] `ResultPanel`에 `ScreenFlash`, `DarkOverlay`, `VignetteImage` 오버레이
- [x] 배경색 (`winBgColor` / `loseBgColor`) 인스펙터에서 설정 가능
- [x] 테스트용 단축키 — `1`=Win, `2`=Lose (`#if UNITY_EDITOR`)

### 결과 패널 UI
- [x] `ResultPanel` — 결과 관련 로직 전담 (버튼, winGO/loseGO, 글자 연출, 스탯 표시)
- [x] `retryButton` 씬 재시작 / `lobbyButton` GoToLobby 스텁
- [x] 버튼 등장 연출 — 화면 밖 아래에서 OutBack 슬라이드 인 (Win 0.4s / Lose 1.1s 딜레이)
- [x] 결과창 스탯 — 총 처치 수 (`TotalKills`) / 총 턴 수 (`TurnNumber`) 표시

### UI 인트로 / 턴 연출
- [x] 게임 시작 시 `turnPanel` 위에서 천천히 내려오는 연출
- [x] 게임 시작 시 `playerWaitingCount` / `enemyWaitingCount` 화면 밖 아래·위에서 OutBack 반동 슬라이드 인
- [x] 턴 변경 시 턴 텍스트 — scale→0 빠르게 → 텍스트 교체 + scale 1 복원 → 위에서 내려오는 연출

### UI 아키텍처
- [x] `UIManager`에 `turnCountText` 캐싱, `PlayerSelectCard` 상태마다 업데이트
- [x] `ResultPanel` — 표시 로직만 담당, 버튼/winGO/loseGO/글자 캐싱 포함
- [x] `Show(GameResult, int kills, int turns)` 시그니처로 스탯 전달

### 에디터 / 씬 구성
- [x] Canvas 설정 (Screen Space - Overlay, Canvas Scaler 1080x1920 / Match 0.5)
- [x] 매니저 오브젝트 생성 + 컴포넌트 배치 / 슬롯 연결
- [x] `BattleSlotPrefab` — BattleSlot + EmptyVisual / HighlightVisual, CardView 런타임 주입
- [x] `GameManager` playerDeck / enemyDeck 각 6장 구성
- [x] `WaitingCardCount`, `TurnBanner`, `ResultPanel` UI 제작 및 연결

---

## Todo

### FloatingDesc UI
- [ ] FloatingDesc 씬 배치 및 인스펙터 연결 (featureText / descText)
- [ ] RectTransform Anchor (0,0) 설정 확인

### 에셋
- [ ] `CardVisualConfig` 4종에 `illustration` / `typeIcon` 스프라이트 할당
  - Bind() 호출 시 null이면 아이콘 비활성화됨 주의
- [ ] 각 카드 SO에 `feature` / `description` 텍스트 입력

### 가산점
- [ ] TitleScene 구성
- [ ] DeckScene (덱 편성/도감)
- [ ] 사운드 이펙트 (공격, 피격, 힐, 사망)

### 제출 준비
- [ ] Android 64비트 빌드 (APK)
- [ ] 플레이 영상 녹화
- [ ] README 작성 (Unity 버전, 구현 기능, 코드 구조, AI 활용 내역)
