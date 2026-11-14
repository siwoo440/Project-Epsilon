using ProjectEpsilon.Data; // 실제 포탑 속성
using UnityEngine; // Unity 이미지와 객체
namespace ProjectEpsilon.Combat // 전투 시각 영역
{ // 영역 시작
    public static class CombatVfx // 공통 속성·반응 표시
    { // 시각 기능 시작
        private const int HitLimit = 48; // 일반 명중 표시 상한
        private const int PriorityLimit = 32; // 반응과 플레이어 표시 상한
        private static readonly string[] Names = { "Physical", "Fire", "Cold", "Electric", "Poison", "Explosion", "Holy", "Dark", "CorrosiveBurn", "CoolingDischarge", "ShatteringImpact", "SacredExplosion", "CorrosiveCurse", "LightDarkCancel", "BodyBreak", "Invulnerable" }; // 실제 아틀라스 순서
        private static Sprite[] sprites; // 실제 속성별 생성 스프라이트
        private static int activeHits; // 활성 일반 명중 표시 수
        private static int activePriority; // 활성 반응과 플레이어 표시 수
        public static int ActiveHitCount => activeHits; // 실제 활성 명중 표시 수
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 실제 플레이 세션 초기화
        private static void ResetSession() // 이전 세션 상태 정리
        { // 함수 시작
            if (sprites != null) // 이전 세션 이미지 확인
            { // 조건 시작
                foreach (Sprite sprite in sprites) // 실제 생성 이미지 순회
                { // 반복 시작
                    if (sprite != null) // 실제 생성 이미지 확인
                    { // 조건 시작
                        Object.Destroy(sprite); // 이전 세션 생성 이미지 정리
                    } // 조건 종료
                } // 반복 종료
            } // 조건 종료
            sprites = null; // 이전 아틀라스 스프라이트 해제
            activeHits = 0; // 이전 명중 표시 수 초기화
            activePriority = 0; // 이전 우선 표시 수 초기화
        } // 함수 종료
        public static Sprite GetAttributeSprite(WeaponAttribute attribute) // 실제 속성별 이미지 조회
        { // 함수 시작
            int index = (int)attribute; // 실제 속성 번호
            return index >= 0 && index < 8 ? GetSprite(index) : null; // 유효 속성 이미지 반환
        } // 함수 종료
        public static Sprite GetReactionSprite(WeaponAttributeReaction reaction) // 실제 반응별 이미지 조회
        { // 함수 시작
            int index = (int)reaction; // 실제 반응 번호
            return index >= 1 && index <= 6 ? GetSprite(7 + index) : null; // 유효 반응 이미지 반환
        } // 함수 종료
        public static Sprite GetInvulnerabilitySprite() // 실제 무적 이미지 조회
        { // 함수 시작
            return GetSprite(15); // 실제 무적 원형 이미지
        } // 함수 종료
        public static Sprite GetBodyBreakSprite() // 실제 몸통 손실 이미지 조회
        { // 함수 시작
            return GetSprite(14); // 기존 몸통 손실 이미지 반환
        } // 함수 종료
        public static GameObject SpawnAttribute(WeaponAttribute attribute, int stage, Vector3 position) // 실제 속성 명중 표시
        { // 함수 시작
            Sprite sprite = GetAttributeSprite(attribute); // 실제 속성 이미지
            float radius = 0.35f + Mathf.Max(0, stage) * 0.035f; // 기존 단계별 표시 반경
            return Spawn(attribute + "_HitPulse", sprite, position, radius, 0.18f, 18, false, Color.white); // 기존 명중 표시와 제한 재사용
        } // 함수 종료
        public static GameObject SpawnReaction(WeaponAttributeReaction reaction, Vector3 position) // 실제 속성 반응 표시
        { // 함수 시작
            return Spawn("Reaction_" + reaction, GetReactionSprite(reaction), position, 0.65f, 0.35f, 22, true, Color.white); // 실제 반응별 우선 표시
        } // 함수 종료
        public static GameObject SpawnPlayerDamage(Vector3 position) // 실제 플레이어 피격 표시
        { // 함수 시작
            return Spawn("PlayerDamageVfx", GetSprite(0), position, 0.6f, 0.22f, 22, true, new Color(1f, 0.3f, 0.3f, 0.9f)); // 실제 적색 피격 표시
        } // 함수 종료
        public static GameObject SpawnBodyBreak(Vector3 position) // 실제 몸통 손실 위치 표시
        { // 함수 시작
            return Spawn("PlayerBodyBreakVfx", GetSprite(14), position, 0.65f, 0.4f, 22, true, Color.white); // 실제 손실 위치 표시
        } // 함수 종료
        private static Sprite GetSprite(int index) // 실제 아틀라스 셀 조회
        { // 함수 시작
            if (sprites == null) // 실제 스프라이트 준비 확인
            { // 조건 시작
                CreateSprites(); // 실제 아틀라스 셀 생성
            } // 조건 종료
            return sprites != null && index >= 0 && index < sprites.Length ? sprites[index] : null; // 유효 이미지 또는 부재 반환
        } // 함수 종료
        private static void CreateSprites() // 생성한 아틀라스의 실제 셀 구성
        { // 함수 시작
            Sprite atlas = Resources.Load<Sprite>("VFX/CombatVfxAtlas"); // 실제 생성 이미지 아틀라스
            if (atlas == null || atlas.texture == null) // 실제 이미지 부재 확인
            { // 조건 시작
                return; // 기존 명중 표시 대체 경로 유지
            } // 조건 종료
            Texture2D texture = atlas.texture; // 실제 아틀라스 질감
            sprites = new Sprite[Names.Length]; // 실제 열여섯 셀 목록
            for (int index = 0; index < sprites.Length; index++) // 실제 아틀라스 셀 순회
            { // 반복 시작
                int column = index % 4; // 실제 셀 열
                int row = index / 4; // 위에서 시작하는 셀 행
                int left = texture.width * column / 4; // 실제 셀 왼쪽 경계
                int right = texture.width * (column + 1) / 4; // 실제 셀 오른쪽 경계
                int bottom = texture.height * (3 - row) / 4; // 실제 셀 아래 경계
                int top = texture.height * (4 - row) / 4; // 실제 셀 위 경계
                Rect rect = new Rect(left, bottom, right - left, top - bottom); // 실제 셀 이미지 범위
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), Mathf.Max(rect.width, rect.height), 0, SpriteMeshType.FullRect); // 실제 셀 스프라이트 생성
                sprite.name = "VFX_" + Names[index]; // 실제 셀 구분 이름
                sprites[index] = sprite; // 실제 이미지 저장
            } // 반복 종료
        } // 함수 종료
        private static GameObject Spawn(string name, Sprite sprite, Vector3 position, float radius, float duration, int order, bool priority, Color color) // 제한된 일회성 시각 표시
        { // 함수 시작
            if (sprite == null || priority && activePriority >= PriorityLimit || !priority && activeHits >= HitLimit) // 실제 이미지와 표시 상한 확인
            { // 조건 시작
                return null; // 무효 또는 상한 초과 표시 생략
            } // 조건 종료
            GameObject instance = new GameObject(name); // 실제 시각 효과 객체
            instance.transform.position = position; // 실제 발생 위치
            WeaponAttackPulse pulse = instance.AddComponent<WeaponAttackPulse>(); // 기존 페이드 표시 재사용
            pulse.Configure(sprite, radius, color, duration); // 실제 이미지와 지속 시간
            instance.GetComponent<SpriteRenderer>().sortingOrder = order; // 공격 예고 아래 표시 순서
            instance.AddComponent<CombatVfxTicket>().Register(priority); // 실제 동시 표시 집계
            if (priority) // 우선 표시 종류 확인
            { // 조건 시작
                activePriority++; // 실제 우선 표시 수 증가
            } // 조건 종료
            else // 일반 명중 표시 확인
            { // 조건 시작
                activeHits++; // 실제 일반 명중 표시 수 증가
            } // 조건 종료
            return instance; // 실제 생성 표시 반환
        } // 함수 종료
        internal static void Release(bool priority) // 실제 표시 종료 집계
        { // 함수 시작
            if (priority) // 우선 표시 종류 확인
            { // 조건 시작
                activePriority = Mathf.Max(0, activePriority - 1); // 실제 우선 표시 수 감소
            } // 조건 종료
            else // 일반 명중 표시 확인
            { // 조건 시작
                activeHits = Mathf.Max(0, activeHits - 1); // 실제 일반 명중 표시 수 감소
            } // 조건 종료
        } // 함수 종료
    } // 시각 기능 종료
} // 영역 종료
