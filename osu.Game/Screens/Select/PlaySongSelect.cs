// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private bool removeAutoModOnResume;
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

            if (removeAutoModOnResume)
            {
                var autoType = Ruleset.Value.CreateInstance().GetAutoplayMod()?.GetType();
                var cinemaType = Ruleset.Value.CreateInstance().GetAllMods().OfType<ModCinema>().FirstOrDefault()?.GetType();

                if (autoType != null)
                    ModSelect.DeselectTypes(new[] { autoType }, true);

                if (cinemaType != null)
                    ModSelect.DeselectTypes(new[] { cinemaType }, true);

                removeAutoModOnResume = false;
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
                var mods = Mods.Value;

                Mod mod;
                Type modType;

                if (GetContainingInputManager().CurrentState?.Keyboard.ShiftPressed == true)
                {
                    // Ctrl + Shift + Enter should start map with cinema mod enabled.
                    mod = Ruleset.Value.CreateInstance().GetAllMods().OfType<ModCinema>().FirstOrDefault();
                    modType = mod?.GetType();

                    if (modType == null)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = "The current ruleset doesn't have a cinema mod avalaible!"
                        });
                        return false;
                    }
                }
                else
                {
                    // Ctrl + Enter should start map with autoplay mod enabled.
                    mod = Ruleset.Value.CreateInstance().GetAutoplayMod();
                    modType = mod?.GetType();

                    if (modType == null)
                    {
                        notifications?.Post(new SimpleNotification
                        {
                            Text = "The current ruleset doesn't have an autoplay mod avalaible!"
                        });
                        return false;
                    }
                }

                if (mods.All(m => m.GetType() != modType))
                    Mods.Value = mods.Append(mod).ToArray();

                removeAutoModOnResume = true;
            }

            SampleConfirm?.Play();

            this.Push(player = new PlayerLoader(() => new Player()));

            return true;
        }
    }
}
