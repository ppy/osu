// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using OpenTK;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantInfo : Container
    {
        private readonly Container flagContainer;
        private readonly OsuSpriteText host;
        private readonly FillFlowContainer levelRangeContainer;
        private readonly OsuSpriteText levelRangeLower;
        private readonly OsuSpriteText levelRangeHigher;

        public User Host
        {
            set
            {
                host.Text = value.Username;
                flagContainer.Children = new[] { new DrawableFlag(value.Country) { RelativeSizeAxes = Axes.Both } };
            }
        }

        public IEnumerable<User> Participants
        {
            set
            {
                var ranks = value.Select(u => u.Statistics.Ranks.Global);
                levelRangeLower.Text = ranks.Min().ToString();
                levelRangeHigher.Text = ranks.Max().ToString();
            }
        }

        public ParticipantInfo(string rankPrefix = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = 15f;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5f, 0f),
                    Children = new Drawable[]
                    {
                        flagContainer = new Container
                        {
                            Width = 22f,
                            RelativeSizeAxes = Axes.Y,
                        },
                        new Container //todo: team banners
                        {
                            Width = 38f,
                            RelativeSizeAxes = Axes.Y,
                            CornerRadius = 2f,
                            Masking = true,
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex(@"ad387e"),
                                },
                            },
                        },
                        new OsuSpriteText
                        {
                            Text = "hosted by",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextSize = 14,
                        },
                        host = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextSize = 14,
                            Font = @"Exo2.0-BoldItalic",
                        },
                    },
                },
                levelRangeContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = rankPrefix,
                            TextSize = 14,
                        },
                        new OsuSpriteText
                        {
                            Text = "#",
                            TextSize = 14,
                        },
                        levelRangeLower = new OsuSpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                        },
                        new OsuSpriteText
                        {
                            Text = " - ",
                            TextSize = 14,
                        },
                        new OsuSpriteText
                        {
                            Text = "#",
                            TextSize = 14,
                        },
                        levelRangeHigher = new OsuSpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            levelRangeContainer.Colour = colours.Gray9;
            host.Colour = colours.Blue;
        }
    }
}
