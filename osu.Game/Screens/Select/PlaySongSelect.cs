// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private Mod removeModOnResume;
        private OsuScreen player;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        public override bool AllowExternalScreenChange => true;

        protected override UserActivity InitialActivity => new UserActivity.ChoosingBeatmap();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.Solid.PencilAlt, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number4);

            ((PlayBeatmapDetailArea)BeatmapDetails).Leaderboard.ScoreSelected += PresentScore;
        }

        protected void PresentScore(ScoreInfo score) =>
            FinaliseSelection(score.Beatmap, score.Ruleset, () => this.Push(new SoloResultsScreen(score)));

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            player = null;

            if (removeModOnResume != null)
            {
                ModSelect.DeselectTypes(new[] { removeModOnResume.GetType() }, true);
                removeModOnResume = null;
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.KeypadEnter:
                    // this is a special hard-coded case; we can't rely on OnPressed (of SongSelect) as GlobalActionContainer is
                    // matching with exact modifier consideration (so Ctrl+Enter would be ignored).
                    FinaliseSelection();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnStart()
        {
            if (player != null) return false;

            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                if (GetContainingInputManager().CurrentState?.Keyboard.ShiftPressed == true)
                {
                    if (!trySelectMod(Ruleset.Value.CreateInstance().GetAllMods().OfType<ModCinema>().FirstOrDefault(), "The current ruleset doesn't have a cinema mod avalaible!"))
                        return false;
                }
                else
                {
                    if (!trySelectMod(Ruleset.Value.CreateInstance().GetAutoplayMod(), "The current ruleset doesn't have an autoplay mod avalaible!"))
                        return false;
                }
            }

            SampleConfirm?.Play();

            this.Push(player = new PlayerLoader(() => new Player()));

            return true;
        }

        private bool trySelectMod(Mod mod, string notificationMessage)
        {
            var mods = Mods.Value;
            var modType = mod?.GetType();

            if (modType == null)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = notificationMessage
                });
                return false;
            }

            if (mods.All(m => m.GetType() != modType))
            {
                ModSelect.SelectTypes(new[] { modType });
                Mods.Value = ModSelect.SelectedMods.Value;
                removeModOnResume = mod;
            }

            return true;
        }
    }
}
