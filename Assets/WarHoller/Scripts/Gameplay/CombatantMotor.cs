using UnityEngine;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Shared locomotion for player and bots (CharacterController).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CombatantMotor : MonoBehaviour
    {
        public float MoveSpeed = 6.2f;
        public float SprintMultiplier = 1.35f;
        public float Gravity = -22f;
        public float JumpSpeed = 7.5f;

        CharacterController _cc;
        float _vertVel;
        Vector3 _planar;

        public bool IsGrounded => _cc != null && _cc.isGrounded;
        public Vector3 Velocity => _planar + Vector3.up * _vertVel;

        void Awake() => _cc = GetComponent<CharacterController>();

        public void SetPlanarWish(Vector3 worldDir, bool sprint)
        {
            var d = worldDir;
            d.y = 0f;
            if (d.sqrMagnitude > 1f) d.Normalize();
            var speed = MoveSpeed * (sprint ? SprintMultiplier : 1f);
            _planar = d * speed;
        }

        public void Jump()
        {
            if (IsGrounded) _vertVel = JumpSpeed;
        }

        public void Tick(float dt)
        {
            if (_cc == null) return;
            if (IsGrounded && _vertVel < 0f) _vertVel = -2f;
            _vertVel += Gravity * dt;
            var motion = (_planar + Vector3.up * _vertVel) * dt;
            _cc.Move(motion);
        }

        public void Warp(Vector3 position, Quaternion rotation)
        {
            _cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _cc.enabled = true;
            _vertVel = 0f;
            _planar = Vector3.zero;
        }
    }
}
