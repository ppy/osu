using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio.Effects;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.ReleaseNote;
using osuTK;

namespace osu.Game.Overlays
{
    public class ReleaseNoteDialog : OsuFocusedOverlayContainer
    {
        private MarkdownContainer mdContainer;

        [Cached]
        private OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Mvis);

        private AudioFilter audiofilter;

        public ReleaseNoteDialog()
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.8f);

            Masking = true;
            CornerRadius = 7.5f;

            Scale = new Vector2(0.9f);

            Anchor = Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, Framework.Game game, AudioManager audioManager)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background5
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.1f,
                    Child = new OsuSpriteText
                    {
                        Text = "发行注记",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 26)
                    }
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.9f,
                    ScrollbarVisible = false,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Child = mdContainer = new ReleaseNoteMarkdownContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        DocumentMargin = new MarginPadding { Bottom = 20, Left = 10, Right = 30 }
                    }
                },
                audiofilter = new AudioFilter(audioManager.TrackMixer)
            };

            mdContainer.Text = Encoding.UTF8.GetString(game.Resources.Get("Documents.releaseNote.md"));
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.ScaleTo(1, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

            audiofilter.CutoffTo(300, 300, Easing.OutQuad);
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.ScaleTo(0.9f, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuint);

            audiofilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 300, Easing.OutQuad);
        }
    }
}
