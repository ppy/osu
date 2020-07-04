// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="Judgements.Judgement"/>.
    /// </summary>
    public class DrawableJudgement : PoolableDrawable
    {
        private const float judgement_size = 128;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Cached drawableCache = new Cached();

        private JudgementResult result;

        public JudgementResult Result
        {
            get => result;
            set
            {
                if (result?.Type == value.Type)
                    return;

                result = value;
                drawableCache.Invalidate();
            }
        }

        public DrawableHitObject JudgedObject;

        protected Container JudgementBody;
        protected SpriteText JudgementText;

        /// <summary>
        /// Duration of initial fade in.
        /// </summary>
        protected virtual double FadeInDuration => 100;

        /// <summary>
        /// Duration to wait until fade out begins. Defaults to <see cref="FadeInDuration"/>.
        /// </summary>
        protected virtual double FadeOutDelay => FadeInDuration;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="Judgements.Judgement"/>.
        /// </summary>
        /// <param name="result">The judgement to visualise.</param>
        /// <param name="judgedObject">The object which was judged.</param>
        public DrawableJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : this()
        {
            Result = result;
            JudgedObject = judgedObject;
        }

        public DrawableJudgement()
        {
            Size = new Vector2(judgement_size);
        }

        protected virtual void ApplyHitAnimations()
        {
            JudgementBody.ScaleTo(0.9f);
            JudgementBody.ScaleTo(1, 500, Easing.OutElastic);

            this.Delay(FadeOutDelay).FadeOut(400);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            prepareDrawables();
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Result != null);

            if (!drawableCache.IsValid)
                prepareDrawables();

            this.FadeInFromZero(FadeInDuration, Easing.OutQuint);
            JudgementBody.ScaleTo(1);
            JudgementBody.RotateTo(0);
            JudgementBody.MoveTo(Vector2.Zero);

            switch (Result.Type)
            {
                case HitResult.None:
                    break;

                case HitResult.Miss:
                    JudgementBody.ScaleTo(1.6f);
                    JudgementBody.ScaleTo(1, 100, Easing.In);

                    JudgementBody.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);
                    JudgementBody.RotateTo(40, 800, Easing.InQuint);

                    this.Delay(600).FadeOut(200);
                    break;

                default:
                    ApplyHitAnimations();
                    break;
            }

            Expire(true);
        }

        private void prepareDrawables()
        {
            var type = Result?.Type ?? HitResult.Perfect; //TODO: better default type from ruleset

            InternalChild = JudgementBody = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = new SkinnableDrawable(new GameplaySkinComponent<HitResult>(type), _ => JudgementText = new OsuSpriteText
                {
                    Text = type.GetDescription().ToUpperInvariant(),
                    Font = OsuFont.Numeric.With(size: 20),
                    Colour = colours.ForHitResult(type),
                    Scale = new Vector2(0.85f, 1),
                }, confineMode: ConfineMode.NoScaling)
            };

            drawableCache.Validate();
        }
    }
}
