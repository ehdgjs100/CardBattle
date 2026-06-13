# 진행 현황 (Progress)

> 마지막 업데이트: 2026-06-12

---

## Done

### 코어 로직 (스크립트, 컴파일 확인됨)
- [x] `Card/Data/CardDataBase.cs` (abstract SO) + `NormalCardData`, `RangedCardData`, `MusoCardData`, `HealerCardData`
- [x] `Card/Visual/CardVisualConfig.cs`
- [x] `Card/Effects/CardEffect.cs` (Strategy) + `NormalEffect`, `RangedEffect`, `MusoEffect`, `HealerEffect`
- [x] `Card/CardInstance.cs`, `Card/CardField.cs` (3슬롯 필드 + 대기 카드 큐, 자동 보충/사망 처리)
- [x] `Core/Enums.cs` (`Owner`, `GameState`, `GameResult`)
- [x] `Core/GameManager.cs` (덱 보관, GameState 관리, `OnStateChanged` 이벤트)
- [x] `Core/TurnManager.cs` (턴 진행, 카드/대상 선택 흐름, 힐러 턴시작 효과)
- [x] `Core/BattleManager.cs` (공격 처리, 승패 판정)
- [x] `AI/EnemyAI.cs` (공격자=HP최대, 대상=HP최소·동점시 슬롯낮은순)

### UI / View 스크립트
- [x] `Card/CardView.cs` (일러스트/프레임색/이름/HP바 바인딩)
- [x] `Card/BattleSlot.cs` (슬롯 바인딩, 하이라이트, `IPointerClickHandler` 클릭 처리)
- [x] `Core/UIManager.cs` (상태/선택 이벤트 구독 → 필드/배너/액션패널/결과창 갱신)
- [x] `UI/HPBar.cs`, `UI/TurnBanner.cs`, `UI/ActionPanel.cs`, `UI/WaitingCardCount.cs`, `UI/ResultPanel.cs`

### 테스트 덱 SO 에셋 (`Assets/ScriptableObjects/`)
- [x] `Cards/Normal_Knight`(기사 HP10), `Cards/Ranged_Archer`(궁수 HP8), `Cards/Muso_Dragon`(드래곤 HP12, splash 0.5), `Cards/Healer_Cleric`(성직자 HP6, heal 1)
- [x] `Visuals/Visual_Knight`(회색) / `Visual_Archer`(파랑) / `Visual_Dragon`(빨강) / `Visual_Cleric`(초록) — frameColor만 설정, 일러스트 없음

### 에디터 작업
- [x] Canvas 설정 (Screen Space - Overlay, Canvas Scaler 1080x1920 / Match 0.5)
- [x] 매니저 오브젝트 생성 + `GameManager`/`TurnManager`/`BattleManager`/`UIManager` 부착

---

## Todo

### 에디터 작업 (다음에 이어서)
- [ ] GameManager `playerDeck` / `enemyDeck` 리스트 구성 (각 6장, 기존 SO 4종 조합)
- [ ] 카드 슬롯 프리팹(BattleSlot + CardView + HPBar 구조) 제작, 적/플레이어 필드 3슬롯씩 배치
- [ ] 상단(적 영역) / 하단(플레이어 영역) 레이아웃 + `WaitingCardCount` 2개
- [ ] `TurnBanner` (CanvasGroup + TMP_Text), `ActionPanel`(공격 버튼), `ResultPanel`(결과 텍스트 + 재시작 버튼)
- [ ] `UIManager`에 슬롯 6개 + 위 UI 컴포넌트 참조 연결
- [ ] Play 모드 테스트: 카드 표시 → 선택/하이라이트 → 공격 → HP 변화/자동교체 → 승패 판정 → 결과창

### 가산점 (Task #8 이후)
- [ ] FX/애니메이션 (DOTween, 공격/피격/사망 연출)
- [ ] TitleScene, ResultScene 구성
- [ ] 카드 일러스트 적용
- [ ] DeckScene (덱 편성/도감)

### 제출 준비 (마지막)
- [ ] Android 64비트 빌드 (APK)
- [ ] 플레이 영상 녹화
- [ ] README 작성 (Unity 버전, 구현 기능, 코드 구조, AI 활용 내역)
