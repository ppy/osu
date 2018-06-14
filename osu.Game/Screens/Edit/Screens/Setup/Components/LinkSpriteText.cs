// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Framework.Input;
using System.Diagnostics;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class LinkSpriteText : OsuHoverContainer
    {
        private readonly OsuSpriteText label;

        public string Link;

        public const float DEFAULT_TEXT_SIZE = 14;

        public float TextSize
        {
            get => label.TextSize;
            set
            {
                if (label.TextSize == value) return;
                label.TextSize = value;
            }
        }

        public string Text
        {
            get => label.Text;
            set
            {
                if (label.Text == value) return;
                label.Text = value;
            }
        }

        public LinkSpriteText()
        {
            AutoSizeAxes = Axes.Both;
            Child = label = new OsuSpriteText
            {
                Font = @"Exo2.0-Bold",
                TextSize = DEFAULT_TEXT_SIZE
            };
        }

        protected override bool OnClick(InputState state)
        {
            if (Link != null)
                Process.Start(new ProcessStartInfo
                {
                    FileName = Link,
                    UseShellExecute = true //see https://github.com/dotnet/corefx/issues/10361
                });
            return true;
        }
    }
}
