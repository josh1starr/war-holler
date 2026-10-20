using UnityEngine;
using WarHoller.Art;

namespace WarHoller.Maps
{
    /// <summary>
    /// Full-bleed stylized barn interior for the main menu: wood walls, hay, light shafts.
    /// </summary>
    public class BarnBackdropBuilder : MonoBehaviour
    {
        public Camera MenuCamera { get; private set; }

        public void Build()
        {
            var root = new GameObject("BarnInterior");
            root.transform.SetParent(transform, false);

            var wood = Mat(new Color(0.38f, 0.24f, 0.12f));
            var darkWood = Mat(new Color(0.22f, 0.14f, 0.08f));
            var hay = Mat(new Color(0.72f, 0.62f, 0.28f));
            var dirt = Mat(new Color(0.3f, 0.22f, 0.12f));

            Box(root.transform, "Floor", new Vector3(0f, 0f, 0f), new Vector3(24f, 0.3f, 18f), dirt);
            Box(root.transform, "BackWall", new Vector3(0f, 4f, 8f), new Vector3(24f, 8f, 0.4f), wood);
            Box(root.transform, "LeftWall", new Vector3(-12f, 4f, 0f), new Vector3(0.4f, 8f, 18f), wood);
            Box(root.transform, "RightWall", new Vector3(12f, 4f, 0f), new Vector3(0.4f, 8f, 18f), wood);
            Box(root.transform, "Roof", new Vector3(0f, 8.2f, 0f), new Vector3(26f, 0.35f, 20f), darkWood);

            // Hay bales
            for (var i = 0; i < 8; i++)
            {
                var x = -8f + (i % 4) * 5f;
                var z = 4f + (i / 4) * 2.5f;
                Box(root.transform, "Hay_" + i, new Vector3(x, 0.7f, z), new Vector3(2.2f, 1.2f, 1.4f), hay);
            }

            // Beams
            for (var i = 0; i < 5; i++)
            {
                Box(root.transform, "Beam_" + i, new Vector3(-8f + i * 4f, 7f, 0f),
                    new Vector3(0.4f, 0.4f, 16f), darkWood);
            }

            // Light shafts (emissive-ish bright quads + spot lights)
            for (var i = 0; i < 3; i++)
            {
                var shaft = Box(root.transform, "LightShaft_" + i,
                    new Vector3(-4f + i * 4f, 5f, 2f),
                    new Vector3(0.8f, 6f, 0.8f),
                    Mat(new Color(1f, 0.95f, 0.7f, 0.15f)));
                // Soften: no collider needed for visuals
                Object.Destroy(shaft.GetComponent<Collider>());

                var spotGo = new GameObject("ShaftLight_" + i);
                spotGo.transform.SetParent(root.transform, false);
                spotGo.transform.position = new Vector3(-4f + i * 4f, 7.5f, 3f);
                spotGo.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
                var spot = spotGo.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(1f, 0.92f, 0.7f);
                spot.intensity = 2.2f;
                spot.range = 18f;
                spot.spotAngle = 35f;
            }

            var sun = new GameObject("BarnFillLight");
            sun.transform.SetParent(root.transform, false);
            sun.transform.rotation = Quaternion.Euler(35f, -20f, 0f);
            var fill = sun.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.55f;
            fill.color = new Color(1f, 0.9f, 0.75f);

            var camGo = new GameObject("MenuCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.position = new Vector3(0f, 2.2f, -7.5f);
            camGo.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
            MenuCamera = camGo.AddComponent<Camera>();
            MenuCamera.clearFlags = CameraClearFlags.SolidColor;
            MenuCamera.backgroundColor = new Color(0.12f, 0.08f, 0.05f);
            MenuCamera.fieldOfView = 50f;
            camGo.AddComponent<AudioListener>();

            RenderSettings.ambientLight = new Color(0.35f, 0.28f, 0.2f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.25f, 0.18f, 0.1f);
            RenderSettings.fogDensity = 0.03f;
        }

        static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        static Material Mat(Color c) => MaterialUtil.CreateColor(c);
    }
}
