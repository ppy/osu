// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public abstract class ShowChildrenButton : OsuHoverContainer
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        protected ShowChildrenButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            Expanded.BindValueChanged(OnExpandedChanged, true);
            base.LoadComplete();
        }

        protected abstract void OnExpandedChanged(ValueChangedEvent<bool> expanded);

        protected override bool OnClick(ClickEvent e)
        {
            Expanded.Value = !Expanded.Value;
            return true;
        }
    }
}
