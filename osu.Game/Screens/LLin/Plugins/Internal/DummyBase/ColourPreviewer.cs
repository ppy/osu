using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.DummyBase
{
    public partial class ColourPreviewer : Container
    {
        private readonly CustomColourProvider provider = new CustomColourProvider();
        private Box bg6 = null!;
        private Box bg5 = null!;
        private Box bg4 = null!;
        private Box bg3 = null!;
        private Box bg2 = null!;
        private Box bg1 = null!;
        private Box hl = null!;
        private Box l4 = null!;
        private Box l3 = null!;
        private Box c2 = null!;
        private OsuSpriteText hueText = null!;

        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            iR.BindValueChanged(_ => updateColor());
            iG.BindValueChanged(_ => updateColor());
            iB.BindValueChanged(_ => updateColor(), true);
        }

        private void updateColor()
        {
            provider.UpdateHueColor(iR.Value, iG.Value, iB.Value);

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
