using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WarHoller.Gameplay;

namespace WarHoller.UI
{
    public class EndScreenUi : MonoBehaviour
    {
        Canvas _canvas;

        public void Show(string winnerName, IReadOnlyDictionary<Health, int> kills, Action onContinue)
        {
            UiFactory.EnsureEventSystem();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _canvas = UiFactory.CreateCanvas("EndScreen", 50);
            UiFactory.Panel(_canvas.transform, "Dim", Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.015f, 0.01f, 0.9f), Vector2.zero, Vector2.one);

            UiFactory.Label(_canvas.transform, "MATCH OVER", 64, TextAnchor.UpperCenter,
                new Color(0.95f, 0.82f, 0.35f), new Vector2(0f, -80f), new Vector2(800f, 80f));

            UiFactory.Label(_canvas.transform, winnerName + " wins the Holler!", 36, TextAnchor.UpperCenter,
                Color.white, new Vector2(0f, -170f), new Vector2(900f, 50f));

            var ranked = kills.OrderByDescending(kv => kv.Value).Take(12);
            var sb = new StringBuilder();
            var i = 1;
            foreach (var kv in ranked)
            {
                sb.AppendLine($"{i}. {(kv.Key != null ? kv.Key.DisplayName : "?")} — {kv.Value} kills");
                i++;
            }

            UiFactory.Label(_canvas.transform, sb.ToString(), 24, TextAnchor.UpperCenter,
                new Color(0.9f, 0.85f, 0.7f), new Vector2(0f, -240f), new Vector2(700f, 420f));

            UiFactory.Button(_canvas.transform, "MAIN MENU", new Vector2(0f, 100f), new Vector2(260f, 64f),
                () => onContinue?.Invoke(), new Color(0.55f, 0.2f, 0.12f, 0.95f))
                .GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        }
    }
}
