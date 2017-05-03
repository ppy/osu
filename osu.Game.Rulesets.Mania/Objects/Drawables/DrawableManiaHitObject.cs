// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableHitObject<ManiaHitObject, ManiaJudgement>
        where TObject : ManiaHitObject
    {
        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                UpdateAccent();
            }
        }

        public new TObject HitObject;

        protected override Container<Drawable> Content => noteFlow;
        private FlowContainer<Drawable> noteFlow;

        public DrawableManiaHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(noteFlow = new FillFlowContainer<Drawable>
            {
                Name = "Main container",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            });
        }

        protected override ManiaJudgement CreateJudgement() => new ManiaJudgement();

        protected virtual void UpdateAccent() { }
    }
}
