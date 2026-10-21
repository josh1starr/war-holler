using UnityEngine;

namespace WarHoller.Maps
{
    /// <summary>
    /// Underground multi-room lab + corridors. Spawns, cover, CTF bases, 3 zones.
    /// </summary>
    public class MethLabBuilder : MapLayout
    {
        Material _concrete;
        Material _concreteDark;
        Material _tile;
        Material _metal;
        Material _pipe;
        Material _chem;
        Material _hazard;
        Material _wood;
        Material _rust;

        public void Build()
        {
            DisplayName = "Meth Lab";
            EnsureMats();
            SpawnPoints.Clear();
            CoverPoints.Clear();
            HatfieldSpawns.Clear();
            McCoySpawns.Clear();
            Zones.Clear();

            var root = new GameObject("MethLab");
            root.transform.SetParent(transform, false);

            // Ground slab so nothing falls into the void
            MapPrimitives.Box(root.transform, "Bedrock", new Vector3(0f, -0.6f, 0f),
                new Vector3(90f, 0.8f, 80f), _concreteDark);

            BuildHub(root.transform);
            BuildCookWest(root.transform);
            BuildStorageEast(root.transform);
            BuildDormNorth(root.transform);
            BuildBaySouth(root.transform);
            BuildSideRooms(root.transform);
            BuildCorridorFloors(root.transform);
            BuildProps(root.transform);
            BuildBasesAndZones(root.transform);
            BuildSpawns();
            RegisterSpawnMarkers(root.transform);
            MapPrimitives.ApplyIndoorLighting(transform, new Color(0.12f, 0.14f, 0.12f), 0.75f);

            PlacePointLight(root.transform, new Vector3(-18f, 3.5f, 0f), new Color(0.6f, 0.9f, 0.7f));
            PlacePointLight(root.transform, new Vector3(18f, 3.5f, 0f), new Color(0.9f, 0.7f, 0.4f));
            PlacePointLight(root.transform, new Vector3(0f, 3.5f, 16f), new Color(0.7f, 0.85f, 1f));
            PlacePointLight(root.transform, new Vector3(0f, 3.5f, -16f), new Color(0.85f, 0.6f, 0.5f));
        }

        void EnsureMats()
        {
            _concrete = MapPrimitives.Mat(new Color(0.4f, 0.4f, 0.38f));
            _concreteDark = MapPrimitives.Mat(new Color(0.28f, 0.28f, 0.26f));
            _tile = MapPrimitives.Mat(new Color(0.5f, 0.55f, 0.5f));
            _metal = MapPrimitives.Mat(new Color(0.45f, 0.48f, 0.5f));
            _pipe = MapPrimitives.Mat(new Color(0.35f, 0.55f, 0.4f));
            _chem = MapPrimitives.Mat(new Color(0.45f, 0.85f, 0.55f));
            _hazard = MapPrimitives.Mat(new Color(0.9f, 0.7f, 0.15f));
            _wood = MapPrimitives.Mat(new Color(0.4f, 0.28f, 0.15f));
            _rust = MapPrimitives.Mat(new Color(0.5f, 0.28f, 0.12f));
        }

        // Doorway half-width used when splitting walls
        const float Door = 1.8f;

        void BuildHub(Transform parent)
        {
            var c = new Vector3(0f, 0f, 0f);
            FloorCeil(parent, "Hub", c, 18f, 18f, 4f);
            // N/S/E/W walls with center door gaps
            WallWithDoorX(parent, c + new Vector3(0f, 2f, 9f), 18f, 4f, true);  // north open to dorm
            WallWithDoorX(parent, c + new Vector3(0f, 2f, -9f), 18f, 4f, true); // south open to bay
            WallWithDoorZ(parent, c + new Vector3(-9f, 2f, 0f), 18f, 4f, true); // west open to cook
            WallWithDoorZ(parent, c + new Vector3(9f, 2f, 0f), 18f, 4f, true);  // east open to storage
        }

        void BuildCookWest(Transform parent)
        {
            var c = new Vector3(-24f, 0f, 0f);
            FloorCeil(parent, "CookWest", c, 16f, 14f, 4f);
            SolidWall(parent, c + new Vector3(0f, 2f, 7f), new Vector3(16f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(0f, 2f, -7f), new Vector3(16f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(-8f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            WallWithDoorZ(parent, c + new Vector3(8f, 2f, 0f), 14f, 4f, true); // door toward hub
        }

        void BuildStorageEast(Transform parent)
        {
            var c = new Vector3(24f, 0f, 0f);
            FloorCeil(parent, "StorageEast", c, 16f, 14f, 4f);
            SolidWall(parent, c + new Vector3(0f, 2f, 7f), new Vector3(16f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(0f, 2f, -7f), new Vector3(16f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(8f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            WallWithDoorZ(parent, c + new Vector3(-8f, 2f, 0f), 14f, 4f, true);
        }

        void BuildDormNorth(Transform parent)
        {
            var c = new Vector3(0f, 0f, 24f);
            FloorCeil(parent, "DormNorth", c, 14f, 14f, 4f);
            SolidWall(parent, c + new Vector3(0f, 2f, 7f), new Vector3(14f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(-7f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            SolidWall(parent, c + new Vector3(7f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            WallWithDoorX(parent, c + new Vector3(0f, 2f, -7f), 14f, 4f, true);
        }

        void BuildBaySouth(Transform parent)
        {
            var c = new Vector3(0f, 0f, -24f);
            FloorCeil(parent, "BaySouth", c, 16f, 14f, 4f);
            SolidWall(parent, c + new Vector3(0f, 2f, -7f), new Vector3(16f, 4f, 0.4f));
            SolidWall(parent, c + new Vector3(-8f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            SolidWall(parent, c + new Vector3(8f, 2f, 0f), new Vector3(0.4f, 4f, 14f));
            WallWithDoorX(parent, c + new Vector3(0f, 2f, 7f), 16f, 4f, true);
        }

        void BuildSideRooms(Transform parent)
        {
            var nw = new Vector3(-20f, 0f, 18f);
            FloorCeil(parent, "SideNW", nw, 10f, 10f, 4f);
            SolidWall(parent, nw + new Vector3(0f, 2f, 5f), new Vector3(10f, 4f, 0.4f));
            SolidWall(parent, nw + new Vector3(-5f, 2f, 0f), new Vector3(0.4f, 4f, 10f));
            SolidWall(parent, nw + new Vector3(0f, 2f, -5f), new Vector3(10f, 4f, 0.4f));
            WallWithDoorZ(parent, nw + new Vector3(5f, 2f, 0f), 10f, 4f, true);

            var se = new Vector3(20f, 0f, -18f);
            FloorCeil(parent, "SideSE", se, 10f, 10f, 4f);
            SolidWall(parent, se + new Vector3(0f, 2f, -5f), new Vector3(10f, 4f, 0.4f));
            SolidWall(parent, se + new Vector3(5f, 2f, 0f), new Vector3(0.4f, 4f, 10f));
            SolidWall(parent, se + new Vector3(0f, 2f, 5f), new Vector3(10f, 4f, 0.4f));
            WallWithDoorZ(parent, se + new Vector3(-5f, 2f, 0f), 10f, 4f, true);
        }

        void BuildCorridorFloors(Transform parent)
        {
            MapPrimitives.Box(parent, "CorrW", new Vector3(-12f, -0.15f, 0f), new Vector3(8f, 0.3f, 4f), _concreteDark);
            MapPrimitives.Box(parent, "CorrE", new Vector3(12f, -0.15f, 0f), new Vector3(8f, 0.3f, 4f), _concreteDark);
            MapPrimitives.Box(parent, "CorrN", new Vector3(0f, -0.15f, 12f), new Vector3(4f, 0.3f, 8f), _concreteDark);
            MapPrimitives.Box(parent, "CorrS", new Vector3(0f, -0.15f, -12f), new Vector3(4f, 0.3f, 8f), _concreteDark);
            MapPrimitives.Box(parent, "CorrNW", new Vector3(-14f, -0.15f, 12f), new Vector3(6f, 0.3f, 3.5f), _concreteDark);
            MapPrimitives.Box(parent, "CorrSE", new Vector3(14f, -0.15f, -12f), new Vector3(6f, 0.3f, 3.5f), _concreteDark);

            // Door frames
            foreach (var p in new[]
                     {
                         new Vector3(-9f, 1.6f, 0f), new Vector3(9f, 1.6f, 0f),
                         new Vector3(0f, 1.6f, 9f), new Vector3(0f, 1.6f, -9f)
                     })
            {
                MapPrimitives.Box(parent, "Frame", p, new Vector3(0.25f, 3.2f, 0.25f), _metal);
            }
        }

        void FloorCeil(Transform parent, string name, Vector3 center, float sx, float sz, float h)
        {
            var room = new GameObject(name);
            room.transform.SetParent(parent, false);
            room.transform.position = center;
            MapPrimitives.Box(room.transform, "Floor", new Vector3(0f, -0.2f, 0f), new Vector3(sx, 0.4f, sz), _tile);
            MapPrimitives.Box(room.transform, "Ceiling", new Vector3(0f, h + 0.2f, 0f), new Vector3(sx, 0.4f, sz), _concreteDark);
        }

        void SolidWall(Transform parent, Vector3 pos, Vector3 scale)
        {
            MapPrimitives.Box(parent, "Wall", pos, scale, _concrete);
        }

        /// <summary>Wall along X axis (facing ±Z) with a center doorway.</summary>
        void WallWithDoorX(Transform parent, Vector3 center, float length, float height, bool withDoor)
        {
            if (!withDoor)
            {
                SolidWall(parent, center, new Vector3(length, height, 0.4f));
                return;
            }
            var side = (length - Door * 2f) * 0.5f;
            if (side < 0.5f) side = 1f;
            SolidWall(parent, center + new Vector3(-(Door + side * 0.5f), 0f, 0f), new Vector3(side, height, 0.4f));
            SolidWall(parent, center + new Vector3(Door + side * 0.5f, 0f, 0f), new Vector3(side, height, 0.4f));
            // Lintel
            SolidWall(parent, center + new Vector3(0f, height * 0.35f, 0f), new Vector3(Door * 2f, height * 0.3f, 0.4f));
        }

        /// <summary>Wall along Z axis (facing ±X) with a center doorway.</summary>
        void WallWithDoorZ(Transform parent, Vector3 center, float length, float height, bool withDoor)
        {
            if (!withDoor)
            {
                SolidWall(parent, center, new Vector3(0.4f, height, length));
                return;
            }
            var side = (length - Door * 2f) * 0.5f;
            if (side < 0.5f) side = 1f;
            SolidWall(parent, center + new Vector3(0f, 0f, -(Door + side * 0.5f)), new Vector3(0.4f, height, side));
            SolidWall(parent, center + new Vector3(0f, 0f, Door + side * 0.5f), new Vector3(0.4f, height, side));
            SolidWall(parent, center + new Vector3(0f, height * 0.35f, 0f), new Vector3(0.4f, height * 0.3f, Door * 2f));
        }

        void BuildProps(Transform parent)
        {
            for (var i = 0; i < 5; i++)
            {
                var p = new Vector3(-26f + (i % 3) * 3f, 0.8f, -3f + (i / 3) * 4f);
                MapPrimitives.Cyl(parent, "Tank_" + i, p, new Vector3(1.1f, 0.8f, 1.1f), _chem);
                MapPrimitives.Cyl(parent, "Pipe_" + i, p + Vector3.up * 1.2f, new Vector3(0.2f, 0.6f, 0.2f), _pipe);
                CoverPoints.Add(p);
            }

            for (var i = 0; i < 10; i++)
            {
                var p = new Vector3(20f + (i % 4) * 2.5f, 0.7f, -4f + (i / 4) * 3f);
                MapPrimitives.Box(parent, "Crate_" + i, p, new Vector3(1.6f, 1.4f, 1.6f),
                    i % 2 == 0 ? _wood : _rust);
                CoverPoints.Add(p);
            }

            for (var i = 0; i < 4; i++)
            {
                var p = new Vector3(-4f + i * 2.8f, 0.4f, 26f);
                MapPrimitives.Box(parent, "Cot_" + i, p, new Vector3(2f, 0.4f, 1f),
                    MapPrimitives.Mat(new Color(0.55f, 0.5f, 0.4f)));
                CoverPoints.Add(p);
            }

            MapPrimitives.PlaceRustCar(parent, new Vector3(-4f, 0.55f, -26f), 20f, CoverPoints);
            MapPrimitives.PlaceTruck(parent, new Vector3(5f, 0.65f, -24f), -15f, CoverPoints);

            for (var i = 0; i < 6; i++)
            {
                var ang = i / 6f * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(ang) * 5f, 0.55f, Mathf.Sin(ang) * 5f);
                MapPrimitives.Cyl(parent, "Hazard_" + i, p, new Vector3(0.6f, 0.55f, 0.6f), _hazard);
                CoverPoints.Add(p);
            }

            for (var i = 0; i < 8; i++)
            {
                MapPrimitives.Cyl(parent, "CeilPipe_" + i, new Vector3(-15f + i * 4f, 3.6f, 2f),
                    new Vector3(0.18f, 4f, 0.18f), _pipe)
                    .transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        void BuildBasesAndZones(Transform parent)
        {
            HatfieldBase = new Vector3(-26f, 0.2f, 0f);
            McCoyBase = new Vector3(26f, 0.2f, 0f);
            BaseCaptureRadius = 5.5f;

            MapPrimitives.Box(parent, "HatPad", HatfieldBase, new Vector3(4f, 0.12f, 4f),
                MapPrimitives.Mat(new Color(0.3f, 0.4f, 0.75f)));
            MapPrimitives.Box(parent, "McPad", McCoyBase, new Vector3(4f, 0.12f, 4f),
                MapPrimitives.Mat(new Color(0.75f, 0.25f, 0.2f)));

            Zones.Add(new ZoneDef { Name = "Hub", Center = new Vector3(0f, 0.05f, 0f), Radius = 7f });
            Zones.Add(new ZoneDef { Name = "Cook", Center = new Vector3(-24f, 0.05f, 0f), Radius = 6.5f });
            Zones.Add(new ZoneDef { Name = "Bay", Center = new Vector3(0f, 0.05f, -24f), Radius = 6.5f });
        }

        void BuildSpawns()
        {
            var hat = new[]
            {
                new Vector3(-28f, 0.2f, -4f), new Vector3(-28f, 0.2f, 4f), new Vector3(-22f, 0.2f, 0f),
                new Vector3(-20f, 0.2f, 16f), new Vector3(-26f, 0.2f, 8f), new Vector3(-18f, 0.2f, -6f)
            };
            var mc = new[]
            {
                new Vector3(28f, 0.2f, -4f), new Vector3(28f, 0.2f, 4f), new Vector3(22f, 0.2f, 0f),
                new Vector3(20f, 0.2f, -16f), new Vector3(26f, 0.2f, -8f), new Vector3(18f, 0.2f, 6f)
            };
            HatfieldSpawns.AddRange(hat);
            McCoySpawns.AddRange(mc);
            SpawnPoints.AddRange(hat);
            SpawnPoints.AddRange(mc);
            SpawnPoints.AddRange(new[]
            {
                new Vector3(0f, 0.2f, 22f), new Vector3(0f, 0.2f, -22f),
                new Vector3(-8f, 0.2f, 8f), new Vector3(8f, 0.2f, -8f)
            });
        }

        static void PlacePointLight(Transform parent, Vector3 pos, Color color)
        {
            var go = new GameObject("LabLight");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 18f;
            light.intensity = 1.4f;
            light.color = color;
        }
    }
}
