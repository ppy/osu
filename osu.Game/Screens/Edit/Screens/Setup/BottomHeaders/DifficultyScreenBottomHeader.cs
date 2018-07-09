// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.BottomHeaders
{
    public class DifficultyScreenBottomHeader : Container
    {
        private readonly OsuSpriteText textLine;

        public DifficultyScreenBottomHeader()
        {
            Width = Setup.SIZE_X - Setup.SCREEN_LEFT_PADDING;

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Children = new[]
                    {
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Children = new[]
                            {
                                textLine = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    TextSize = 12,
                                    Font = @"Exo2.0-BoldItalic",
                                    Text = @"Hold the Shift key for precise value adjustment."
                                },
                            }
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            textLine.Colour = osuColour.Yellow;
        }
    }
}
