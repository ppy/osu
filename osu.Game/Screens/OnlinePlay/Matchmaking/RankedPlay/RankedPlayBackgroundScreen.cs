// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayBackgroundScreen : BackgroundScreen
    {
        private RankedPlayBackground background { get; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public Bindable<bool> ShowBeatmapBackground { get; } = new BindableBool();

        public RankedPlayColourScheme? ColourScheme
        {
            set
            {
                if (value != null)
                    background.FadeColours(value.PrimaryDarker.Darken(0.5f), value.PrimaryDarkest.Darken(0.5f));
                else
                    background.FadeColours(Color4Extensions.FromHex("#15061e"), Color4Extensions.FromHex("#240d36"));
            }
        }

        public RankedPlayBackgroundScreen()
        {
            InternalChild = background = new RankedPlayBackground
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        private CancellationTokenSource? pendingBackgroundLoad;
        private BeatmapBackground? currentBackground;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => updateBackground());
            ShowBeatmapBackground.BindValueChanged(_ => updateBackground());
            updateBackground();
        }

        private void updateBackground()
        {
            pendingBackgroundLoad?.Cancel();

            if (beatmap.Value == null || !ShowBeatmapBackground.Value)
            {
                currentBackground?.PopOut().Expire();
                currentBackground = null;
                return;
            }

            pendingBackgroundLoad = new CancellationTokenSource();

            LoadComponentAsync(new BeatmapBackground(beatmap.Value), background =>
            {
                currentBackground?.PopOut().Expire();

                AddInternal(background);
                currentBackground = background;

                background.PopIn();
            }, pendingBackgroundLoad.Token);
        }

        [LongRunningLoad]
        private partial class BeatmapBackground(WorkingBeatmap beatmap) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChild = new BufferedContainer(cachedFrameBuffer: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    FrameBufferScale = new Vector2(0.15f),
                    GrayscaleStrength = 0.3f,
                    BlurSigma = new Vector2(5),
                    Colour = Color4Extensions.FromHex("#cccccc"),
                    Child = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Texture = beatmap.GetBackground(),
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }

            public void PopIn() => this.FadeOut()
                                       .FadeTo(0.4f, 300)
                                       .ScaleTo(1.2f)
                                       .ScaleTo(1f, 600, Easing.OutExpo);

            public TransformSequence<BeatmapBackground> PopOut() =>
                this.FadeOut(300).ScaleTo(1.1f, 600, Easing.OutExpo);
        }
    }
}
