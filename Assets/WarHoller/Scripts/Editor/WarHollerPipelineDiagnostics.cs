#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WarHoller.Art;
using WarHoller.Bootstrap;

namespace WarHoller.EditorTools
{
    /// <summary>
    /// Editor-only diagnostics so pink-screen debugging is one Console glance away on Play.
    /// </summary>
    [InitializeOnLoad]
    static class WarHollerPipelineDiagnostics
    {
        static WarHollerPipelineDiagnostics()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            RenderPipelineBootstrap.EnsureBuiltIn(forceLog: true);

            var current = GraphicsSettings.currentRenderPipeline;
            var standardOk = Shader.Find("Standard") != null;
            var unlitOk = Shader.Find("Unlit/Color") != null;
            Debug.Log(
                $"[WarHoller/Editor] Entered Play Mode — Built-in RP; currentRenderPipeline is " +
                $"{(current == null ? "NULL (good)" : current.name + " (UNEXPECTED)")}; " +
                $"Standard Shader.Find={(standardOk ? "OK" : "FAILED")}; " +
                $"Unlit/Color Shader.Find={(unlitOk ? "OK" : "FAILED")}; " +
                $"MaterialUtil={MaterialUtil.CachedShaderName ?? MaterialUtil.ResolveShader()?.name}");
        }

        [MenuItem("War Holler/Diagnostics/Log Render Pipeline Status")]
        static void MenuLog()
        {
            RenderPipelineBootstrap.EnsureBuiltIn(forceLog: true);
        }
    }
}
#endif
