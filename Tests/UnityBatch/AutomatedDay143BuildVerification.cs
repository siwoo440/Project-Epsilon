using System; // 例外処理
using System.IO; // ビルド保存先
using System.Linq; // 有効シーン抽出
using UnityEditor; // 実際ビルド機能
using UnityEditor.Build.Reporting; // 実際ビルド結果
using UnityEngine; // ユニティ機能

namespace ProjectEpsilon.Editor // バッチ検証領域
{ // 領域開始
    public static class AutomatedDay143BuildVerification // Windows実行ファイル検証
    { // 試験開始
        public static void Run() // 実際の隔離ビルド実行
        { // 関数開始
            try // 実際ビルド検証
            { // 処理開始
                string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(); // 有効な実際のシーン順
                if (scenes.Length != 4 || scenes[0] != "Assets/Scenes/MainMenu.unity") // 実際の開始シーン確認
                { // 条件開始
                    throw new InvalidOperationException("有効な四つの開始シーンが不正"); // 誤った試験ビルド防止
                } // 条件終了
                bool performance = Array.IndexOf(Environment.GetCommandLineArgs(), "-epsilonPerformanceBuild") >= 0; // 성능 측정 빌드 옵션
                string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", performance ? "Day178Build" : "Day143Build")); // 기존 아이콘 빌드와 분리된 출력 폴더
                Directory.CreateDirectory(folder); // 試験ビルド保存先
                string executable = Path.Combine(folder, performance ? "Project-Epsilon-Performance.exe" : "Project-Epsilon-IconTest.exe"); // 빌드 종류별 실행 파일
                BuildPlayerOptions options = new BuildPlayerOptions(); // 実際のビルド設定
                options.scenes = scenes; // 実際の四つのシーン
                options.locationPathName = executable; // 実際の出力先
                options.target = BuildTarget.StandaloneWindows64; // 実際のWindows対象
                options.options = performance ? BuildOptions.Development : BuildOptions.None; // 성능 검증 시 Profiler 연결 가능 빌드
                BuildReport report = BuildPipeline.BuildPlayer(options); // 実際のWindows試験ビルド
                if (report.summary.result != BuildResult.Succeeded || !File.Exists(executable)) // 実際の出力確認
                { // 条件開始
                    throw new InvalidOperationException("ビルド失敗: " + report.summary.result); // 実際の失敗通知
                } // 条件終了
                Debug.Log((performance ? "[Day178Build]" : "[Day143Build]") + " PASS: " + executable); // 실제 성공과 출력 경로
                EditorApplication.Exit(0); // 成功終了
            } // 処理終了
            catch (Exception error) // 実際の試験失敗
            { // 失敗開始
                Debug.LogError("[Day143Build] FAIL: " + error); // 実際の失敗理由
                EditorApplication.Exit(1); // 失敗終了
            } // 処理終了
        } // 関数終了
    } // 試験終了
} // 領域終了
