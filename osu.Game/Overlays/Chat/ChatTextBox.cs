// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Chat
{
    public class ChatTextBox : FocusedTextBox
    {
        public readonly BindableBool ShowSearch = new BindableBool();

        public override bool HandleLeftRightArrows => !ShowSearch.Value;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowSearch.BindValueChanged(change =>
            {
                bool showSearch = change.NewValue;

                PlaceholderText = showSearch ? "type here to search" : "type here";
                Text = string.Empty;
            }, true);
        }

        protected override void Commit()
        {
            if (ShowSearch.Value)
                return;

            base.Commit();
        }
    }
}
