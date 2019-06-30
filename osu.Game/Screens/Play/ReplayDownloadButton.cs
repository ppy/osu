// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    public class ReplayDownloadButton : DownloadTrackingComposite<ScoreInfo, ScoreManager>, IHasTooltip
    {
        [Resolved]
        private ScoreManager scores { get; set; }

        private OsuClickableContainer button;
        private SpriteIcon downloadIcon;
        private SpriteIcon playIcon;
        private ShakeContainer shakeContainer;
        private CircularContainer circle;

        public string TooltipText
        {
            get
            {
                switch (replayAvailability)
                {
                    case ReplayAvailability.Local:
                        return @"Watch replay";

                    case ReplayAvailability.Online:
                        return @"Download replay";

                    default:
                        return @"Replay unavailable";
                }
            }
        }

        private ReplayAvailability replayAvailability
        {
            get
            {
                if (scores.IsAvailableLocally(Model.Value))
                    return ReplayAvailability.Local;

                if (Model.Value is APILegacyScoreInfo apiScore && apiScore.Replay)
                    return ReplayAvailability.Online;

                return ReplayAvailability.NotAvailable;
            }
        }

        public ReplayDownloadButton(ScoreInfo score)
            : base(score)
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, OsuColour colours)
        {
            InternalChild = shakeContainer = new ShakeContainer
            {
                AutoSizeAxes = Axes.Both,
                Child = circle = new CircularContainer
                {
                    Masking = true,
                    Size = new Vector2(40),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.4f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                    },
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
                },
            };

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.LocallyAvailable:
                        game?.PresentScore(Model.Value);
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
                        playIcon.ResizeTo(Vector2.Zero, 400, Easing.OutQuint);
                        downloadIcon.ResizeTo(13, 400, Easing.OutQuint);
                        circle.FadeEdgeEffectTo(colours.Yellow, 400, Easing.OutQuint);
                        break;

                    case DownloadState.LocallyAvailable:
                        playIcon.ResizeTo(13, 400, Easing.OutQuint);
                        downloadIcon.ResizeTo(Vector2.Zero, 400, Easing.OutQuint);
                        circle.FadeEdgeEffectTo(Color4.Black.Opacity(0.4f), 400, Easing.OutQuint);
                        break;

                    case DownloadState.NotDownloaded:
                        playIcon.ResizeTo(Vector2.Zero, 400, Easing.OutQuint);
                        downloadIcon.ResizeTo(13, 400, Easing.OutQuint);
                        circle.FadeEdgeEffectTo(Color4.Black.Opacity(0.4f), 400, Easing.OutQuint);
                        break;
                }
            }, true);

            if (replayAvailability == ReplayAvailability.NotAvailable)
            {
                button.Enabled.Value = false;
                button.Alpha = 0.6f;
            }
        }

        private enum ReplayAvailability
        {
            Local,
            Online,
            NotAvailable,
        }
    }
}
