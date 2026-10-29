# 아이콘 출처 · 라이선스

`png/` 의 아이콘은 Google **Material Icons (Outlined)** 계열이다 — Apache License 2.0
(<https://www.apache.org/licenses/LICENSE-2.0>), 저작권 Google LLC.

- 2026-10-29 추가분 6종(`Factory`·`DirectionsCar`·`BatteryFull`·`ViewModule`·`FolderOpen`·`Tune`)은
  npm `@material-design-icons/svg@0.14.15` 의 `outlined/*.svg` 를 72×72(24 그리드의 3배), 검정 단색 +
  투명 배경 PNG 로 래스터화했다.
- 같은 방식으로 만든 `list_alt`·`fact_check` 가 기존 `ListAlt.png`·`FactCheck.png` 와 픽셀 단위로 일치한다
  (평균 알파 차이 0.06·0.34) — 기존 아이콘도 같은 출처·규격이다.
- 🔴 Material **Symbols**(`@material-symbols/svg-*`)는 선이 가늘어 기존 아이콘과 따로 논다. 추가할 때는
  클래식 Material Icons Outlined 를 쓴다.

추가 절차: PNG 를 `png/` 에 넣고 `python IconGenerator.py` 로 `IconKeys.cs` 를 다시 만든다.
🔴 `EIconKeys` 는 알파벳순이라 중간에 끼우면 뒤 값들의 정수가 밀린다 — C# 에서 `EIconKeys.X` 를 쓰는
소비 프로젝트는 새 DLL 로 다시 빌드해야 한다(XAML 의 이름 지정은 영향 없음).
