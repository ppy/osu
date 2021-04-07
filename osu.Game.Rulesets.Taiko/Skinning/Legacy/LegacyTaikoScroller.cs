// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyTaikoScroller : CompositeDrawable
    {
        public Bindable<JudgementResult> LastResult = new Bindable<JudgementResult>();

        public LegacyTaikoScroller()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(GameplayBeatmap gameplayBeatmap)
        {
            if (gameplayBeatmap != null)
                ((IBindable<JudgementResult>)LastResult).BindTo(gameplayBeatmap.LastJudgementResult);
        }

        private bool passing;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LastResult.BindValueChanged(result =>
            {
                var r = result.NewValue;

                // always ignore hitobjects that don't affect combo (drumroll ticks etc.)
                if (r?.Type.AffectsCombo() == false)
                    return;

                passing = r == null || r.IsHit;

                foreach (var sprite in InternalChildren.OfType<ScrollerSprite>())
                    sprite.Passing = passing;
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            // store X before checking wide enough so if we perform layout there is no positional discrepancy.
            float currentX = (InternalChildren?.FirstOrDefault()?.X ?? 0) - (float)Clock.ElapsedFrameTime * 0.1f;

            // ensure we have enough sprites
            if (!InternalChildren.Any()
                || InternalChildren.First().ScreenSpaceDrawQuad.Width * InternalChildren.Count < ScreenSpaceDrawQuad.Width * 2)
                AddInternal(new ScrollerSprite { Passing = passing });

            var first = InternalChildren.First();
            var last = InternalChildren.Last();

            foreach (var sprite in InternalChildren)
            {
                // add the x coordinates and perform re-layout on all sprites as spacing may change with gameplay scale.
                sprite.X = currentX;
                currentX += sprite.DrawWidth;
            }

            if (first.ScreenSpaceDrawQuad.TopLeft.X >= ScreenSpaceDrawQuad.TopLeft.X)
            {
                foreach (var internalChild in InternalChildren)
                    internalChild.X -= first.DrawWidth;
            }

            if (last.ScreenSpaceDrawQuad.TopRight.X <= ScreenSpaceDrawQuad.TopRight.X)
            {
                foreach (var internalChild in InternalChildren)
                    internalChild.X += first.DrawWidth;
            }
        }

        private class ScrollerSprite : CompositeDrawable
        {
            private Sprite passingSprite;
            private Sprite failingSprite;

            private bool passing = true;

            public bool Passing
            {
                get => passing;
                set
                {
                    if (value == passing)
                        return;

                    passing = value;

                    if (IsLoaded)
                        updatePassing();
                }
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                FillMode = FillMode.Fit;

                InternalChildren = new Drawable[]
                {
                    passingSprite = new Sprite { Texture = skin.GetTexture("taiko-slider") },
                    failingSprite = new Sprite { Texture = skin.GetTexture("taiko-slider-fail"), Alpha = 0 },
                };

                updatePassing();
            }

            protected override void Update()
            {
                base.Update();

                foreach (var c in InternalChildren)
                    c.Scale = new Vector2(DrawHeight / c.Height);
            }

            private void updatePassing()
            {
                if (passing)
                {
                    passingSprite.Show();
                    failingSprite.FadeOut(200);
                }
                else
                {
                    failingSprite.FadeIn(200);
                    passingSprite.Delay(200).FadeOut();
                }
            }
        }
    }
}
