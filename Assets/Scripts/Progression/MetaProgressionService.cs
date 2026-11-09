using System; // 직렬화와 예외 기능
using System.Collections.Generic; // 완료한 런 목록
using UnityEngine; // Unity 저장 기능

namespace ProjectEpsilon.Progression // 영구 성장 영역
{ // 영역 시작
    public enum MetaNode // 영구 성장 노드 번호
    { // 열거형 시작
        Health, Healing, LastBodySpeed, // 생존 계열
        Turn, Stamina, EmergencyBoost, // 기동 계열
        Damage, AttackSpeed, HighGradeDamage, // 전투 계열
        Experience, PickupRange, Reroll // 성장 계열
    } // 열거형 끝
    public interface IMetaStorage // 독립 영구 저장 계약
    { // 인터페이스 시작
        bool TryRead(out string json); // 실제 저장 읽기
        bool TryWrite(string json); // 실제 저장 기록
    } // 인터페이스 끝
    public sealed class PlayerPrefsMetaStorage : IMetaStorage // 입력 설정과 분리된 저장소
    { // 클래스 시작
        private readonly string key; // 실제 메타 저장 키
        public PlayerPrefsMetaStorage(string storageKey) // 실제 저장소 구성
        { // 생성자 시작
            key = storageKey; // 메타 전용 키 저장
        } // 생성자 끝
        public bool TryRead(out string json) // 실제 메타 저장 읽기
        { // 메서드 시작
            json = null; // 신규 저장 기본값
            try // 실제 저장 조회
            { // 보호 영역 시작
                if (PlayerPrefs.HasKey(key)) // 기존 메타 저장 확인
                { // 조건 시작
                    json = PlayerPrefs.GetString(key); // 기존 저장 내용 조회
                } // 조건 끝
                return true; // 실제 읽기 성공
            } // 보호 영역 끝
            catch (Exception error) // 실제 저장 오류 확인
            { // 예외 시작
                Debug.LogError("[Project Epsilon] 영구 저장 읽기 실패: " + error.Message); // 읽기 오류 기록
                return false; // 기존 데이터 덮어쓰기 차단
            } // 예외 끝
        } // 메서드 끝
        public bool TryWrite(string json) // 실제 메타 저장 기록
        { // 메서드 시작
            try // 실제 저장 시도
            { // 보호 영역 시작
                PlayerPrefs.SetString(key, json); // 메타 전용 키 기록
                PlayerPrefs.Save(); // 실제 디스크 기록 요청
                return true; // 실제 기록 완료
            } // 보호 영역 끝
            catch (Exception error) // 실제 저장 오류 확인
            { // 예외 시작
                Debug.LogError("[Project Epsilon] 영구 저장 쓰기 실패: " + error.Message); // 쓰기 오류 기록
                return false; // 메모리 확정 차단
            } // 예외 끝
        } // 메서드 끝
    } // 클래스 끝
    [Serializable] // 실제 저장 형식
    public sealed class MetaSaveData // 저장할 메타 진행
    { // 클래스 시작
        public int version = 1; // 저장 형식 버전
        public int cores; // 보유 진화 코어
        public bool finishedFirstRun; // 첫 런 완료 여부
        public int mapMask = 1; // 해금한 맵 비트
        public int characterMask; // 임시 캐릭터 해금 비트
        public int turretAttributeMask; // 속성별 시험 포탑 해금 비트
        public int[] levels = new int[12]; // 열두 성장 노드
        public List<string> rewardedRuns = new List<string>(); // 중복 보상 방지 런 식별자
    } // 클래스 끝
    public sealed class MetaProgressionService // 저장과 성장 규칙 서비스
    { // 클래스 시작
        private static readonly int[] Costs = { 3, 5, 8, 12, 18 }; // 일반 노드 단계별 비용
        private readonly IMetaStorage storage; // 실제 저장소
        private MetaSaveData data; // 검증된 현재 진행
        public bool IsAvailable { get; private set; } // 저장 사용 가능 여부
        public int Cores => data == null ? 0 : data.cores; // 실제 보유 코어
        public bool FinishedFirstRun => data != null && data.finishedFirstRun; // 첫 런 완료 여부
        public MetaProgressionService(IMetaStorage source) // 실제 저장 서비스 구성
        { // 생성자 시작
            storage = source; // 독립 저장소 연결
            string json; // 실제 저장 내용
            if (storage == null || !storage.TryRead(out json)) // 저장 읽기 실패 확인
            { // 조건 시작
                return; // 기존 데이터 보존
            } // 조건 끝
            if (json == null) // 신규 저장 확인
            { // 조건 시작
                data = new MetaSaveData(); // 신규 메타 생성
                IsAvailable = true; // 실제 사용 가능 상태
                return; // 초기 저장 없이 대기
            } // 조건 끝
            try // 실제 저장 내용 해석
            { // 보호 영역 시작
                if (!json.Contains("\"version\"") || !json.Contains("\"cores\"") || !json.Contains("\"finishedFirstRun\"") || !json.Contains("\"mapMask\"") || !json.Contains("\"characterMask\"") || !json.Contains("\"turretAttributeMask\"") || !json.Contains("\"levels\"") || !json.Contains("\"rewardedRuns\"")) // 필수 저장 항목 확인
                { // 조건 시작
                    Debug.LogError("[Project Epsilon] 메타 저장 항목이 누락되어 원본을 보존합니다."); // 누락 저장 알림
                    return; // 불완전한 저장 덮어쓰기 방지
                } // 조건 끝
                MetaSaveData loaded = JsonUtility.FromJson<MetaSaveData>(json); // 저장 형식 복원
                if (!IsValid(loaded)) // 저장 무결성 확인
                { // 조건 시작
                    Debug.LogError("[Project Epsilon] 메타 저장 형식이 올바르지 않아 원본을 보존합니다."); // 손상 저장 알림
                    return; // 손상 데이터 덮어쓰기 방지
                } // 조건 끝
                data = loaded; // 검증된 저장 적용
                IsAvailable = true; // 실제 사용 가능 상태
            } // 보호 영역 끝
            catch (Exception error) // 역직렬화 실패 확인
            { // 예외 시작
                Debug.LogError("[Project Epsilon] 메타 저장 복원 실패: " + error.Message); // 실제 오류 기록
            } // 예외 끝
        } // 생성자 끝
        private static bool IsValid(MetaSaveData saved) // 실제 저장 무결성 확인
        { // 메서드 시작
            if (saved == null || saved.version != 1 || saved.cores < 0 || saved.mapMask < 0 || saved.characterMask < 0 || saved.turretAttributeMask < 0 || saved.levels == null || saved.levels.Length != 12 || saved.rewardedRuns == null) // 필수 저장 필드 확인
            { // 조건 시작
                return false; // 손상 저장 거부
            } // 조건 끝
            for (int index = 0; index < saved.levels.Length; index++) // 실제 노드 단계 순회
            { // 반복 시작
                int maximum = index % 3 == 2 ? 1 : 5; // 노드별 최대 단계
                if (saved.levels[index] < 0 || saved.levels[index] > maximum) // 실제 단계 범위 확인
                { // 조건 시작
                    return false; // 잘못된 단계 거부
                } // 조건 끝
            } // 반복 끝
            return (saved.mapMask & 1) != 0; // 기본 평원 해금 확인
        } // 메서드 끝
        private MetaSaveData Copy() // 트랜잭션용 진행 복사
        { // 메서드 시작
            return JsonUtility.FromJson<MetaSaveData>(JsonUtility.ToJson(data)); // 실제 저장 사본 생성
        } // 메서드 끝
        private bool Commit(MetaSaveData next) // 저장 후 진행 확정
        { // 메서드 시작
            if (!IsAvailable || !IsValid(next) || !storage.TryWrite(JsonUtility.ToJson(next))) // 실제 저장 성공 확인
            { // 조건 시작
                return false; // 실패 시 기존 진행 유지
            } // 조건 끝
            data = next; // 저장 성공 후 메모리 확정
            return true; // 실제 저장 완료
        } // 메서드 끝
        public int GetLevel(MetaNode node) // 실제 노드 단계 조회
        { // 메서드 시작
            int index = (int)node; // 실제 노드 번호
            return data == null || index < 0 || index >= 12 ? 0 : data.levels[index]; // 안전한 단계 반환
        } // 메서드 끝
        public int GetNextCost(MetaNode node) // 다음 성장 비용 조회
        { // 메서드 시작
            int index = (int)node; // 실제 노드 번호
            if (data == null || index < 0 || index >= 12) // 실제 저장과 노드 확인
            { // 조건 시작
                return -1; // 무효 비용 반환
            } // 조건 끝
            int level = data.levels[index]; // 실제 현재 단계
            return index % 3 == 2 ? level == 0 ? 25 : -1 : level < 5 ? Costs[level] : -1; // 최종 또는 일반 노드 비용
        } // 메서드 끝
        public bool CanPurchase(MetaNode node) // 실제 성장 가능 여부
        { // 메서드 시작
            int index = (int)node; // 실제 노드 번호
            int cost = GetNextCost(node); // 실제 다음 비용
            if (!IsAvailable || !FinishedFirstRun || cost < 0 || data.cores < cost) // 저장과 첫 런 및 재화 확인
            { // 조건 시작
                return false; // 성장 차단
            } // 조건 끝
            int branch = index - index % 3; // 실제 계열 시작 번호
            return index % 3 == 0 || data.levels[branch] >= 1 && (index % 3 == 1 || data.levels[branch + 1] >= 1); // 시험용 선행 단계 확인
        } // 메서드 끝
        public bool TryPurchase(MetaNode node) // 실제 성장 구매
        { // 메서드 시작
            if (!CanPurchase(node)) // 실제 구매 가능 확인
            { // 조건 시작
                return false; // 무효 구매 차단
            } // 조건 끝
            MetaSaveData next = Copy(); // 실제 저장 사본
            next.cores -= GetNextCost(node); // 실제 비용 차감
            next.levels[(int)node]++; // 실제 단계 증가
            return Commit(next); // 저장 성공 시 확정
        } // 메서드 끝
        public bool TryRefundAll() // 실제 무료 성장 초기화
        { // 메서드 시작
            if (!IsAvailable || !FinishedFirstRun) // 저장과 기능 해금 확인
            { // 조건 시작
                return false; // 초기화 차단
            } // 조건 끝
            MetaSaveData next = Copy(); // 실제 저장 사본
            int refund = 0; // 반환할 코어
            for (int index = 0; index < next.levels.Length; index++) // 실제 노드 순회
            { // 반복 시작
                for (int level = 0; level < next.levels[index]; level++) // 구매 단계 순회
                { // 반복 시작
                    refund += index % 3 == 2 ? 25 : Costs[level]; // 실제 구매 비용 합산
                } // 반복 끝
                next.levels[index] = 0; // 실제 단계 초기화
            } // 반복 끝
            next.cores = next.cores > int.MaxValue - refund ? int.MaxValue : next.cores + refund; // 안전한 재화 반환
            return Commit(next); // 저장 성공 시 확정
        } // 메서드 끝
        public bool IsMapUnlocked(int index) // 실제 맵 해금 확인
        { // 메서드 시작
            return data != null && index >= 0 && index < 3 && (data.mapMask & (1 << index)) != 0; // 실제 해금 비트 확인
        } // 메서드 끝
        public bool IsCharacterUnlocked(int index) // 시험 캐릭터 해금 확인
        { // 메서드 시작
            return data != null && index >= 0 && index < 8 && (data.characterMask & (1 << index)) != 0; // 실제 해금 비트 확인
        } // 메서드 끝
        public bool IsTurretAttributeUnlocked(int attribute) // 시험 포탑 속성 해금 확인
        { // 메서드 시작
            return data != null && attribute >= 0 && attribute < 8 && (data.turretAttributeMask & (1 << attribute)) != 0; // 실제 해금 비트 확인
        } // 메서드 끝
        public bool TryUnlockTurretAttribute(int attribute) // 실제 속성별 포탑 해금
        { // 메서드 시작
            if (!IsAvailable || attribute < 0 || attribute >= 8) // 실제 저장과 속성 확인
            { // 조건 시작
                return false; // 무효 해금 차단
            } // 조건 끝
            if (IsTurretAttributeUnlocked(attribute)) // 이미 해금된 속성 확인
            { // 조건 시작
                return true; // 중복 기록 방지
            } // 조건 끝
            MetaSaveData next = Copy(); // 실제 저장 사본
            next.turretAttributeMask |= 1 << attribute; // 실제 속성 해금 기록
            return Commit(next); // 저장 성공 시 확정
        } // 메서드 끝
        public bool TryCompleteRun(string runId, double seconds, int kills, int elites, bool bossDefeated, int mapIndex) // 실제 런 보상 확정
        { // 메서드 시작
            if (!IsAvailable || string.IsNullOrEmpty(runId) || double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0d || kills < 0 || elites < 0 || mapIndex < 0 || mapIndex >= 3) // 실제 성과 유효성 확인
            { // 조건 시작
                return false; // 무효 보상 차단
            } // 조건 끝
            if (data.rewardedRuns.Contains(runId)) // 이미 보상한 런 확인
            { // 조건 시작
                return true; // 재지급 없이 완료
            } // 조건 끝
            double survival = Math.Floor(seconds / 120d); // 실제 생존 보상
            long total = survival >= int.MaxValue ? int.MaxValue : (long)survival; // 안전한 생존 보상
            total = Math.Min(int.MaxValue, total + kills / 100L + elites + (bossDefeated ? 5L : 0L)); // 실제 정수 보상 합계
            MetaSaveData next = Copy(); // 실제 저장 사본
            next.cores = (int)Math.Min(int.MaxValue, next.cores + total); // 안전한 코어 지급
            next.finishedFirstRun = true; // 첫 런 완료 기록
            next.rewardedRuns.Add(runId); // 중복 지급 방지 기록
            if (bossDefeated && mapIndex == 0) // 평원 보스 처치 확인
            { // 조건 시작
                next.mapMask |= 1 << 1; // 실제 폐허 맵 해금
                next.characterMask |= 1; // 시험용 캐릭터 해금
            } // 조건 끝
            if (bossDefeated && mapIndex == 1) // 폐허 보스 처치 확인
            { // 조건 시작
                next.mapMask |= 1 << 2; // 실제 습지 맵 해금
            } // 조건 끝
            return Commit(next); // 저장 성공 시 보상 확정
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
