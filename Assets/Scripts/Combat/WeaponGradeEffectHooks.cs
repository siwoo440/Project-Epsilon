using System;
using ProjectEpsilon.Data;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    public enum WeaponGradeEffectTrigger
    {
        Acquired,
        Merged,
        Attack
    }

    public readonly struct WeaponGradeEffectContext
    {
        public WeaponData Weapon { get; }
        public int Grade { get; }
        public Vector3 Origin { get; }
        public float Damage { get; }
        public WeaponGradeEffectTrigger Trigger { get; }

        public WeaponGradeEffectContext(
            WeaponData weapon,
            int grade,
            Vector3 origin,
            float damage,
            WeaponGradeEffectTrigger trigger
        )
        {
            Weapon = weapon;
            Grade = grade;
            Origin = origin;
            Damage = damage;
            Trigger = trigger;
        }
    }

    public sealed class WeaponGradeEffectHooks : MonoBehaviour
    {
        public event Action<WeaponGradeEffectContext> EffectTriggered; // 모든 등급 효과 알림

        public event Action<WeaponGradeEffectContext>
            GradeThreeTriggered;

        public event Action<WeaponGradeEffectContext>
            GradeFiveTriggered;

        public void Notify(
            WeaponGradeEffectContext context
        )
        {
            if (context.Weapon == null)
            {
                return;
            }

            EffectTriggered?.Invoke(context); // 공통 효과 알림 전달

            if (context.Grade >= 3)
            {
                GradeThreeTriggered?.Invoke(
                    context
                );
            }

            if (context.Grade >= 5)
            {
                GradeFiveTriggered?.Invoke(
                    context
                );
            }
        }
    }
}

