// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class PlaySongSelect : SongSelect
    {
        private OsuScreen? playerLoader;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        public override bool AllowExternalScreenChange => true;

        public override MenuItem[] CreateForwardNavigationMenuItemsForBeatmap(Func<BeatmapInfo> getBeatmap) => new MenuItem[]
        {
            new OsuMenuItem(ButtonSystemStrings.Play.ToSentence(), MenuItemType.Highlighted, () => FinaliseSelection(getBeatmap())),
            new OsuMenuItem(ButtonSystemStrings.Edit.ToSentence(), MenuItemType.Standard, () => Edit(getBeatmap()))
        };

        protected override UserActivity InitialActivity => new UserActivity.ChoosingBeatmap();

        private PlayBeatmapDetailArea playBeatmapDetailArea = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(ButtonSystemStrings.Edit.ToSentence(), @"beatmap", FontAwesome.Solid.PencilAlt, colours.Yellow, () => Edit());

            AddInternal(new SongSelectTouchInputDetector());
        }

        protected void PresentScore(ScoreInfo score) =>
            FinaliseSelection(score.BeatmapInfo, score.Ruleset, () => this.Push(new SoloResultsScreen(score, false)));

        protected override BeatmapDetailArea CreateBeatmapDetailArea()
        {
            playBeatmapDetailArea = new PlayBeatmapDetailArea
            {
                Leaderboard =
                {
                    ScoreSelected = PresentScore
                }
            };

            return playBeatmapDetailArea;
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

        private IReadOnlyList<Mod>? modsAtGameplayStart;

        private ModAutoplay? getAutoplayMod() => Ruleset.Value.CreateInstance().GetAutoplayMod();

        protected override bool OnStart()
        {
            if (playerLoader != null) return false;

            if (!this.IsCurrentScreen())
                return false;

            modsAtGameplayStart = Mods.Value;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var autoInstance = getAutoplayMod();

                if (autoInstance == null)
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = NotificationsStrings.NoAutoplayMod
                    });
                    return false;
                }

                var mods = Mods.Value.Append(autoInstance).ToArray();

                if (!ModUtils.CheckCompatibleSet(mods, out var invalid))
                    mods = mods.Except(invalid).Append(autoInstance).ToArray();

                Mods.Value = mods;
            }

            SampleConfirm?.Play();

            this.Push(playerLoader = new PlayerLoader(createPlayer));
            return true;

            Player createPlayer()
            {
                Player player;

                var replayGeneratingMod = Mods.Value.OfType<ICreateReplayData>().FirstOrDefault();

                if (replayGeneratingMod != null)
                {
                    player = new ReplayPlayer((beatmap, mods) => replayGeneratingMod.CreateScoreFromReplayData(beatmap, mods))
                    {
                        LeaderboardScores = { BindTarget = playBeatmapDetailArea.Leaderboard.Scores }
                    };
                }
                else
                {
                    player = new SoloPlayer
                    {
                        LeaderboardScores = { BindTarget = playBeatmapDetailArea.Leaderboard.Scores }
                    };
                }

                return player;
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            // Scores will be refreshed on arriving at this screen.
            // Clear them to avoid animation overload on returning to song select.
            playBeatmapDetailArea.Leaderboard.ClearScores();
            base.OnSuspending(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            revertMods();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            revertMods();
            return false;
        }

        private void revertMods()
        {
            if (playerLoader == null) return;

            Mods.Value = modsAtGameplayStart;
            playerLoader = null;
        }
    }
}
