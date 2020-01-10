// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Rankings
{
    public class SpotlightSelector : Container
    {
        private readonly Box background;
        private readonly SpotlightsDropdown dropdown;
        private readonly DimmedLoadingLayer loading;

        [Resolved]
        private IAPIProvider api { get; set; }

        public SpotlightSelector()
        {
            RelativeSizeAxes = Axes.X;
            Height = 200;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                dropdown = new SpotlightsDropdown
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.8f,
                    Margin = new MarginPadding { Top = 10 }
                },
                loading = new DimmedLoadingLayer(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoam;
            dropdown.AccentColour = colours.GreySeafoamDarker;
        }

        public void FetchSpotlights()
        {
            loading.Show();

            var request = new GetSpotlightsRequest();
            request.Success += response =>
            {
                dropdown.Items = response.Spotlights;
                loading.Hide();
            };
            api.Queue(request);
        }

        private class SpotlightsDropdown : OsuDropdown<APISpotlight>
        {
            protected override DropdownMenu CreateMenu() => base.CreateMenu().With(menu => menu.MaxHeight = 400);
        }
    }
}
