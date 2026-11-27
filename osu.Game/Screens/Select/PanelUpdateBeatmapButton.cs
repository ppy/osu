// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.Select
{
    public partial class PanelUpdateBeatmapButton : OsuAnimatedButton
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

        public PanelUpdateBeatmapButton()
        {
            AutoSizeAxes = Axes.X;
            Height = 22f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float icon_size = 12;

            Content.Anchor = Anchor.Centre;
            Content.Origin = Anchor.Centre;

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
                            Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                            Text = WebCommonStrings.ButtonsUpdate,
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
            icon.Spin(400, RotationDirection.Clockwise, icon.Rotation);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.Spin(4000, RotationDirection.Clockwise, icon.Rotation);
            base.OnHoverLost(e);
        }

        public override LocalisableString TooltipText => Enabled.Value ? SongSelectStrings.UpdateBeatmapTooltip : string.Empty;

        private bool updateConfirmed;

        private void performUpdate()
        {
            Debug.Assert(beatmapSet != null);

            if (!api.IsLoggedIn)
            {
                loginOverlay?.Show();
                return;
            }

            // TODO: See if we can make this code path use the standard Download method.
            // Right now it isn't because of the final `attachExistingDownload` call needing to fire post-download operation, seemingly.

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

            beatmapDownloader.DownloadAsUpdate(beatmapSet);
            attachExistingDownload();
        }

        private void attachExistingDownload()
        {
            Debug.Assert(beatmapSet != null);
            var download = beatmapDownloader.GetExistingDownload(beatmapSet);

            if (download != null)
            {
                Enabled.Value = false;

                download.DownloadProgressed += progress => progressFill.ResizeWidthTo(progress, 100, Easing.OutQuint);
                download.Failure += _ => attachExistingDownload();
            }
            else
            {
                Enabled.Value = true;

                progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
            }
        }
    }
}
