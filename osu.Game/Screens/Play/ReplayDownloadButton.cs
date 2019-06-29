using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online;
using osu.Game.Scoring;
using osuTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Effects;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Play
{
    public class ReplayDownloadButton : DownloadTrackingComposite<ScoreInfo, ScoreManager>, IHasTooltip
    {
        private const int size = 40;

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        [Resolved]
        private ScoreManager scores { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private OsuClickableContainer button;
        private SpriteIcon downloadIcon;
        private SpriteIcon playIcon;
        private ShakeContainer shakeContainer;

        public string TooltipText
        {
            get
            {
                if (scores.IsAvailableLocally(Model.Value))
                    return @"Watch replay";

                if (Model.Value is APILegacyScoreInfo apiScore && apiScore.Replay)
                    return @"Download replay";

                return @"Replay unavailable";
            }
        }

        public ReplayDownloadButton(ScoreInfo score)
            : base(score)
        {
            Size = new Vector2(size);
            CornerRadius = size / 2;
            Masking = true;
        }

        private bool hasReplay => (Model.Value is APILegacyScoreInfo apiScore && apiScore.Replay) || scores.IsAvailableLocally(Model.Value);

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.4f),
                Type = EdgeEffectType.Shadow,
                Radius = 5,
            };

            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new OsuClickableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GrayF,
                        },
                        playIcon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Play,
                            Size = Vector2.Zero,
                            Colour = colours.Gray3,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        downloadIcon = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.FileDownload,
                            Size = Vector2.Zero,
                            Colour = colours.Gray3,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                }
            };

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.LocallyAvailable:
                        game.PresentScore(Model.Value);
                        break;

                    case DownloadState.NotDownloaded:
                        scores.Download(Model.Value);
                        break;

                    case DownloadState.Downloading:
                        shakeContainer.Shake();
                        break;
                }
            };

            State.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case DownloadState.Downloading:
                        FadeEdgeEffectTo(colours.Yellow, 400, Easing.OutQuint);
                        button.Enabled.Value = false;
                        break;

                    case DownloadState.LocallyAvailable:
                        playIcon.ResizeTo(13, 300, Easing.OutQuint);
                        downloadIcon.ResizeTo(Vector2.Zero, 300, Easing.OutExpo);
                        FadeEdgeEffectTo(Color4.Black.Opacity(0.4f), 400, Easing.OutQuint);
                        button.Enabled.Value = true;
                        break;

                    case DownloadState.NotDownloaded:
                        playIcon.ResizeTo(Vector2.Zero, 300, Easing.OutQuint);
                        downloadIcon.ResizeTo(13, 300, Easing.OutExpo);
                        FadeEdgeEffectTo(Color4.Black.Opacity(0.4f), 400, Easing.OutQuint);
                        button.Enabled.Value = true;
                        break;
                }
            }, true);
        }
    }
}
