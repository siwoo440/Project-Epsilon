using System; // 예외 처리
using UnityEditor; // Unity 에셋 가져오기
using UnityEngine; // Unity 스프라이트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    public static class AutomatedMapAssetImportVerification // 맵 이미지 가져오기 검증
    {
        public static void Run() // 실제 Unity 배치 진입
        {
            try // 이미지 설정과 검증
            {
                string root = "Assets/Resources/MapElements"; // 실제 맵 이미지 폴더
                AssetDatabase.Refresh(); // 새 파일 가져오기
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root }); // 실제 텍스처 검색
                Require(guids.Length == 33, "Expected 33 individual map textures; found " + guids.Length); // 개별 이미지 수 확인
                foreach (string guid in guids) // 실제 이미지 순회
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid); // 실제 이미지 경로
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter; // 실제 가져오기 설정
                    Require(importer != null, "Missing texture importer: " + path); // 가져오기 설정 확인
                    importer.textureType = TextureImporterType.Sprite; // 2D 스프라이트 설정
                    importer.spriteImportMode = SpriteImportMode.Single; // 이미지별 단일 스프라이트
                    importer.spritePixelsPerUnit = 256f; // 512픽셀당 2유닛 기준
                    TextureImporterSettings settings = new TextureImporterSettings(); // 실제 텍스처 세부 설정
                    importer.ReadTextureSettings(settings); // 기존 세부 설정 보존
                    settings.spriteAlignment = (int)SpriteAlignment.Center; // 중앙 피벗 기준
                    settings.spriteMeshType = SpriteMeshType.FullRect; // 안정적인 사각 표시
                    importer.SetTextureSettings(settings); // 변경된 세부 설정 적용
                    importer.alphaIsTransparency = true; // 투명 가장자리 유지
                    importer.mipmapEnabled = false; // 2D 이미지 밉맵 비활성
                    importer.filterMode = FilterMode.Bilinear; // 부드러운 2D 확대
                    importer.textureCompression = TextureImporterCompression.Uncompressed; // 시안 색상과 알파 보존
                    importer.SaveAndReimport(); // 실제 설정 저장과 재가져오기
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); // 실제 스프라이트 조회
                    Require(sprite != null && Mathf.Approximately(sprite.pixelsPerUnit, 256f) && Mathf.Approximately(sprite.pivot.x, 256f) && Mathf.Approximately(sprite.pivot.y, 256f), "Sprite settings mismatch: " + path); // 해상도와 중앙 피벗 확인
                }
                Debug.Log("[MapAssetImportVerification] PASS: 33 individual sprites imported with transparent alpha, centered pivots and 256 pixels per unit."); // 실제 성공 기록
                EditorApplication.Exit(0); // 성공 종료
            }
            catch (Exception exception) // 실패 상황 확인
            {
                Debug.LogError("[MapAssetImportVerification] FAIL: " + exception); // 실제 실패 기록
                EditorApplication.Exit(1); // 실패 종료
            }
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실패 원인 전달
            }
        }
    }
}
