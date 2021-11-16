// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class PerformancePointsCounter : RollingCounter<int>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override bool IsRollingProportional => true;

        protected override double RollingDuration => 1000;

        private const float alpha_when_invalid = 0.3f;

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private ScoreProcessor scoreProcessor { get; set; }

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private GameplayState gameplayState { get; set; }

        [CanBeNull]
        private List<TimedDifficultyAttributes> timedAttributes;

        private readonly CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        private JudgementResult lastJudgement;

        public PerformancePointsCounter()
        {
            Current.Value = DisplayedCount = 0;
        }

        private Mod[] clonedMods;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;

            if (gameplayState != null)
            {
                clonedMods = gameplayState.Mods.Select(m => m.DeepClone()).ToArray();

                var gameplayWorkingBeatmap = new GameplayWorkingBeatmap(gameplayState.Beatmap);
                difficultyCache.GetTimedDifficultyAttributesAsync(gameplayWorkingBeatmap, gameplayState.Ruleset, clonedMods, loadCancellationSource.Token)
                               .ContinueWith(r => Schedule(() =>
                               {
                                   timedAttributes = r.Result;
                                   IsValid = true;
                                   if (lastJudgement != null)
                                       onJudgementChanged(lastJudgement);
                               }), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement += onJudgementChanged;
                scoreProcessor.JudgementReverted += onJudgementChanged;
            }

            if (gameplayState?.LastJudgementResult.Value != null)
                onJudgementChanged(gameplayState.LastJudgementResult.Value);
        }

        private bool isValid;

        protected bool IsValid
        {
            set
            {
                if (value == isValid)
                    return;

                isValid = value;
                DrawableCount.FadeTo(isValid ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            }
        }

        private void onJudgementChanged(JudgementResult judgement)
        {
            lastJudgement = judgement;

            var attrib = getAttributeAtTime(judgement);

            if (gameplayState == null || attrib == null)
            {
                IsValid = false;
                return;
            }

            // awkward but we need to make sure the true mods are not passed to PerformanceCalculator as it makes a mess of track applications.
            var scoreInfo = gameplayState.Score.ScoreInfo.DeepClone();
            scoreInfo.Mods = clonedMods;

            var calculator = gameplayState.Ruleset.CreatePerformanceCalculator(attrib, scoreInfo);

            Current.Value = (int)Math.Round(calculator?.Calculate() ?? 0, MidpointRounding.AwayFromZero);
            IsValid = true;
        }

        [CanBeNull]
        private DifficultyAttributes getAttributeAtTime(JudgementResult judgement)
        {
            if (timedAttributes == null || timedAttributes.Count == 0)
                return null;

            int attribIndex = timedAttributes.BinarySearch(new TimedDifficultyAttributes(judgement.HitObject.GetEndTime(), null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;

            return timedAttributes[Math.Clamp(attribIndex, 0, timedAttributes.Count - 1)].Attributes;
        }

        protected override LocalisableString FormatCount(int count) => count.ToString(@"D");

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement -= onJudgementChanged;
                scoreProcessor.JudgementReverted -= onJudgementChanged;
            }

            loadCancellationSource?.Cancel();
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
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = @"pp",
                            Font = OsuFont.Numeric.With(size: 8),
                            Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                        }
                    }
                };
            }
        }

        // TODO: This class shouldn't exist, but requires breaking changes to allow DifficultyCalculator to receive an IBeatmap.
        private class GameplayWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap gameplayBeatmap;

            public GameplayWorkingBeatmap(IBeatmap gameplayBeatmap)
                : base(gameplayBeatmap.BeatmapInfo, null)
            {
                this.gameplayBeatmap = gameplayBeatmap;
            }

            public override IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods = null, TimeSpan? timeout = null)
                => gameplayBeatmap;

            protected override IBeatmap GetBeatmap() => gameplayBeatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
