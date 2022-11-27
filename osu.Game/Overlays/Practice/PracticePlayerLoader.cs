// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    [Cached]
    public partial class PracticePlayerLoader : PlayerLoader
    {
        public PracticePlayerLoader()
            : base(() => new PracticePlayer())
        {
        }

        public BindableNumber<double> CustomStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = .001
        };

        public BindableNumber<double> CustomEnd = new BindableNumber<double>(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = .001
        };
    }
}
