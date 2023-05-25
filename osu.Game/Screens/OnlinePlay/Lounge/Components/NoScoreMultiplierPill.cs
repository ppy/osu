// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable


using osu.Framework.Bindables;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class NoScoreMultiplierPill : OnlinePlayPill
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            TextFlow.Text = "No mod multipliers";
            NoScoreMultiplier.BindValueChanged(onNoScoreMultiplierChanged, true);
        }

        private void onNoScoreMultiplierChanged(ValueChangedEvent<bool> mode)
        {
            if (mode.NewValue)
                Show();
            else
                Hide();
        }
    }
}
