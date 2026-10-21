using System.Collections.Generic;
using UnityEngine;
using WarHoller.Art;
using WarHoller.Core;

namespace WarHoller.Gameplay
{
    /// <summary>Zone Control pad: contested by nearby living combatants; holders tick score.</summary>
    public class ControlZone : MonoBehaviour
    {
        public string ZoneName = "Zone";
        public float Radius = 8f;
        public FactionIds? ControllingTeam;
        public bool Contested;

        Renderer _ring;
        Material _neutral;
        Material _hat;
        Material _mc;
        Material _fight;

        public static ControlZone Create(Transform parent, string name, Vector3 center, float radius)
        {
            var go = new GameObject("Zone_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var zone = go.AddComponent<ControlZone>();
            zone.ZoneName = name;
            zone.Radius = radius;
            zone.BuildVisual();
            return zone;
        }

        void BuildVisual()
        {
            _neutral = MaterialUtil.CreateColor(new Color(0.7f, 0.7f, 0.35f, 0.55f));
            _hat = MaterialUtil.CreateColor(new Color(0.35f, 0.5f, 0.85f, 0.55f));
            _mc = MaterialUtil.CreateColor(new Color(0.85f, 0.3f, 0.25f, 0.55f));
            _fight = MaterialUtil.CreateColor(new Color(0.9f, 0.75f, 0.2f, 0.65f));

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Pad";
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            disc.transform.localScale = new Vector3(Radius * 2f, 0.05f, Radius * 2f);
            _ring = disc.GetComponent<Renderer>();
            _ring.material = _neutral;
            Object.Destroy(disc.GetComponent<Collider>());

            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Marker";
            post.transform.SetParent(transform, false);
            post.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            post.transform.localScale = new Vector3(0.25f, 1.5f, 0.25f);
            post.GetComponent<Renderer>().material = MaterialUtil.CreateColor(new Color(0.85f, 0.8f, 0.4f));
        }

        public void RefreshOccupancy(IEnumerable<Health> combatants, System.Func<Health, FactionIds> teamOf)
        {
            var hat = 0;
            var mc = 0;
            foreach (var h in combatants)
            {
                if (h == null || h.IsDead) continue;
                var d = Vector3.Distance(h.transform.position, transform.position);
                if (d > Radius) continue;
                var team = teamOf(h);
                if (team == FactionIds.Hatfield) hat++;
                else mc++;
            }

            Contested = hat > 0 && mc > 0;
            if (Contested)
            {
                ControllingTeam = null;
                if (_ring != null) _ring.material = _fight;
            }
            else if (hat > mc)
            {
                ControllingTeam = FactionIds.Hatfield;
                if (_ring != null) _ring.material = _hat;
            }
            else if (mc > hat)
            {
                ControllingTeam = FactionIds.McCoy;
                if (_ring != null) _ring.material = _mc;
            }
            else
            {
                ControllingTeam = null;
                if (_ring != null) _ring.material = _neutral;
            }
        }
    }
}
