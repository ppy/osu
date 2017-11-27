// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Framework.Audio;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.BeatmapSet
{
    public class HeaderButton : OsuButton
    {
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public HeaderButton()
        {
            Height = 0;
            RelativeSizeAxes = Axes.Y;

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            Masking = true;
            CornerRadius = 3;
            BackgroundColour = OsuColour.FromHex(@"094c5f");
            this.Triangles.ColourLight = OsuColour.FromHex(@"0f7c9b");
            this.Triangles.ColourDark = OsuColour.FromHex(@"094c5f");
            this.Triangles.TriangleScale = 1.5f;
        }

    }
}
