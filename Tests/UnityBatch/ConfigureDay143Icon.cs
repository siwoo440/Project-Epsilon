using System; // 예외 처리
using System.Collections.Generic; // 아이콘 크기 목록
using System.IO; // 이미지 파일 저장
using UnityEditor; // 유니티 빌드 설정
using UnityEditor.Build; // 실제 빌드 플랫폼
using UnityEngine; // 이미지 변환 기능

namespace ProjectEpsilon.Editor // 배치 설정 영역
{ // 영역 시작
    public static class ConfigureDay143Icon // 실제 Windows 아이콘 구성
    { // 설정 시작
        public static void Run() // 크기별 이미지와 빌드 아이콘 구성
        { // 함수 시작
            try // 실제 아이콘 설정
            { // 처리 시작
                const string folder = "Assets/Art/GameIcon"; // 실제 아이콘 에셋 폴더
                string sourcePath = Path.Combine(folder, "GameIcon_Source.png"); // 원본 생성 이미지
                Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, true); // 원본 이미지 버퍼
                Require(ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath)), "아이콘 원본을 불러오지 못함"); // 실제 원본 로드 확인
                source.filterMode = FilterMode.Trilinear; // 축소 시 부드러운 표본 사용
                source.Apply(true); // 축소용 단계 생성
                int[] required = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application); // Unity가 요구하는 Windows 크기
                Require(required != null && required.Length > 0, "Windows 아이콘 슬롯 없음"); // 실제 플랫폼 지원 확인
                HashSet<int> sizes = new HashSet<int>(required); // 중복 없는 실제 필요 크기
                foreach (int extra in new[] { 1024, 256, 64, 32, 16 }) // 요청한 확인 이미지 크기
                { // 순회 시작
                    sizes.Add(extra); // 확인 크기 추가
                } // 순회 종료
                foreach (int size in sizes) // 실제 크기별 이미지 생성
                { // 순회 시작
                    SaveSize(source, folder, size); // 원본에서 투명 PNG 축소 저장
                } // 순회 종료
                UnityEngine.Object.DestroyImmediate(source); // 원본 이미지 버퍼 정리
                AssetDatabase.Refresh(); // 실제 새 에셋 가져오기
                foreach (int size in sizes) // 실제 이미지 임포트 설정
                { // 순회 시작
                    string path = $"{folder}/GameIcon_{size}.png"; // 실제 크기별 에셋 경로
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter; // 실제 텍스처 설정
                    Require(importer != null, "아이콘 임포터 누락: " + path); // 실제 텍스처 임포터 확인
                    importer.textureType = TextureImporterType.Default; // 앱 아이콘용 텍스처
                    importer.alphaIsTransparency = true; // 투명 가장자리 유지
                    importer.mipmapEnabled = false; // 아이콘 밉맵 제외
                    importer.textureCompression = TextureImporterCompression.Uncompressed; // 아이콘 선명도 유지
                    importer.maxTextureSize = 2048; // 원본 크기 보존
                    importer.SaveAndReimport(); // 실제 설정 반영
                } // 순회 종료
                Texture2D[] icons = new Texture2D[required.Length]; // 실제 Windows 슬롯 목록
                for (int index = 0; index < required.Length; index++) // 실제 슬롯 순서 순회
                { // 순회 시작
                    icons[index] = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/GameIcon_{required[index]}.png"); // 실제 슬롯 크기의 이미지
                    Require(icons[index] != null, "슬롯 이미지 누락: " + required[index]); // 실제 이미지 확인
                } // 순회 종료
                PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application); // 실제 Windows 빌드 아이콘 설정
                AssetDatabase.SaveAssets(); // 실제 프로젝트 설정 저장
                Debug.Log("[Day143Configure] PASS: configured sizes " + string.Join(",", required)); // 실제 연결 결과
                EditorApplication.Exit(0); // 성공 종료
            } // 처리 종료
            catch (Exception error) // 실제 설정 실패 처리
            { // 실패 시작
                Debug.LogError("[Day143Configure] FAIL: " + error); // 실제 실패 원인
                EditorApplication.Exit(1); // 실패 종료
            } // 실패 종료
        } // 함수 종료
        private static void SaveSize(Texture2D source, string folder, int size) // 원본의 크기별 PNG 저장
        { // 함수 시작
            RenderTexture previous = RenderTexture.active; // 기존 렌더 대상
            RenderTexture target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default); // 투명 크기별 렌더 대상
            Graphics.Blit(source, target); // 실제 원본 이미지 축소
            RenderTexture.active = target; // 실제 픽셀 읽기 대상
            Texture2D scaled = new Texture2D(size, size, TextureFormat.RGBA32, false); // 투명 결과 이미지
            scaled.ReadPixels(new Rect(0f, 0f, size, size), 0, 0); // 실제 축소 픽셀 복사
            scaled.Apply(); // 이미지 픽셀 반영
            File.WriteAllBytes($"{folder}/GameIcon_{size}.png", scaled.EncodeToPNG()); // 실제 PNG 저장
            RenderTexture.active = previous; // 기존 렌더 대상 복구
            RenderTexture.ReleaseTemporary(target); // 임시 렌더 대상 정리
            UnityEngine.Object.DestroyImmediate(scaled); // 임시 이미지 정리
        } // 함수 종료
        private static void Require(bool value, string message) // 실제 설정 조건 확인
        { // 함수 시작
            if (!value) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 설정 실패 전달
            } // 조건 종료
        } // 함수 종료
    } // 설정 종료
} // 영역 종료
