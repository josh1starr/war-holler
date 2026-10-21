using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WarHoller.Core;
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
        Text _modeHint;
        GameObject _boardRoot;

        public void Bind(GameSession session, Health player)
        {
            _session = session;
            _player = player;
            UiFactory.EnsureEventSystem();
            _canvas = UiFactory.CreateCanvas("MatchHud", 5);

            UiFactory.Label(_canvas.transform, "+", 36, TextAnchor.MiddleCenter,
                new Color(1f, 1f, 1f, 0.7f), Vector2.zero, new Vector2(40f, 40f));

            _timer = UiFactory.Label(_canvas.transform, "10:00", 36, TextAnchor.UpperCenter,
                new Color(0.95f, 0.9f, 0.7f), new Vector2(0f, -20f), new Vector2(200f, 48f));
            _timer.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            _timer.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

            _score = UiFactory.Label(_canvas.transform, "", 26, TextAnchor.UpperRight,
                Color.white, new Vector2(-160f, -30f), new Vector2(300f, 80f));
            _score.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
            _score.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);

            _modeHint = UiFactory.Label(_canvas.transform, LobbyRotation.ModeLabel(session.Mode), 20,
                TextAnchor.UpperCenter, new Color(0.85f, 0.8f, 0.6f), new Vector2(0f, -60f), new Vector2(400f, 28f));
            _modeHint.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            _modeHint.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

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

            _score.text = BuildScoreText();

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
                _board.text = BuildScoreboard();
        }

        string BuildScoreText()
        {
            switch (_session.Mode)
            {
                case ModeIds.CaptureTheFlag:
                    if (_session.Ctf == null) return "Pig Flag";
                    return $"Hatfields {_session.Ctf.HatfieldCaptures}/{CaptureTheFlagMode.CaptureLimit}\n" +
                           $"McCoys {_session.Ctf.McCoyCaptures}/{CaptureTheFlagMode.CaptureLimit}";
                case ModeIds.ZoneControl:
                    if (_session.Zones == null) return "Zones";
                    return $"Hatfields {_session.Zones.ScoreInt(FactionIds.Hatfield)}/{ZoneControlMode.ScoreLimit}\n" +
                           $"McCoys {_session.Zones.ScoreInt(FactionIds.McCoy)}/{ZoneControlMode.ScoreLimit}";
                default:
                    var kills = _session.Kills.TryGetValue(_player, out var k) ? k : 0;
                    return $"Kills {kills} / {GameSession.KillLimit}";
            }
        }

        string BuildScoreboard()
        {
            var board = new StringBuilder();
            board.AppendLine("SCOREBOARD  (hold Tab / Esc)");
            board.AppendLine("---------------------------");

            if (_session.Mode == ModeIds.CaptureTheFlag && _session.Ctf != null)
            {
                board.AppendLine($"Hatfields pigs: {_session.Ctf.HatfieldCaptures}");
                board.AppendLine($"McCoys pigs:    {_session.Ctf.McCoyCaptures}");
                board.AppendLine();
            }
            else if (_session.Mode == ModeIds.ZoneControl && _session.Zones != null)
            {
                board.AppendLine($"Hatfields pts: {_session.Zones.ScoreInt(FactionIds.Hatfield)}");
                board.AppendLine($"McCoys pts:    {_session.Zones.ScoreInt(FactionIds.McCoy)}");
                foreach (var z in _session.Zones.Zones)
                {
                    var who = z.Contested ? "CONTESTED" :
                        z.ControllingTeam.HasValue ? z.ControllingTeam.Value.ToString() : "neutral";
                    board.AppendLine($"  {z.ZoneName}: {who}");
                }
                board.AppendLine();
            }

            var ranked = _session.Kills.OrderByDescending(kv => kv.Value).ToList();
            var rank = 1;
            foreach (var kv in ranked)
            {
                var name = kv.Key != null ? kv.Key.DisplayName : "?";
                var team = _session.TeamOf(kv.Key);
                var tag = PlayerLoadout.IsTeamMode ? $" [{team}]" : "";
                board.AppendLine($"{rank,2}. {name,-22}{tag}  {kv.Value} kills");
                rank++;
            }
            return board.ToString();
        }
    }
}
