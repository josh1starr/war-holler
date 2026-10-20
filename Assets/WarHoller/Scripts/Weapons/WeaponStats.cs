using WarHoller.Core;
using UnityEngine;

namespace WarHoller.Weapons
{
    [System.Serializable]
    public class WeaponStats
    {
        public WeaponIds Id;
        public string DisplayName;
        public float Damage;
        public float FireRate;        // shots per second
        public float HipSpreadDeg;
        public float AdsSpreadDeg;
        public float AdsFov;
        public float Range;
        public int MagSize;
        public float ReloadSeconds;
        public int PelletCount;       // shotgun > 1
        public float RecoilPitch;
        public Color Tint;

        public static WeaponStats Get(WeaponIds id)
        {
            switch (id)
            {
                case WeaponIds.Revolver:
                    return new WeaponStats
                    {
                        Id = WeaponIds.Revolver,
                        DisplayName = "Revolver",
                        Damage = 34f,
                        FireRate = 2.2f,
                        HipSpreadDeg = 3.2f,
                        AdsSpreadDeg = 0.6f,
                        AdsFov = 50f,
                        Range = 55f,
                        MagSize = 6,
                        ReloadSeconds = 2.4f,
                        PelletCount = 1,
                        RecoilPitch = 2.5f,
                        Tint = new Color(0.45f, 0.35f, 0.2f)
                    };
                case WeaponIds.Shotgun:
                    return new WeaponStats
                    {
                        Id = WeaponIds.Shotgun,
                        DisplayName = "Shotgun",
                        Damage = 12f, // per pellet
                        FireRate = 1.05f,
                        HipSpreadDeg = 7f,
                        AdsSpreadDeg = 4f,
                        AdsFov = 55f,
                        Range = 28f,
                        MagSize = 5,
                        ReloadSeconds = 3.2f,
                        PelletCount = 8,
                        RecoilPitch = 4.5f,
                        Tint = new Color(0.35f, 0.25f, 0.15f)
                    };
                default:
                    return new WeaponStats
                    {
                        Id = WeaponIds.LeverActionRifle,
                        DisplayName = "Lever-Action Rifle",
                        Damage = 42f,
                        FireRate = 1.35f,
                        HipSpreadDeg = 2.4f,
                        AdsSpreadDeg = 0.35f,
                        AdsFov = 42f,
                        Range = 90f,
                        MagSize = 8,
                        ReloadSeconds = 2.8f,
                        PelletCount = 1,
                        RecoilPitch = 2.0f,
                        Tint = new Color(0.4f, 0.28f, 0.14f)
                    };
            }
        }
    }
}
