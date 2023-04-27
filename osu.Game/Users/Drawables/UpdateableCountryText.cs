// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Framework.Extensions;

namespace osu.Game.Users.Drawables
{
    public partial class UpdateableCountryText : OsuHoverContainer
    {
        
        public bool ShowPlaceholderOnUnknown = true;

        [Resolved]
        private RankingsOverlay? rankingsOverlay { get; set; }
        public UpdateableCountryText()
        {
            AutoSizeAxes = Axes.Both;
        }

        // [BackgroundDependencyLoader]
        public void load(CountryCode countryCode)
        {
            Action = () =>
            {
                rankingsOverlay?.ShowCountry(countryCode);
            };

            Child = new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 14f, weight: FontWeight.Regular),
                Text = countryCode.GetDescription(),
            };
        }
    }
    
}