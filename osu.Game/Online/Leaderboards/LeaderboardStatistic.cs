// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardStatistic : Container
    {
        public FillDirection Direction
        {
            get => fillFlowContainer.Direction;
            set
            {
                fillFlowContainer.Direction = value;
                updateDirection();
            }
        }

        private readonly bool perfect;

        private readonly Container content;
        private readonly FillFlowContainer fillFlowContainer;

        private readonly OsuSpriteText nameText;
        private readonly OsuSpriteText valueText;

        public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

        public LeaderboardStatistic(LocalisableString name, LocalisableString value, bool perfect, float? minWidth = null)
        {
            this.perfect = perfect;

            AutoSizeAxes = Axes.Both;
            Child = content = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    fillFlowContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new[]
                        {
                            nameText = new OsuSpriteText
                            {
                                Text = name,
                                Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold),
                            },
                            valueText = new OsuSpriteText
                            {
                                BypassAutoSizeAxes = Axes.X,
                                Text = value,
                                Font = OsuFont.Style.Body,
                            },
                        }
                    },
                },
            };

            if (minWidth != null)
                Add(Empty().With(d => d.Width = minWidth.Value));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            nameText.Colour = colourProvider.Content2;
            valueText.Colour = perfect ? colours.Lime1 : Color4.White;
        }

        private void updateDirection()
        {
            if (Direction == FillDirection.Vertical)
            {
                // We don't want the value setting the horizontal size, since it leads to wonky accuracy container length,
                // since the accuracy is sometimes longer than its name.
                valueText.BypassAutoSizeAxes = Axes.X;

                nameText.Anchor = nameText.Origin = Anchor.TopLeft;
                valueText.Anchor = valueText.Origin = Anchor.TopLeft;

                fillFlowContainer.Spacing = Vector2.Zero;
            }
            else
            {
                // When laid out horizontally, both name and value need to contribute to the horizontal size.
                valueText.BypassAutoSizeAxes = Axes.None;

                nameText.Anchor = nameText.Origin = Anchor.BottomLeft;
                valueText.Anchor = valueText.Origin = Anchor.BottomLeft;

                fillFlowContainer.Spacing = new Vector2(5);
            }
        }
    }
}
