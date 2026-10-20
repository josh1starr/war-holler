using System.Collections.Generic;
using UnityEngine;
using WarHoller.Art;

namespace WarHoller.Maps
{
    /// <summary>
    /// Procedural "The Holler": wooded valley, stream, shacks, rusted cars, country clutter.
    /// Spawn points + cover for bots. Extension points for WowMart / MethLab builders.
    /// </summary>
    public class TheHollerBuilder : MonoBehaviour
    {
        public readonly List<Vector3> SpawnPoints = new List<Vector3>();
        public readonly List<Vector3> CoverPoints = new List<Vector3>();

        Material _dirt;
        Material _grass;
        Material _wood;
        Material _water;
        Material _rust;
        Material _metal;

        public void Build()
        {
            EnsureMats();
            var root = new GameObject("TheHoller");
            root.transform.SetParent(transform, false);

            BuildTerrain(root.transform);
            BuildStream(root.transform);
            BuildShacks(root.transform);
            BuildClutter(root.transform);
            BuildTrees(root.transform);
            BuildSpawnsAndCover();
            BuildLighting();
        }

        void EnsureMats()
        {
            _dirt = Mat(new Color(0.35f, 0.25f, 0.14f));
            _grass = Mat(new Color(0.28f, 0.42f, 0.18f));
            _wood = Mat(new Color(0.42f, 0.28f, 0.14f));
            _water = Mat(new Color(0.2f, 0.4f, 0.55f));
            _rust = Mat(new Color(0.45f, 0.25f, 0.12f));
            _metal = Mat(new Color(0.35f, 0.38f, 0.35f));
        }

        void BuildTerrain(Transform parent)
        {
            // Valley floor
            Box(parent, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(120f, 1f, 120f), _grass);
            // Hills / valley sides
            Box(parent, "HillWest", new Vector3(-45f, 4f, 0f), new Vector3(20f, 10f, 100f), _dirt);
            Box(parent, "HillEast", new Vector3(45f, 4f, 0f), new Vector3(20f, 10f, 100f), _dirt);
            Box(parent, "HillNorth", new Vector3(0f, 3f, 48f), new Vector3(90f, 8f, 16f), _dirt);
            Box(parent, "HillSouth", new Vector3(0f, 3f, -48f), new Vector3(90f, 8f, 16f), _dirt);
        }

        void BuildStream(Transform parent)
        {
            Box(parent, "Stream", new Vector3(0f, 0.05f, 0f), new Vector3(6f, 0.2f, 90f), _water);
            // Stepping stones / banks
            for (var z = -40; z <= 40; z += 8)
            {
                Box(parent, "BankL_" + z, new Vector3(-4.5f, 0.2f, z), new Vector3(2f, 0.4f, 3f), _dirt);
                Box(parent, "BankR_" + z, new Vector3(4.5f, 0.2f, z), new Vector3(2f, 0.4f, 3f), _dirt);
            }
        }

        void BuildShacks(Transform parent)
        {
            PlaceShack(parent, "ShackA", new Vector3(-18f, 0f, -16f), 25f);
            PlaceShack(parent, "ShackB", new Vector3(20f, 0f, 12f), -40f);
            PlaceShack(parent, "ShackC", new Vector3(-12f, 0f, 22f), 110f);
            PlaceShack(parent, "ShackD", new Vector3(16f, 0f, -24f), 70f);
        }

        void PlaceShack(Transform parent, string name, Vector3 pos, float yaw)
        {
            var shack = new GameObject(name);
            shack.transform.SetParent(parent, false);
            shack.transform.position = pos;
            shack.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Box(shack.transform, "Floor", new Vector3(0f, 0.1f, 0f), new Vector3(8f, 0.2f, 6f), _wood);
            Box(shack.transform, "WallN", new Vector3(0f, 1.5f, 3f), new Vector3(8f, 3f, 0.3f), _wood);
            Box(shack.transform, "WallS", new Vector3(0f, 1.5f, -3f), new Vector3(8f, 3f, 0.3f), _wood);
            Box(shack.transform, "WallW", new Vector3(-4f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f), _wood);
            Box(shack.transform, "WallE", new Vector3(4f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f), _wood);
            // Door gap: leave south wall intact but add porch cover outside
            Box(shack.transform, "Roof", new Vector3(0f, 3.2f, 0f), new Vector3(9f, 0.25f, 7f), _rust);
            Box(shack.transform, "Porch", new Vector3(0f, 0.15f, -4f), new Vector3(4f, 0.2f, 2f), _wood);

            CoverPoints.Add(pos + shack.transform.TransformDirection(new Vector3(0f, 0f, -5f)));
            CoverPoints.Add(pos + shack.transform.TransformDirection(new Vector3(5f, 0f, 0f)));
        }

        void BuildClutter(Transform parent)
        {
            // Rusted cars
            PlaceCar(parent, new Vector3(-8f, 0.6f, -8f), 30f);
            PlaceCar(parent, new Vector3(10f, 0.6f, 18f), -15f);
            PlaceCar(parent, new Vector3(5f, 0.6f, -30f), 80f);

            // Appliances / barrels / hay
            for (var i = 0; i < 14; i++)
            {
                var p = new Vector3(Random.Range(-30f, 30f), 0.5f, Random.Range(-35f, 35f));
                if (Mathf.Abs(p.x) < 4f) p.x += 6f * Mathf.Sign(p.x == 0 ? 1 : p.x);
                var kind = i % 3;
                if (kind == 0)
                    Box(parent, "Fridge_" + i, p, new Vector3(1.1f, 2f, 1.1f), _metal);
                else if (kind == 1)
                    Cyl(parent, "Barrel_" + i, p, new Vector3(0.7f, 0.6f, 0.7f), _rust);
                else
                    Box(parent, "Hay_" + i, p, new Vector3(1.5f, 0.8f, 1f), Mat(new Color(0.7f, 0.6f, 0.25f)));
                CoverPoints.Add(p);
            }
        }

        void PlaceCar(Transform parent, Vector3 pos, float yaw)
        {
            var car = new GameObject("RustCar");
            car.transform.SetParent(parent, false);
            car.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            Box(car.transform, "Body", Vector3.zero, new Vector3(2.2f, 1.0f, 4.5f), _rust);
            Box(car.transform, "Cabin", new Vector3(0f, 0.7f, -0.3f), new Vector3(2f, 0.8f, 2.2f), _rust);
            Cyl(car.transform, "WheelFL", new Vector3(-1.1f, -0.4f, 1.4f), new Vector3(0.35f, 0.2f, 0.35f), _metal);
            Cyl(car.transform, "WheelFR", new Vector3(1.1f, -0.4f, 1.4f), new Vector3(0.35f, 0.2f, 0.35f), _metal);
            Cyl(car.transform, "WheelRL", new Vector3(-1.1f, -0.4f, -1.4f), new Vector3(0.35f, 0.2f, 0.35f), _metal);
            Cyl(car.transform, "WheelRR", new Vector3(1.1f, -0.4f, -1.4f), new Vector3(0.35f, 0.2f, 0.35f), _metal);
            CoverPoints.Add(pos);
        }

        void BuildTrees(Transform parent)
        {
            var rng = new System.Random(42);
            for (var i = 0; i < 60; i++)
            {
                var x = (float)(rng.NextDouble() * 90 - 45);
                var z = (float)(rng.NextDouble() * 90 - 45);
                if (Mathf.Abs(x) < 8f && Mathf.Abs(z) < 40f) continue; // keep stream clear
                var tree = new GameObject("Tree_" + i);
                tree.transform.SetParent(parent, false);
                tree.transform.position = new Vector3(x, 0f, z);
                Cyl(tree.transform, "Trunk", new Vector3(0f, 1.5f, 0f), new Vector3(0.35f, 1.5f, 0.35f), _wood);
                var leaf = Prim(PrimitiveType.Sphere, tree.transform, new Vector3(0f, 3.5f, 0f),
                    Vector3.one * Random.Range(2.2f, 3.2f), _grass, "Canopy");
                _ = leaf;
            }
        }

        void BuildSpawnsAndCover()
        {
            SpawnPoints.Clear();
            var ring = new[]
            {
                new Vector3(-22f, 0.2f, -20f),
                new Vector3(22f, 0.2f, -18f),
                new Vector3(-24f, 0.2f, 18f),
                new Vector3(24f, 0.2f, 20f),
                new Vector3(0f, 0.2f, -32f),
                new Vector3(0f, 0.2f, 32f),
                new Vector3(-32f, 0.2f, 0f),
                new Vector3(32f, 0.2f, 0f),
                new Vector3(-14f, 0.2f, -8f),
                new Vector3(14f, 0.2f, 8f),
                new Vector3(-10f, 0.2f, 28f),
                new Vector3(12f, 0.2f, -28f)
            };
            SpawnPoints.AddRange(ring);

            foreach (var s in SpawnPoints)
            {
                var marker = new GameObject("Spawn");
                marker.tag = "Spawn";
                marker.transform.SetParent(transform, false);
                marker.transform.position = s;
            }
        }

        void BuildLighting()
        {
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var sun = new GameObject("Sun");
                sun.transform.SetParent(transform, false);
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.intensity = 1.1f;
                light.shadows = LightShadows.Soft;
            }

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.5f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.2f, 0.15f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.55f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;

            // Solid sky clear on every live camera — never depend on a skybox shader.
            var sky = new Color(0.45f, 0.68f, 0.92f);
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (cam == null || !cam.enabled) continue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = sky;
            }
        }

        static GameObject Box(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        static GameObject Cyl(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        static GameObject Prim(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Material mat, string name)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        static Material Mat(Color c) => MaterialUtil.CreateColor(c);

        // EXTENSION: WowMartBuilder / MethLabBuilder can mirror this pattern for LobbyRotation maps.
    }
}
