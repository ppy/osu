// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osuTK.Graphics;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Comments
{
    public abstract class ShowChildrenButton : OsuHoverContainer
    {
        public readonly BindableBool Expanded = new BindableBool(true);
        public readonly Bindable<List<Comment>> ChildComments = new Bindable<List<Comment>>();

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
            ChildComments.BindValueChanged(OnChildrenChanged, true);
            base.LoadComplete();
        }

        protected abstract void OnExpandedChanged(ValueChangedEvent<bool> expanded);
        protected abstract void OnChildrenChanged(ValueChangedEvent<List<Comment>> children);
    }
}
