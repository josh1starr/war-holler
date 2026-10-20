using System;
using UnityEngine;

namespace WarHoller.Gameplay
{
    public class Health : MonoBehaviour
    {
        public float MaxHp = 100f;
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;
        public string DisplayName = "Nobody";
        public int TeamOrSlot;
        public bool IsPlayer;

        public event Action<Health, float, Health> OnDamaged;
        public event Action<Health, Health> OnDied;
        public event Action<Health> OnRespawned;

        void Awake() => Current = MaxHp;

        public void FullHeal()
        {
            Current = MaxHp;
            OnRespawned?.Invoke(this);
        }

        public void ApplyDamage(float amount, Health attacker)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            OnDamaged?.Invoke(this, amount, attacker);
            if (Current <= 0f)
                OnDied?.Invoke(this, attacker);
        }
    }
}
