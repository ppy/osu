// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Comments.Buttons
{
    public class ShowRepliesButton : CommentRepliesButton
    {
        public Action Action;

        public readonly BindableBool Expanded = new BindableBool(true);

        public ShowRepliesButton(int count)
        {
            Text = "reply".ToQuantity(count);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(expanded => SetIconDirection(expanded.NewValue), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Expanded.Toggle();
            Action?.Invoke();
            return base.OnClick(e);
        }
    }
}
