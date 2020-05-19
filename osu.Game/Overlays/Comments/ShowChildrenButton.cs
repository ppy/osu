// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Comments
{
    public abstract class ShowChildrenButton : OsuHoverContainer
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        protected ShowChildrenButton()
        {
            AutoSizeAxes = Axes.Both;
            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }

        protected override void LoadComplete()
        {
            Action = Expanded.Toggle;

            Expanded.BindValueChanged(OnExpandedChanged, true);
            base.LoadComplete();
        }

        protected abstract void OnExpandedChanged(ValueChangedEvent<bool> expanded);
    }
}
