// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
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

        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        protected DrawableManiaHitObject(TObject hitObject, ManiaAction? action = null)
            : base(hitObject)
        {
            HitObject = hitObject;

            if (action != null)
                Action = action.Value;
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            Direction.BindTo(scrollingInfo.Direction);
            Direction.BindValueChanged(OnDirectionChanged, true);
        }

        protected virtual void OnDirectionChanged(ScrollingDirection direction)
        {
            Anchor = Origin = direction == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
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
