# 진행 현황 (Progress)

> 마지막 업데이트: 2026-06-17

---

## Done

### 코어 로직
- [x] `Card/Data/CardDataBase.cs` (abstract SO) + `NormalCardData`, `RangedCardData`, `MusoCardData`, `HealerCardData`, `TankerCardData`, `AssassinCardData`
- [x] `Card/Visual/CardVisualConfig.cs` (`illustration`, `typeIcon`, `frameColor` 등)
- [x] `Card/Effects/CardEffect.cs` (Strategy) + `NormalEffect`, `RangedEffect`, `MusoEffect`, `HealerEffect`, `TankerEffect`, `AssassinEffect`
- [x] `Card/CardInstance.cs`, `Card/CardField.cs` (3슬롯 필드 + 대기 카드 큐, 자동 보충/사망 처리)
- [x] `Core/Enums.cs` (`Owner`, `GameState`, `GameResult`, `CardType`, `CardRarity`)
- [x] `Core/GameManager.cs` (덱 보관, GameState 관리, `OnStateChanged` 이벤트, CardManager fallback)
- [x] `Core/TurnManager.cs` (턴 진행, 카드/대상 선택 흐름, 힐러 턴시작 효과, `TurnNumber` 프로퍼티)
- [x] `Core/BattleManager.cs` (공격 처리, 승패 판정, `TotalKills` 프로퍼티)
- [x] `AI/EnemyAI.cs` (공격자=HP최대, 대상=HP최소·동점시 슬롯낮은순, Assassin 탱커 무시)

### 카드 타입 시스템
- [x] `CardType` enum + `CardRarity` enum (`Normal`, `Special`, `Epic`)
- [x] 등급별 border 색상 (`CardDataBase.GetRarityColor()`) — 인게임/상점 공유
- [x] `CardDataBase`에 `rarity`, `hpPerUpgrade`, `ApplyUpgrade(int level)` 추가
- [x] `HealerCardData` — `healPerUpgrade` 강화 override
- [x] `MusoCardData` — `splashRatioPerUpgrade` 강화 override

### 탱커 카드
- [x] `TankerCardData` / `TankerEffect` (공격 불가, 반격 피해 없음)
- [x] 탱커가 필드에 있으면 탱커만 공격 가능 — 플레이어/적 AI 양쪽 적용
- [x] 탱커 클릭 시 빨간 테두리 + 흔들림 연출

### 암살자 카드
- [x] `AssassinCardData` / `AssassinEffect` (탱커 무시, 반격 피해 없음)
- [x] 순간이동 연출 — scale 0→이동→scale 복구, slash 회전 공격
- [x] `CardAttackAnimator.PlayAssassinAttack` — Inspector에서 타이밍 조정 가능

### 전투 연출
- [x] 근접 공격 lunge + 히트 리액션 (DOTween)
- [x] 원거리 공격 투사체 연출 (`Projectile.cs`)
- [x] 무쌍 스플래시 히트 VFX (스플래시 데미지 공격 전 HP 기준으로 수정)
- [x] 공격/피격/사망 순서 동기화
- [x] 힐 연출 — VFX 타이밍에 HP 적용, 힐 텍스트 `+N` 표시
- [x] 덱에서 카드 배치 연출 (앞뒤 플립 포함)
- [x] 피격/사망 VFX (`receivedHitFXPrefab`, `deathFXPrefab`)
- [x] 모든 연출 타이밍 Inspector에서 조정 가능 (`CardAttackAnimator` 헤더별 분리)

### 카드 선택 UI
- [x] 선택 하이라이트 — 흰색 테두리+글로우 쉐이더
- [x] 빈 슬롯 시각화

### 카드 시각
- [x] `frameOutline` 무한 회전 (DOTween Loop)
- [x] `cardDescText` — `feature` 필드 표시
- [x] `FloatingDesc` — 롱프레스(0.4s) 시 feature + description 표시 (모바일 대응)
- [x] `CardUIView.cs` — 상점 카드 표시용 (CardPrefab과 동일 비주얼, 전투 스크립트 없음)

### WIN/LOSE 연출
- [x] Win/Lose 글자 애니메이션, 결과 패널 스탯 표시
- [x] `retryButton` 씬 재시작 / `lobbyButton` → LobbyScene 이동

### UI 인트로 / 턴 연출
- [x] 게임 시작 인트로 애니메이션
- [x] 턴 텍스트 변경 연출
- [x] `ButtonScaleEffect.cs` — 버튼 누를 때 scale 팝 애니메이션

### 로비 시스템
- [x] `SceneNames.cs` — 씬 이름 상수 (`LobbyScene`, `GameScene`)
- [x] `LobbyManager.cs` — 게임시작/카드편집/상점 버튼 + 씬 전환
- [x] `LobbyPanel.cs` — 공용 패널 Show/Hide 슬라이드 애니메이션

### 카드 관리 시스템
- [x] `CardLibrary.cs` (SO) — 전체 카드 마스터 목록, 등급별 필터
- [x] `OwnedCardEntry.cs` — 소유 카드 단위 (CardDataBase + upgradeLevel)
- [x] `CardManager.cs` (Singleton, DontDestroyOnLoad) — 컬렉션/덱/뽑기 관리
- [x] 강화 1회 제한 (`MaxUpgradeLevel = 1`)

### 상점 시스템
- [x] `ShopManager.cs` — 1뽑/10뽑, chest 스프라이트 교체, 결과 패널
- [x] 뽑기 확률 — Normal 60% / Special 30% / Epic 10% (Inspector 조정 가능)
- [x] 뽑기 결과 카드 팝인 스태거 애니메이션
- [x] 확인 시 컬렉션 추가 / 닫기 시 미획득

---

## Todo

### 인스펙터 연결 (씬/에셋)
- [ ] CardPrefab에 `rarityBorderImage` 연결
- [ ] CardUIPrefab에 `CardUIView` 컴포넌트 연결 및 필드 세팅
- [ ] CardLibrary SO 생성 및 카드 등록
- [ ] CardManager 씬 배치 및 세팅
- [ ] LobbyScene UI 구성 (버튼, 패널 배치 및 연결)
- [ ] ShopPanel chest 스프라이트 연결

### 카드 에딧 화면
- [ ] 카드 편집 Panel UI 구성 (소유 카드 목록, 덱 편성)
- [ ] 강화 UI (강화 버튼, 레벨 표시)

### 에셋
- [ ] `CardVisualConfig` 각 카드에 `illustration` / `typeIcon` 스프라이트 할당
- [ ] 각 카드 SO에 `feature` / `description` 텍스트 입력
- [ ] `CardRarity` 설정 (Normal/Special/Epic 배분)

### 가산점
- [ ] 사운드 이펙트 (공격, 피격, 힐, 사망)

### 제출 준비
- [ ] Android 64비트 빌드 (APK)
- [ ] 플레이 영상 녹화
- [ ] README 작성 (Unity 버전, 구현 기능, 코드 구조, AI 활용 내역)
