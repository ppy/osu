// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayerLoader : PlayerLoader
    {
        public PracticePlayerLoader()
        {
            //Creating the player this way allows for avoiding DI
            CreatePlayer = () => new PracticePlayer(this);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            MetadataInfo.Add(new OsuSpriteText
            {
                Y = 20,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 25),
                Colour = colour.Green,
                Text = PracticePlayerLoaderStrings.PracticeMode
            });
        }

        public BindableNumber<double> CustomStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.001f
        };

        public BindableNumber<double> CustomEnd = new BindableNumber<double>(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.001f
        };

        //We want the practice overlay to show itself automatically on the first attempt
        public bool IsFirstTry { get; set; } = true;
    }
}
