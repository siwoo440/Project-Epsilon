using System; // 예외 처리
using System.IO; // 실제 이미지 파일 확인
using UnityEditor; // 유니티 빌드 설정
using UnityEditor.Build; // 플랫폼별 빌드 대상
using UnityEngine; // 실제 이미지 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedDay143IconVerification // Windows 게임 아이콘 시험
    { // 시험 시작
        public static void Run() // 실제 아이콘 설정 확인
        { // 함수 시작
            try // 실제 아이콘 검사
            { // 처리 시작
                int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application); // 실제 Windows 필요 아이콘 크기
                Texture2D[] icons = PlayerSettings.GetIcons(NamedBuildTarget.Standalone, IconKind.Application); // 실제 Windows 아이콘 설정
                Require(sizes != null && sizes.Length > 0 && icons != null && icons.Length == sizes.Length, "Windows 아이콘 크기 또는 설정 누락"); // 실제 플랫폼 설정 확인
                for (int index = 0; index < sizes.Length; index++) // 실제 아이콘 크기별 순회
                { // 순회 시작
                    Require(icons[index] != null && AssetDatabase.GetAssetPath(icons[index]).StartsWith("Assets/Art/GameIcon/"), "Windows 아이콘 슬롯 누락: " + sizes[index]); // 실제 이미지 연결 확인
                    Require(icons[index].width == sizes[index] && icons[index].height == sizes[index], "아이콘 이미지 크기 불일치: " + sizes[index]); // 실제 지정 크기 확인
                } // 순회 종료
                foreach (int size in new[] { 1024, 256, 64, 32, 16 }) // 확인용 이미지 크기 순회
                { // 순회 시작
                    Texture2D preview = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Art/GameIcon/GameIcon_{size}.png"); // 실제 크기별 확인 이미지
                    Require(preview != null && preview.width == size && preview.height == size, "확인용 이미지 누락: " + size); // 실제 이미지 크기 확인
                    Texture2D pixels = new Texture2D(2, 2, TextureFormat.RGBA32, false); // 원본 투명도 확인 버퍼
                    Require(ImageConversion.LoadImage(pixels, File.ReadAllBytes($"Assets/Art/GameIcon/GameIcon_{size}.png")), "확인용 이미지 로드 실패: " + size); // 실제 PNG 픽셀 로드
                    Require(pixels.GetPixel(0, 0).a < 0.01f && pixels.GetPixel(size - 1, size - 1).a < 0.01f && pixels.GetPixel(size / 2, size / 2).a > 0.5f, "아이콘 투명 배경 또는 그림 누락: " + size); // 투명 모서리와 실제 그림 확인
                    UnityEngine.Object.DestroyImmediate(pixels); // 시험용 이미지 정리
                } // 순회 종료
                Debug.Log("[Day143Icon] PASS: Windows slots and transparent size previews: " + string.Join(",", sizes)); // 실제 확인 결과
                EditorApplication.Exit(0); // 성공 종료
            } // 처리 종료
            catch (Exception error) // 실제 실패 처리
            { // 실패 시작
                Debug.LogError("[Day143Icon] FAIL: " + error); // 실제 실패 원인 기록
                EditorApplication.Exit(1); // 실패 종료
            } // 실패 종료
        } // 함수 종료
        private static void Require(bool value, string message) // 시험 조건 확인
        { // 함수 시작
            if (!value) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 시험 실패 전달
            } // 조건 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
