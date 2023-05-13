// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatTextBox : HistoryTextBox
    {
        public readonly BindableBool ShowSearch = new BindableBool();

        public override bool HandleLeftRightArrows => !ShowSearch.Value;

        protected override bool ClearTextOnBackKey => false;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowSearch.BindValueChanged(change =>
            {
                bool showSearch = change.NewValue;

                PlaceholderText = showSearch ? HomeStrings.SearchPlaceholder : ChatStrings.InputPlaceholder;
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
