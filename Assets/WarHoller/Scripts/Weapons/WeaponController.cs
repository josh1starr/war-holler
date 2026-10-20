using UnityEngine;
using WarHoller.Art;
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
        float _nextDryFireTime;
        float _reloadDoneAt;
        float _baseFov = 75f;
        float _muzzleFlashUntil;
        GameObject _viewModel;
        GameObject _muzzleFlashQuad;
        Light _muzzleLight;
        AudioSource _audio;
        static AudioClip _shotClip;
        static AudioClip _dryClip;

        public void Configure(WeaponIds id, Health owner, Camera cam)
        {
            Stats = WeaponStats.Get(id);
            Owner = owner;
            AimCamera = cam;
            if (AimCamera != null) _baseFov = AimCamera.fieldOfView;
            AmmoInMag = Stats.MagSize;
            EnsureAudio();
            BuildViewModel();
        }

        void EnsureAudio()
        {
            if (_audio == null)
            {
                _audio = GetComponent<AudioSource>();
                if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = Owner != null && Owner.IsPlayer ? 0f : 0.65f;
                _audio.rolloffMode = AudioRolloffMode.Linear;
                _audio.maxDistance = 60f;
            }

            if (_shotClip == null) _shotClip = MakeGunshotClip();
            if (_dryClip == null) _dryClip = MakeDryFireClip();
        }

        void BuildViewModel()
        {
            if (_viewModel != null) Destroy(_viewModel);
            if (AimCamera == null) return;

            // Bots use a disabled aim camera — skip first-person viewmodel.
            if (!AimCamera.enabled && Owner != null && !Owner.IsPlayer)
            {
                var botMuzzle = new GameObject("Muzzle");
                botMuzzle.transform.SetParent(transform, false);
                botMuzzle.transform.localPosition = new Vector3(0f, 1.4f, 0.6f);
                Muzzle = botMuzzle.transform;
                return;
            }

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
            r.material = MaterialUtil.CreateColor(Stats.Tint);

            var muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.SetParent(_viewModel.transform, false);
            muzzleGo.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            Muzzle = muzzleGo.transform;

            // Unlit flash quad — obvious even without VFX packages.
            _muzzleFlashQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _muzzleFlashQuad.name = "MuzzleFlash";
            Destroy(_muzzleFlashQuad.GetComponent<Collider>());
            _muzzleFlashQuad.transform.SetParent(Muzzle, false);
            _muzzleFlashQuad.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            _muzzleFlashQuad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            _muzzleFlashQuad.transform.localScale = Vector3.one * 0.18f;
            var flashMat = MaterialUtil.CreateColor(new Color(1f, 0.85f, 0.35f, 1f));
            _muzzleFlashQuad.GetComponent<Renderer>().material = flashMat;
            _muzzleFlashQuad.SetActive(false);

            _muzzleLight = muzzleGo.AddComponent<Light>();
            _muzzleLight.type = LightType.Point;
            _muzzleLight.range = 4f;
            _muzzleLight.intensity = 0f;
            _muzzleLight.color = new Color(1f, 0.75f, 0.35f);
        }

        /// <summary>
        /// Sets ADS intent only. FOV/viewmodel lerp runs in Update so this never blocks TryFire.
        /// </summary>
        public void SetAds(bool ads)
        {
            IsAds = ads && !IsReloading;
        }

        public void TryFire()
        {
            if (Stats == null || Owner == null || Owner.IsDead) return;
            if (IsReloading) return;
            if (Time.time < _nextFireTime) return;

            if (AmmoInMag <= 0)
            {
                PlayDryFire();
                TryReload();
                return;
            }

            AmmoInMag--;
            _nextFireTime = Time.time + 1f / Mathf.Max(0.1f, Stats.FireRate);

            PlayGunshot();
            ShowMuzzleFlash();

            var origin = AimCamera != null ? AimCamera.transform.position : transform.position + Vector3.up * 1.5f;
            var forward = AimCamera != null ? AimCamera.transform.forward : transform.forward;
            var pellets = Mathf.Max(1, Stats.PelletCount);
            var spread = IsAds ? Stats.AdsSpreadDeg : Stats.HipSpreadDeg;

            var hitSomeone = false;
            var pelletsHit = 0;
            for (var i = 0; i < pellets; i++)
            {
                var dir = ApplySpread(forward, spread);
                if (Physics.Raycast(origin, dir, out var hit, Stats.Range, HitMask, QueryTriggerInteraction.Ignore))
                {
                    var health = hit.collider.GetComponentInParent<Health>();
                    if (health != null && health != Owner && !health.IsDead)
                    {
                        health.ApplyDamage(Stats.Damage, Owner);
                        hitSomeone = true;
                        pelletsHit++;
                    }
                }
            }

            if (Owner.IsPlayer)
            {
                Debug.Log(
                    $"[WarHoller] FIRE {Stats.DisplayName} ammo={AmmoInMag}/{Stats.MagSize} " +
                    $"hit={(hitSomeone ? pelletsHit + " pellet(s) on target" : "no hit")} " +
                    $"origin={origin} fwd={forward}");
            }
        }

        public void TryReload()
        {
            if (Stats == null || IsReloading) return;
            if (AmmoInMag >= Stats.MagSize) return;
            IsReloading = true;
            IsAds = false;
            _reloadDoneAt = Time.time + Stats.ReloadSeconds;
            if (Owner != null && Owner.IsPlayer)
                Debug.Log($"[WarHoller] Reloading {Stats.DisplayName}…");
        }

        void Update()
        {
            if (IsReloading && Time.time >= _reloadDoneAt)
            {
                AmmoInMag = Stats.MagSize;
                IsReloading = false;
                if (Owner != null && Owner.IsPlayer)
                    Debug.Log($"[WarHoller] Reload done — mag full ({AmmoInMag}).");
            }

            // ADS FOV / viewmodel lerp — never gates TryFire.
            if (AimCamera != null && Stats != null && AimCamera.enabled)
            {
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

            if (_muzzleFlashUntil > 0f && Time.time >= _muzzleFlashUntil)
            {
                _muzzleFlashUntil = 0f;
                if (_muzzleFlashQuad != null) _muzzleFlashQuad.SetActive(false);
                if (_muzzleLight != null) _muzzleLight.intensity = 0f;
            }
        }

        void PlayGunshot()
        {
            EnsureAudio();
            if (_audio == null || _shotClip == null) return;
            var vol = Mathf.Clamp01(SettingsStore.SfxVolume) * 0.9f;
            _audio.pitch = Random.Range(0.92f, 1.08f);
            _audio.PlayOneShot(_shotClip, vol);
        }

        void PlayDryFire()
        {
            if (Time.time < _nextDryFireTime) return;
            _nextDryFireTime = Time.time + 0.22f;
            EnsureAudio();
            if (_audio == null || _dryClip == null) return;
            var vol = Mathf.Clamp01(SettingsStore.SfxVolume) * 0.55f;
            _audio.pitch = 1f;
            _audio.PlayOneShot(_dryClip, vol);
            if (Owner != null && Owner.IsPlayer)
                Debug.Log("[WarHoller] Dry-fire (empty mag) — click");
        }

        void ShowMuzzleFlash()
        {
            _muzzleFlashUntil = Time.time + 0.05f;
            if (_muzzleFlashQuad != null)
            {
                _muzzleFlashQuad.SetActive(true);
                _muzzleFlashQuad.transform.localRotation =
                    Quaternion.Euler(0f, 180f, Random.Range(0f, 360f));
            }
            if (_muzzleLight != null)
                _muzzleLight.intensity = 3.5f;
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

        static AudioClip MakeGunshotClip()
        {
            const int sampleRate = 22050;
            const float duration = 0.18f;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Exp(-t * 28f);
                var noise = (Random.value * 2f - 1f) * 0.55f;
                var boom = Mathf.Sin(2f * Mathf.PI * 95f * t) * 0.45f;
                var crack = Mathf.Sin(2f * Mathf.PI * 780f * t) * Mathf.Exp(-t * 55f) * 0.35f;
                data[i] = (noise + boom + crack) * env;
            }

            var clip = AudioClip.Create("ProceduralGunshot", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip MakeDryFireClip()
        {
            const int sampleRate = 22050;
            const float duration = 0.06f;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = Mathf.Exp(-t * 80f);
                data[i] = (Random.value * 2f - 1f) * env * 0.25f
                          + Mathf.Sin(2f * Mathf.PI * 1400f * t) * env * 0.2f;
            }

            var clip = AudioClip.Create("ProceduralDryFire", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
