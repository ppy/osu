// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Components
{
    public partial class TournamentsRoomFooterButton : FooterButton
    {
        public TournamentsTabs TabType;
        public LocalisableString TabText;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Green;
            DeselectedColour = SelectedColour.Opacity(1.0f);

            TextContainer.Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AlwaysPresent = true,
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    new OsuSpriteText
                    {
                        AlwaysPresent = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = TabText,
                    },
                }
            });
        }
    }
}
