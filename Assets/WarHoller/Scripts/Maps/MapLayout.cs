using System.Collections.Generic;
using UnityEngine;
using WarHoller.Core;

namespace WarHoller.Maps
{
    /// <summary>
    /// Runtime map data every builder fills: spawns, cover, CTF bases, zone markers.
    /// </summary>
    public class MapLayout : MonoBehaviour
    {
        public string DisplayName = "Map";
        public readonly List<Vector3> SpawnPoints = new List<Vector3>();
        public readonly List<Vector3> CoverPoints = new List<Vector3>();
        public readonly List<Vector3> HatfieldSpawns = new List<Vector3>();
        public readonly List<Vector3> McCoySpawns = new List<Vector3>();
        public readonly List<ZoneDef> Zones = new List<ZoneDef>();

        public Vector3 HatfieldBase = new Vector3(-28f, 0.2f, 0f);
        public Vector3 McCoyBase = new Vector3(28f, 0.2f, 0f);
        public float BaseCaptureRadius = 6f;

        public struct ZoneDef
        {
            public string Name;
            public Vector3 Center;
            public float Radius;
        }

        public Vector3 PickSpawn(FactionIds? team, bool preferTeamPad)
        {
            if (preferTeamPad && team.HasValue)
            {
                var list = team.Value == FactionIds.Hatfield ? HatfieldSpawns : McCoySpawns;
                if (list.Count > 0)
                    return list[Random.Range(0, list.Count)];
            }

            if (SpawnPoints.Count == 0) return Vector3.up * 0.2f;
            return SpawnPoints[Random.Range(0, SpawnPoints.Count)];
        }

        public void RegisterSpawnMarkers(Transform parent)
        {
            foreach (var s in SpawnPoints)
            {
                var marker = new GameObject("Spawn");
                marker.tag = "Spawn";
                marker.transform.SetParent(parent, false);
                marker.transform.position = s;
            }
        }
    }
}
