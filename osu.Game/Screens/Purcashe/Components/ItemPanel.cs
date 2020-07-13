using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.PurePlayer.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe.Components
{
    public class ItemPanel : Container
    {
        public int PPCount;
        public RankStats Rank;
        public string TexturePath;

        private Container mask;
        private Container content;
        private OsuSpriteText itemName;
        private MfConfigManager config;
        private SpriteIcon lockIcon;
        private Container centreContent;

        private bool UnMasked = false;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MfConfigManager config)
        {
            this.config = config;
            CornerRadius = 12.5f;
            Masking = true;
            Size = new Vector2(157, 384);

            InternalChildren = new Drawable[]
            {
                mask = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4Extensions.FromHex("#111"),
                            RelativeSizeAxes = Axes.Both,
                        },
                        lockIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = FontAwesome.Solid.Lock,
                            Size = new Vector2(45)
                        }
                    }
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                    Alpha = 0,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4Extensions.FromHex("#222"),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Name = "Item Name Container",
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.1f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                itemName = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"{PPCount.ToString()}pp"
                                }
                            }
                        },
                        centreContent = new Container
                        {
                            Name = "Centre Contnt Container",
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.9f,
                            CornerRadius = 12.5f,
                            Masking = true,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4Extensions.FromHex("#333"),
                                    RelativeSizeAxes = Axes.Both
                                },
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    FillMode = FillMode.Fill,
                                    Texture = textures.Get($"{TexturePath ?? "Online/avatar-guest"}"),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            UnMask();
            return base.OnClick(e);
        }

        private ColourInfo UpdateRankColor()
        {
            switch(Rank)
            {
                case RankStats.F:
                case RankStats.D:
                    return Color4.Red;
                
                case RankStats.C:
                    return Color4.Khaki;

                case RankStats.B:
                    return Color4.Indigo;

                case RankStats.A:
                    return Color4Extensions.FromHex(@"88da20");
                
                case RankStats.S:
                    return Color4.Orange;
                
                case RankStats.SS:
                case RankStats.Beatmap:
                case RankStats.Triangle:
                    return Color4.Gold;

                case RankStats.Bruh:
                default:
                    return Color4.Gray;
            }
        }

        public void UnMask()
        {
            if ( UnMasked ) return;

            UnMasked = true;

            lockIcon.Icon = FontAwesome.Solid.Unlock;
            lockIcon.ScaleTo(1.2f, 300, Easing.OutQuint);

            this.Delay(500).Schedule(() =>
            {
                //视觉效果
                switch(Rank)
                {
                    case RankStats.Triangle:
                    case RankStats.Beatmap:
                        mask.Add(
                            new Box
                            {
                                Colour = Color4.Gold,
                                RelativeSizeAxes = Axes.Both,
                            });
                        this.TweenEdgeEffectTo(new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 10,
                            Colour = Color4.Gold
                        });
                    break;
                }

                //其他设定
                switch(Rank)
                {
                    case RankStats.Triangle:
                        PPCount = 0;
                        itemName.Text = "粒子动画!";
                        centreContent.Add(new MfBgTriangles());
                        config.Set(MfSetting.EasterEggBGTriangle, true);
                        break;

                    case RankStats.Beatmap:
                        PPCount = 0;
                        itemName.Text = "实时谱面背景!";
                        centreContent.Add(new BeatmapCover());
                        config.Set(MfSetting.EasterEggBGBeatmap, true);
                        break;

                    default:
                        break;
                }

                mask.FadeOut(300);
                lockIcon.ScaleTo(0.8f, 300);
                content.Show();
            });
        }
    
        public enum RankStats
        {
            F,
            D,
            C,
            B,
            A,
            S,
            SS,
            Bruh,
            Triangle,
            Beatmap,
            Last,
        }

        public enum ReturnRare
        {
            no1,
            no2,
            no3,
            no4,
            no5,
            no6,
            no7,
            yes,
            last
        }
    }
}