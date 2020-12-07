// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Profile.Sections
{
    public class SpectateSection : ProfileSection
    {
        public override string Title => "Spectate";
        public override string Identifier => "spectate";

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        public SpectateSection()
        {
            Children = new[]
            {
                new PurpleTriangleButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Watch",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Action = () => game?.PerformFromScreen(s => s.Push(new Spectator(User.Value))),
                }
            };
        }
    }
}
