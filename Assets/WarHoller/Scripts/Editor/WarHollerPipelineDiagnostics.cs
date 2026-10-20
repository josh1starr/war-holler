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

            RenderPipelineBootstrap.EnsurePipeline(forceLog: true);

            var current = GraphicsSettings.currentRenderPipeline;
            var litOk = Shader.Find("Universal Render Pipeline/Lit") != null;
            var standardOk = Shader.Find("Standard") != null;
            Debug.Log(
                $"[WarHoller/Editor] Entered Play Mode — currentRenderPipeline is " +
                $"{(current == null ? "NULL" : current.name)}; " +
                $"URP Lit Shader.Find={(litOk ? "OK" : "FAILED")}; " +
                $"Standard Shader.Find={(standardOk ? "OK" : "FAILED")}; " +
                $"MaterialUtil={MaterialUtil.CachedShaderName ?? MaterialUtil.ResolveShader()?.name}");
        }

        [MenuItem("War Holler/Diagnostics/Log Render Pipeline Status")]
        static void MenuLog()
        {
            RenderPipelineBootstrap.EnsurePipeline(forceLog: true);
        }
    }
}
#endif
