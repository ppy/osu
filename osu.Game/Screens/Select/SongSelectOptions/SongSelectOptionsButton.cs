using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select
{
    public class SongSelectOptionsButton : ClickableContainer
    {
        private static readonly Vector2 shearing = new Vector2(0.15f, 0);

        public string TextLineA
        {
            get { return spriteTextA?.Text; }
            set
            {
                if (spriteTextA != null)
                    spriteTextA.Text = value;
            }
        }

        public string TextLineB
        {
            get { return spriteTextB?.Text; }
            set
            {
                if (spriteTextB != null)
                    spriteTextB.Text = value;
            }
        }

        private SpriteText spriteTextA;
        private SpriteText spriteTextB;
        private Box box;
        private TextAwesome icon;
        public FontAwesome Icon
        {
            get { return icon.Icon; }
            set
            {
                icon.Icon = value;
            }
        }

        public new Color4 Colour
        {
            get { return box.Colour; }
            set
            {
                box.Colour = value;
            }
        }

        public Action On_Clicked;

        public SongSelectOptionsButton()
        {
            Masking = true;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = new Color4(0, 0, 0, 0.2f),
                Radius = 10,
                Roundness = 5,
            };
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(2, 0),
                    Alpha = 0.8f,
                },
                new FlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Direction = FlowDirection.VerticalOnly,
                    Shear = -shearing,
                    Position = new Vector2(-12.5f, -20),
                    Spacing = new Vector2(0, 40),
                    Children = new Drawable[]
                    {
                        icon = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 30,
                        },
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FlowDirection.VerticalOnly,
                            Children = new Drawable[]
                            {
                                spriteTextA = new SpriteText
                                {
                                    Font = @"Exo2.0-Bold",
                                    TextSize = 17,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0f, 0f, 0f, 1f),
                                },
                                spriteTextB = new SpriteText
                                {
                                    Font = @"Exo2.0-Bold",
                                    TextSize = 17,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0f, 0f, 0f, 1f),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnClick(InputState state)
        {
            On_Clicked?.Invoke();
            base.OnClick(state);
            return true;
        }
    }
}
