using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine.AddressableAssets;

namespace SGGames.Game.Develop.Editor
{
    public static class DebugSystemCheckBuild
    {
        const string kDebugSceneDirectory = "Assets/SGGame/Scenes/DebugScenes/";
        const string kGroupName = "Debug System Checks";
        internal static bool IsProductBuildPrepared { get; private set; }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(options =>
                ExecuteBuild(options, BuildPlayerWindow.DefaultBuildMethods.BuildPlayer));
        }

        public static BuildReport BuildPlayer(BuildPlayerOptions options)
        {
            BuildReport report = null;
            ExecuteBuild(options, preparedOptions => report = BuildPipeline.BuildPlayer(preparedOptions));
            return report;
        }

        static void ExecuteBuild(BuildPlayerOptions options, Action<BuildPlayerOptions> buildPlayer)
        {
            if (!IsProductBuild(options))
            {
                buildPlayer(options);
                return;
            }

            if (IsProductBuildPrepared)
            {
                throw new BuildFailedException("A product build is already in progress.");
            }

            options.scenes = (options.scenes ?? Array.Empty<string>())
                .Where(path => !IsDebugScene(path)).ToArray();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var schema = settings?.FindGroup(kGroupName)?.GetSchema<BundledAssetGroupSchema>();
            bool wasIncluded = schema != null && schema.IncludeInBuild;
            var previousBuildOption = settings != null
                ? settings.BuildAddressablesWithPlayerBuild
                : AddressableAssetSettings.PlayerBuildOption.PreferencesValue;

            try
            {
                if (schema != null)
                {
                    schema.IncludeInBuild = false;
                }

                if (settings != null)
                {
                    AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
                    settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
                }

                if (Directory.Exists(Addressables.BuildPath))
                {
                    throw new BuildFailedException("Previous Addressables player content could not be cleared. Product build stopped to prevent debug content from being included.");
                }

                IsProductBuildPrepared = true;
                buildPlayer(options);
            }
            finally
            {
                IsProductBuildPrepared = false;
                if (schema != null)
                {
                    schema.IncludeInBuild = wasIncluded;
                    AssetDatabase.SaveAssetIfDirty(schema);
                }

                if (settings != null)
                {
                    settings.BuildAddressablesWithPlayerBuild = previousBuildOption;
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssetIfDirty(settings);
                }
            }
        }

        internal static bool IsProductBuild(BuildPlayerOptions options)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(options.target);
            var namedTarget = targetGroup == BuildTargetGroup.Standalone && options.subtarget == (int)StandaloneBuildSubtarget.Server
                ? NamedBuildTarget.Server
                : NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedTarget, out var defines);
            var profile = BuildProfile.GetActiveBuildProfile();
            return defines.Contains("IS_PRODUCT")
                || (options.extraScriptingDefines?.Contains("IS_PRODUCT") ?? false)
                || (profile != null && options.target == EditorUserBuildSettings.activeBuildTarget
                    && profile.scriptingDefines.Contains("IS_PRODUCT"));
        }

        internal static bool IsDebugScene(string path)
        {
            return path.Replace('\\', '/').StartsWith(kDebugSceneDirectory, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class DebugSystemCheckBuildGuard : BuildPlayerProcessor
    {
        public override int callbackOrder => -10000;

        public override void PrepareForBuild(BuildPlayerContext context)
        {
            var options = context.BuildPlayerOptions;
            if (DebugSystemCheckBuild.IsProductBuild(options)
                && (!DebugSystemCheckBuild.IsProductBuildPrepared
                    || (options.scenes?.Any(DebugSystemCheckBuild.IsDebugScene) ?? false)))
            {
                throw new BuildFailedException("Use DebugSystemCheckBuild.BuildPlayer(options) or the Unity Build button for IS_PRODUCT builds so debug scenes and Addressables content are excluded.");
            }
        }
    }
}
