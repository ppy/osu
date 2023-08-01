// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    /// <summary>
    /// A header used in the multiplayer interface which shows text / details beneath a line.
    /// </summary>
    public partial class OverlinedHeader : OnlinePlayComposite
    {
        private bool showLine = true;

        public bool ShowLine
        {
            get => showLine;
            set
            {
                showLine = value;
                line.Alpha = value ? 1 : 0;
            }
        }

        public Bindable<string> Details = new Bindable<string>();

        private readonly Circle line;
        private readonly OsuSpriteText details;

        public OverlinedHeader(LocalisableString title)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Margin = new MarginPadding { Bottom = 5 };

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    line = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Margin = new MarginPadding { Bottom = 2 }
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Top = 5 },
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = title,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold)
                            },
                            details = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold)
                            },
                        }
                    },
                }
            };

            Details.BindValueChanged(val => details.Text = val.NewValue);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            line.Colour = colours.Yellow;
            details.Colour = colours.Yellow;
        }
    }
}
