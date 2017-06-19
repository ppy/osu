// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableScrollingHitObject<ManiaHitObject, ManiaJudgement>
        where TObject : ManiaHitObject
    {
        /// <summary>
        /// The key that will trigger input for this hit object.
        /// </summary>
        protected Bindable<Key> Key { get; private set; } = new Bindable<Key>();

        public new TObject HitObject;

        protected DrawableManiaHitObject(TObject hitObject, Bindable<Key> key = null)
            : base(hitObject)
        {
            HitObject = hitObject;

            if (key != null)
                Key.BindTo(key);

            RelativePositionAxes = Axes.Y;
            Y = (float)HitObject.StartTime;
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;
            }
        }

        protected override ManiaJudgement CreateJudgement() => new ManiaJudgement();
    }
}
