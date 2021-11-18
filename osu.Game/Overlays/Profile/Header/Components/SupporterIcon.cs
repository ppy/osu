// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class SupporterIcon : OsuClickableContainer
    {
        private readonly Box background;
        private readonly FillFlowContainer iconContainer;
        private readonly CircularContainer content;

        public override LocalisableString TooltipText => UsersStrings.ShowIsSupporter;

        public int SupportLevel
        {
            set
            {
                int count = Math.Clamp(value, 0, 3);

                if (count == 0)
                {
                    content.Hide();
                }
                else
                {
                    content.Show();
                    iconContainer.Clear();

                    for (int i = 0; i < count; i++)
                    {
                        iconContainer.Add(new SpriteIcon
                        {
                            Width = 12,
                            RelativeSizeAxes = Axes.Y,
                            Icon = FontAwesome.Solid.Heart,
                        });
                    }

                    iconContainer.Padding = new MarginPadding { Horizontal = DrawHeight / 2 };
                }
            }
        }

        public SupporterIcon()
        {
            AutoSizeAxes = Axes.X;

            Child = content = new CircularContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                Alpha = 0,
                Children = new Drawable[]
                {
                    background = new Box { RelativeSizeAxes = Axes.Both },
                    iconContainer = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        Height = 0.6f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                }
            };
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game)
        {
            background.Colour = colours.Pink;

            Action = () => game?.OpenUrlExternally(@"/home/support");
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(colours.PinkLight, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(colours.Pink, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
