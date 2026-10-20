using UnityEngine;
using WarHoller.Core;
using WarHoller.Gameplay;

namespace WarHoller.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        public WeaponStats Stats;
        public Transform Muzzle;
        public Camera AimCamera;
        public Health Owner;
        public LayerMask HitMask = ~0;

        public int AmmoInMag { get; private set; }
        public bool IsAds { get; private set; }
        public bool IsReloading { get; private set; }

        float _nextFireTime;
        float _reloadDoneAt;
        float _baseFov = 75f;
        GameObject _viewModel;

        public void Configure(WeaponIds id, Health owner, Camera cam)
        {
            Stats = WeaponStats.Get(id);
            Owner = owner;
            AimCamera = cam;
            if (AimCamera != null) _baseFov = AimCamera.fieldOfView;
            AmmoInMag = Stats.MagSize;
            BuildViewModel();
        }

        void BuildViewModel()
        {
            if (_viewModel != null) Destroy(_viewModel);
            if (AimCamera == null) return;

            _viewModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _viewModel.name = "ViewModel_" + Stats.DisplayName;
            Destroy(_viewModel.GetComponent<Collider>());
            _viewModel.transform.SetParent(AimCamera.transform, false);
            _viewModel.transform.localPosition = new Vector3(0.28f, -0.22f, 0.55f);
            _viewModel.transform.localRotation = Quaternion.Euler(5f, -8f, 5f);
            _viewModel.transform.localScale = Stats.Id == WeaponIds.Shotgun
                ? new Vector3(0.12f, 0.12f, 0.55f)
                : Stats.Id == WeaponIds.Revolver
                    ? new Vector3(0.08f, 0.12f, 0.28f)
                    : new Vector3(0.08f, 0.1f, 0.7f);

            var r = _viewModel.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            r.material.color = Stats.Tint;

            var muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.SetParent(_viewModel.transform, false);
            muzzleGo.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            Muzzle = muzzleGo.transform;
        }

        public void SetAds(bool ads)
        {
            IsAds = ads && !IsReloading;
            if (AimCamera == null || Stats == null) return;
            var target = IsAds ? Stats.AdsFov : _baseFov;
            AimCamera.fieldOfView = Mathf.Lerp(AimCamera.fieldOfView, target, Time.deltaTime * 12f);
            if (_viewModel != null)
            {
                var hip = new Vector3(0.28f, -0.22f, 0.55f);
                var adsPos = new Vector3(0f, -0.14f, 0.45f);
                _viewModel.transform.localPosition = Vector3.Lerp(
                    _viewModel.transform.localPosition, IsAds ? adsPos : hip, Time.deltaTime * 14f);
            }
        }

        public void TryFire()
        {
            if (Stats == null || Owner == null || Owner.IsDead) return;
            if (IsReloading) return;
            if (Time.time < _nextFireTime) return;
            if (AmmoInMag <= 0)
            {
                TryReload();
                return;
            }

            AmmoInMag--;
            _nextFireTime = Time.time + 1f / Mathf.Max(0.1f, Stats.FireRate);

            var origin = AimCamera != null ? AimCamera.transform.position : transform.position + Vector3.up * 1.5f;
            var forward = AimCamera != null ? AimCamera.transform.forward : transform.forward;
            var pellets = Mathf.Max(1, Stats.PelletCount);
            var spread = IsAds ? Stats.AdsSpreadDeg : Stats.HipSpreadDeg;

            for (var i = 0; i < pellets; i++)
            {
                var dir = ApplySpread(forward, spread);
                if (Physics.Raycast(origin, dir, out var hit, Stats.Range, HitMask, QueryTriggerInteraction.Ignore))
                {
                    var health = hit.collider.GetComponentInParent<Health>();
                    if (health != null && health != Owner && !health.IsDead)
                        health.ApplyDamage(Stats.Damage, Owner);
                }
            }
        }

        public void TryReload()
        {
            if (Stats == null || IsReloading) return;
            if (AmmoInMag >= Stats.MagSize) return;
            IsReloading = true;
            IsAds = false;
            _reloadDoneAt = Time.time + Stats.ReloadSeconds;
        }

        void Update()
        {
            if (IsReloading && Time.time >= _reloadDoneAt)
            {
                AmmoInMag = Stats.MagSize;
                IsReloading = false;
            }
        }

        static Vector3 ApplySpread(Vector3 forward, float deg)
        {
            var right = Vector3.Cross(forward, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.001f) right = Vector3.right;
            var up = Vector3.Cross(right, forward).normalized;
            var yaw = Random.Range(-deg, deg);
            var pitch = Random.Range(-deg, deg);
            return (Quaternion.AngleAxis(yaw, up) * Quaternion.AngleAxis(pitch, right) * forward).normalized;
        }
    }
}
