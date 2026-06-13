# GoHomeCat — Claude Guidelines

## Code Quality

- 답변 전 스스로 검증 후 제시
- 주석 작성 금지 (자명한 코드로 대체)
- 불필요한 로그(`Debug.Log` 등) 작성 금지
- 불필요한 가이드/설명 제공 금지

## Architecture

- SOLID 원칙 준수
- 기능별 스크립트 분리 (단일 책임 원칙 우선)
- Unity Physics 사용 금지 — 모든 이동 판정은 GridMap 데이터로 처리

## Optimization

- 모든 코드 작성 시 최적화 우선 고려
- 메모리: 오브젝트 풀링, 불필요한 GC 할당 최소화
- 렌더링: DrawCall 최소화 (Sprite Atlas, GPU Instancing 활용)
- 이동 판정 루프: StaticLayer 배열 O(1) 조회 유지

## Git & Security

- API 키, 개인정보, 시크릿 값 코드에 포함 금지
- 민감 정보는 `.gitignore` 처리
