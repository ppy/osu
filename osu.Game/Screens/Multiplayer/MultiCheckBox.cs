// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiCheckBox : OsuCheckbox
    {
        public MultiCheckBox() : base(1)
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            LabelPadding = new MarginPadding { Left = 17 };
            LabelFont = @"Exo2.0-Bold";
            LabelColor = Color4.Gold;

            NubAnchor = Anchor.CentreLeft;
            NubOrigin = Anchor.CentreLeft;
            NubMargin = new MarginPadding { Top = 2 };
        }
    }
}
