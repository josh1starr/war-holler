using UnityEngine;
using UnityEngine.Rendering;
using WarHoller.Art;

namespace WarHoller.Bootstrap
{
    /// <summary>
    /// Forces Built-in Render Pipeline: clears any Scriptable Render Pipeline assignment
    /// so MaterialUtil Standard / Unlit/Color materials paint correctly.
    /// </summary>
    public static class RenderPipelineBootstrap
    {
        static bool _ran;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad() => EnsureBuiltIn(forceLog: true);

        /// <summary>Idempotent clear of SRP + diagnostic log. Safe to call from GameBootstrap.Awake.</summary>
        public static void EnsureBuiltIn(bool forceLog = false)
        {
            var hadSrp = GraphicsSettings.defaultRenderPipeline != null
                         || QualitySettings.renderPipeline != null
                         || GraphicsSettings.currentRenderPipeline != null;

            if (_ran && !hadSrp && !forceLog)
                return;

            // Built-in: no custom SRP on Graphics or Quality.
            GraphicsSettings.defaultRenderPipeline = null;
            QualitySettings.renderPipeline = null;
            MaterialUtil.InvalidateCache();
            _ran = true;

            LogDiagnostics(hadSrp);
        }

        // Keep old name as alias so any stray call sites still compile/force Built-in.
        public static void EnsurePipeline(bool forceLog = false) => EnsureBuiltIn(forceLog);

        static void LogDiagnostics(bool clearedSrp)
        {
            var current = GraphicsSettings.currentRenderPipeline;
            var def = GraphicsSettings.defaultRenderPipeline;
            var quality = QualitySettings.renderPipeline;
            var standard = Shader.Find("Standard");
            var unlit = Shader.Find("Unlit/Color");
            var sprites = Shader.Find("Sprites/Default");
            var legacy = Shader.Find("Legacy Shaders/Diffuse");
            var resolved = MaterialUtil.ResolveShader();

            Debug.Log(
                "[WarHoller] Built-in pipeline bootstrap — " +
                $"clearedSrp={clearedSrp}, " +
                $"currentRenderPipeline={(current != null ? current.name : "NULL")}, " +
                $"defaultRenderPipeline={(def != null ? def.name : "NULL")}, " +
                $"qualityRenderPipeline={(quality != null ? quality.name : "NULL")}, " +
                $"Shader.Find Standard={(standard != null)}, Unlit/Color={(unlit != null)}, " +
                $"Sprites/Default={(sprites != null)}, Legacy Diffuse={(legacy != null)}, " +
                $"MaterialUtil resolved={(resolved != null ? resolved.name : "NULL")}");
        }
    }
}
