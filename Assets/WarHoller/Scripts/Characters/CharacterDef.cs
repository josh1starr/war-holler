using WarHoller.Core;
using UnityEngine;

namespace WarHoller.Characters
{
    [System.Serializable]
    public class CharacterDef
    {
        public string Id;
        public string DisplayName;
        public FactionIds Faction;
        public bool IsFemale;
        public bool IsParody;
        public string ParodyNote;
        public Color PrimaryColor;
        public Color AccentColor;
        public float HairLength;
        public bool HasHeadset;

        public CharacterDef(
            string id,
            string displayName,
            FactionIds faction,
            bool isFemale,
            Color primary,
            Color accent,
            float hairLength = 0.2f,
            bool hasHeadset = false,
            bool isParody = false,
            string parodyNote = null)
        {
            Id = id;
            DisplayName = displayName;
            Faction = faction;
            IsFemale = isFemale;
            PrimaryColor = primary;
            AccentColor = accent;
            HairLength = hairLength;
            HasHeadset = hasHeadset;
            IsParody = isParody;
            ParodyNote = parodyNote;
        }
    }
}
