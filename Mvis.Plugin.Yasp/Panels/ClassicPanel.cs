using Mvis.Plugin.Yasp.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.Yasp.Panels
{
    public partial class ClassicPanel : CompositeDrawable, IPanel
    {
        private readonly Bindable<float> scaleBindable = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load(YaspPlugin plugin)
        {
            var config = (YaspConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(plugin);
            config.BindWith<float>(YaspSettings.Scale, scaleBindable);
            scaleBindable.BindValueChanged(v =>
            {
                this.ScaleTo(v.NewValue, 300, Easing.OutQuint);
            }, true);

            AutoSizeAxes = Axes.Both;
        }

        public void Refresh(WorkingBeatmap beatmap)
        {
            LoadComponentAsync(new FillFlowContainer
            {
                Height = 90,
                AutoSizeAxes = Axes.X,
                Spacing = new Vector2(10),
                Direction = FillDirection.Horizontal,
                Margin = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 3
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Y,
                        Width = 90,
                        Masking = true,
                        CornerRadius = 5f,
                        Child = new BeatmapCover.Cover(beatmap)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                                Text = new RomanisableString(beatmap?.Metadata.TitleUnicode, beatmap?.Metadata.Title)
                            },
                            new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 25),
                                Text = new RomanisableString(beatmap?.Metadata.ArtistUnicode, beatmap?.Metadata.Artist)
                            },
                            new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 25),
                                Text = beatmap?.Metadata.Source ?? "???"
                            }
                        }
                    }
                }
            }.WithEffect(new BlurEffect
            {
                Colour = Color4.Black.Opacity(0.7f),
                DrawOriginal = true
            }), newPanel =>
            {
                var prevPanel = currentPanel;

                newPanel.MoveToX(10).FadeOut();
                prevPanel?.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint).Expire();
                newPanel.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

                AddInternal(newPanel);

                currentPanel = newPanel;
            });
        }

        private BufferedContainer? currentPanel;

        public override void Show()
        {
            this.MoveToX(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);
        }

        public override void Hide()
        {
            this.MoveToX(-10, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);
        }
    }
}
