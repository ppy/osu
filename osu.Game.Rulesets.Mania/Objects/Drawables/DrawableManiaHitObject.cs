// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableHitObject<ManiaHitObject>
        where TObject : ManiaHitObject
    {
        /// <summary>
        /// The key that will trigger input for this hit object.
        /// </summary>
        protected ManiaAction Action { get; }

        public new TObject HitObject;

        protected DrawableManiaHitObject(TObject hitObject, ManiaAction? action = null)
            : base(hitObject)
        {
            HitObject = hitObject;

            if (action != null)
                Action = action.Value;
        }

        [BackgroundDependencyLoader]
        private void load(ScrollingInfo scrollingInfo)
        {
            Anchor = scrollingInfo.Direction == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
            Origin = Anchor;
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In).Expire();
                    break;
                case ArmedState.Hit:
                    this.FadeOut(150, Easing.OutQuint).Expire();
                    break;
            }
        }
    }
}
