using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WarHoller.Gameplay;
using WarHoller.Weapons;

namespace WarHoller.UI
{
    public class MatchHud : MonoBehaviour
    {
        GameSession _session;
        Health _player;
        Canvas _canvas;
        Text _timer;
        Text _score;
        Text _ammo;
        Text _feed;
        Text _board;
        GameObject _boardRoot;

        public void Bind(GameSession session, Health player)
        {
            _session = session;
            _player = player;
            UiFactory.EnsureEventSystem();
            _canvas = UiFactory.CreateCanvas("MatchHud", 5);

            // Crosshair
            UiFactory.Label(_canvas.transform, "+", 36, TextAnchor.MiddleCenter,
                new Color(1f, 1f, 1f, 0.7f), Vector2.zero, new Vector2(40f, 40f));

            _timer = UiFactory.Label(_canvas.transform, "10:00", 36, TextAnchor.UpperCenter,
                new Color(0.95f, 0.9f, 0.7f), new Vector2(0f, -20f), new Vector2(200f, 48f));
            _timer.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            _timer.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

            _score = UiFactory.Label(_canvas.transform, "Kills 0 / 25", 28, TextAnchor.UpperRight,
                Color.white, new Vector2(-140f, -30f), new Vector2(260f, 40f));
            _score.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
            _score.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);

            _ammo = UiFactory.Label(_canvas.transform, "8 / 8", 32, TextAnchor.LowerRight,
                new Color(0.95f, 0.85f, 0.5f), new Vector2(-120f, 40f), new Vector2(220f, 48f));
            _ammo.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
            _ammo.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);

            _feed = UiFactory.Label(_canvas.transform, "", 20, TextAnchor.UpperLeft,
                new Color(1f, 0.9f, 0.7f), new Vector2(20f, -20f), new Vector2(520f, 180f));
            _feed.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
            _feed.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);

            var boardPanel = UiFactory.Panel(_canvas.transform, "Scoreboard", Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.04f, 0.03f, 0.85f), new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f));
            _boardRoot = boardPanel.gameObject;
            _boardRoot.SetActive(false);
            _board = UiFactory.Label(_boardRoot.transform, "", 24, TextAnchor.UpperLeft,
                Color.white, new Vector2(20f, -20f), new Vector2(900f, 600f));
        }

        void Update()
        {
            if (_session == null || _player == null) return;

            var t = Mathf.CeilToInt(_session.TimeLeft);
            var m = t / 60;
            var s = t % 60;
            _timer.text = $"{m:00}:{s:00}";

            var kills = _session.Kills.TryGetValue(_player, out var k) ? k : 0;
            _score.text = $"Kills {kills} / {GameSession.KillLimit}";

            var weapon = _player.GetComponentInChildren<WeaponController>();
            if (weapon != null && weapon.Stats != null)
            {
                _ammo.text = weapon.IsReloading
                    ? "Reloading..."
                    : $"{weapon.AmmoInMag} / {weapon.Stats.MagSize}  {(weapon.IsAds ? "[ADS]" : "")}";
            }

            var sb = new StringBuilder();
            foreach (var line in _session.KillFeed)
                sb.AppendLine(line);
            _feed.text = sb.ToString();

            var pc = _player.GetComponent<PlayerController>();
            var showBoard = pc != null && pc.ScoreboardHeld;
            _boardRoot.SetActive(showBoard);
            if (showBoard)
            {
                var ranked = _session.Kills.OrderByDescending(kv => kv.Value).ToList();
                var board = new StringBuilder();
                board.AppendLine("SCOREBOARD  (hold Tab / Esc)");
                board.AppendLine("---------------------------");
                var rank = 1;
                foreach (var kv in ranked)
                {
                    var name = kv.Key != null ? kv.Key.DisplayName : "?";
                    board.AppendLine($"{rank,2}. {name,-24} {kv.Value}");
                    rank++;
                }
                _board.text = board.ToString();
            }
        }
    }
}
