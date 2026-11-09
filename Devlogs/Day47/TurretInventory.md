---

# 47일차 포탑별 시험 구현 현황

---

## 기준

실제 보상 풀의 48개 에셋을 기준으로 작성했다. 아래 ★3·★5는 시험 구현이며, 최종 효과·이름·수치는 모두 미확정이다. 47일차의 두 ★5 예시는 각 전용 통제 시험으로 확인하고, 다른 효과는 공격 유형별 공통 시험 효과다.

| 속성 | 공격 | 포탑 ID | 표시 이름 | ★3 시험 효과 | ★5 시험 효과 |
| --- | --- | --- | --- | --- | --- |
| 물리 | 근접 | debug_blade | 강철 근접 포탑 | 최근접 2명 | 2명 + 4회마다 범위 강타 |
| 화염 | 직선 | debug_blaster | 화염탄 포탑 | 주변 1명 전달 | 화상 적 처치 폭발 |
| 화염 | 범위 | debug_pulse | 화염 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 화염 | 근접 | debug_cutter | 화염 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 물리 | 직선 | debug_needle | 강철 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 물리 | 범위 | debug_nova | 강철 충격파 포탑 | 범위 1.25배 | 범위 1.5배 |
| 물리 | 근접 | prototype_physical_dagger | 시험용 물리 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 물리 | 직선 | prototype_physical_javelin | 시험용 물리 속사 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 물리 | 범위 | prototype_physical_hammer | 시험용 물리 중파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 화염 | 근접 | prototype_fire_brand | 시험용 화염 중근접 포탑 | 최근접 2명 | 최근접 3명 |
| 화염 | 직선 | prototype_fire_lance | 시험용 화염 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 화염 | 범위 | prototype_fire_burst | 시험용 화염 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 냉기 | 근접 | prototype_cold_blade | 시험용 냉기 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 냉기 | 직선 | prototype_cold_spear | 시험용 냉기 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 냉기 | 범위 | prototype_cold_aura | 시험용 냉기 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 냉기 | 근접 | prototype_cold_dagger | 시험용 냉기 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 냉기 | 직선 | prototype_cold_bolt | 시험용 냉기 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 냉기 | 범위 | prototype_cold_wave | 시험용 냉기 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 전기 | 근접 | prototype_electric_blade | 시험용 전기 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 전기 | 직선 | prototype_electric_spear | 시험용 전기 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 전기 | 범위 | prototype_electric_aura | 시험용 전기 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 전기 | 근접 | prototype_electric_dagger | 시험용 전기 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 전기 | 직선 | prototype_electric_bolt | 시험용 전기 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 전기 | 범위 | prototype_electric_wave | 시험용 전기 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 독 | 근접 | prototype_poison_blade | 시험용 독 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 독 | 직선 | prototype_poison_spear | 시험용 독 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 독 | 범위 | prototype_poison_aura | 시험용 독 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 독 | 근접 | prototype_poison_dagger | 시험용 독 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 독 | 직선 | prototype_poison_bolt | 시험용 독 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 독 | 범위 | prototype_poison_wave | 시험용 독 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 폭발 | 근접 | prototype_explosion_blade | 시험용 폭발 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 폭발 | 직선 | prototype_explosion_spear | 시험용 폭발 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 폭발 | 범위 | prototype_explosion_aura | 시험용 폭발 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 폭발 | 근접 | prototype_explosion_dagger | 시험용 폭발 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 폭발 | 직선 | prototype_explosion_bolt | 시험용 폭발 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 폭발 | 범위 | prototype_explosion_wave | 시험용 폭발 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 신성 | 근접 | prototype_holy_blade | 시험용 신성 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 신성 | 직선 | prototype_holy_spear | 시험용 신성 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 신성 | 범위 | prototype_holy_aura | 시험용 신성 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 신성 | 근접 | prototype_holy_dagger | 시험용 신성 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 신성 | 직선 | prototype_holy_bolt | 시험용 신성 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 신성 | 범위 | prototype_holy_wave | 시험용 신성 파동 포탑 | 범위 1.25배 | 범위 1.5배 |
| 암흑 | 근접 | prototype_dark_blade | 시험용 암흑 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 암흑 | 직선 | prototype_dark_spear | 시험용 암흑 관통 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 암흑 | 범위 | prototype_dark_aura | 시험용 암흑 범위 포탑 | 범위 1.25배 | 범위 1.5배 |
| 암흑 | 근접 | prototype_dark_dagger | 시험용 암흑 속사 근접 포탑 | 최근접 2명 | 최근접 3명 |
| 암흑 | 직선 | prototype_dark_bolt | 시험용 암흑 직사 포탑 | 주변 1명 전달 | 전달 피해 1.5배 |
| 암흑 | 범위 | prototype_dark_wave | 시험용 암흑 파동 포탑 | 범위 1.25배 | 범위 1.5배 |


---

## 검증 범위

전체 48종의 실제 공격 경로는 별도 Unity 배치의 통제 표적으로 확인한다. 각 행의 최종 기획 확정과 수동 플레이·개별 전용 이미지 여부를 뜻하지 않는다.
