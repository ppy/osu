// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Carousel;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class UpdateBeatmapSetButton : OsuAnimatedButton
    {
        private BeatmapSetInfo? beatmapSet;

        public BeatmapSetInfo? BeatmapSet
        {
            get => beatmapSet;
            set
            {
                beatmapSet = value;

                if (IsLoaded)
                    beatmapChanged();
            }
        }

        private SpriteIcon icon = null!;
        private Box progressFill = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private LoginOverlay? loginOverlay { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        public UpdateBeatmapSetButton()
        {
            Size = new Vector2(75f, 22f);
        }

        private Bindable<bool> preferNoVideo = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            const float icon_size = 14;

            preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);

            Content.Anchor = Anchor.Centre;
            Content.Origin = Anchor.Centre;
            Content.Shear = new Vector2(OsuGame.SHEAR, 0);

            Content.AddRange(new Drawable[]
            {
                progressFill = new Box
                {
                    Colour = Color4.White,
                    Alpha = 0.2f,
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Horizontal = 5, Vertical = 3 },
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    Shear = new Vector2(-OsuGame.SHEAR, 0),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Size = new Vector2(icon_size),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.SyncAlt,
                                    Size = new Vector2(icon_size),
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
                            Text = "Update",
                        }
                    }
                },
            });

            Action = performUpdate;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmapChanged();
        }

        private void beatmapChanged()
        {
            Alpha = beatmapSet?.AllBeatmapsUpToDate == false ? 1 : 0;
            icon.Spin(4000, RotationDirection.Clockwise);
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.Spin(400, RotationDirection.Clockwise);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.Spin(4000, RotationDirection.Clockwise);
            base.OnHoverLost(e);
        }

        private bool updateConfirmed;

        private void performUpdate()
        {
            Debug.Assert(beatmapSet != null);

            if (!api.IsLoggedIn)
            {
                loginOverlay?.Show();
                return;
            }

            if (dialogOverlay != null && beatmapSet.Status == BeatmapOnlineStatus.LocallyModified && !updateConfirmed)
            {
                dialogOverlay.Push(new UpdateLocalConfirmationDialog(() =>
                {
                    updateConfirmed = true;
                    performUpdate();
                }));

                return;
            }

            updateConfirmed = false;

            beatmapDownloader.DownloadAsUpdate(beatmapSet, preferNoVideo.Value);
            attachExistingDownload();
        }

        private void attachExistingDownload()
        {
            Debug.Assert(beatmapSet != null);
            var download = beatmapDownloader.GetExistingDownload(beatmapSet);

            if (download != null)
            {
                Enabled.Value = false;
                TooltipText = string.Empty;

                download.DownloadProgressed += progress => progressFill.ResizeWidthTo(progress, 100, Easing.OutQuint);
                download.Failure += _ => attachExistingDownload();
            }
            else
            {
                Enabled.Value = true;
                TooltipText = "Update beatmap with online changes";

                progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
            }
        }
    }
}
