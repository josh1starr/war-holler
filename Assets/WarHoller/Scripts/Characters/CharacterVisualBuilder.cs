using UnityEngine;
using WarHoller.Art;

namespace WarHoller.Characters
{
    /// <summary>
    /// Builds low-poly mannequin bodies from primitives (menu + match).
    /// </summary>
    public static class CharacterVisualBuilder
    {
        public static GameObject Build(CharacterDef def, Transform parent, bool includeCollider)
        {
            var root = new GameObject(def.DisplayName);
            if (parent != null) root.transform.SetParent(parent, false);

            var bodyMat = MakeMat(def.PrimaryColor);
            var accentMat = MakeMat(def.AccentColor);
            var skinMat = MakeMat(new Color(0.86f, 0.7f, 0.55f));
            var hairMat = MakeMat(def.IsParody
                ? new Color(0.25f, 0.2f, 0.15f)
                : new Color(0.15f, 0.1f, 0.08f));

            // Torso
            var torso = Prim(PrimitiveType.Capsule, root.transform, new Vector3(0f, 1.0f, 0f),
                new Vector3(0.55f, 0.45f, 0.35f), bodyMat, "Torso");
            // Head
            Prim(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.65f, 0f),
                Vector3.one * 0.35f, skinMat, "Head");
            // Legs
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(-0.15f, 0.4f, 0f),
                new Vector3(0.2f, 0.4f, 0.2f), accentMat, "LegL");
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(0.15f, 0.4f, 0f),
                new Vector3(0.2f, 0.4f, 0.2f), accentMat, "LegR");
            // Arms
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(-0.45f, 1.15f, 0f),
                new Vector3(0.15f, 0.35f, 0.15f), bodyMat, "ArmL");
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(0.45f, 1.15f, 0f),
                new Vector3(0.15f, 0.35f, 0.15f), bodyMat, "ArmR");

            // Hair / beanie
            var hairScaleY = Mathf.Lerp(0.08f, 0.55f, def.HairLength);
            Prim(PrimitiveType.Sphere, root.transform,
                new Vector3(0f, 1.75f + hairScaleY * 0.2f, -0.02f),
                new Vector3(0.32f, hairScaleY, 0.32f), hairMat, "Hair");

            if (def.HasHeadset)
            {
                var mic = Prim(PrimitiveType.Cube, root.transform, new Vector3(0.22f, 1.62f, 0.05f),
                    new Vector3(0.08f, 0.08f, 0.18f), MakeMat(Color.black), "HeadsetMic");
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0.28f, 1.68f, 0f),
                    new Vector3(0.12f, 0.05f, 0.12f), MakeMat(new Color(0.1f, 0.1f, 0.1f)), "HeadsetCup");
            }

            if (def.IsFemale)
            {
                // Simple skirt / dress cue
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.75f, 0f),
                    new Vector3(0.7f, 0.35f, 0.45f), accentMat, "Skirt");
            }

            // Strip primitive colliders; combatants use CharacterController only.
            // Use Destroy (not DestroyImmediate) so this is safe at runtime in builds / Play Mode.
            foreach (var col in root.GetComponentsInChildren<Collider>())
                Object.Destroy(col);

            if (includeCollider)
            {
                var cc = root.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.35f;
                cc.center = new Vector3(0f, 0.9f, 0f);

                // Child hitbox so CharacterController isn't fighting another collider on root.
                var hitGo = new GameObject("Hitbox");
                hitGo.transform.SetParent(root.transform, false);
                var hit = hitGo.AddComponent<CapsuleCollider>();
                hit.height = 1.8f;
                hit.radius = 0.35f;
                hit.center = new Vector3(0f, 0.9f, 0f);
            }

            // Silence unused
            _ = torso;
            return root;
        }

        static GameObject Prim(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Material mat, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
            return go;
        }

        static Material MakeMat(Color c) => MaterialUtil.CreateColor(c);
    }
}
