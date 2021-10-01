// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
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
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultPerformancePointsCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private ScoreProcessor scoreProcessor { get; set; }

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private Player player { get; set; }

        private TimedDifficultyAttributes[] timedAttributes;
        private Ruleset gameplayRuleset;

        public DefaultPerformancePointsCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;

            if (player != null)
            {
                gameplayRuleset = player.GameplayRuleset;
                timedAttributes = gameplayRuleset.CreateDifficultyCalculator(new GameplayWorkingBeatmap(player.GameplayBeatmap)).CalculateTimed(player.Mods.Value.ToArray()).ToArray();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (scoreProcessor != null)
                scoreProcessor.NewJudgement += onNewJudgement;
        }

        private void onNewJudgement(JudgementResult judgement)
        {
            if (player == null || timedAttributes.Length == 0)
                return;

            var attribIndex = Array.BinarySearch(timedAttributes, 0, timedAttributes.Length, new TimedDifficultyAttributes(judgement.HitObject.GetEndTime(), null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;
            attribIndex = Math.Clamp(attribIndex, 0, timedAttributes.Length - 1);

            var ppProcessor = gameplayRuleset.CreatePerformanceCalculator(timedAttributes[attribIndex].Attributes, player.Score.ScoreInfo);
            Current.Value = (int)(ppProcessor?.Calculate() ?? 0);
        }

        protected override LocalisableString FormatCount(int count) => count.ToString(@"D");

        protected override IHasText CreateText() => new TextComponent();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor != null)
                scoreProcessor.NewJudgement -= onNewJudgement;
        }

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = @"pp",
                            Font = OsuFont.Numeric.With(size: 8)
                        }
                    }
                };
            }
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
                => gameplayBeatmap.PlayableBeatmap;

            protected override IBeatmap GetBeatmap() => gameplayBeatmap.PlayableBeatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
