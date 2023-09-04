// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class MatchTypePill : OnlinePlayPill
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Type.BindValueChanged(onMatchTypeChanged, true);
        }

        private void onMatchTypeChanged(ValueChangedEvent<MatchType> type)
        {
            TextFlow.Text = type.NewValue.GetLocalisableDescription();
        }
    }
}
