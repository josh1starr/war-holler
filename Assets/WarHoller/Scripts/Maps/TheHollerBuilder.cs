using UnityEngine;

namespace WarHoller.Maps
{
    /// <summary>
    /// Procedural "The Holler": denser woods, stream banks, shacks, rust cars/appliances, litter.
    /// Spawns, cover, Hatfield/McCoy bases, three zone pads.
    /// </summary>
    public class TheHollerBuilder : MapLayout
    {
        Material _dirt;
        Material _grass;
        Material _grassDark;
        Material _wood;
        Material _woodDark;
        Material _water;
        Material _rust;
        Material _metal;
        Material _tin;
        Material _hay;

        public void Build()
        {
            DisplayName = "The Holler";
            EnsureMats();
            SpawnPoints.Clear();
            CoverPoints.Clear();
            HatfieldSpawns.Clear();
            McCoySpawns.Clear();
            Zones.Clear();

            var root = new GameObject("TheHoller");
            root.transform.SetParent(transform, false);

            BuildTerrain(root.transform);
            BuildStream(root.transform);
            BuildShacks(root.transform);
            BuildClutter(root.transform);
            BuildLitter(root.transform);
            BuildTrees(root.transform);
            BuildBasesAndZones(root.transform);
            BuildSpawnsAndCover();
            RegisterSpawnMarkers(root.transform);
            MapPrimitives.ApplyOutdoorLighting(transform, new Color(0.42f, 0.66f, 0.9f));
        }

        void EnsureMats()
        {
            _dirt = MapPrimitives.Mat(new Color(0.38f, 0.26f, 0.14f));
            _grass = MapPrimitives.Mat(new Color(0.26f, 0.4f, 0.16f));
            _grassDark = MapPrimitives.Mat(new Color(0.2f, 0.32f, 0.12f));
            _wood = MapPrimitives.Mat(new Color(0.42f, 0.28f, 0.14f));
            _woodDark = MapPrimitives.Mat(new Color(0.28f, 0.18f, 0.1f));
            _water = MapPrimitives.Mat(new Color(0.18f, 0.38f, 0.52f));
            _rust = MapPrimitives.Mat(new Color(0.48f, 0.26f, 0.12f));
            _metal = MapPrimitives.Mat(new Color(0.35f, 0.38f, 0.35f));
            _tin = MapPrimitives.Mat(new Color(0.45f, 0.42f, 0.38f));
            _hay = MapPrimitives.Mat(new Color(0.72f, 0.6f, 0.28f));
        }

        void BuildTerrain(Transform parent)
        {
            MapPrimitives.Box(parent, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(130f, 1f, 130f), _grass);
            // Patchy dirt near stream
            MapPrimitives.Box(parent, "DirtPatch", new Vector3(0f, -0.42f, 0f), new Vector3(18f, 0.9f, 100f), _dirt);
            MapPrimitives.Box(parent, "HillWest", new Vector3(-48f, 5f, 0f), new Vector3(22f, 12f, 110f), _dirt);
            MapPrimitives.Box(parent, "HillEast", new Vector3(48f, 5f, 0f), new Vector3(22f, 12f, 110f), _dirt);
            MapPrimitives.Box(parent, "HillNorth", new Vector3(0f, 4f, 52f), new Vector3(100f, 10f, 18f), _grassDark);
            MapPrimitives.Box(parent, "HillSouth", new Vector3(0f, 4f, -52f), new Vector3(100f, 10f, 18f), _grassDark);
            // Ridges for silhouette
            MapPrimitives.Box(parent, "RidgeNW", new Vector3(-30f, 2.5f, 30f), new Vector3(14f, 5f, 10f), _dirt);
            MapPrimitives.Box(parent, "RidgeSE", new Vector3(28f, 2.2f, -28f), new Vector3(12f, 4.5f, 10f), _dirt);
        }

        void BuildStream(Transform parent)
        {
            MapPrimitives.Box(parent, "Stream", new Vector3(0f, 0.05f, 0f), new Vector3(5.5f, 0.18f, 95f), _water);
            for (var z = -42; z <= 42; z += 6)
            {
                var wobble = Mathf.Sin(z * 0.2f) * 0.6f;
                MapPrimitives.Box(parent, "BankL_" + z, new Vector3(-4.2f + wobble, 0.22f, z),
                    new Vector3(2.4f, 0.45f, 2.8f), _dirt);
                MapPrimitives.Box(parent, "BankR_" + z, new Vector3(4.2f - wobble, 0.22f, z),
                    new Vector3(2.4f, 0.45f, 2.8f), _dirt);
                // Rocks
                MapPrimitives.Sphere(parent, "Rock_" + z, new Vector3(Random.Range(-2.5f, 2.5f), 0.15f, z + 1.5f),
                    Vector3.one * Random.Range(0.4f, 0.9f), _metal);
            }
        }

        void BuildShacks(Transform parent)
        {
            PlaceShack(parent, "ShackA", new Vector3(-18f, 0f, -16f), 25f);
            PlaceShack(parent, "ShackB", new Vector3(20f, 0f, 12f), -40f);
            PlaceShack(parent, "ShackC", new Vector3(-12f, 0f, 22f), 110f);
            PlaceShack(parent, "ShackD", new Vector3(16f, 0f, -24f), 70f);
            PlaceShack(parent, "ShackE", new Vector3(-26f, 0f, 8f), -15f);
            PlaceShack(parent, "ShackF", new Vector3(24f, 0f, -8f), 155f);
            PlaceOuthouse(parent, new Vector3(-8f, 0f, -22f), 20f);
            PlaceOuthouse(parent, new Vector3(12f, 0f, 26f), -30f);
        }

        void PlaceShack(Transform parent, string name, Vector3 pos, float yaw)
        {
            var shack = new GameObject(name);
            shack.transform.SetParent(parent, false);
            shack.transform.position = pos;
            shack.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            MapPrimitives.Box(shack.transform, "Floor", new Vector3(0f, 0.1f, 0f), new Vector3(8f, 0.2f, 6f), _wood);
            MapPrimitives.Box(shack.transform, "WallN", new Vector3(0f, 1.5f, 3f), new Vector3(8f, 3f, 0.3f), _wood);
            MapPrimitives.Box(shack.transform, "WallS", new Vector3(0f, 1.5f, -3f), new Vector3(8f, 3f, 0.3f), _woodDark);
            MapPrimitives.Box(shack.transform, "WallW", new Vector3(-4f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f), _wood);
            MapPrimitives.Box(shack.transform, "WallE", new Vector3(4f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f), _wood);
            MapPrimitives.Box(shack.transform, "Roof", new Vector3(0f, 3.2f, 0f), new Vector3(9f, 0.25f, 7f), _tin);
            MapPrimitives.Box(shack.transform, "Porch", new Vector3(0f, 0.15f, -4f), new Vector3(4f, 0.2f, 2f), _wood);
            MapPrimitives.Box(shack.transform, "Chimney", new Vector3(2.5f, 3.8f, 1f), new Vector3(0.7f, 1.4f, 0.7f), _rust);

            CoverPoints.Add(pos + shack.transform.TransformDirection(new Vector3(0f, 0f, -5f)));
            CoverPoints.Add(pos + shack.transform.TransformDirection(new Vector3(5f, 0f, 0f)));
        }

        void PlaceOuthouse(Transform parent, Vector3 pos, float yaw)
        {
            var o = new GameObject("Outhouse");
            o.transform.SetParent(parent, false);
            o.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            MapPrimitives.Box(o.transform, "Body", new Vector3(0f, 1f, 0f), new Vector3(1.4f, 2f, 1.4f), _woodDark);
            MapPrimitives.Box(o.transform, "Roof", new Vector3(0f, 2.2f, 0f), new Vector3(1.7f, 0.2f, 1.7f), _tin);
            CoverPoints.Add(pos);
        }

        void BuildClutter(Transform parent)
        {
            MapPrimitives.PlaceRustCar(parent, new Vector3(-8f, 0.6f, -8f), 30f, CoverPoints);
            MapPrimitives.PlaceRustCar(parent, new Vector3(10f, 0.6f, 18f), -15f, CoverPoints);
            MapPrimitives.PlaceRustCar(parent, new Vector3(5f, 0.6f, -30f), 80f, CoverPoints);
            MapPrimitives.PlaceRustCar(parent, new Vector3(-22f, 0.6f, -28f), 120f, CoverPoints);
            MapPrimitives.PlaceTruck(parent, new Vector3(18f, 0.7f, 28f), -55f, CoverPoints);
            MapPrimitives.PlaceTruck(parent, new Vector3(-30f, 0.7f, 18f), 40f, CoverPoints);

            for (var i = 0; i < 22; i++)
            {
                var p = new Vector3(Random.Range(-34f, 34f), 0.5f, Random.Range(-38f, 38f));
                if (Mathf.Abs(p.x) < 5f) p.x += 7f * Mathf.Sign(p.x == 0 ? 1 : p.x);
                var kind = i % 4;
                if (kind == 0)
                    MapPrimitives.Box(parent, "Fridge_" + i, p, new Vector3(1.1f, 2f, 1.1f), _metal);
                else if (kind == 1)
                    MapPrimitives.Cyl(parent, "Barrel_" + i, p, new Vector3(0.7f, 0.6f, 0.7f), _rust);
                else if (kind == 2)
                    MapPrimitives.Box(parent, "Washer_" + i, p, new Vector3(1.2f, 1.2f, 1.0f), _tin);
                else
                    MapPrimitives.Box(parent, "Hay_" + i, p, new Vector3(1.5f, 0.8f, 1f), _hay);
                CoverPoints.Add(p);
            }
        }

        void BuildLitter(Transform parent)
        {
            var litterColors = new[]
            {
                new Color(0.55f, 0.5f, 0.35f),
                new Color(0.3f, 0.35f, 0.25f),
                new Color(0.5f, 0.2f, 0.15f),
                new Color(0.4f, 0.4f, 0.42f)
            };
            for (var i = 0; i < 40; i++)
            {
                var p = new Vector3(Random.Range(-40f, 40f), 0.08f, Random.Range(-42f, 42f));
                if (Mathf.Abs(p.x) < 3f) continue;
                var mat = MapPrimitives.Mat(litterColors[i % litterColors.Length]);
                MapPrimitives.Box(parent, "Litter_" + i, p,
                    new Vector3(Random.Range(0.2f, 0.7f), 0.08f, Random.Range(0.2f, 0.5f)), mat);
            }
        }

        void BuildTrees(Transform parent)
        {
            var rng = new System.Random(42);
            for (var i = 0; i < 110; i++)
            {
                var x = (float)(rng.NextDouble() * 100 - 50);
                var z = (float)(rng.NextDouble() * 100 - 50);
                if (Mathf.Abs(x) < 7f && Mathf.Abs(z) < 45f) continue;
                if (Mathf.Abs(x) > 42f || Mathf.Abs(z) > 45f) { /* edge dense OK */ }
                var tree = new GameObject("Tree_" + i);
                tree.transform.SetParent(parent, false);
                tree.transform.position = new Vector3(x, 0f, z);
                var trunkH = 1.3f + (float)rng.NextDouble() * 0.8f;
                MapPrimitives.Cyl(tree.transform, "Trunk", new Vector3(0f, trunkH, 0f),
                    new Vector3(0.28f + (float)rng.NextDouble() * 0.15f, trunkH, 0.28f), _woodDark);
                var canopyScale = 2.0f + (float)rng.NextDouble() * 1.6f;
                var leafMat = rng.NextDouble() > 0.5 ? _grass : _grassDark;
                MapPrimitives.Sphere(tree.transform, "Canopy", new Vector3(0f, trunkH * 2f + 0.4f, 0f),
                    Vector3.one * canopyScale, leafMat);
                if (rng.NextDouble() > 0.55)
                    MapPrimitives.Sphere(tree.transform, "Canopy2",
                        new Vector3((float)rng.NextDouble() - 0.5f, trunkH * 2f + 0.1f, (float)rng.NextDouble() - 0.5f),
                        Vector3.one * (canopyScale * 0.7f), leafMat);
            }

            // Deadfall logs
            for (var i = 0; i < 8; i++)
            {
                var p = new Vector3(Random.Range(-30f, 30f), 0.25f, Random.Range(-30f, 30f));
                if (Mathf.Abs(p.x) < 5f) p.x += 8f;
                var log = MapPrimitives.Cyl(parent, "Log_" + i, p, new Vector3(0.35f, 1.8f, 0.35f), _woodDark);
                log.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 90f);
                CoverPoints.Add(p);
            }
        }

        void BuildBasesAndZones(Transform parent)
        {
            HatfieldBase = new Vector3(-32f, 0.2f, 0f);
            McCoyBase = new Vector3(32f, 0.2f, 0f);
            BaseCaptureRadius = 6.5f;

            // Visual lean-tos at bases
            MapPrimitives.Box(parent, "HatCamp", HatfieldBase + new Vector3(0f, 0.5f, 0f),
                new Vector3(4f, 0.2f, 4f), _wood);
            MapPrimitives.Box(parent, "McCamp", McCoyBase + new Vector3(0f, 0.5f, 0f),
                new Vector3(4f, 0.2f, 4f), _wood);

            Zones.Add(new ZoneDef { Name = "Creek Bend", Center = new Vector3(-14f, 0.05f, -12f), Radius = 8f });
            Zones.Add(new ZoneDef { Name = "Crossin'", Center = new Vector3(0f, 0.05f, 14f), Radius = 8.5f });
            Zones.Add(new ZoneDef { Name = "East Ridge", Center = new Vector3(16f, 0.05f, -8f), Radius = 8f });
        }

        void BuildSpawnsAndCover()
        {
            var hat = new[]
            {
                new Vector3(-30f, 0.2f, -8f), new Vector3(-28f, 0.2f, 8f), new Vector3(-34f, 0.2f, 0f),
                new Vector3(-22f, 0.2f, -20f), new Vector3(-24f, 0.2f, 18f), new Vector3(-18f, 0.2f, 4f)
            };
            var mc = new[]
            {
                new Vector3(30f, 0.2f, -8f), new Vector3(28f, 0.2f, 8f), new Vector3(34f, 0.2f, 0f),
                new Vector3(22f, 0.2f, -20f), new Vector3(24f, 0.2f, 18f), new Vector3(18f, 0.2f, 4f)
            };
            HatfieldSpawns.AddRange(hat);
            McCoySpawns.AddRange(mc);

            SpawnPoints.AddRange(hat);
            SpawnPoints.AddRange(mc);
            SpawnPoints.AddRange(new[]
            {
                new Vector3(0f, 0.2f, -32f), new Vector3(0f, 0.2f, 32f),
                new Vector3(-14f, 0.2f, -8f), new Vector3(14f, 0.2f, 8f)
            });
        }
    }
}
