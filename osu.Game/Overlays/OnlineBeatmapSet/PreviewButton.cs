// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class PreviewButton : OsuClickableContainer
    {
        private readonly BeatmapSetInfo set;
        private readonly Box bg, progress;
        private readonly SpriteIcon icon;

        private AudioManager audio;
        private Track preview;

        private bool playing;
        public bool Playing
        {
            get { return playing; }
            set
            {
                if (value == playing) return;
                playing = value;

                if (Playing)
                {
                    icon.Icon = FontAwesome.fa_stop;
                    progress.FadeIn(100);

                    loadPreview();
                    preview.Start();
                }
                else
                {
                    icon.Icon = FontAwesome.fa_play;
                    progress.FadeOut(100);
                    preview.Stop();
                }
            }
        }

        public PreviewButton(BeatmapSetInfo set)
        {
            this.set = set;
            Height = 42;

            Children = new Drawable[]
            {
                bg = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.25f),
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Child = progress = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0f,
                        Alpha = 0f,
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_play,
                    Size = new Vector2(18),
                    Shadow = false,
                },
            };

            Action = () => Playing = !Playing;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            this.audio = audio;
            progress.Colour = colours.Yellow;

            loadPreview();
        }

        protected override void Update()
        {
            base.Update();

            if (Playing)
            {
                progress.Width = (float)(preview.CurrentTime / preview.Length);
                if (preview.HasCompleted) Playing = false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            Playing = false;
            base.Dispose(isDisposing);
        }

        protected override bool OnHover(InputState state)
        {
            bg.FadeColour(Color4.Black.Opacity(0.5f), 100);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            bg.FadeColour(Color4.Black.Opacity(0.25f), 100);
            base.OnHoverLost(state);
        }

        private void loadPreview()
        {
            if (preview?.HasCompleted ?? true)
            {
                preview = audio.Track.Get(set.OnlineInfo.Preview);
                preview.Volume.Value = 0.5;
            }
            else
            {
                preview.Seek(0);
            }
        }
    }
}
