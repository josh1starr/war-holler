using System;
using System.Collections.Generic;
using UnityEngine;
using WarHoller.Core;
using WarHoller.Maps;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Hold zones to earn points. First team to 500 wins. Contested zones award nothing.
    /// </summary>
    public class ZoneControlMode : MonoBehaviour
    {
        public const int ScoreLimit = 500;
        public const float PointsPerSecondPerZone = 8f;

        public readonly List<ControlZone> Zones = new List<ControlZone>();
        public float HatfieldScore;
        public float McCoyScore;

        public event Action<FactionIds> OnReachedLimit;

        public void Begin(MapLayout map)
        {
            HatfieldScore = 0f;
            McCoyScore = 0f;
            Zones.Clear();

            if (map.Zones.Count == 0)
            {
                // Fallback triangle if a map forgot markers
                Zones.Add(ControlZone.Create(transform, "A", new Vector3(-16f, 0.05f, -10f), 8f));
                Zones.Add(ControlZone.Create(transform, "B", new Vector3(0f, 0.05f, 12f), 8f));
                Zones.Add(ControlZone.Create(transform, "C", new Vector3(16f, 0.05f, -10f), 8f));
            }
            else
            {
                foreach (var z in map.Zones)
                    Zones.Add(ControlZone.Create(transform, z.Name, z.Center, z.Radius));
            }
        }

        public void Tick(IEnumerable<Health> combatants, Func<Health, FactionIds> teamOf)
        {
            foreach (var zone in Zones)
                zone.RefreshOccupancy(combatants, teamOf);

            var dt = Time.deltaTime;
            foreach (var zone in Zones)
            {
                if (!zone.ControllingTeam.HasValue || zone.Contested) continue;
                if (zone.ControllingTeam.Value == FactionIds.Hatfield)
                    HatfieldScore += PointsPerSecondPerZone * dt;
                else
                    McCoyScore += PointsPerSecondPerZone * dt;
            }

            if (HatfieldScore >= ScoreLimit)
            {
                HatfieldScore = ScoreLimit;
                OnReachedLimit?.Invoke(FactionIds.Hatfield);
            }
            else if (McCoyScore >= ScoreLimit)
            {
                McCoyScore = ScoreLimit;
                OnReachedLimit?.Invoke(FactionIds.McCoy);
            }
        }

        public int ScoreInt(FactionIds team) =>
            Mathf.FloorToInt(team == FactionIds.Hatfield ? HatfieldScore : McCoyScore);
    }
}
