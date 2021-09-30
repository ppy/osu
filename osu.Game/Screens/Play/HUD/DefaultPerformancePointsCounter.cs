// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultPerformancePointsCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        [Resolved]
        private Player player { get; set; }

        private DifficultyCalculator.TimedDifficultyAttributes[] timedAttributes;
        private Ruleset gameplayRuleset;

        public DefaultPerformancePointsCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;

            gameplayRuleset = player.GameplayRuleset;
            timedAttributes = gameplayRuleset.CreateDifficultyCalculator(new GameplayWorkingBeatmap(player.GameplayBeatmap)).CalculateTimed(player.Mods.Value.ToArray()).ToArray();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            scoreProcessor.NewJudgement += onNewJudgement;
        }

        private void onNewJudgement(JudgementResult judgement)
        {
            var attribIndex = Array.BinarySearch(timedAttributes, 0, timedAttributes.Length, new DifficultyCalculator.TimedDifficultyAttributes(judgement.HitObject.GetEndTime(), null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;
            attribIndex = Math.Clamp(attribIndex, 0, timedAttributes.Length - 1);

            var ppProcessor = gameplayRuleset.CreatePerformanceCalculator(timedAttributes[attribIndex].Attributes, player.Score.ScoreInfo);
            Current.Value = (int)(ppProcessor?.Calculate() ?? 0);
        }

        protected override LocalisableString FormatCount(int count) => $@"{count}pp";

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(size: 20f));

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor != null)
                scoreProcessor.NewJudgement -= onNewJudgement;
        }

        private class GameplayWorkingBeatmap : WorkingBeatmap
        {
            private readonly GameplayBeatmap gameplayBeatmap;

            public GameplayWorkingBeatmap(GameplayBeatmap gameplayBeatmap)
                : base(gameplayBeatmap.BeatmapInfo, null)
            {
                this.gameplayBeatmap = gameplayBeatmap;
            }

            public override IBeatmap GetPlayableBeatmap(RulesetInfo ruleset, IReadOnlyList<Mod> mods = null, TimeSpan? timeout = null)
                => gameplayBeatmap;

            protected override IBeatmap GetBeatmap() => gameplayBeatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
