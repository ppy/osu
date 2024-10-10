// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A player that exposes many components that would otherwise not be available, for testing purposes.
    /// </summary>
    public partial class TestPlayer : SoloPlayer
    {
        protected override bool PauseOnFocusLost { get; }

        public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

        public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

        public new HUDOverlay HUDOverlay => base.HUDOverlay;

        public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        public new HealthProcessor HealthProcessor => base.HealthProcessor;

        public bool TokenCreationRequested { get; private set; }

        public Score SubmittedScore { get; private set; }

        public new bool PauseCooldownActive => base.PauseCooldownActive;

        public readonly List<JudgementResult> Results = new List<JudgementResult>();

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        public TestPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base(new PlayerConfiguration
            {
                AllowPause = allowPause,
                ShowResults = showResults
            })
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }

        protected override bool ShouldExitOnTokenRetrievalFailure(Exception exception) => false;

        protected override APIRequest<APIScoreToken> CreateTokenRequest()
        {
            TokenCreationRequested = true;
            return base.CreateTokenRequest();
        }

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            SubmittedScore = score;
            return base.CreateSubmissionRequest(score, token);
        }

        protected override void PrepareReplay()
        {
            // Generally, replay generation is handled by whatever is constructing the player.
            // This is implemented locally here to ease migration of test scenes that have some executions
            // running with autoplay and some not, but are not written in a way that lends to instantiating
            // different `Player` types.
            //
            // Eventually we will want to remove this and update all test usages which rely on autoplay to use
            // a `TestReplayPlayer`.
            var autoplayMod = Mods.Value.OfType<ModAutoplay>().FirstOrDefault();

            if (autoplayMod != null)
            {
                DrawableRuleset?.SetReplayScore(autoplayMod.CreateScoreFromReplayData(GameplayState.Beatmap, Mods.Value));
                return;
            }

            base.PrepareReplay();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!LoadedBeatmapSuccessfully)
                return;

            ScoreProcessor.NewJudgement += r => Results.Add(r);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            bool exiting = base.OnExiting(e);

            // SubmittingPlayer performs EndPlaying on a fire-and-forget async task, which allows for the chance of BeginPlaying to be called before EndPlaying is called here.
            // Until this is handled properly at game-side, ensure EndPlaying is called before exiting player.
            // see: https://github.com/ppy/osu/issues/22220
            if (LoadedBeatmapSuccessfully)
                spectatorClient?.EndPlaying(GameplayState);

            return exiting;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // Specific to tests, the player can be disposed without OnExiting() ever being called.
            // We should make sure that the gameplay session has finished even in this case.
            if (LoadedBeatmapSuccessfully)
                spectatorClient?.EndPlaying(GameplayState);
        }
    }
}
