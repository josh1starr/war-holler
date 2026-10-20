using UnityEngine;
using UnityEngine.Rendering;
using WarHoller.Art;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WarHoller.Bootstrap
{
    /// <summary>
    /// Assigns a URP pipeline asset at the earliest possible moment so runtime materials
    /// resolve against a live SRP instead of pink/magenta Missing shaders.
    /// Prefers Resources copies under WarHoller/Resources/URP; Editor can also load Assets/Settings.
    /// </summary>
    public static class RenderPipelineBootstrap
    {
        const string ResourcesPcPath = "URP/PC_RPAsset";
        const string ResourcesMobilePath = "URP/Mobile_RPAsset";
        const string EditorPcPath = "Assets/Settings/PC_RPAsset.asset";
        const string EditorMobilePath = "Assets/Settings/Mobile_RPAsset.asset";

        static bool _ran;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad() => EnsurePipeline(forceLog: true);

        /// <summary>Idempotent assign + diagnostic log. Safe to call from GameBootstrap.Awake.</summary>
        public static void EnsurePipeline(bool forceLog = false)
        {
            var alreadyHadPipeline = GraphicsSettings.currentRenderPipeline != null
                                     || GraphicsSettings.defaultRenderPipeline != null;

            if (_ran && alreadyHadPipeline && !forceLog)
                return;

            var asset = LoadPipelineAsset();
            if (asset != null)
            {
                GraphicsSettings.defaultRenderPipeline = asset;
                QualitySettings.renderPipeline = asset;
                MaterialUtil.InvalidateCache();
                _ran = true;
            }
            else if (!_ran)
            {
                Debug.LogWarning(
                    "[WarHoller] RenderPipelineBootstrap: no URP asset found in Resources or Assets/Settings. " +
                    "If the screen is pink, set Project Settings → Graphics → Scriptable Render Pipeline to None (Built-in) " +
                    "so MaterialUtil Standard / Unlit/Color fallbacks can paint correctly.");
                MaterialUtil.InvalidateCache();
                _ran = true;
            }

            LogDiagnostics(asset);
        }

        static RenderPipelineAsset LoadPipelineAsset()
        {
            // Prefer Resources so Play Mode / builds work even if GraphicsSettings YAML GUID broke.
            var fromResources = Resources.Load<RenderPipelineAsset>(ResourcesPcPath)
                                ?? Resources.Load<RenderPipelineAsset>(ResourcesMobilePath);
            if (fromResources != null)
                return fromResources;

#if UNITY_EDITOR
            var fromEditor = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(EditorPcPath)
                             ?? AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(EditorMobilePath);
            if (fromEditor != null)
                return fromEditor;
#endif
            return GraphicsSettings.defaultRenderPipeline ?? QualitySettings.renderPipeline;
        }

        static void LogDiagnostics(RenderPipelineAsset assigned)
        {
            var current = GraphicsSettings.currentRenderPipeline;
            var def = GraphicsSettings.defaultRenderPipeline;
            var quality = QualitySettings.renderPipeline;
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var standard = Shader.Find("Standard");
            var unlit = Shader.Find("Unlit/Color");
            var sprites = Shader.Find("Sprites/Default");
            var resolved = MaterialUtil.ResolveShader();

            Debug.Log(
                "[WarHoller] Pipeline bootstrap — " +
                $"assigned={(assigned != null ? assigned.name : "null")}, " +
                $"currentRenderPipeline={(current != null ? current.name : "NULL")}, " +
                $"defaultRenderPipeline={(def != null ? def.name : "NULL")}, " +
                $"qualityRenderPipeline={(quality != null ? quality.name : "NULL")}, " +
                $"Shader.Find Lit={(lit != null)}, Standard={(standard != null)}, " +
                $"Unlit/Color={(unlit != null)}, Sprites/Default={(sprites != null)}, " +
                $"MaterialUtil resolved={(resolved != null ? resolved.name : "NULL")}");
        }
    }
}
