// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticePlayerLoader : PlayerLoader
    {
        public PracticePlayerLoader()
        {
            //Creating the player this way allows for avoiding DI
            CreatePlayer = () => new PracticePlayer(this);
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
