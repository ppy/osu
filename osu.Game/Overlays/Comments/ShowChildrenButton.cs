// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public abstract class ShowChildrenButton : CommentActionButton
    {
        public readonly BindableBool Expanded = new BindableBool(true);

        protected override void LoadComplete()
        {
            Action = Expanded.Toggle;

            Expanded.BindValueChanged(OnExpandedChanged, true);
            base.LoadComplete();
        }

        protected abstract void OnExpandedChanged(ValueChangedEvent<bool> expanded);
    }
}
