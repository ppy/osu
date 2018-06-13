// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
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
    public class GeneralScreenBottomHeader : Container
    {

        public GeneralScreenBottomHeader()
        {
            Child = new Container
            {
                Children = new[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Colour = Color4.Yellow,
                                TextSize = 14,
                                Font = @"Exo2.0-BoldItalic",
                                Text = @"Due to large number of beatmap submissions, the standard of approval is relatively high."
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Colour = Color4.Yellow,
                                TextSize = 14,
                                Font = @"Exo2.0-BoldItalic",
                                Text = @"Please ensure your beatmap is at least timed properly, or it will likely be ignored."
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Spacing = new Vector2(20),
                        Children = new[]
                        {
                            new LinkSpriteText
                            {
                                Text = @"Editor Guide",
                                Link = @"https://osu.ppy.sh/help/wiki/Beatmapping",
                            },
                            new LinkSpriteText
                            {
                                Text = @"Official Submission Criteria",
                                Link = @"https://osu.ppy.sh/help/wiki/Ranking_Criteria",
                            }
                        }
                    }
                }
            };
        }
    }
}
