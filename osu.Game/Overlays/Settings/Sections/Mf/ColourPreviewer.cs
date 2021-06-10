using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class ColourPreviewer : Container
    {
        private readonly CustomColourProvider provider = new CustomColourProvider(0, 0, 0);
        private Box bg6;
        private Box bg5;
        private Box bg4;
        private Box bg3;
        private Box bg2;
        private Box bg1;
        private Box hl;
        private Box l4;
        private Box l3;
        private Box c2;
        private OsuSpriteText hueText;

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 75;
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Horizontal = 15 };
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Children = new Drawable[]
                    {
                        bg5 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        bg4 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        bg3 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        bg2 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        bg1 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Margin = new MarginPadding { Top = 25 },
                    Children = new Drawable[]
                    {
                        bg6 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        hl = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        l4 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        l3 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                        c2 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.2f
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Margin = new MarginPadding { Top = 50 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black
                        },
                        hueText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                }
            };
        }

        public void UpdateColor(float r, float g, float b)
        {
            provider.UpdateHueColor(r, g, b);

            bg5.Colour = provider.Background5;
            bg4.Colour = provider.Background4;
            bg3.Colour = provider.Background3;
            bg2.Colour = provider.Background2;
            bg1.Colour = provider.Background1;

            bg6.Colour = provider.Background6;
            hl.Colour = provider.Highlight1;
            l4.Colour = provider.Light4;
            l3.Colour = provider.Light3;
            c2.Colour = provider.Content2;

            hueText.Text = $"Hue: {(provider.HueColour.Value * 360):#0.00}";
        }
    }
}
