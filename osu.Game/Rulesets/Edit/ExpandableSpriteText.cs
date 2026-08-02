// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Edit
{
    internal partial class ExpandableSpriteText : OsuSpriteText, IExpandable
    {
        public BindableBool Expanded { get; } = new BindableBool();

        [Resolved(canBeNull: true)]
        private IExpandingContainer? expandingContainer { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
            {
                Expanded.Value = containerExpanded.NewValue;
            }, true);

            Expanded.BindValueChanged(expanded =>
            {
                this.FadeTo(expanded.NewValue ? 1 : 0, 150, Easing.OutQuint);
            }, true);
        }
    }
}
