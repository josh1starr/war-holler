using UnityEngine;
using UnityEngine.Rendering;

namespace WarHoller.Art
{
    /// <summary>
    /// Runtime material factory with a pipeline-aware shader fallback chain.
    /// Sets both URP (_BaseColor) and Built-in (_Color) so materials stay tinted on either pipeline.
    /// </summary>
    public static class MaterialUtil
    {
        static readonly string[] UrpFirst =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Standard",
            "Unlit/Color",
            "Sprites/Default",
            "Hidden/InternalErrorShader"
        };

        static readonly string[] BuiltinFirst =
        {
            "Standard",
            "Unlit/Color",
            "Sprites/Default",
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Hidden/InternalErrorShader"
        };

        static Shader _cachedShader;
        static string _cachedShaderName;
        static bool _loggedChoice;

        /// <summary>Solid-color material using the best available URP/builtin shader.</summary>
        public static Material CreateColor(Color color)
        {
            var shader = ResolveShader();
            if (shader == null)
                shader = Shader.Find("Hidden/InternalErrorShader");

            var mat = new Material(shader);
            ApplyColor(mat, color);
            return mat;
        }

        public static Shader ResolveShader()
        {
            if (_cachedShader != null)
                return _cachedShader;

            // If no SRP is active, prefer Built-in shaders so we never pink-out waiting on URP.
            var pipelineActive = GraphicsSettings.currentRenderPipeline != null
                                 || GraphicsSettings.defaultRenderPipeline != null;
            var candidates = pipelineActive ? UrpFirst : BuiltinFirst;

            foreach (var name in candidates)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                    continue;

                _cachedShader = shader;
                _cachedShaderName = name;

                if (!_loggedChoice)
                {
                    _loggedChoice = true;
                    if (name == "Hidden/InternalErrorShader")
                        Debug.LogError("[WarHoller] No usable surface shader found; materials will be pink until shaders import.");
                    else if (!name.StartsWith("Universal Render Pipeline"))
                        Debug.LogWarning($"[WarHoller] Using Built-in/fallback shader '{name}' (pipelineActive={pipelineActive}).");
                    else
                        Debug.Log($"[WarHoller] MaterialUtil using URP shader '{name}'.");
                }

                return shader;
            }

            Debug.LogError("[WarHoller] Shader.Find returned null for every candidate.");
            return null;
        }

        /// <summary>Drop cached shader so the next CreateColor re-probes after a pipeline assign.</summary>
        public static void InvalidateCache()
        {
            _cachedShader = null;
            _cachedShaderName = null;
            _loggedChoice = false;
        }

        public static string CachedShaderName => _cachedShaderName;

        static void ApplyColor(Material mat, Color color)
        {
            if (mat == null) return;

            // Write both property names so the material works on URP *and* Built-in.
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            try { mat.color = color; }
            catch (System.Exception) { /* some shaders reject material.color */ }
        }
    }
}
