using System;
using UnityEngine;
using UnityEngine.UI;
using WarHoller.Characters;
using WarHoller.Core;
using WarHoller.Weapons;

namespace WarHoller.UI
{
    public class CharacterSelectUi : MonoBehaviour
    {
        public event Action OnConfirm;
        public event Action OnBack;

        Canvas _canvas;
        Text _detail;
        string _selectedId;
        WeaponIds _weapon = WeaponIds.LeverActionRifle;

        public void Show()
        {
            UiFactory.EnsureEventSystem();
            _selectedId = PlayerLoadout.CharacterId;
            _weapon = PlayerLoadout.Weapon;
            _canvas = UiFactory.CreateCanvas("CharacterSelectCanvas", 20);
            var root = _canvas.transform;

            UiFactory.Panel(root, "Dim", Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.03f, 0.02f, 0.88f), Vector2.zero, Vector2.one)
                .GetComponent<RectTransform>().offsetMin = Vector2.zero;

            UiFactory.Label(root, "CHARACTER SELECT", 56, TextAnchor.UpperCenter,
                new Color(0.95f, 0.82f, 0.35f), new Vector2(0f, -40f), new Vector2(900f, 70f));

            // Hatfields column
            UiFactory.Label(root, "HATFIELDS", 32, TextAnchor.UpperCenter,
                new Color(0.55f, 0.7f, 0.95f), new Vector2(-380f, -120f), new Vector2(360f, 40f));
            var hy = -180f;
            foreach (var c in CharacterRoster.ByFaction(FactionIds.Hatfield))
            {
                var captured = c;
                var label = c.DisplayName + (c.IsFemale ? " ♀" : "") + (c.IsParody ? " *" : "");
                UiFactory.CreateButton(root, label, new Vector2(-380f, hy), new Vector2(340f, 48f),
                    () => SelectCharacter(captured.Id),
                    new Color(0.2f, 0.28f, 0.4f, 0.95f));
                hy -= 60f;
            }

            // McCoys column
            UiFactory.Label(root, "McCOYS", 32, TextAnchor.UpperCenter,
                new Color(0.95f, 0.45f, 0.4f), new Vector2(380f, -120f), new Vector2(360f, 40f));
            var my = -180f;
            foreach (var c in CharacterRoster.ByFaction(FactionIds.McCoy))
            {
                var captured = c;
                var label = c.DisplayName + (c.IsFemale ? " ♀" : "");
                UiFactory.CreateButton(root, label, new Vector2(380f, my), new Vector2(340f, 48f),
                    () => SelectCharacter(captured.Id),
                    new Color(0.4f, 0.18f, 0.15f, 0.95f));
                my -= 60f;
            }

            _detail = UiFactory.Label(root, "", 22, TextAnchor.UpperCenter,
                new Color(0.85f, 0.8f, 0.65f), new Vector2(0f, -180f), new Vector2(480f, 160f));

            UiFactory.Label(root, "WEAPON", 28, TextAnchor.UpperCenter,
                new Color(0.95f, 0.85f, 0.5f), new Vector2(0f, -360f), new Vector2(400f, 36f));

            UiFactory.CreateButton(root, "Lever-Action Rifle", new Vector2(-280f, -420f), new Vector2(240f, 48f),
                () => SelectWeapon(WeaponIds.LeverActionRifle));
            UiFactory.CreateButton(root, "Revolver", new Vector2(0f, -420f), new Vector2(200f, 48f),
                () => SelectWeapon(WeaponIds.Revolver));
            UiFactory.CreateButton(root, "Shotgun", new Vector2(260f, -420f), new Vector2(200f, 48f),
                () => SelectWeapon(WeaponIds.Shotgun));

            var rot = LobbyRotation.Current;
            UiFactory.Label(root,
                $"Match: {rot.DisplayName}",
                20, TextAnchor.UpperCenter, new Color(0.85f, 0.78f, 0.55f),
                new Vector2(0f, -480f), new Vector2(700f, 28f));

            UiFactory.CreateButton(root, "CONFIRM", new Vector2(0f, 90f), new Vector2(260f, 64f), () =>
            {
                PlayerLoadout.CharacterId = _selectedId;
                PlayerLoadout.Weapon = _weapon;
                // Same rotation as menu Play — consume so the next visit advances.
                LobbyRotation.ConsumeForPlay();
                OnConfirm?.Invoke();
            }, new Color(0.55f, 0.2f, 0.12f, 0.95f)).GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);

            UiFactory.CreateButton(root, "BACK", new Vector2(120f, 90f), new Vector2(140f, 48f),
                () => OnBack?.Invoke(), new Color(0.2f, 0.15f, 0.1f, 0.9f))
                .GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);

            UiFactory.Label(root, "* Asmon Hatfield is a parody / inspired look — not official.", 16,
                TextAnchor.LowerCenter, new Color(0.7f, 0.65f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 28f));

            RefreshDetail();
        }

        void SelectCharacter(string id)
        {
            _selectedId = id;
            RefreshDetail();
        }

        void SelectWeapon(WeaponIds id)
        {
            _weapon = id;
            RefreshDetail();
        }

        void RefreshDetail()
        {
            var c = CharacterRoster.Get(_selectedId);
            var w = WeaponStats.Get(_weapon);
            var parody = c.IsParody ? "\n" + c.ParodyNote : "";
            _detail.text = $"{c.DisplayName}\n{c.Faction}\nWeapon: {w.DisplayName}{parody}";
        }

        public void Teardown()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
            Destroy(this);
        }
    }
}
