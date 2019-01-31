// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public abstract class DirectPanel : Container
    {
        public readonly BeatmapSetInfo SetInfo;

        private const double hover_transition_time = 400;

        private Container content;

        private BeatmapSetOverlay beatmapSetOverlay;

        public PreviewTrack Preview => PlayButton.Preview;
        public Bindable<bool> PreviewPlaying => PlayButton.Playing;
        protected abstract PlayButton PlayButton { get; }
        protected abstract Box PreviewBar { get; }

        protected virtual bool FadePlayButton => true;

        protected override Container<Drawable> Content => content;

        protected DirectPanel(BeatmapSetInfo setInfo)
        {
            SetInfo = setInfo;
        }

        private readonly EdgeEffectParameters edgeEffectNormal = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0f, 1f),
            Radius = 2f,
            Colour = Color4.Black.Opacity(0.25f),
        };

        private readonly EdgeEffectParameters edgeEffectHovered = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0f, 5f),
            Radius = 10f,
            Colour = Color4.Black.Opacity(0.3f),
        };

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager beatmaps, OsuColour colours, BeatmapSetOverlay beatmapSetOverlay)
        {
            this.beatmapSetOverlay = beatmapSetOverlay;

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                EdgeEffect = edgeEffectNormal,
                Children = new[]
                {
                    CreateBackground(),
                    new DownloadProgressBar(SetInfo)
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Depth = -1,
                    },
                }
            });
        }

        protected override void Update()
        {
            base.Update();

            if (PreviewPlaying && Preview != null && Preview.TrackLoaded)
            {
                PreviewBar.Width = (float)(Preview.CurrentTime / Preview.Length);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            content.TweenEdgeEffectTo(edgeEffectHovered, hover_transition_time, Easing.OutQuint);
            content.MoveToY(-4, hover_transition_time, Easing.OutQuint);
            if (FadePlayButton)
                PlayButton.FadeIn(120, Easing.InOutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.TweenEdgeEffectTo(edgeEffectNormal, hover_transition_time, Easing.OutQuint);
            content.MoveToY(0, hover_transition_time, Easing.OutQuint);
            if (FadePlayButton && !PreviewPlaying)
                PlayButton.FadeOut(120, Easing.InOutQuint);

            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            ShowInformation();
            return true;
        }

        protected void ShowInformation() => beatmapSetOverlay?.ShowBeatmapSet(SetInfo);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(200, Easing.Out);

            PreviewPlaying.ValueChanged += newValue => PlayButton.FadeTo(newValue || IsHovered || !FadePlayButton ? 1 : 0, 120, Easing.InOutQuint);
            PreviewPlaying.ValueChanged += newValue => PreviewBar.FadeTo(newValue ? 1 : 0, 120, Easing.InOutQuint);
        }

        protected List<DifficultyIcon> GetDifficultyIcons()
        {
            var icons = new List<DifficultyIcon>();

            foreach (var b in SetInfo.Beatmaps.OrderBy(beatmap => beatmap.StarDifficulty))
                icons.Add(new DifficultyIcon(b));

            return icons;
        }

        protected Drawable CreateBackground() => new UpdateableBeatmapSetCover
        {
            RelativeSizeAxes = Axes.Both,
            BeatmapSet = SetInfo,
        };

        public class Statistic : FillFlowContainer
        {
            private readonly SpriteText text;

            private int value;

            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    text.Text = Value.ToString(@"N0");
                }
            }

            public Statistic(FontAwesome icon, int value = 0)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5f, 0f);

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Font = @"Exo2.0-SemiBoldItalic",
                    },
                    new SpriteIcon
                    {
                        Icon = icon,
                        Shadow = true,
                        Size = new Vector2(14),
                        Margin = new MarginPadding { Top = 1 },
                    },
                };

                Value = value;
            }
        }
    }
}
