using UnityEngine;

namespace WarHoller.Maps
{
    /// <summary>
    /// Big-box store interior + parking lot with cars/trucks. Spawns, cover, CTF bases, 3 zones.
    /// </summary>
    public class WowMartBuilder : MapLayout
    {
        Material _asphalt;
        Material _concrete;
        Material _paint;
        Material _shelf;
        Material _wall;
        Material _floor;
        Material _yellow;
        Material _glass;
        Material _cart;

        public void Build()
        {
            DisplayName = "WowMart";
            EnsureMats();
            ClearLists();

            var root = new GameObject("WowMart");
            root.transform.SetParent(transform, false);

            BuildParkingLot(root.transform);
            BuildStoreShell(root.transform);
            BuildAisles(root.transform);
            BuildFurniture(root.transform);
            BuildBasesAndZones(root.transform);
            BuildSpawns();
            RegisterSpawnMarkers(root.transform);
            MapPrimitives.ApplyOutdoorLighting(transform, new Color(0.55f, 0.7f, 0.85f));
        }

        void ClearLists()
        {
            SpawnPoints.Clear();
            CoverPoints.Clear();
            HatfieldSpawns.Clear();
            McCoySpawns.Clear();
            Zones.Clear();
        }

        void EnsureMats()
        {
            _asphalt = MapPrimitives.Mat(new Color(0.22f, 0.22f, 0.24f));
            _concrete = MapPrimitives.Mat(new Color(0.45f, 0.44f, 0.4f));
            _paint = MapPrimitives.Mat(new Color(0.85f, 0.75f, 0.2f));
            _shelf = MapPrimitives.Mat(new Color(0.55f, 0.35f, 0.2f));
            _wall = MapPrimitives.Mat(new Color(0.72f, 0.74f, 0.7f));
            _floor = MapPrimitives.Mat(new Color(0.55f, 0.58f, 0.6f));
            _yellow = MapPrimitives.Mat(new Color(0.9f, 0.75f, 0.15f));
            _glass = MapPrimitives.Mat(new Color(0.55f, 0.7f, 0.8f));
            _cart = MapPrimitives.Mat(new Color(0.4f, 0.42f, 0.45f));
        }

        void BuildParkingLot(Transform parent)
        {
            MapPrimitives.Box(parent, "Lot", new Vector3(0f, -0.4f, -28f), new Vector3(90f, 0.8f, 50f), _asphalt);
            MapPrimitives.Box(parent, "LotStripe", new Vector3(0f, 0.05f, -28f), new Vector3(2f, 0.05f, 40f), _paint);

            // Parking stall lines + cars
            for (var i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                var x = i * 8f;
                MapPrimitives.Box(parent, "Stall_" + i, new Vector3(x, 0.06f, -28f), new Vector3(0.15f, 0.04f, 8f), _paint);
                if (i % 2 == 0)
                    MapPrimitives.PlaceRustCar(parent, new Vector3(x, 0.55f, -32f), 90f, CoverPoints);
                else
                    MapPrimitives.PlaceTruck(parent, new Vector3(x, 0.65f, -24f), -90f, CoverPoints);
            }

            // Cart corral
            MapPrimitives.Box(parent, "Corral", new Vector3(18f, 0.6f, -18f), new Vector3(6f, 1.2f, 3f), _cart);
            for (var i = 0; i < 6; i++)
            {
                var p = new Vector3(16f + i * 0.9f, 0.5f, -18f);
                MapPrimitives.Box(parent, "Cart_" + i, p, new Vector3(0.7f, 0.9f, 1.1f), _cart);
                CoverPoints.Add(p);
            }

            // Lamp posts
            for (var i = -2; i <= 2; i++)
            {
                var post = MapPrimitives.Cyl(parent, "Lamp_" + i, new Vector3(i * 16f, 3f, -40f),
                    new Vector3(0.25f, 3f, 0.25f), _concrete);
                MapPrimitives.Box(parent, "LampHead_" + i, new Vector3(i * 16f, 6.2f, -40f),
                    new Vector3(1.4f, 0.3f, 0.8f), _yellow);
                _ = post;
            }
        }

        void BuildStoreShell(Transform parent)
        {
            // Store sits north of lot
            MapPrimitives.Box(parent, "StoreFloor", new Vector3(0f, -0.2f, 18f), new Vector3(70f, 0.4f, 55f), _floor);
            MapPrimitives.Box(parent, "WallN", new Vector3(0f, 4f, 44f), new Vector3(70f, 8f, 0.5f), _wall);
            // South wall split around a walkable entrance (center gap ~10 units)
            MapPrimitives.Box(parent, "WallSL", new Vector3(-20f, 4f, -8f), new Vector3(30f, 8f, 0.5f), _wall);
            MapPrimitives.Box(parent, "WallSR", new Vector3(20f, 4f, -8f), new Vector3(30f, 8f, 0.5f), _wall);
            MapPrimitives.Box(parent, "WallSLintel", new Vector3(0f, 6.5f, -8f), new Vector3(10f, 3f, 0.5f), _wall);
            MapPrimitives.Box(parent, "WallW", new Vector3(-35f, 4f, 18f), new Vector3(0.5f, 8f, 55f), _wall);
            MapPrimitives.Box(parent, "WallE", new Vector3(35f, 4f, 18f), new Vector3(0.5f, 8f, 55f), _wall);
            MapPrimitives.Box(parent, "Roof", new Vector3(0f, 8.2f, 18f), new Vector3(72f, 0.4f, 57f), _concrete);

            // Glass panes flanking the doorway
            MapPrimitives.Box(parent, "DoorL", new Vector3(-6.5f, 2.5f, -8f), new Vector3(3f, 5f, 0.2f), _glass);
            MapPrimitives.Box(parent, "DoorR", new Vector3(6.5f, 2.5f, -8f), new Vector3(3f, 5f, 0.2f), _glass);
            MapPrimitives.Box(parent, "EntryMat", new Vector3(0f, 0.05f, -6f), new Vector3(8f, 0.08f, 4f),
                MapPrimitives.Mat(new Color(0.35f, 0.2f, 0.15f)));

            // Big yellow facade sign
            MapPrimitives.Box(parent, "Sign", new Vector3(0f, 6.5f, -8.6f), new Vector3(28f, 2.2f, 0.3f), _yellow);
        }

        void BuildAisles(Transform parent)
        {
            for (var row = 0; row < 5; row++)
            {
                var z = 0f + row * 8f;
                for (var col = 0; col < 4; col++)
                {
                    var x = -22f + col * 14f;
                    PlaceShelfRun(parent, new Vector3(x, 0f, z), row * 10 + col);
                }
            }

            // Checkout counters near entrance
            for (var i = 0; i < 4; i++)
            {
                var p = new Vector3(-12f + i * 8f, 0.6f, -2f);
                MapPrimitives.Box(parent, "Checkout_" + i, p, new Vector3(3f, 1.1f, 1.4f), _shelf);
                CoverPoints.Add(p);
            }
        }

        void PlaceShelfRun(Transform parent, Vector3 pos, int id)
        {
            var shelf = new GameObject("Shelf_" + id);
            shelf.transform.SetParent(parent, false);
            shelf.transform.position = pos;
            MapPrimitives.Box(shelf.transform, "Base", new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 8f), _shelf);
            // Goods blocks for color variation
            var goods = new[]
            {
                new Color(0.7f, 0.25f, 0.2f), new Color(0.25f, 0.45f, 0.7f),
                new Color(0.3f, 0.55f, 0.3f), new Color(0.8f, 0.6f, 0.2f)
            };
            for (var i = 0; i < 4; i++)
            {
                MapPrimitives.Box(shelf.transform, "Goods_" + i,
                    new Vector3(0.55f, 0.5f + i * 0.55f, -2f + i),
                    new Vector3(0.4f, 0.4f, 1.2f), MapPrimitives.Mat(goods[i]));
            }
            CoverPoints.Add(pos + Vector3.forward * 4f);
            CoverPoints.Add(pos - Vector3.forward * 4f);
        }

        void BuildFurniture(Transform parent)
        {
            // Pallet stacks + dumpsters outside
            MapPrimitives.Box(parent, "Pallets", new Vector3(-28f, 0.8f, -16f), new Vector3(3f, 1.6f, 2f),
                MapPrimitives.Mat(new Color(0.55f, 0.4f, 0.2f)));
            MapPrimitives.Box(parent, "Dumpster", new Vector3(30f, 1f, -14f), new Vector3(3f, 2f, 2f),
                MapPrimitives.Mat(new Color(0.25f, 0.4f, 0.28f)));
            CoverPoints.Add(new Vector3(-28f, 0f, -16f));
            CoverPoints.Add(new Vector3(30f, 0f, -14f));
        }

        void BuildBasesAndZones(Transform parent)
        {
            HatfieldBase = new Vector3(-28f, 0.2f, 30f);
            McCoyBase = new Vector3(28f, 0.2f, 30f);
            BaseCaptureRadius = 6f;

            MapPrimitives.Box(parent, "HatBasePad", HatfieldBase, new Vector3(5f, 0.15f, 5f),
                MapPrimitives.Mat(new Color(0.3f, 0.4f, 0.7f)));
            MapPrimitives.Box(parent, "McBasePad", McCoyBase, new Vector3(5f, 0.15f, 5f),
                MapPrimitives.Mat(new Color(0.7f, 0.25f, 0.2f)));

            Zones.Add(new ZoneDef { Name = "Aisle 3", Center = new Vector3(-8f, 0.05f, 12f), Radius = 7.5f });
            Zones.Add(new ZoneDef { Name = "Entrance", Center = new Vector3(0f, 0.05f, -4f), Radius = 8f });
            Zones.Add(new ZoneDef { Name = "Lot C", Center = new Vector3(10f, 0.05f, -28f), Radius = 9f });
        }

        void BuildSpawns()
        {
            var hat = new[]
            {
                new Vector3(-30f, 0.2f, 28f), new Vector3(-26f, 0.2f, 34f), new Vector3(-32f, 0.2f, 22f),
                new Vector3(-20f, 0.2f, 36f), new Vector3(-24f, 0.2f, 18f), new Vector3(-18f, 0.2f, 28f)
            };
            var mc = new[]
            {
                new Vector3(30f, 0.2f, 28f), new Vector3(26f, 0.2f, 34f), new Vector3(32f, 0.2f, 22f),
                new Vector3(20f, 0.2f, 36f), new Vector3(24f, 0.2f, 18f), new Vector3(18f, 0.2f, 28f)
            };
            HatfieldSpawns.AddRange(hat);
            McCoySpawns.AddRange(mc);
            SpawnPoints.AddRange(hat);
            SpawnPoints.AddRange(mc);
            SpawnPoints.AddRange(new[]
            {
                new Vector3(0f, 0.2f, -36f), new Vector3(-16f, 0.2f, -22f),
                new Vector3(16f, 0.2f, -22f), new Vector3(0f, 0.2f, 20f)
            });
        }
    }
}
