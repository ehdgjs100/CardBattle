# 진행 현황 (Progress)

> 마지막 업데이트: 2026-06-14

---

## Done

### 코어 로직 (스크립트, 컴파일 확인됨)
- [x] `Card/Data/CardDataBase.cs` (abstract SO) + `NormalCardData`, `RangedCardData`, `MusoCardData`, `HealerCardData`
- [x] `Card/Visual/CardVisualConfig.cs` (`illustration`, `typeIcon`, `frameColor` 등)
- [x] `Card/Effects/CardEffect.cs` (Strategy) + `NormalEffect`, `RangedEffect`, `MusoEffect`, `HealerEffect`
- [x] `Card/CardInstance.cs`, `Card/CardField.cs` (3슬롯 필드 + 대기 카드 큐, 자동 보충/사망 처리)
- [x] `Core/Enums.cs` (`Owner`, `GameState`, `GameResult`)
- [x] `Core/GameManager.cs` (덱 보관, GameState 관리, `OnStateChanged` 이벤트)
- [x] `Core/TurnManager.cs` (턴 진행, 카드/대상 선택 흐름, 힐러 턴시작 효과)
- [x] `Core/BattleManager.cs` (공격 처리, 승패 판정)
- [x] `AI/EnemyAI.cs` (공격자=HP최대, 대상=HP최소·동점시 슬롯낮은순)

### UI / View 스크립트
- [x] `Card/CardView.cs` (일러스트/프레임색/타입아이콘/이름/HP텍스트 바인딩, `SetFaceDown`으로 앞뒤 토글)
- [x] `Card/BattleSlot.cs` (슬롯 바인딩, 하이라이트, `IPointerClickHandler` 클릭 처리)
- [x] `Core/UIManager.cs` (상태/선택 이벤트 구독 → 필드/배너/결과창 갱신)
- [x] `UI/HPText.cs`(HP를 텍스트로 표시, 기존 `HPBar.cs`는 삭제), `UI/TurnBanner.cs`, `UI/WaitingCardCount.cs`, `UI/ResultPanel.cs`

### 테스트 덱 SO 에셋 (`Assets/Datas/`)
- [x] `Cards/Normal_Knight`(기사 HP10), `Cards/Ranged_Archer`(궁수 HP8), `Cards/Muso_Dragon`(드래곤 HP12, splash 0.5), `Cards/Healer_Cleric`(성직자 HP6, heal 1)
- [x] `Visuals/Visual_Knight`(회색) / `Visual_Archer`(파랑) / `Visual_Dragon`(빨강) / `Visual_Cleric`(초록) — frameColor만 설정, illustration/typeIcon 미설정

### 카드 프리팹 (`Assets/Prefabs/CardPrefab.prefab`)
- [x] 구조: `CardPrefab(CardView) > Front(Background, Illustration, TypeIcon, NameText, HPText) / CardBack`
- [x] 앞/뒤 토글: `CardView.SetFaceDown(bool)` → `Front`/`CardBack` 활성화 전환
- [x] Front 배경과 CardBack 배경 분리 (루트 공유 Image 제거, 각각 독립적인 sprite 적용 가능)
- [x] HP를 bar 대신 텍스트로 표시
- [x] 카드 프레임 / HP 아이콘 / 타입 아이콘 실제 아트 적용 완료, 테스트 카드 3개(TestCard_Knight/Archer/Dragon) 씬에 배치해 비주얼 확인

### 에디터 작업
- [x] Canvas 설정 (Screen Space - Overlay, Canvas Scaler 1080x1920 / Match 0.5)
- [x] 매니저 오브젝트 생성 + `GameManager`/`TurnManager`/`BattleManager`/`UIManager` 부착
- [x] GameManager `playerDeck` / `enemyDeck` 리스트 구성 (각 6장, 기존 SO 4종 조합: Normal/Archer/Muso/Cleric/Normal/Archer)
- [x] `BattleSlotPrefab` 제작 (BattleSlot + EmptyVisual/HighlightVisual, CardView는 완전히 분리), `CardCanvas`에 Player_Slot01~03(상단)/Enemy_Slot01~03(하단) 3슬롯씩 배치
- [x] `BattleSlot.SetCardView`로 런타임에 CardView 주입, `UIManager.Awake`에서 슬롯마다 `CardPrefab` 인스턴스 생성 후 바인딩
- [x] `GameManager`의 `SpawnTestCards`/테스트용 필드 제거, `UIManager.playerSlots`/`enemySlots`/`cardViewPrefab` 연결
- [x] `UIManager`의 미배치 UI(`playerWaitingCount`/`enemyWaitingCount`/`turnBanner`/`resultPanel`) 참조에 null 조건부 연산자 적용 (NRE 방지)
- [x] Play 모드 확인: 플레이어/적 카드 3장씩 슬롯에 정상 배치 (노말/궁수/무쌍 ↔ 궁수/노말/성직자)
- [x] 상단(적 영역) / 하단(플레이어 영역) `WaitingCardCount` 2개 제작 (`PlayerWaitingCount`/`EnemyWaitingCount`, TMP 텍스트로 대기 카드 수 표시)
- [x] `TurnBanner` (CanvasGroup + TMP_Text), `ResultPanel`(결과 텍스트 + 재시작 버튼) 제작 후 `UIManager`에 연결
- [x] Play 모드 테스트: 카드 선택/하이라이트 → 공격 확정(`ConfirmAttack`) → 대상 선택 → HP 변화/자동교체 → 대기 카드 수 갱신 → 턴 배너 갱신, 콘솔 에러 0건 확인
- [x] 공격 흐름 변경: `ActionPanel`(공격 버튼) 제거, 내 카드 선택 → 적 카드 클릭 시 즉시 공격, 다른 내 카드 클릭 시 공격자 전환 (`PlayerSelectTarget` 상태/`ConfirmAttack` 제거)
- [x] 공격 연출 추가: `Card/CardAttackAnimator.cs` (DOTween) — 근접(`IsMelee`) 카드는 대상 쪽으로 이동(lunge) 후 scale 펀치 + 복귀, 피격 카드는 슬라임처럼 scale이 한 번 커졌다가 작아지는 히트 리액션. `CardEffect`에 `IsMelee` 추가(`Normal`/`Healer`=true, `Ranged`/`Muso`=false), `BattleManager.OnAttackPerformed` 이벤트로 `UIManager.HandleAttackPerformed`에서 연출 트리거
- [x] 공격 순서 동기화: `BattleManager.ApplyAttack`/`OnAttackPerformed`가 완료 콜백을 받아, 공격 연출(복귀 포함)이 끝난 뒤에 `ProcessDeaths`/필드 갱신 및 다음 공격(`EndTurn`→상대 턴)이 진행되도록 변경. 플레이어 공격 애니메이션 완료 후에만 적 턴이 시작되며, 적 공격도 동일한 연출 파이프라인을 통해 애니메이션 재생 후 다음 턴으로 진행
- [x] `TurnManager.enemyTurnDelay`(0.3s, DOTween `DOVirtual.DelayedCall`) 추가: 플레이어 공격 결과(필드 갱신, 사망 카드 교체, 복귀 완료)가 화면에 보인 뒤 약간의 텀을 두고 적 턴 공격이 시작되도록 함
- [x] 원거리/무쌍(`IsMelee=false`) 공격 시 공격 카드에도 연출 추가: `CardAttackAnimator.PlayAttackPulse()`(scale 펀치)를 공격 카드에 재생하고, 피격 카드는 기존 `PlayHitReaction` 유지 (`UIManager.HandleAttackPerformed`)

---

## Todo

### 에디터 작업 (다음에 이어서)
- [ ] `CardVisualConfig` 4종(Normal/Archer/Muso/Cleric)에 `illustration`/`typeIcon` 스프라이트 할당
  - 주의: 현재 CardPrefab의 Illustration/TypeIcon에 직접 넣은 placeholder 이미지는 `Bind()` 호출 시 `visual.illustration`/`visual.typeIcon`(현재 null)로 덮어써지며, `typeIcon`이 null이면 아이콘이 비활성화됨
- [ ] 승패(Win/Lose) 상황까지 게임을 진행해 `ResultPanel` 표시 및 재시작 버튼 동작 확인

### 가산점 (Task #8 이후)
- [ ] FX/애니메이션 (DOTween, 공격/피격/사망 연출)
- [ ] TitleScene, ResultScene 구성
- [ ] DeckScene (덱 편성/도감)

### 제출 준비 (마지막)
- [ ] Android 64비트 빌드 (APK)
- [ ] 플레이 영상 녹화
- [ ] README 작성 (Unity 버전, 구현 기능, 코드 구조, AI 활용 내역)
