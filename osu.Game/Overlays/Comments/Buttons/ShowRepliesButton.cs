// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Comments.Buttons
{
    public partial class ShowRepliesButton : CommentRepliesButton
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        public ShowRepliesButton(int count)
        {
            Count = count;
        }

        public int Count
        {
            set => Text = "reply".ToQuantity(value);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(expanded => SetIconDirection(expanded.NewValue), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Expanded.Toggle();
            return true;
        }
    }
}
