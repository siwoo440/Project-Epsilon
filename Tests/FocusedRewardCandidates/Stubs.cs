using System; // 실제 난수와 특성 대체
using System.Collections.Generic; // 실제 슬롯 목록 대체

namespace UnityEngine // Unity 추첨 기능 대체
{
    public class ScriptableObject // Unity 데이터 기본형 대체
    {
    }
    public sealed class SerializeField : Attribute // 직렬화 표시 대체
    {
    }
    public sealed class CreateAssetMenuAttribute : Attribute // 데이터 생성 메뉴 대체
    {
        public string fileName; // 실제 데이터 이름
        public string menuName; // 실제 생성 메뉴
    }
    public static class Mathf // 실제 수학 기능 대체
    {
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value)); // 정수 범위 제한
        public static float Clamp01(float value) => Math.Min(1f, Math.Max(0f, value)); // 확률 범위 제한
    }
    public static class Random // 재현 가능한 Unity 난수 대체
    {
        private static System.Random generator = new System.Random(1); // 실제 시험 난수
        public static float value => (float)generator.NextDouble(); // 실제 확률 표본
        public static int Range(int min, int max) => generator.Next(min, max); // 실제 정수 표본
        public static void InitState(int seed) // 시험 시드 설정
        {
            generator = new System.Random(seed); // 독립 난수 재설정
        }
    }
}

namespace ProjectEpsilon.Data // 포탑과 캐릭터 데이터 대체
{
    public enum WeaponAttribute // 속성 대체
    {
        Physical, // 물리 속성
        Holy, // 신성 속성
        Dark // 암흑 속성
    }
    public enum CharacterPassive // 캐릭터 특성 대체
    {
        None, // 특성 없음
        HolySpecialization, // 신성 특화
        DarkSpecialization, // 암흑 특화
        FusionCandidates // 보유 포탑 특화
    }
    public sealed class WeaponData // 실제 식별자와 등급 대체
    {
        public string Id { get; set; } // 실제 포탑 식별자
        public int MaxGrade { get; set; } = 5; // 실제 최대 등급
        public WeaponAttribute Attribute { get; set; } // 실제 포탑 속성
    }
    public sealed class CharacterData // 실제 특성 가중치 대체
    {
        public CharacterPassive Passive { get; set; } // 실제 캐릭터 특성
        public float PreferredCandidateWeight { get; set; } = 2f; // 실제 특화 가중치
    }
}

namespace ProjectEpsilon.Combat // 실제 장착 포탑 대체
{
    public sealed class SnakeWeaponSlot // 실제 한 몸통 포탑 대체
    {
        public ProjectEpsilon.Data.WeaponData Weapon { get; set; } // 실제 장착 포탑
        public int Grade { get; set; } = 1; // 실제 장착 등급
        public bool IsEmpty => Weapon == null; // 실제 빈 슬롯 확인
    }
    public sealed class SnakeWeaponManager // 실제 포탑 관리자 대체
    {
        public List<SnakeWeaponSlot> Slots { get; } = new List<SnakeWeaponSlot>(); // 실제 몸통 슬롯 목록
        public bool HasCompletedGradeFive(ProjectEpsilon.Data.WeaponData weapon) // 실제 5등급 제외 조건 대체
        {
            return Slots.Exists(slot => slot != null && !slot.IsEmpty && slot.Grade >= 5 && slot.Weapon.Id == weapon.Id); // 동일 식별자 완성 여부
        }
    }
}

namespace ProjectEpsilon.Player // 실제 선택 캐릭터 대체
{
    public static class CharacterRunInitializer // 실제 시작 캐릭터 대체
    {
        public static ProjectEpsilon.Data.CharacterData ActiveCharacter; // 실제 현재 캐릭터
    }
}

namespace ProjectEpsilon.Progression // 실제 해금 관리자 대체
{
    public sealed class MetaProgressionController // 실제 포탑 해금 대체
    {
        public static MetaProgressionController Instance; // 실제 관리자 대체
        public bool IsTurretUnlocked(ProjectEpsilon.Data.WeaponData weapon) => true; // 시험 포탑 해금 허용
    }
}
