// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public abstract partial class ManiaSelectionBlueprint<T> : HitObjectSelectionBlueprint<T>
        where T : ManiaHitObject
    {
        [Resolved]
        private Playfield playfield { get; set; } = null!;

        protected ScrollingHitObjectContainer HitObjectContainer => ((ManiaPlayfield)playfield).GetColumn(HitObject.Column).HitObjectContainer;

        protected ManiaSelectionBlueprint(T hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        private readonly IBindable<ScrollingDirection> directionBindable = new Bindable<ScrollingDirection>();

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            directionBindable.BindTo(scrollingInfo.Direction);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            directionBindable.BindValueChanged(OnDirectionChanged, true);
        }

        protected abstract void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction);

        protected override void Update()
        {
            base.Update();

            Position = Parent!.ToLocalSpace(HitObjectContainer.ScreenSpacePositionAtTime(HitObject.StartTime)) - AnchorPosition;
            Width = HitObjectContainer.DrawWidth;
        }
    }
}
