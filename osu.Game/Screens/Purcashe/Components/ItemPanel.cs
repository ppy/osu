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
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Collections.Interface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe.Components
{
    public class ItemPanel : Container
    {
        private int ppCount => RollResult.PP;
        private LevelStats level => RollResult.Level;
        private string texturePath => RollResult.TexturePath;
        private Rank rank => RollResult.Rank;

        public RollResult RollResult { get; set; }

        private Container content;
        private MConfigManager config;
        private TextureStore textures;
        private Container mainContent;

        public BindableInt PP = new BindableInt();
        private Box flashBox;
        private Sprite defaultBackground;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MConfigManager config)
        {
            this.config = config;
            this.textures = textures;

            Size = new Vector2(157, 400);

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    CornerRadius = 12.5f,
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new Drawable[]
                    {
                        flashBox = new Box
                        {
                            Colour = Color4.White.Opacity(1),
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MinValue
                        },
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
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = RollResult.RollName
                                }
                            }
                        },
                        mainContent = new Container
                        {
                            Name = "Centre Content Container",
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.9f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Masking = true,
                            CornerRadius = 12.5f,
                            Children = new Drawable[]
                            {
                                defaultBackground = new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    FillMode = FillMode.Fill,
                                    Texture = textures.Get($"{texturePath ?? "Online/avatar-guest"}"),
                                    Colour = Color4.Black
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.8f))
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Font = OsuFont.Numeric.With(size: 20),
                                    Margin = new MarginPadding { Bottom = 15 },
                                    Text = RollResult.PP + "pp"
                                },
                            }
                        },
                        new HoverSounds(HoverSampleSet.Soft)
                    }
                }
            };

            unMask();
        }

        protected override bool OnHover(HoverEvent e)
        {
            content.ScaleTo(1.1f, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.ScaleTo(1, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private void flash(Color4 color)
        {
            flashBox.Colour = color.Opacity(0);
            flashBox.FlashColour(color, 2000, Easing.OutQuint);
        }

        private void unMask()
        {
            defaultBackground.Colour = Color4.White;

            //其他设定
            switch (level)
            {
                case LevelStats.Triangle:
                    mainContent.Add(new MfBgTriangles { Masking = false });
                    break;

                case LevelStats.Beatmap:
                    defaultBackground.Expire();
                    mainContent.Add(new BeatmapCover(null)
                    {
                        Depth = float.MaxValue
                    });
                    break;

                case LevelStats.Pippi:
                    defaultBackground.Expire();
                    mainContent.Add(new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        FillMode = FillMode.Fill,
                        Texture = textures.Get("Backgrounds/registration"),
                        Depth = float.MaxValue
                    });
                    break;
            }

            //视觉效果
            content.TweenEdgeEffectTo(new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = 10,
                Colour = PurcasheColorProvider.GetColor(rank)
            });

            switch (rank)
            {
                case Rank.Rare:
                    flash(Color4.Silver);
                    break;

                case Rank.Legendary:
                    flash(Color4.Gold);
                    break;

                default:
                    flash(Color4.Gray);
                    break;
            }

            content.Show();
        }

        public void ApplyResult()
        {
            PP.Value += ppCount;

            //其他设定
            switch (level)
            {
                case LevelStats.Triangle:
                    config.Set(MSetting.PurcasheBgTriangles, true);
                    break;

                case LevelStats.Beatmap:
                    config.Set(MSetting.PurcasheBgBeatmap, true);
                    break;
            }
        }

        public enum LevelStats
        {
            Regular,
            Triangle,
            Beatmap,
            Pippi,
            Last,
        }

        public enum Rank
        {
            Oops,
            Common,
            Rare,
            Legendary
        }
    }
}
