using UnityEngine;
using WarHoller.Gameplay;
using WarHoller.Maps;
using WarHoller.Weapons;

namespace WarHoller.Bots
{
    /// <summary>
    /// Simple FFA bot: chase nearest living enemy, shoot when in cone, wander to cover occasionally.
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
        float _nextThink;
        Vector3 _wanderPoint;
        float _nextBark;

        void Awake()
        {
            _motor = GetComponent<CombatantMotor>();
            _health = GetComponent<Health>();
            if (Weapon == null) Weapon = GetComponentInChildren<WeaponController>();
        }

        void Update()
        {
            if (_health.IsDead)
            {
                _motor.SetPlanarWish(Vector3.zero, false);
                return;
            }

            if (Time.time >= _nextThink)
            {
                _nextThink = Time.time + ThinkInterval;
                AcquireTarget();
                PickWanderIfNeeded();
            }

            if (_target != null && !_target.IsDead)
            {
                var to = _target.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f)
                {
                    var look = Quaternion.LookRotation(to.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * AimSpeed);
                }

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
                var to = _wanderPoint - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(to.normalized), Time.deltaTime * 2f);
                    _motor.SetPlanarWish(to.normalized, false);
                }
                else
                {
                    _motor.SetPlanarWish(Vector3.zero, false);
                }
            }

            _motor.Tick(Time.deltaTime);

            if (Time.time > _nextBark)
            {
                _nextBark = Time.time + Random.Range(8f, 18f);
                var voice = FindFirstObjectByType<WarHoller.Audio.VoiceLinePlayer>();
                if (voice != null && Random.value < 0.35f) voice.PlayRandomBark();
            }
        }

        void AcquireTarget()
        {
            Health best = null;
            var bestDist = float.MaxValue;
            foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None))
            {
                if (h == _health || h.IsDead) continue;
                var d = Vector3.Distance(transform.position, h.transform.position);
                if (d < bestDist && d < LoseInterestRange)
                {
                    bestDist = d;
                    best = h;
                }
            }
            _target = best;
        }

        void PickWanderIfNeeded()
        {
            if (_target != null) return;
            if ((_wanderPoint - transform.position).sqrMagnitude < 2f || Random.value < 0.25f)
            {
                var map = FindFirstObjectByType<TheHollerBuilder>();
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
