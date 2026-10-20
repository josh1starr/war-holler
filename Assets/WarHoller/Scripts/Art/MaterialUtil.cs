using UnityEngine;

namespace WarHoller.Art
{
    /// <summary>
    /// Runtime material factory for Built-in Render Pipeline only.
    /// Tries Standard → Unlit/Color → Sprites/Default → Legacy Shaders/Diffuse; sets _Color.
    /// </summary>
    public static class MaterialUtil
    {
        static readonly string[] BuiltinShaders =
        {
            "Standard",
            "Unlit/Color",
            "Sprites/Default",
            "Legacy Shaders/Diffuse"
        };

        static Shader _cachedShader;
        static string _cachedShaderName;
        static bool _loggedChoice;

        /// <summary>Solid-color material using a Built-in pipeline shader.</summary>
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

            foreach (var name in BuiltinShaders)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                    continue;

                _cachedShader = shader;
                _cachedShaderName = name;

                if (!_loggedChoice)
                {
                    _loggedChoice = true;
                    Debug.Log($"[WarHoller] MaterialUtil using Built-in shader '{name}'.");
                }

                return shader;
            }

            Debug.LogError(
                "[WarHoller] No Built-in surface shader found (Standard / Unlit/Color / Sprites/Default / Legacy Diffuse). " +
                "Materials will be pink until shaders import — quit Unity, delete Library/, reopen.");
            return null;
        }

        /// <summary>Drop cached shader so the next CreateColor re-probes.</summary>
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

            mat.SetColor("_Color", color);
            try { mat.color = color; }
            catch (System.Exception) { /* some shaders reject material.color */ }
        }
    }
}
