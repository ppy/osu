// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ObjectsRemainingCounter : RollingCounter<int>, ISerialisableDrawable
    {
        protected override double RollingDuration => 250;

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Colour = colour.YellowLighter;
            Current.Value = DisplayedCount = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += onJudgementChanged;
            scoreProcessor.JudgementReverted += onJudgementChanged;
            updateCount();
        }

        private void onJudgementChanged(JudgementResult _) => updateCount();

        private void updateCount()
        {
            int remaining = scoreProcessor.MaxHits - scoreProcessor.JudgedHits;
            Current.Value = remaining < 0 ? 0 : remaining;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsNotNull())
            {
                scoreProcessor.NewJudgement -= onJudgementChanged;
                scoreProcessor.JudgementReverted -= onJudgementChanged;
            }

            base.Dispose(isDisposing);
        }

        protected override LocalisableString FormatCount(int count)
        {
            return count.ToString();
        }

        protected override IHasText CreateText() => new TextComponent();

        private partial class TextComponent : CompositeDrawable, IHasText
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
                            Font = OsuFont.Numeric.With(size: 8),
                            Text = new TranslatableString(@"objects_remaining_counter.remaining", @"REMAINING"),
                            Padding = new MarginPadding { Bottom = 2f },
                        }
                    }
                };
            }
        }

        public bool UsesFixedAnchor { get; set; }
    }
}