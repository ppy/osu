// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public abstract class ShowChildrenButton : OsuHoverContainer
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        protected ShowChildrenButton()
        {
            AutoSizeAxes = Axes.Both;
            Action = () => Expanded.Value = !Expanded.Value;

            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }

        protected override void LoadComplete()
        {
            Expanded.BindValueChanged(OnExpandedChanged, true);
            base.LoadComplete();
        }

        protected virtual void OnExpandedChanged(ValueChangedEvent<bool> expanded)
        {
        }

        protected override bool OnClick(ClickEvent e)
        {
            Expanded.Value = !Expanded.Value;
            return true;
        }
    }
}
