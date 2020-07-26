using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osu.Game.Screens.Mvis.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe.Components
{
    public class ItemPanel : Container
    {
        public int PPCount;
        public LevelStats Level;
        public string TexturePath;
        private Rank rank;

        private Container mask;
        private Container content;
        private OsuSpriteText itemName;
        private MfConfigManager config;
        private TextureStore textures;
        private SpriteIcon lockIcon;
        private Container centreContent;

        private Bindable<int> Coins = new Bindable<int>();
        private bool UnMasked = false;
        private Box maskBox;
        private OsuSpriteText ppChangeText;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MfConfigManager config)
        {
            this.config = config;
            this.textures = textures;
            config.BindWith(MfSetting.EasterEggCoinCount, Coins);

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
                        maskBox = new Box
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
                                }
                            }
                        },
                        centreContent = new Container
                        {
                            Name = "Centre Content Container",
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.9f,
                            CornerRadius = 12.5f,
                            Masking = true,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    FillMode = FillMode.Fill,
                                    Texture = textures.Get($"{TexturePath ?? "Online/avatar-guest"}"),
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Bottom Content Container",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.1f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.8f))
                                },
                                ppChangeText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Numeric.With(size: 20)
                                },
                            }
                        },
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            UnMask();
            return base.OnClick(e);
        }

        public void UnMask()
        {
            if (UnMasked) return;

            lockIcon.Icon = FontAwesome.Solid.Unlock;
            lockIcon.ScaleTo(1.2f, 300, Easing.OutQuint);

            UnMasked = true;

            this.Delay(500).Schedule(() =>
            {
                if (Coins.Value < 0 || (Coins.Value - 50) < 0)
                {
                    lockIcon.Icon = FontAwesome.Solid.Lock;
                    lockIcon.ScaleTo(1f, 300, Easing.OutQuint);
                    maskBox.FadeColour(Color4.DarkRed, 300, Easing.OutQuint);
                    this.Delay(500).ScaleTo(0f, 300).FadeOut(300);
                    this.Expire();
                    return;
                }
                else
                {
                    Coins.Value -= 50;
                }

                //其他设定
                switch (Level)
                {
                    case LevelStats.Triangle:
                        itemName.Text = "粒子动画!";
                        centreContent.Add(new MfBgTriangles(){Masking = false});
                        config.Set(MfSetting.EasterEggBGTriangle, true);
                        PPCount = 1000;
                        break;

                    case LevelStats.Beatmap:
                        PPCount = 1000;
                        itemName.Text = "实时谱面背景!";
                        centreContent.Add(new BeatmapCover());
                        config.Set(MfSetting.EasterEggBGBeatmap, true);
                        break;

                    case LevelStats.Pippi:
                        PPCount = 500;
                        itemName.Text = "一只Pippi!";
                        centreContent.Add(new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            FillMode = FillMode.Fill,
                            Texture = textures.Get("Backgrounds/registration"),
                        });
                        break;

                    default:
                        itemName.Text = $"{PPCount.ToString()}pp";
                        break;
                }

                if (PPCount >= 400) this.rank = Rank.Legendary;
                if (PPCount > 300 && PPCount < 1000) this.rank = Rank.Rare;

                //视觉效果
                switch(rank)
                {
                    case Rank.Rare:
                        lockIcon.Hide();
                        maskBox.Colour = Colour4.Silver;
                        this.TweenEdgeEffectTo(new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 10,
                            Colour = Color4.Silver
                        });
                        break;
                    
                    case Rank.Legendary:
                        lockIcon.Hide();
                        maskBox.Colour = Colour4.Gold;
                        this.TweenEdgeEffectTo(new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 10,
                            Colour = Color4.Gold
                        });
                        break;
                    
                    default:
                        break;
                }

                var symbol = "";
                if (PPCount > 0) symbol = "+";
                ppChangeText.Text = $"{symbol}" + $"{PPCount.ToString()}";
                config.Set(MfSetting.EasterEggCoinCount, (Coins.Value + PPCount));

                mask.FadeOut(300);
                lockIcon.ScaleTo(0.8f, 300);
                content.Show();
            });
        }

        public enum LevelStats
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
            Pippi,
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
    
        public enum Rank
        {
            Common,
            Oops,
            Rare,
            Legendary
        }
    }
}