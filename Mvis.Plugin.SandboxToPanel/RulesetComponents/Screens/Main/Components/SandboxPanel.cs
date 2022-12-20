using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Main.Components
{
    public partial class SandboxPanel : CompositeDrawable
    {
        public static readonly float WIDTH = 300;

        public Action Action;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly string name;
        private readonly string textureName;
        private readonly Creator creator;

        public SandboxPanel(string name, string textureName = "", Creator creator = null)
        {
            this.name = name;
            this.textureName = textureName;
            this.creator = creator;
        }

        private Sprite texture;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Container spriteHolder;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
            Width = WIDTH;
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 3;
            BorderColour = colours.Yellow;
            EdgeEffect = new EdgeEffectParameters
            {
                Radius = 1,
                Colour = colours.Yellow,
                Hollow = true,
                Type = EdgeEffectType.Glow
            };
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f
                },
                spriteHolder = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = name,
                    Colour = colours.Yellow,
                    Font = OsuFont.GetFont(size: 40, weight: FontWeight.SemiBold)
                }
            };

            if (creator != null)
            {
                var linkFlow = new LinkFlowContainer(t =>
                {
                    t.Font = OsuFont.GetFont(size: 20);
                })
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 25 }
                };

                linkFlow.AddText("Created by ");
                linkFlow.AddLink(creator.Name, creator.URL);

                AddInternal(linkFlow);
            }

            if (!string.IsNullOrEmpty(textureName))
            {
                spriteHolder.Add(texture = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(textureName),
                    Colour = Color4.Gray
                });
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (texture != null)
                texture.FadeColour(Color4.DarkGray, 250, Easing.OutQuint);

            TweenEdgeEffectTo(new EdgeEffectParameters
            {
                Radius = 15,
                Colour = colours.Yellow,
                Hollow = true,
                Type = EdgeEffectType.Glow
            }, 250, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (texture != null)
                texture.FadeColour(Color4.Gray, 250, Easing.OutQuint);

            TweenEdgeEffectTo(new EdgeEffectParameters
            {
                Radius = 1,
                Colour = colours.Yellow,
                Hollow = true,
                Type = EdgeEffectType.Glow
            }, 250, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }
    }

    public partial class Creator
    {
        public string URL { get; private set; }

        public string Name { get; private set; }

        public Creator(string url, string name)
        {
            URL = url;
            Name = name;
        }
    }
}
