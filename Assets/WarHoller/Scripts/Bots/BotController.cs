using UnityEngine;
using WarHoller.Core;
using WarHoller.Gameplay;
using WarHoller.Maps;
using WarHoller.Weapons;

namespace WarHoller.Bots
{
    /// <summary>
    /// Bot AI: FFA chase; team modes avoid friendlies; CTF grabs/returns pigs; Zone Control contests pads.
    /// </summary>
    [RequireComponent(typeof(CombatantMotor))]
    [RequireComponent(typeof(Health))]
    public class BotController : MonoBehaviour
    {
        public WeaponController Weapon;
        public float ThinkInterval = 0.2f;
        public float AimSpeed = 4f;
        public float ShootRange = 45f;
        public float LoseInterestRange = 70f;

        CombatantMotor _motor;
        Health _health;
        Health _target;
        GameSession _session;
        float _nextThink;
        Vector3 _wanderPoint;
        float _nextBark;
        enum Objective { Fight, GrabPig, ReturnPig, ContestZone, Defend }
        Objective _objective = Objective.Fight;

        void Awake()
        {
            _motor = GetComponent<CombatantMotor>();
            _health = GetComponent<Health>();
            if (Weapon == null) Weapon = GetComponentInChildren<WeaponController>();
        }

        public void BindSession(GameSession session) => _session = session;

        void Update()
        {
            if (_session == null)
                _session = FindFirstObjectByType<GameSession>();

            if (_health.IsDead)
            {
                _motor.SetPlanarWish(Vector3.zero, false);
                return;
            }

            if (Time.time >= _nextThink)
            {
                _nextThink = Time.time + ThinkInterval;
                ChooseObjective();
                AcquireTarget();
                PickWanderIfNeeded();
            }

            TickMovementAndCombat();
            _motor.Tick(Time.deltaTime);

            if (Time.time > _nextBark)
            {
                _nextBark = Time.time + Random.Range(8f, 18f);
                var voice = FindFirstObjectByType<WarHoller.Audio.VoiceLinePlayer>();
                if (voice != null && Random.value < 0.35f) voice.PlayRandomBark();
            }
        }

        void ChooseObjective()
        {
            _objective = Objective.Fight;
            if (_session == null) return;

            if (_session.Mode == ModeIds.CaptureTheFlag && _session.Ctf != null)
            {
                var team = _session.TeamOf(_health);
                var enemyPig = _session.Ctf.EnemyPig(team);
                var ownPig = _session.Ctf.OwnPig(team);

                if (enemyPig != null && enemyPig.Carrier == _health)
                {
                    _objective = Objective.ReturnPig;
                    return;
                }

                if (enemyPig != null && !enemyPig.IsCarried)
                {
                    _objective = Objective.GrabPig;
                    return;
                }

                // Enemy has our pig — hunt carrier; else pressure enemy base
                if (ownPig != null && ownPig.IsCarried)
                {
                    _objective = Objective.Fight;
                    return;
                }

                if (Random.value < 0.35f)
                    _objective = Objective.Defend;
                return;
            }

            if (_session.Mode == ModeIds.ZoneControl && _session.Zones != null && _session.Zones.Zones.Count > 0)
            {
                _objective = Objective.ContestZone;
            }
        }

        void TickMovementAndCombat()
        {
            Vector3 moveTarget = _wanderPoint;
            var sprint = false;

            if (_objective == Objective.GrabPig && _session?.Ctf != null)
            {
                var pig = _session.Ctf.EnemyPig(_session.TeamOf(_health));
                if (pig != null) moveTarget = pig.transform.position;
                sprint = true;
            }
            else if (_objective == Objective.ReturnPig && _session?.Ctf != null)
            {
                moveTarget = _session.Ctf.OwnBase(_session.TeamOf(_health));
                sprint = true;
            }
            else if (_objective == Objective.ContestZone && _session?.Zones != null)
            {
                var best = PickZoneToContest();
                if (best != null) moveTarget = best.transform.position;
            }
            else if (_objective == Objective.Defend && _session?.Ctf != null)
            {
                moveTarget = _session.Ctf.OwnBase(_session.TeamOf(_health));
            }
            else if (_target != null && !_target.IsDead)
            {
                moveTarget = _target.transform.position;
            }

            var to = moveTarget - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.25f)
            {
                var look = Quaternion.LookRotation(to.normalized, Vector3.up);
                // Face shoot target if fighting nearby
                if (_target != null && !_target.IsDead && _objective == Objective.Fight)
                {
                    var toT = _target.transform.position - transform.position;
                    toT.y = 0f;
                    if (toT.sqrMagnitude > 0.01f)
                        look = Quaternion.LookRotation(toT.normalized, Vector3.up);
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * AimSpeed);
            }

            if (_target != null && !_target.IsDead && _objective == Objective.Fight)
            {
                var dist = Vector3.Distance(transform.position, _target.transform.position);
                var wish = dist > 8f ? to.normalized : (dist < 4f ? -to.normalized : Vector3.zero);
                _motor.SetPlanarWish(wish, dist > 18f);
                if (Weapon != null && dist < ShootRange)
                {
                    var los = HasLineOfSight(_target);
                    Weapon.SetAds(los && dist > 12f);
                    if (los) Weapon.TryFire();
                }
            }
            else
            {
                if (to.sqrMagnitude > 1f)
                    _motor.SetPlanarWish(to.normalized, sprint);
                else
                    _motor.SetPlanarWish(Vector3.zero, false);

                // Shoot opportunistically while on objective
                if (_target != null && !_target.IsDead && Weapon != null)
                {
                    var dist = Vector3.Distance(transform.position, _target.transform.position);
                    if (dist < ShootRange * 0.85f && HasLineOfSight(_target))
                        Weapon.TryFire();
                }
            }
        }

        ControlZone PickZoneToContest()
        {
            var team = _session.TeamOf(_health);
            ControlZone best = null;
            var bestScore = float.MinValue;
            foreach (var z in _session.Zones.Zones)
            {
                var score = 0f;
                if (!z.ControllingTeam.HasValue) score += 30f;
                else if (z.ControllingTeam.Value != team) score += 50f;
                else score += 10f;
                if (z.Contested) score += 20f;
                score -= Vector3.Distance(transform.position, z.transform.position) * 0.5f;
                // Sticky: prefer assigned zone by bot slot
                if ((GetInstanceID() & 3) == (z.ZoneName.GetHashCode() & 3)) score += 15f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = z;
                }
            }
            return best;
        }

        void AcquireTarget()
        {
            Health best = null;
            var bestDist = float.MaxValue;
            foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None))
            {
                if (h == _health || h.IsDead) continue;
                if (_session != null && !_session.AreEnemies(_health, h)) continue;
                var d = Vector3.Distance(transform.position, h.transform.position);
                if (d < bestDist && d < LoseInterestRange)
                {
                    bestDist = d;
                    best = h;
                }
            }

            // Prefer pig carrier in CTF
            if (_session != null && _session.Mode == ModeIds.CaptureTheFlag && _session.Ctf != null)
            {
                var team = _session.TeamOf(_health);
                var own = _session.Ctf.OwnPig(team);
                if (own != null && own.Carrier != null && !own.Carrier.IsDead)
                    best = own.Carrier;
            }

            _target = best;
        }

        void PickWanderIfNeeded()
        {
            if (_objective != Objective.Fight) return;
            if (_target != null) return;
            if ((_wanderPoint - transform.position).sqrMagnitude < 2f || Random.value < 0.25f)
            {
                var map = _session != null ? _session.ActiveMap : FindFirstObjectByType<MapLayout>();
                if (map != null && map.CoverPoints.Count > 0)
                    _wanderPoint = map.CoverPoints[Random.Range(0, map.CoverPoints.Count)];
                else
                    _wanderPoint = transform.position + new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-20f, 20f));
            }
        }

        bool HasLineOfSight(Health target)
        {
            var origin = transform.position + Vector3.up * 1.5f;
            var dest = target.transform.position + Vector3.up * 1.2f;
            var dir = dest - origin;
            if (Physics.Raycast(origin, dir.normalized, out var hit, dir.magnitude + 0.1f))
                return hit.collider.GetComponentInParent<Health>() == target;
            return true;
        }
    }
}
