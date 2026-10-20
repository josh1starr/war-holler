using UnityEngine;
using WarHoller.Weapons;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Legacy Input (keyboard/mouse): WASD, mouse look, LMB fire, RMB ADS, R reload, Esc scoreboard/pause.
    /// New Input System package is listed in manifest for future migration — this slice uses Input.* for zero setup.
    /// </summary>
    [RequireComponent(typeof(CombatantMotor))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        public float LookSensitivity = 2.2f;
        public Transform CameraPivot;
        public WeaponController Weapon;
        public bool InputEnabled = true;

        CombatantMotor _motor;
        Health _health;
        float _pitch;
        bool _scoreboardHeld;

        public bool ScoreboardHeld => _scoreboardHeld;
        public Camera PlayerCamera { get; private set; }

        void Awake()
        {
            _motor = GetComponent<CombatantMotor>();
            _health = GetComponent<Health>();
            _health.IsPlayer = true;
            EnsureCamera();
        }

        void EnsureCamera()
        {
            if (CameraPivot == null)
            {
                var pivot = new GameObject("CameraPivot");
                pivot.transform.SetParent(transform, false);
                pivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                CameraPivot = pivot.transform;
            }

            PlayerCamera = CameraPivot.GetComponentInChildren<Camera>();
            if (PlayerCamera == null)
            {
                var camGo = new GameObject("PlayerCamera");
                camGo.transform.SetParent(CameraPivot, false);
                PlayerCamera = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                PlayerCamera.nearClipPlane = 0.05f;
                PlayerCamera.fieldOfView = 75f;
            }

            if (Weapon == null)
                Weapon = GetComponentInChildren<WeaponController>();
        }

        void Update()
        {
            if (!InputEnabled || _health.IsDead)
            {
                _motor.SetPlanarWish(Vector3.zero, false);
                _scoreboardHeld = false;
                return;
            }

            _scoreboardHeld = Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.Escape);

            var mx = Input.GetAxisRaw("Mouse X") * LookSensitivity;
            var my = Input.GetAxisRaw("Mouse Y") * LookSensitivity;
            transform.Rotate(0f, mx, 0f);
            _pitch = Mathf.Clamp(_pitch - my, -85f, 85f);
            CameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            var wish = transform.TransformDirection(input.normalized);
            var sprint = Input.GetKey(KeyCode.LeftShift);
            _motor.SetPlanarWish(wish, sprint);
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
                _motor.Jump();

            if (Weapon != null)
            {
                Weapon.SetAds(Input.GetMouseButton(1));
                if (Input.GetMouseButton(0)) Weapon.TryFire();
                if (Input.GetKeyDown(KeyCode.R)) Weapon.TryReload();
            }

            _motor.Tick(Time.deltaTime);
        }

        public void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
