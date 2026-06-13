# 턴제 카드 배틀 게임 기획서

> 테마: 판타지 (기사, 마법사, 드래곤) | Unity 2D 모바일 (세로형)

---

## 1. 게임 개요

| 항목 | 내용 |
|------|------|
| 장르 | 턴제 카드 배틀 |
| 플랫폼 | Android 64비트 |
| 화면 방향 | 세로 (Portrait) |
| 테마 | 판타지 — 기사, 마법사, 드래곤 등 |
| 목표 | 상대방의 모든 카드를 제거하면 승리 |

---

## 2. 씬 구성 (Scene Architecture)

```
TitleScene
  └─ BattleScene  ← 핵심 씬
       └─ ResultScene (승/패 팝업 또는 별도 씬)
  └─ DeckScene    ← 가산점: 덱 편성
```

### TitleScene
- 게임 로고 + 시작 버튼
- BGM 재생 시작

### BattleScene
- 전투 메인 화면 전체 구현
- 상단: 적 필드(슬롯 3개) + 대기 카드 수 표시
- 하단: 플레이어 필드(슬롯 3개) + 대기 카드 수 표시
- 중앙: 현재 턴 표시 UI + 액션 버튼 영역

### ResultScene
- 승리 / 패배 결과 연출
- 다시하기 / 타이틀로 버튼

### DeckScene (가산점)
- 보유 카드 목록 표시
- 덱에 카드 추가/제거 (최대 6장)

---

## 3. 아키텍처 설계

### 레이어 구조

```
[ Scene Layer ]
    ↓ 호출
[ Manager Layer ]  — 모두 싱글톤 (MonoBehaviour)
    ↓ 데이터 흐름
[ Card System ]
    ↓ 위임
[ AI / FX Layer ]
```

### 매니저 목록

| 클래스 | 역할 |
|--------|------|
| `GameManager` | 게임 전체 상태 (GameState enum) 관리, 씬 전환 |
| `TurnManager` | 플레이어↔AI 턴 순서 제어, 힐러 턴 시작 효과 트리거 |
| `BattleManager` | 공격 처리, HP 계산, 카드 제거, 신규 카드 자동 배치 |
| `UIManager` | 턴 표시, 버튼 활성화, 결과 화면 표시 |

### GameState enum

```csharp
public enum GameState
{
    Init,
    PlayerTurn,
    PlayerSelectCard,
    PlayerSelectTarget,
    ApplyEffect,
    EnemyTurn,
    CheckResult,
    Win,
    Lose
}
```

---

## 4. 카드 시스템 설계

### 설계 원칙

| 원칙 | 적용 |
|------|------|
| SO 상속 분리 | 타입별 고유 필드만 보유, 불필요한 필드 오염 없음 |
| VisualConfig 별도 SO | 비주얼 변경이 로직에 영향 없음 (디자이너/개발자 작업 분리) |
| Factory Method | `CreateEffect()` — BattleManager는 카드 타입을 몰라도 됨 |
| Strategy 패턴 | `CardEffect` 추상 클래스 — 새 카드 추가 시 기존 코드 수정 불필요 |
| Data / Instance 분리 | SO는 메모리에 1개만 존재, CardInstance는 런타임 상태만 보유 |

---

### 4-1. CardDataBase (abstract ScriptableObject)

모든 카드 SO의 공통 베이스. 공통 필드만 정의하고 타입별 고유 필드는 하위 클래스에 위임.

```csharp
public abstract class CardDataBase : ScriptableObject
{
    public string cardName;
    public int maxHP;
    public string description;
    public CardVisualConfig visual;   // 비주얼 SO 참조 (별도 분리)

    // 팩토리 메서드 — 각 하위 SO가 자신의 Effect를 직접 생성
    public abstract CardEffect CreateEffect();
}
```

---

### 4-2. 타입별 CardData SO (CardDataBase 상속)

각 카드 타입은 자신만의 고유 필드와 `CreateEffect()`만 구현.
에디터에서 해당 타입의 SO를 열면 관련 필드만 노출됨.

#### NormalCardData
```csharp
[CreateAssetMenu(menuName = "Card/Normal")]
public class NormalCardData : CardDataBase
{
    // 고유 필드 없음 — 공통 필드만으로 동작
    public override CardEffect CreateEffect() => new NormalEffect();
}
```

#### RangedCardData
```csharp
[CreateAssetMenu(menuName = "Card/Ranged")]
public class RangedCardData : CardDataBase
{
    // 고유 필드 없음
    public override CardEffect CreateEffect() => new RangedEffect();
}
```

#### MusoCardData
```csharp
[CreateAssetMenu(menuName = "Card/Muso")]
public class MusoCardData : CardDataBase
{
    [Range(0f, 1f)] public float splashRatio = 0.5f;  // 인접 피해 비율 (에디터 조정 가능)

    public override CardEffect CreateEffect() => new MusoEffect(splashRatio);
}
```

#### HealerCardData
```csharp
[CreateAssetMenu(menuName = "Card/Healer")]
public class HealerCardData : CardDataBase
{
    public int healAmount = 1;  // 힐량 (에디터 조정 가능)

    public override CardEffect CreateEffect() => new HealerEffect(healAmount);
}
```

---

### 4-3. CardVisualConfig (별도 ScriptableObject)

비주얼 관련 데이터를 로직 SO와 완전 분리.
일러스트 교체, 프레임 색 변경, FX 교체가 로직에 영향을 주지 않음.
나중에 스킨 시스템으로도 확장 가능.

```csharp
[CreateAssetMenu(menuName = "Card/VisualConfig")]
public class CardVisualConfig : ScriptableObject
{
    public Sprite illustration;                      // 카드 일러스트
    public Color frameColor;                         // 타입별 프레임 색상
    public RuntimeAnimatorController animController; // 카드별 애니메이터
    public GameObject attackFXPrefab;                // 공격 파티클 프리팹
    public GameObject hitFXPrefab;                   // 피격 파티클 프리팹
    public GameObject deathFXPrefab;                 // 사망 파티클 프리팹
}
```

| 카드 타입 | frameColor | 대표 캐릭터 | 일러스트 스타일 |
|-----------|-----------|------------|----------------|
| 일반 (Normal) | 회색 | 철갑 기사 | 정면 전신, 중후한 분위기 |
| 원거리 (Ranged) | 파랑 | 엘프 궁수 / 마법사 | 측면 자세, 원거리 포즈 |
| 무쌍 (Muso) | 빨강 | 드래곤 / 전설 전사 | 다이나믹 포즈, 강렬한 색감 |
| 힐러 (Healer) | 초록 | 성직자 / 요정 | 온화한 분위기, 빛 이펙트 |

---

### 4-4. CardEffect (abstract) — Strategy 패턴

`BattleManager`는 `Execute()` 하나만 호출. 카드 타입을 알 필요 없음.
새 카드 타입 추가 시 `BattleManager` 수정 불필요 (OCP 준수).

```csharp
public abstract class CardEffect
{
    // attacker: 공격하는 카드, targets: 선택된 대상 카드 목록
    public abstract void Execute(CardInstance attacker, List<CardInstance> targets);
}
```

| 타입 | 클래스 | 동작 |
|------|--------|------|
| 일반 (Normal) | `NormalEffect` | 공격: 내 currentHP만큼 피해. 반격: 상대 currentHP만큼 피해 |
| 원거리 (Ranged) | `RangedEffect` | 공격: 내 currentHP만큼 피해. 반격 없음 |
| 무쌍 (Muso) | `MusoEffect` | 주 대상 100% 피해 + 인접 무작위 1장 splashRatio% 피해 |
| 힐러 (Healer) | `HealerEffect` | 턴 시작 시 아군 전원(자신 제외) +healAmount HP. 공격은 Normal과 동일 |

#### MusoEffect 인접 카드 로직

```
[ 슬롯 0 ] [ 슬롯 1 ] [ 슬롯 2 ]

대상이 슬롯 1이면 → 인접: 슬롯 0, 슬롯 2 중 랜덤 1장
대상이 슬롯 0이면 → 인접: 슬롯 1만
대상이 슬롯 2이면 → 인접: 슬롯 1만
인접 카드가 없으면 → 추가 피해 없음
```

#### BattleManager 호출부 — 타입 분기 없음

```csharp
// ❌ 이전 설계 — 새 카드 추가마다 BattleManager 수정 필요
switch (attacker.data.cardType) { case CardType.Normal: ... case CardType.Muso: ... }

// ✅ 개선된 설계 — 다형성으로 완전 위임, BattleManager 수정 불필요
public void ApplyAttack(CardInstance attacker, List<CardInstance> targets)
{
    attacker.effect.Execute(attacker, targets);
}
```

---

### 4-5. CardInstance (런타임 상태)

SO는 메모리에 타입 수만큼만 존재. CardInstance는 런타임 상태만 보유하고 SO를 참조.
Effect는 `CardDataBase.CreateEffect()`로 생성 시점에 1회만 생성.

```csharp
public class CardInstance
{
    public CardDataBase data;     // SO 참조 (공유, 메모리 1개)
    public CardEffect effect;     // 생성 시점에 data.CreateEffect()로 주입
    public int currentHP;
    public bool isDeployed;       // 전장 배치 여부
    public int slotIndex;         // 현재 슬롯 (0~2), 미배치 시 -1
    public Owner owner;           // Player / Enemy

    public bool IsAlive => currentHP > 0;

    public CardInstance(CardDataBase data)
    {
        this.data   = data;
        this.effect = data.CreateEffect();  // 팩토리 메서드로 Effect 생성
        this.currentHP = data.maxHP;
    }
}
```

---

### 4-6. CardView (MonoBehaviour) — MVC View

`CardInstance`를 바인딩하면 `CardVisualConfig`를 읽어 자동으로 비주얼 적용.
로직 변경 없이 VisualConfig 교체만으로 완전히 다른 디자인 적용 가능.

```csharp
public class CardView : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private Animator animator;

    private CardInstance _instance;
    private CardVisualConfig _visual;

    public void Bind(CardInstance instance)
    {
        _instance = instance;
        _visual   = instance.data.visual;

        // VisualConfig에서 비주얼 자동 적용
        illustrationImage.sprite = _visual.illustration;
        frameImage.color         = _visual.frameColor;
        animator.runtimeAnimatorController = _visual.animController;
        cardNameText.text        = instance.data.cardName;
        hpBar.Init(instance.data.maxHP, instance.currentHP);
    }

    public void PlayAttackAnim() => animator.SetTrigger("Attack");
    public void PlayHitAnim()    => animator.SetTrigger("Hit");
    public void PlayDeathAnim()  => animator.SetTrigger("Death");

    // FX는 VisualConfig의 프리팹을 FXManager에 위임
    public void PlayAttackFX(Vector3 targetPos)
        => FXManager.Instance.Play(_visual.attackFXPrefab, transform.position, targetPos);
    public void PlayHitFX()
        => FXManager.Instance.Play(_visual.hitFXPrefab, transform.position);
}
```

---

### 4-7. BattleSlot

필드 슬롯 3개를 관리. 카드 배치 / 제거 / 자동 보충 처리.

```csharp
public class BattleSlot : MonoBehaviour
{
    public int slotIndex;           // 0, 1, 2
    public CardInstance card;       // null이면 빈 슬롯
    public CardView cardView;       // 현재 슬롯의 CardView
    public bool IsEmpty => card == null;
}
```

---

### 4-8. 새 카드 타입 추가 시 작업 범위

기존 코드를 수정하지 않고 아래 3가지만 추가하면 완결됨.

```
1. XxxCardData.cs 작성    (CardDataBase 상속, 고유 필드 + CreateEffect())
2. XxxEffect.cs 작성      (CardEffect 상속, Execute() 구현)
3. 에디터에서 SO 에셋 생성 + CardVisualConfig 에셋 생성
```

---

## 5. 카드 배치 규칙

```
게임 시작
  ├─ 플레이어: 총 6장 보유
  │    ├─ 전장 슬롯 0~2: 3장 공개 배치
  │    └─ 대기 카드: 3장 (뒤집힌 상태)
  └─ 적: 동일하게 6장

카드 제거 발생
  └─ 빈 슬롯 발생 && 대기 카드 존재
       └─ 자동으로 다음 대기 카드를 해당 슬롯에 배치
```

---

## 6. 턴 진행 흐름

```
게임 시작
  └─ TurnManager.StartTurn()
       ├─ 힐러 HP 회복 처리 (전장 HealerCardInstance 순회)
       └─ 현재 턴 확인
            ├─ [플레이어 턴]
            │    1. 아군 카드 선택 (탭)
            │    2. 행동 선택 (공격 버튼)
            │    3. 적 카드 선택 (탭)
            │    4. BattleManager.ApplyAttack(attacker, targets) 실행
            │    5. 승패 판정 (CheckResult)
            │    6. 이상 없으면 → 적 턴으로
            └─ [적 턴]
                 1. EnemyAI.DecideAction() → (attacker, targets) 반환
                 2. 자동으로 공격 실행 (딜레이 연출 포함)
                 3. BattleManager.ApplyAttack(attacker, targets) 실행
                 4. 승패 판정 (CheckResult)
                 5. 이상 없으면 → 플레이어 턴으로
```

---

## 7. 적 AI 설계 (공격 우선순위 기반)

```csharp
public class EnemyAI
{
    // 공격 카드 선택: 아군 전장 카드 중 currentHP 가장 높은 카드 (최대 피해)
    // 대상 선택 우선순위:
    //   1순위: currentHP가 가장 낮은 적 카드 (처치 가능성 최대화)
    //   2순위: 동점이면 슬롯 인덱스 낮은 쪽

    public (CardInstance attacker, List<CardInstance> targets) DecideAction(
        List<CardInstance> enemyField,
        List<CardInstance> playerField
    );
}
```

### 우선순위 계산 예시

```
플레이어 필드: [기사 HP 3] [마법사 HP 8] [드래곤 HP 12]
AI 공격 카드: 원거리 HP 7

→ 가장 HP 낮은 기사(HP 3) 선택
→ 7 피해 적용 → 기사 제거 (HP 0)
→ 신규 대기 카드 자동 배치
```

---

## 8. UI 구성

### 전투 화면 레이아웃 (세로형)

```
┌─────────────────────┐
│  [적 대기 카드 수]    │
│  ┌────┐┌────┐┌────┐  │  ← 적 필드 슬롯 3개
│  │    ││    ││    │  │    (HP 바, 카드명, 프레임 색상)
│  └────┘└────┘└────┘  │
│                      │
│   ── 현재 턴 표시 ──  │  ← TurnBanner
│                      │
│  ┌────┐┌────┐┌────┐  │  ← 플레이어 필드 슬롯 3개
│  │    ││    ││    │  │    (선택 시 하이라이트)
│  └────┘└────┘└────┘  │
│  [플레이어 대기 카드]  │
│      [공격]           │  ← 행동 버튼 (카드 선택 후 활성화)
└─────────────────────┘
```

### UI 컴포넌트 목록

| 컴포넌트 | 설명 |
|----------|------|
| `TurnBanner` | "플레이어 턴" / "적 턴" 텍스트 + 페이드 인 연출 |
| `CardSlotUI` | 슬롯 카드 표시 (HP 바, 카드명, 프레임 색상 자동 적용) |
| `HPBar` | 현재/최대 HP 비율로 색상 변화 (녹→황→적) |
| `ActionPanel` | 공격 버튼, 카드 선택 전엔 비활성 |
| `WaitingCardCount` | 대기 카드 수 표시 (숫자 + 뒤집힌 카드 아이콘) |
| `ResultPanel` | 승패 결과 팝업 (Win/Lose 텍스트 + 버튼) |

---

## 9. FX 및 애니메이션 계획 (가산점)

### 공격/피격 FX — VisualConfig의 프리팹을 FXManager가 처리

| 상황 | 연출 |
|------|------|
| 일반 공격 | 공격 카드 앞으로 이동 후 복귀 + 피격 카드 흔들림 |
| 원거리 공격 | 투사체 파티클 (화살/마법구) 날아가는 연출 |
| 무쌍 공격 | 주 대상 크게 흔들림 + 인접 카드 작은 흔들림 |
| 힐 발동 | 아군 카드 주변 녹색 파티클 + HP 숫자 팝업 |
| 카드 사망 | 페이드 아웃 + 파티클 소산 |

### 애니메이션

- 카드 배치 시: 아래에서 슬라이드 인
- 카드 선택 시: 살짝 위로 이동 + frameColor 하이라이트
- 턴 전환 시: TurnBanner 슬라이드 인
- 새 카드 자동 배치: 대기 카드 더미에서 슬롯으로 이동하는 트윈

### 사용 도구

- DOTween (트윈 애니메이션)
- Unity Particle System (FX 프리팹, VisualConfig에서 타입별 지정)
- Unity Animator + RuntimeAnimatorController (CardView에서 VisualConfig로 교체)

---

## 10. 폴더 구조

```
Assets/
├─ Scripts/
│   ├─ Core/
│   │   ├─ GameManager.cs
│   │   ├─ TurnManager.cs
│   │   ├─ BattleManager.cs
│   │   └─ UIManager.cs
│   ├─ Card/
│   │   ├─ Data/
│   │   │   ├─ CardDataBase.cs         (abstract ScriptableObject)
│   │   │   ├─ NormalCardData.cs
│   │   │   ├─ RangedCardData.cs
│   │   │   ├─ MusoCardData.cs
│   │   │   └─ HealerCardData.cs
│   │   ├─ Visual/
│   │   │   └─ CardVisualConfig.cs     (ScriptableObject)
│   │   ├─ Effects/
│   │   │   ├─ CardEffect.cs           (abstract)
│   │   │   ├─ NormalEffect.cs
│   │   │   ├─ RangedEffect.cs
│   │   │   ├─ MusoEffect.cs
│   │   │   └─ HealerEffect.cs
│   │   ├─ CardInstance.cs
│   │   ├─ CardView.cs
│   │   └─ BattleSlot.cs
│   ├─ AI/
│   │   └─ EnemyAI.cs
│   ├─ UI/
│   │   ├─ TurnBanner.cs
│   │   ├─ ActionPanel.cs
│   │   ├─ HPBar.cs
│   │   └─ ResultPanel.cs
│   └─ FX/
│       └─ FXManager.cs
├─ ScriptableObjects/
│   ├─ Cards/
│   │   ├─ Normal_Knight.asset         (NormalCardData)
│   │   ├─ Ranged_Archer.asset         (RangedCardData)
│   │   ├─ Muso_Dragon.asset           (MusoCardData)
│   │   └─ Healer_Cleric.asset         (HealerCardData)
│   └─ Visuals/
│       ├─ Visual_Knight.asset         (CardVisualConfig)
│       ├─ Visual_Archer.asset
│       ├─ Visual_Dragon.asset
│       └─ Visual_Cleric.asset
├─ Prefabs/
│   ├─ CardPrefab
│   ├─ BattleSlotPrefab
│   └─ FX/
│       ├─ FX_NormalAttack
│       ├─ FX_RangedProjectile
│       ├─ FX_MusoSlash
│       ├─ FX_HealAura
│       └─ FX_Death
└─ Scenes/
    ├─ TitleScene
    ├─ BattleScene
    ├─ ResultScene
    └─ DeckScene (가산점)
```

---

## 11. 구현 우선순위

| 단계 | 내용 | 비고 |
|------|------|------|
| 1단계 | `CardDataBase` 상속 구조 + `CardVisualConfig` SO 설계 | 카드 시스템 기반 |
| 2단계 | `CardInstance` + 4종 `CardEffect` 구현 | 핵심 로직 |
| 3단계 | `BattleSlot` + 카드 배치/제거/자동 보충 | 필수 |
| 4단계 | `TurnManager` + 플레이어 입력 흐름 | 필수 |
| 5단계 | `EnemyAI` + 자동 턴 처리 | 필수 |
| 6단계 | `UIManager` + 전투 화면 UI | 필수 |
| 7단계 | `CardView.Bind()` + VisualConfig 비주얼 자동 적용 | 가산점 |
| 8단계 | FX 프리팹 + DOTween 애니메이션 | 가산점 |
| 9단계 | DeckScene + 덱 편성 기능 | 가산점 |
