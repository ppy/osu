using M.Resources.Fonts;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Mf
{
    public partial class FontInfoTooltip : VisibilityContainer, ITooltip
    {
        private static readonly FontUsage default_font = new FontUsage("Noto-CJK-Basic");

        private readonly BasicInfoLine name;
        private readonly BasicInfoLine author;
        private readonly BasicInfoLine homepage;
        private readonly BasicInfoLine license;
        private readonly BasicInfoLine familyName;

        private readonly OsuSpriteText preview1;
        private readonly OsuSpriteText preview2;

        private readonly BoolInfoLine light;
        private readonly BoolInfoLine medium;
        private readonly BoolInfoLine semiBold;
        private readonly BoolInfoLine bold;
        private readonly BoolInfoLine black;
        private readonly BasicInfoLine desc;

        public FontInfoTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding(10),
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        preview1 = new OsuSpriteText
                        {
                            Text = "我能吞下玻璃而不伤身体。",
                            Font = default_font
                        },
                        preview2 = new OsuSpriteText
                        {
                            Text = "The quick brown fox jumps over the lazy dog.",
                            Font = default_font
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = Color4.DarkGray.Opacity(0.3f)
                        },
                        name = new BasicInfoLine("名称"),
                        desc = new BasicInfoLine("描述"),
                        author = new BasicInfoLine("作者"),
                        homepage = new BasicInfoLine("主页"),
                        license = new BasicInfoLine("许可证"),
                        familyName = new BasicInfoLine("家族名"),
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = Color4.DarkGray.Opacity(0.3f)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                light = new BoolInfoLine("Light"),
                                medium = new BoolInfoLine("Medium"),
                                semiBold = new BoolInfoLine("SemiBold"),
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                bold = new BoolInfoLine("Bold"),
                                black = new BoolInfoLine("Black"),
                            }
                        }
                    }
                }
            };
        }

        private partial class BasicInfoLine : InfoLine<string>
        {
            protected override Anchor NameAnchor => Anchor.TopLeft;
            protected override Anchor NameOrigin => Anchor.TopLeft;

            public BasicInfoLine(string keyName)
                : base(keyName)
            {
            }

            public override string Value
            {
                set => valueText.Text = value;
            }

            private readonly OsuSpriteText valueText = new OsuSpriteText
            {
                Font = default_font,
                MaxWidth = 500
            };

            protected override Drawable CreateValueIndicator() => valueText;
        }

        private partial class BoolInfoLine : InfoLine<bool>
        {
            public BoolInfoLine(string keyName)
                : base(keyName)
            {
            }

            public override bool Value
            {
                set =>
                    icon.Icon = value
                        ? FontAwesome.Solid.Check
                        : FontAwesome.Solid.Times;
            }

            private readonly SpriteIcon icon = new SpriteIcon
            {
                Size = new Vector2(14),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft
            };

            protected override Drawable CreateValueIndicator() => icon;
        }

        private abstract partial class InfoLine<T> : FillFlowContainer
        {
            public abstract T Value { set; }

            protected abstract Drawable CreateValueIndicator();
            protected virtual Anchor NameAnchor => Anchor.CentreLeft;
            protected virtual Anchor NameOrigin => Anchor.CentreLeft;

            protected InfoLine(string keyName)
            {
                AutoSizeAxes = Axes.Both;
                MaximumSize = new Vector2(1, 100);
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5);

                InternalChildren = new[]
                {
                    new OsuSpriteText
                    {
                        Font = default_font,
                        Text = keyName,
                        Anchor = NameAnchor,
                        Origin = NameOrigin
                    },
                    CreateValueIndicator()
                };
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(200);
        }

        protected override void PopOut()
        {
            this.FadeOut(200);
        }

        public void SetContent(object content)
        {
            if (!(content is Font font)) return;

            name.Value = font.Name;
            desc.Value = font.Description;
            author.Value = font.Author;
            homepage.Value = font.Homepage;
            license.Value = font.License;
            familyName.Value = font.FamilyName;

            preview1.Font = preview2.Font = new FontUsage($"{font.FamilyName}-Regular");

            light.Value = font.LightAvaliable;
            medium.Value = font.MediumAvaliable;
            semiBold.Value = font.SemiBoldAvaliable;
            bold.Value = font.BoldAvaliable;
            black.Value = font.BlackAvaliable;
        }

        public void Move(Vector2 pos)
        {
            this.MoveTo(pos, 200, Easing.OutQuint);
        }
    }
}
