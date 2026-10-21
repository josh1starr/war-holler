using System;
using UnityEngine;
using WarHoller.Core;
using WarHoller.Maps;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Local CTF: steal the enemy pig, return it to your base while your pig is home. First to 3 captures.
    /// </summary>
    public class CaptureTheFlagMode : MonoBehaviour
    {
        public const int CaptureLimit = 3;

        public PigFlag HatfieldPig;
        public PigFlag McCoyPig;
        public int HatfieldCaptures;
        public int McCoyCaptures;

        MapLayout _map;
        float _pickupTick;

        public event Action<FactionIds, string> OnCapture;
        public event Action<FactionIds> OnReachedLimit;

        public void Begin(MapLayout map)
        {
            _map = map;
            HatfieldCaptures = 0;
            McCoyCaptures = 0;

            PlaceBaseMarkers(map);

            HatfieldPig = PigFlag.Create(transform, FactionIds.Hatfield, map.HatfieldBase + Vector3.up * 0.05f);
            McCoyPig = PigFlag.Create(transform, FactionIds.McCoy, map.McCoyBase + Vector3.up * 0.05f);
        }

        void PlaceBaseMarkers(MapLayout map)
        {
            PlacePad(map.HatfieldBase, FactionIds.Hatfield, "HatfieldBase");
            PlacePad(map.McCoyBase, FactionIds.McCoy, "McCoyBase");
        }

        void PlacePad(Vector3 pos, FactionIds team, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            var radius = _map != null ? _map.BaseCaptureRadius : 6f;
            go.transform.localScale = new Vector3(radius * 2f, 0.08f, radius * 2f);
            go.GetComponent<Renderer>().material = WarHoller.Art.MaterialUtil.CreateColor(
                team == FactionIds.Hatfield
                    ? new Color(0.3f, 0.4f, 0.7f, 0.5f)
                    : new Color(0.7f, 0.25f, 0.2f, 0.5f));
            Destroy(go.GetComponent<Collider>());

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = name + "_Pole";
            pole.transform.SetParent(transform, false);
            pole.transform.position = pos + Vector3.up * 2f;
            pole.transform.localScale = new Vector3(0.2f, 2f, 0.2f);
            pole.GetComponent<Renderer>().material = WarHoller.Art.MaterialUtil.CreateColor(
                team == FactionIds.Hatfield
                    ? new Color(0.4f, 0.5f, 0.85f)
                    : new Color(0.85f, 0.3f, 0.25f));
        }

        public void Tick(System.Collections.Generic.IEnumerable<Health> combatants, Func<Health, FactionIds> teamOf)
        {
            _pickupTick -= Time.deltaTime;
            if (_pickupTick > 0f) return;
            _pickupTick = 0.15f;

            foreach (var h in combatants)
            {
                if (h == null || h.IsDead) continue;
                var team = teamOf(h);

                // Steal enemy pig
                if (team == FactionIds.Hatfield)
                    McCoyPig.TryPickup(h, team);
                else
                    HatfieldPig.TryPickup(h, team);

                // Score if carrying enemy pig into own base while own pig is home
                TryScore(h, team);
            }
        }

        void TryScore(Health carrier, FactionIds team)
        {
            var enemyPig = team == FactionIds.Hatfield ? McCoyPig : HatfieldPig;
            var ownPig = team == FactionIds.Hatfield ? HatfieldPig : McCoyPig;
            if (enemyPig.Carrier != carrier) return;
            if (!ownPig.IsHome) return;

            var home = team == FactionIds.Hatfield ? _map.HatfieldBase : _map.McCoyBase;
            if (Vector3.Distance(carrier.transform.position, home) > _map.BaseCaptureRadius) return;

            enemyPig.ReturnHome();
            if (team == FactionIds.Hatfield) HatfieldCaptures++;
            else McCoyCaptures++;

            OnCapture?.Invoke(team, carrier.DisplayName);

            if (HatfieldCaptures >= CaptureLimit)
                OnReachedLimit?.Invoke(FactionIds.Hatfield);
            else if (McCoyCaptures >= CaptureLimit)
                OnReachedLimit?.Invoke(FactionIds.McCoy);
        }

        public PigFlag EnemyPig(FactionIds team) =>
            team == FactionIds.Hatfield ? McCoyPig : HatfieldPig;

        public PigFlag OwnPig(FactionIds team) =>
            team == FactionIds.Hatfield ? HatfieldPig : McCoyPig;

        public Vector3 OwnBase(FactionIds team) =>
            team == FactionIds.Hatfield ? _map.HatfieldBase : _map.McCoyBase;

        public int Captures(FactionIds team) =>
            team == FactionIds.Hatfield ? HatfieldCaptures : McCoyCaptures;
    }
}
