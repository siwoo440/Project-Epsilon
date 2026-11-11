using System; // C# 기본 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    public enum WeaponGradeEffectTrigger // 포탑·등급 자료 구조
    {
        Acquired, // 대상 항목 전달
        Merged, // 대상 항목 전달
        Attack // 공격 처리
    }

    public readonly struct WeaponGradeEffectContext // 포탑·등급 자료 구조
    {
        public WeaponData Weapon { get; } // 포탑·데이터 구성 선언
        public int Grade { get; } // 등급·조회 구성 선언
        public Vector3 Origin { get; } // 벡터·발사 위치 구성 선언
        public float Damage { get; } // 피해·조회 구성 선언
        public WeaponGradeEffectTrigger Trigger { get; } // 포탑·등급 구성 선언

        public WeaponGradeEffectContext( // 포탑·등급 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade, // 등급 항목 전달
            Vector3 origin, // 벡터·발사 위치 항목 전달
            float damage, // 피해 항목 전달
            WeaponGradeEffectTrigger trigger // 포탑·등급 처리
        ) // 앞선 항목 종료
        {
            Weapon = weapon; // 포탑 값 설정
            Grade = grade; // 등급 값 설정
            Origin = origin; // 발사 위치 값 설정
            Damage = damage; // 피해 값 설정
            Trigger = trigger; // 접촉 값 설정
        }
    }

    public sealed class WeaponGradeEffectHooks : MonoBehaviour // 포탑·등급 자료 구조
    {
        public event Action<WeaponGradeEffectContext> EffectTriggered; // 모든 등급 효과 알림

        public event Action<WeaponGradeEffectContext> // 이벤트·동작 구성 선언
            GradeThreeTriggered; // 등급 처리

        public event Action<WeaponGradeEffectContext> // 이벤트·동작 구성 선언
            GradeFiveTriggered; // 등급 처리

        public void Notify( // 알림 호출 시작
            WeaponGradeEffectContext context // 포탑·등급 처리
        ) // 앞선 항목 종료
        {
            if (context.Weapon == null) // 포탑 조건 확인
            {
                return; // 현재 기능 종료
            }

            EffectTriggered?.Invoke(context); // 공통 효과 알림 전달

            if (context.Grade >= 3) // 등급 조건 확인
            {
                GradeThreeTriggered?.Invoke( // 등급 호출 시작
                    context // 대상 처리
                ); // 앞선 항목 종료
            }

            if (context.Grade >= 5) // 등급 조건 확인
            {
                GradeFiveTriggered?.Invoke( // 등급 호출 시작
                    context // 대상 처리
                ); // 앞선 항목 종료
            }
        }
    }
}

