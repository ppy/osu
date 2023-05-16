// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class QueueModePill : OnlinePlayPill
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            QueueMode.BindValueChanged(onQueueModeChanged, true);
        }

        private void onQueueModeChanged(ValueChangedEvent<QueueMode> mode)
        {
            TextFlow.Clear();
            TextFlow.AddText(mode.NewValue.GetLocalisableDescription());
        }
    }
}
