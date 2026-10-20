using UnityEngine;

namespace WarHoller.Art
{
    /// <summary>
    /// Runtime material factory with a URP-first shader fallback chain.
    /// Avoids pink/magenta when Lit is missing or the pipeline is still warming up.
    /// </summary>
    public static class MaterialUtil
    {
        static readonly string[] ShaderCandidates =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Unlit/Color",
            "Standard",
            "Hidden/InternalErrorShader"
        };

        static Shader _cachedShader;
        static string _cachedShaderName;

        /// <summary>Solid-color material using the best available URP/builtin shader.</summary>
        public static Material CreateColor(Color color)
        {
            var shader = ResolveShader();
            if (shader == null)
            {
                // Last-ditch: Unity always has this, even when surface shaders failed to load.
                shader = Shader.Find("Hidden/InternalErrorShader");
            }

            var mat = new Material(shader);
            ApplyColor(mat, color);
            return mat;
        }

        public static Shader ResolveShader()
        {
            if (_cachedShader != null)
                return _cachedShader;

            foreach (var name in ShaderCandidates)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                    continue;

                _cachedShader = shader;
                _cachedShaderName = name;

                if (name == "Hidden/InternalErrorShader")
                    Debug.LogError("[WarHoller] No usable surface shader found; materials will be pink until URP shaders import.");
                else if (name != "Universal Render Pipeline/Lit")
                    Debug.LogWarning($"[WarHoller] Using fallback shader '{name}' (URP Lit unavailable).");

                return shader;
            }

            Debug.LogError("[WarHoller] Shader.Find returned null for every candidate.");
            return null;
        }

        static void ApplyColor(Material mat, Color color)
        {
            if (mat == null) return;

            // Prefer URP/Lit base color; fall back to legacy _Color / material.color.
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            mat.color = color;
        }
    }
}
