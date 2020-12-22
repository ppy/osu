// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRoll : DrawableTaikoStrongableHitObject<DrumRoll, DrumRoll.StrongNestedHit>
    {
        /// <summary>
        /// Number of rolling hits required to reach the dark/final colour.
        /// </summary>
        private const int rolling_hits_for_engaged_colour = 5;

        /// <summary>
        /// Rolling number of tick hits. This increases for hits and decreases for misses.
        /// </summary>
        private int rollingHits;

        private readonly Container tickContainer;

        private Color4 colourIdle;
        private Color4 colourEngaged;

        public DrawableDrumRoll()
            : this(null)
        {
        }

        public DrawableDrumRoll([CanBeNull] DrumRoll drumRoll)
            : base(drumRoll)
        {
            RelativeSizeAxes = Axes.Y;

            Content.Add(tickContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            colourIdle = colours.YellowDark;
            colourEngaged = colours.YellowDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            OnNewResult += onNewResult;
        }

        protected override void RecreatePieces()
        {
            base.RecreatePieces();
            updateColour();
        }

        protected override void OnFree()
        {
            base.OnFree();
            rollingHits = 0;
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableDrumRollTick tick:
                    tickContainer.Add(tick);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            tickContainer.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case DrumRollTick tick:
                    return new DrawableDrumRollTick(tick);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.DrumRollBody),
            _ => new ElongatedCirclePiece());

        public override bool OnPressed(TaikoAction action) => false;

        private void onNewResult(DrawableHitObject obj, JudgementResult result)
        {
            if (!(obj is DrawableDrumRollTick))
                return;

            if (result.IsHit)
                rollingHits++;
            else
                rollingHits--;

            rollingHits = Math.Clamp(rollingHits, 0, rolling_hits_for_engaged_colour);

            updateColour(100);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (userTriggered)
                return;

            if (timeOffset < 0)
                return;

            int countHit = NestedHitObjects.Count(o => o.IsHit);

            if (countHit >= HitObject.RequiredGoodHits)
            {
                ApplyResult(r => r.Type = countHit >= HitObject.RequiredGreatHits ? HitResult.Great : HitResult.Ok);
            }
            else
                ApplyResult(r => r.Type = r.Judgement.MinResult);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                case ArmedState.Miss:
                    this.FadeOut(100);
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            OriginPosition = new Vector2(DrawHeight);
            Content.X = DrawHeight / 2;
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(DrumRoll.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        private void updateColour(double fadeDuration = 0)
        {
            Color4 newColour = Interpolation.ValueAt((float)rollingHits / rolling_hits_for_engaged_colour, colourIdle, colourEngaged, 0, 1);
            (MainPiece.Drawable as IHasAccentColour)?.FadeAccent(newColour, fadeDuration);
        }

        public class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableDrumRoll ParentHitObject => (DrawableDrumRoll)base.ParentHitObject;

            public StrongNestedHit()
                : this(null)
            {
            }

            public StrongNestedHit([CanBeNull] DrumRoll.StrongNestedHit nestedHit)
                : base(nestedHit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!ParentHitObject.Judged)
                    return;

                ApplyResult(r => r.Type = ParentHitObject.IsHit ? r.Judgement.MaxResult : r.Judgement.MinResult);
            }

            public override bool OnPressed(TaikoAction action) => false;
        }
    }
}
