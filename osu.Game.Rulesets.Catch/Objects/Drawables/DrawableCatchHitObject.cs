// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public abstract class PalpableDrawableCatchHitObject<TObject> : DrawableCatchHitObject<TObject>
        where TObject : PalpableCatchHitObject
    {
        protected Container ScaleContainer { get; private set; }

        protected PalpableDrawableCatchHitObject(TObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
            Masking = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                ScaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            });

            ScaleContainer.Scale = new Vector2(HitObject.Scale);
        }

        protected override Color4 GetComboColour(IReadOnlyList<Color4> comboColours) =>
            comboColours[(HitObject.IndexInBeatmap + 1) % comboColours.Count];
    }

    public abstract class DrawableCatchHitObject<TObject> : DrawableCatchHitObject
        where TObject : CatchHitObject
    {
        public new TObject HitObject;

        protected DrawableCatchHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
            Anchor = Anchor.BottomLeft;
        }
    }

    public abstract class DrawableCatchHitObject : DrawableHitObject<CatchHitObject>
    {
        public virtual bool StaysOnPlate => HitObject.CanBePlated;

        public float DisplayRadius => DrawSize.X / 2 * Scale.X * HitObject.Scale;

        protected override float SamplePlaybackPosition => HitObject.X / CatchPlayfield.WIDTH;

        protected DrawableCatchHitObject(CatchHitObject hitObject)
            : base(hitObject)
        {
            X = hitObject.X;
        }

        public Func<CatchHitObject, bool> CheckPosition;

        public bool IsOnPlate;

        public override bool RemoveWhenNotAlive => IsOnPlate;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (CheckPosition == null) return;

            if (timeOffset >= 0 && Result != null)
                ApplyResult(r => r.Type = CheckPosition.Invoke(HitObject) ? r.Judgement.MaxResult : r.Judgement.MinResult);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            var endTime = HitObject.GetEndTime();

            using (BeginAbsoluteSequence(endTime, true))
            {
                switch (state)
                {
                    case ArmedState.Miss:
                        this.FadeOut(250).RotateTo(Rotation * 2, 250, Easing.Out);
                        break;

                    case ArmedState.Hit:
                        this.FadeOut();
                        break;
                }
            }
        }
    }
}
