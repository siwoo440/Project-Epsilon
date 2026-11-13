using System; // 예외 처리
using System.Collections.Generic; // 이미지 중복 검사
using System.IO; // 파일 목록
using ProjectEpsilon.Data; // 포탑 데이터
using UnityEditor; // Unity 자산 검사
using UnityEngine; // 스프라이트 검사

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedDay72ArtworkVerification // 포탑 그림 연결 검사
    { // 검사 시작
        public static void Run() // 배치 진입점
        { // 실행 시작
            try // 검사 실행
            { // 실행 시작
                Dictionary<string, string> aliases = new Dictionary<string, string> // 기존 포탑의 기획서 이름
                { // 목록 시작
                    { "DebugBlade", "PhysicalSteelMelee" }, // 강철 근접 포탑
                    { "DebugNeedle", "PhysicalSteelPierce" }, // 강철 관통 포탑
                    { "DebugNova", "PhysicalSteelShockwave" }, // 강철 충격파 포탑
                    { "DebugBlaster", "FireProjectile" }, // 화염탄 포탑
                    { "DebugCutter", "FireMelee" }, // 화염 근접 포탑
                    { "DebugPulse", "FireArea" } // 화염 범위 포탑
                }; // 목록 종료
                HashSet<Sprite> images = new HashSet<Sprite>(); // 개별 그림 집합
                HashSet<Sprite> icons = new HashSet<Sprite>(); // 개별 아이콘 집합
                int total = 0; // 포탑 수
                foreach (string path in Directory.GetFiles("Assets/Data/Weapons", "*.asset", SearchOption.TopDirectoryOnly)) // 기본 포탑 순회
                { // 순회 시작
                    string filename = Path.GetFileNameWithoutExtension(path); // 포탑 파일 이름
                    string name = aliases.TryGetValue(filename, out string alias) ? alias : filename.Replace("Prototype", ""); // 기획서 이미지 이름
                    string imagePath = "Assets/Art/Sprites/Turrets/Turret_" + name + ".png"; // 실제 기획서 그림 경로
                    string iconPath = "Assets/Art/Sprites/TurretIcons/Icon_Turret_" + name + ".png"; // 실제 기획서 아이콘 경로
                    WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path.Replace('\\', '/')); // 실제 포탑 데이터
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath); // 실제 포탑 그림
                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath); // 실제 포탑 아이콘
                    TextureImporter importer = AssetImporter.GetAtPath(imagePath) as TextureImporter; // 이미지 설정
                    TextureImporter iconImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter; // 아이콘 설정
                    Require(weapon != null && sprite != null && weapon.Artwork == sprite, "기획서 그림 연결 불일치: " + path); // 기획서별 그림 확인
                    Require(images.Add(sprite), "포탑 그림 중복: " + path); // 개별 그림 확인
                    Require(icon != null && weapon.Icon == icon && icons.Add(icon) && icon != sprite, "개별 아이콘 연결 불일치: " + path); // 개별 아이콘 확인
                    Require(iconImporter != null && iconImporter.textureType == TextureImporterType.Sprite && iconImporter.spriteImportMode == SpriteImportMode.Single, "아이콘 가져오기 실패: " + iconPath); // 아이콘 설정 확인
                    Require(importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single, "그림 가져오기 실패: " + imagePath); // 스프라이트 가져오기 확인
                    Require(Mathf.Approximately(sprite.pixelsPerUnit, 1000f), "그림 크기 불일치: " + imagePath); // 포탑 크기 확인
                    Require(Mathf.Abs(sprite.pivot.x / sprite.rect.width - 0.5f) < 0.001f && Mathf.Abs(sprite.pivot.y / sprite.rect.height - 0.5f) < 0.001f, "그림 중심 불일치: " + imagePath); // 회전 중심 확인
                    Require(weapon.ArtworkDisplayTint == Color.white, "그림 원본 색상 변형: " + path); // 속성별 원본 색상 확인
                    total++; // 포탑 수 증가
                } // 순회 종료
                Require(total == 48 && images.Count == 48 && icons.Count == 48, "포탑 또는 그림 수 불일치"); // 전체 포탑과 개별 그림 확인
                Debug.Log("[Day72ArtworkVerification] PASS weapons=" + total + " distinctSprites=" + images.Count + " distinctIcons=" + icons.Count); // 성공 결과 기록
                EditorApplication.Exit(0); // 정상 종료
            } // 실행 종료
            catch (Exception error) // 실패 처리
            { // 실패 시작
                Debug.LogError("[Day72ArtworkVerification] FAIL " + error); // 오류 결과 기록
                EditorApplication.Exit(1); // 실패 종료
            } // 실패 종료
        } // 실행 종료
        private static void Require(bool condition, string message) // 조건 확인
        { // 확인 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 내용 전달
            } // 조건 종료
        } // 확인 종료
    } // 검사 종료
} // 영역 종료
