#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GongWon.Editor
{
    /// <summary>
    /// 自动化打包脚本 — 一键打包Android APK
    /// 菜单：Tools/GongWon/Build Android APK
    /// </summary>
    public class BuildScript
    {
        private static string[] scenes = {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Map_GhostValley.unity",
            "Assets/Scenes/Map_RiverLand.unity",
            "Assets/Scenes/Map_RiverLand_4Team.unity"
        };

        [MenuItem("Tools/GongWon/Build Android APK")]
        public static void BuildAndroid()
        {
            Debug.Log("[BuildScript] 开始打包 Android APK...");

            // 配置玩家设置
            PlayerSettings.companyName = "GongWon Studio";
            PlayerSettings.productName = "공원가족";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29; // Android 10
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35; // Android 15
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // 输出路径
            string outputPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                "GongWon_APK",
                "공원가족_v1.0.0.apk"
            );

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

            // 构建选项
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // 执行打包
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] 打包成功！APK路径: {outputPath}");
                Debug.Log($"[BuildScript] 大小: {summary.totalSize / 1024 / 1024} MB");
                EditorUtility.DisplayDialog("打包成功", $"APK已生成:\n{outputPath}", "确定");
            }
            else
            {
                Debug.LogError($"[BuildScript] 打包失败！结果: {summary.result}");
                EditorUtility.DisplayDialog("打包失败", $"打包过程中出现错误，请查看Console日志", "确定");
            }
        }

        [MenuItem("Tools/GongWon/Build Developer Tool APK")]
        public static void BuildDeveloperTool()
        {
            Debug.Log("[BuildScript] 开始打包开发者工具 APK...");

            PlayerSettings.companyName = "GongWon Studio";
            PlayerSettings.productName = "공원가족 DevTool";
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // 开发者工具需要悬浮窗权限
            PlayerSettings.Android.forceInternetPermission = true;

            string outputPath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                "GongWon_APK",
                "공원가족-1.0_DevTool.apk"
            );

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/DevTool.unity" },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] 开发者工具打包成功！路径: {outputPath}");
            }
            else
            {
                Debug.LogError("[BuildScript] 开发者工具打包失败！");
            }
        }

        [MenuItem("Tools/GongWon/Setup Project Settings")]
        public static void SetupProjectSettings()
        {
            Debug.Log("[BuildScript] 配置项目设置...");

            // 包名
            PlayerSettings.applicationIdentifier = "com.gongwon.family";

            // 图标（需要设置）
            // PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });

            // 横屏/竖屏
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // 性能设置
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            // 内存
            PlayerSettings.gcIncremental = true;

            Debug.Log("[BuildScript] 项目设置配置完成");
            EditorUtility.DisplayDialog("配置完成", "项目设置已配置完成", "确定");
        }
    }
}
#endif
