# Project Aloss - Development Log

---

## 2026-01-06

### 🔧 Environment Setup
- Unity 6.3 LTS 프로젝트 생성
- Git / GitHub 연동 완료
- Cursor AI 에디터 연동

### 🗂 Project Structure
- Assets/Aloss 기준 폴더 구조 확정
  - Core
  - Combat
  - Skill
  - Character
  - UI
  - Data
  - Art
  - Audio

### 🧱 Core Systems
- GameManager
  - Singleton 패턴 적용
  - DontDestroyOnLoad 처리
- SceneLoader
  - BootScene 시작 시 MenuScene 자동 이동

### 🎬 Scene Flow
- BootScene → MenuScene → BattleScene 구조 완성
- Unity 6 Build Profiles 기준 Scene List 설정

### 🖥 Menu Scene
- Canvas / EventSystem / Camera 구성
- "GAME START" 텍스트 (TextMeshPro) 중앙 정렬
- Start 버튼 추가
- MenuController 스크립트 구현
  - Start 버튼 클릭 시 BattleScene 로드

### ✅ Current State
- 게임 실행 시:
  1. BootScene 로드
  2. 자동으로 MenuScene 전환
  3. START 버튼 표시
  4. BattleScene 진입 가능 상태

### ⏭ Next Steps
- BattleScene 전투 UI 뼈대 구성
- 턴 기반 전투 구조 설계
- Skill 데이터(ScriptableObject) 설계


## 2026-01-07 Battle Prototype v0.1

### Completed
- 1 turn = 1 action battle flow
- Distance affects hit chance, not skill availability
- Basic skills implemented:
  - Move +1 / -1
  - Backstep +2
  - Attack (flat damage)
  - Guard (next damage -4)
- Tooltip shows HIT% and DMG only on attack skill hover

### Design Decisions
- Distance never blocks actions, only modifies accuracy
- No stamina for prototype phase
- Tooltip appears only when relevant (attack-type skills)

### Next
- Tooltip positioning refinement (button-relative or cursor-based)
- Visual feedback for hit/miss
- Skill data abstraction (ScriptableObject)
