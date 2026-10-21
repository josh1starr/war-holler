using UnityEngine;
using WarHoller.Art;

namespace WarHoller.Maps
{
    /// <summary>Shared primitive helpers for procedural map builders (Built-in RP only).</summary>
    public static class MapPrimitives
    {
        public static Material Mat(Color c) => MaterialUtil.CreateColor(c);

        public static GameObject Box(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        public static GameObject Cyl(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        public static void ApplyOutdoorLighting(Transform parent, Color skyClear)
        {
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var sun = new GameObject("Sun");
                sun.transform.SetParent(parent, false);
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.intensity = 1.15f;
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

            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (cam == null || !cam.enabled) continue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = skyClear;
            }
        }

        public static void ApplyIndoorLighting(Transform parent, Color clear, float intensity = 0.85f)
        {
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var sun = new GameObject("KeyLight");
                sun.transform.SetParent(parent, false);
                sun.transform.rotation = Quaternion.Euler(40f, 25f, 0f);
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.92f, 0.8f);
                light.intensity = intensity;
                light.shadows = LightShadows.Soft;
            }

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.32f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.25f, 0.22f, 0.18f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;

            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (cam == null || !cam.enabled) continue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = clear;
            }
        }

        public static void PlaceRustCar(Transform parent, Vector3 pos, float yaw, System.Collections.Generic.List<Vector3> cover)
        {
            var rust = Mat(new Color(0.45f, 0.25f, 0.12f));
            var metal = Mat(new Color(0.32f, 0.34f, 0.32f));
            var car = new GameObject("RustCar");
            car.transform.SetParent(parent, false);
            car.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            Box(car.transform, "Body", Vector3.zero, new Vector3(2.2f, 1.0f, 4.5f), rust);
            Box(car.transform, "Cabin", new Vector3(0f, 0.7f, -0.3f), new Vector3(2f, 0.8f, 2.2f), rust);
            Cyl(car.transform, "WheelFL", new Vector3(-1.1f, -0.4f, 1.4f), new Vector3(0.35f, 0.2f, 0.35f), metal);
            Cyl(car.transform, "WheelFR", new Vector3(1.1f, -0.4f, 1.4f), new Vector3(0.35f, 0.2f, 0.35f), metal);
            Cyl(car.transform, "WheelRL", new Vector3(-1.1f, -0.4f, -1.4f), new Vector3(0.35f, 0.2f, 0.35f), metal);
            Cyl(car.transform, "WheelRR", new Vector3(1.1f, -0.4f, -1.4f), new Vector3(0.35f, 0.2f, 0.35f), metal);
            cover?.Add(pos);
        }

        public static void PlaceTruck(Transform parent, Vector3 pos, float yaw, System.Collections.Generic.List<Vector3> cover)
        {
            var body = Mat(new Color(0.4f, 0.22f, 0.1f));
            var metal = Mat(new Color(0.3f, 0.3f, 0.28f));
            var truck = new GameObject("Truck");
            truck.transform.SetParent(parent, false);
            truck.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            Box(truck.transform, "Cab", new Vector3(0f, 0.9f, 1.6f), new Vector3(2.4f, 1.6f, 2.2f), body);
            Box(truck.transform, "Bed", new Vector3(0f, 0.7f, -1.4f), new Vector3(2.3f, 0.9f, 3.4f), metal);
            Cyl(truck.transform, "WFL", new Vector3(-1.2f, -0.2f, 1.8f), new Vector3(0.4f, 0.22f, 0.4f), metal);
            Cyl(truck.transform, "WFR", new Vector3(1.2f, -0.2f, 1.8f), new Vector3(0.4f, 0.22f, 0.4f), metal);
            Cyl(truck.transform, "WRL", new Vector3(-1.2f, -0.2f, -1.8f), new Vector3(0.4f, 0.22f, 0.4f), metal);
            Cyl(truck.transform, "WRR", new Vector3(1.2f, -0.2f, -1.8f), new Vector3(0.4f, 0.22f, 0.4f), metal);
            cover?.Add(pos);
        }
    }
}
