// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Primitives;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class HitObjectSelectionBlueprint : SelectionBlueprint<HitObject>
    {
        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="HitObjectSelectionBlueprint"/> applies to.
        /// </summary>
        public DrawableHitObject DrawableObject { get; internal set; }

        /// <summary>
        /// Whether the blueprint should be shown even when the <see cref="DrawableObject"/> is not alive.
        /// </summary>
        protected virtual bool AlwaysShowWhenSelected => false;

        /// <summary>
        /// Whether extra animations should be shown to convey hit position / state in addition to gameplay animations.
        /// </summary>
        protected Bindable<bool> ShowHitMarkers { get; private set; }

        protected override bool ShouldBeAlive => (DrawableObject?.IsAlive == true && DrawableObject.IsPresent) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        protected HitObjectSelectionBlueprint(HitObject hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            ShowHitMarkers = config.GetBindable<bool>(OsuSetting.EditorShowHitMarkers);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.ScreenSpaceDrawQuad.Centre;

        public override Quad SelectionQuad => DrawableObject.ScreenSpaceDrawQuad;
    }

    public abstract partial class HitObjectSelectionBlueprint<T> : HitObjectSelectionBlueprint
        where T : HitObject
    {
        public T HitObject => (T)Item;

        protected HitObjectSelectionBlueprint(T item)
            : base(item)
        {
        }
    }
}
