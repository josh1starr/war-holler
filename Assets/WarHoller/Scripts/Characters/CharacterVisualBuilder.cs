using UnityEngine;
using WarHoller.Art;

namespace WarHoller.Characters
{
    /// <summary>
    /// Low-poly early-1900s Appalachian/country mannequins: hats, suspenders, dresses, boots.
    /// Asmon parody keeps the labeled headset gag.
    /// </summary>
    public static class CharacterVisualBuilder
    {
        public static GameObject Build(CharacterDef def, Transform parent, bool includeCollider)
        {
            var root = new GameObject(def.DisplayName);
            if (parent != null) root.transform.SetParent(parent, false);

            var cloth = MakeMat(def.PrimaryColor);
            var accent = MakeMat(def.AccentColor);
            var skin = MakeMat(new Color(0.82f, 0.66f, 0.5f));
            var hair = MakeMat(def.IsParody
                ? new Color(0.28f, 0.22f, 0.16f)
                : new Color(0.12f, 0.09f, 0.06f));
            var leather = MakeMat(new Color(0.28f, 0.16f, 0.08f));
            var boot = MakeMat(new Color(0.18f, 0.12f, 0.08f));
            var hatFelt = MakeMat(new Color(0.32f, 0.26f, 0.18f));
            var strap = MakeMat(new Color(0.2f, 0.14f, 0.08f));

            // Torso — period shirt / blouse
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(0f, 1.05f, 0f),
                new Vector3(0.52f, 0.42f, 0.32f), cloth, "Torso");

            // Head
            Prim(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.65f, 0f),
                Vector3.one * 0.34f, skin, "Head");

            // Trousers / long dress
            if (def.IsFemale)
            {
                // Floor-length skirt silhouette
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.55f, 0f),
                    new Vector3(0.85f, 1.0f, 0.55f), accent, "LongSkirt");
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.05f, 0f),
                    new Vector3(0.72f, 0.35f, 0.42f), cloth, "Bodice");
                // Apron
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.7f, 0.22f),
                    new Vector3(0.45f, 0.7f, 0.05f), MakeMat(new Color(0.78f, 0.72f, 0.58f)), "Apron");
            }
            else
            {
                Prim(PrimitiveType.Capsule, root.transform, new Vector3(-0.14f, 0.42f, 0f),
                    new Vector3(0.18f, 0.38f, 0.18f), accent, "LegL");
                Prim(PrimitiveType.Capsule, root.transform, new Vector3(0.14f, 0.42f, 0f),
                    new Vector3(0.18f, 0.38f, 0.18f), accent, "LegR");
                // Suspenders
                Prim(PrimitiveType.Cube, root.transform, new Vector3(-0.12f, 1.15f, 0.12f),
                    new Vector3(0.06f, 0.55f, 0.04f), strap, "SuspenderL");
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0.12f, 1.15f, 0.12f),
                    new Vector3(0.06f, 0.55f, 0.04f), strap, "SuspenderR");
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0f, 0.85f, 0.12f),
                    new Vector3(0.35f, 0.05f, 0.04f), strap, "SuspenderJoin");
            }

            // Arms
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(-0.42f, 1.12f, 0f),
                new Vector3(0.13f, 0.32f, 0.13f), cloth, "ArmL");
            Prim(PrimitiveType.Capsule, root.transform, new Vector3(0.42f, 1.12f, 0f),
                new Vector3(0.13f, 0.32f, 0.13f), cloth, "ArmR");

            // Boots
            Prim(PrimitiveType.Cube, root.transform, new Vector3(-0.14f, 0.08f, 0.05f),
                new Vector3(0.2f, 0.16f, 0.32f), boot, "BootL");
            Prim(PrimitiveType.Cube, root.transform, new Vector3(0.14f, 0.08f, 0.05f),
                new Vector3(0.2f, 0.16f, 0.32f), boot, "BootR");

            // Hair
            var hairScaleY = Mathf.Lerp(0.08f, 0.55f, def.HairLength);
            Prim(PrimitiveType.Sphere, root.transform,
                new Vector3(0f, 1.74f + hairScaleY * 0.15f, -0.02f),
                new Vector3(0.3f, hairScaleY, 0.3f), hair, "Hair");

            if (def.IsFemale)
            {
                // Bun / pinned hair
                Prim(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.78f, -0.12f),
                    new Vector3(0.22f, 0.18f, 0.22f), hair, "Bun");
                // Bonnet / wide brim for ladies
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.88f, 0f),
                    new Vector3(0.55f, 0.06f, 0.55f), MakeMat(new Color(0.75f, 0.7f, 0.55f)), "BonnetBrim");
                Prim(PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.92f, 0f),
                    new Vector3(0.28f, 0.16f, 0.28f), MakeMat(new Color(0.7f, 0.65f, 0.5f)), "BonnetCrown");
            }
            else if (!def.IsParody)
            {
                // Broad country hat
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.86f, 0f),
                    new Vector3(0.62f, 0.05f, 0.62f), hatFelt, "HatBrim");
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.98f, 0f),
                    new Vector3(0.28f, 0.14f, 0.28f), hatFelt, "HatCrown");
            }

            if (def.HasHeadset)
            {
                // Keep modern headset gag on Asmon only
                Prim(PrimitiveType.Cube, root.transform, new Vector3(0.22f, 1.62f, 0.05f),
                    new Vector3(0.08f, 0.08f, 0.18f), MakeMat(Color.black), "HeadsetMic");
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0.28f, 1.68f, 0f),
                    new Vector3(0.12f, 0.05f, 0.12f), MakeMat(new Color(0.1f, 0.1f, 0.1f)), "HeadsetCup");
                // Still give him a battered hat under the gag
                Prim(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 1.9f, 0f),
                    new Vector3(0.5f, 0.04f, 0.5f), leather, "BatteredBrim");
            }

            foreach (var col in root.GetComponentsInChildren<Collider>())
                Object.Destroy(col);

            if (includeCollider)
            {
                var cc = root.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.35f;
                cc.center = new Vector3(0f, 0.9f, 0f);

                var hitGo = new GameObject("Hitbox");
                hitGo.transform.SetParent(root.transform, false);
                var hit = hitGo.AddComponent<CapsuleCollider>();
                hit.height = 1.8f;
                hit.radius = 0.35f;
                hit.center = new Vector3(0f, 0.9f, 0f);
            }

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
