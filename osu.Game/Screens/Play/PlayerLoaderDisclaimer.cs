// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class PlayerLoaderDisclaimer : CompositeDrawable
    {
        private readonly LocalisableString title;
        private readonly LocalisableString content;

        public PlayerLoaderDisclaimer(LocalisableString title, LocalisableString content)
        {
            this.title = title;
            this.content = content;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Width = SettingsToolboxGroup.CONTAINER_WIDTH;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Width = 7,
                    Height = 15,
                    Margin = new MarginPadding { Top = 2 },
                    Colour = colours.Orange1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = 12 },
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2),
                    Children = new[]
                    {
                        new TextFlowContainer(t => t.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = title,
                        },
                        new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 16))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = content,
                        }
                    }
                }
            };
        }
    }
}
