// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftReadyButton : ReadyButton
    {
        [Resolved(typeof(Room), nameof(Room.EndDate))]
        private Bindable<DateTimeOffset?> endDate { get; set; }

        public TimeshiftReadyButton()
        {
            Text = "Start";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Green;
            Triangles.ColourDark = colours.Green;
            Triangles.ColourLight = colours.GreenLight;
        }

        protected override void Update()
        {
            base.Update();

            Enabled.Value = endDate.Value != null && DateTimeOffset.UtcNow.AddSeconds(30).AddMilliseconds(GameBeatmap.Value.Track.Length) < endDate.Value;
        }
    }
}
