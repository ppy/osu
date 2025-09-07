// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Localisation;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public abstract partial class BeatmapCard : OsuClickableContainer, IHasContextMenu
    {
        public const float TRANSITION_DURATION = 340;
        public const float CORNER_RADIUS = 8;

        protected const float WIDTH = 345;

        public IBindable<bool> Expanded { get; }

        public readonly APIBeatmapSet BeatmapSet;

        protected readonly Bindable<BeatmapSetFavouriteState> FavouriteState;

        protected abstract Drawable IdleContent { get; }
        protected abstract Drawable DownloadInProgressContent { get; }

        protected readonly BeatmapDownloadTracker DownloadTracker;

        private readonly Bindable<bool> preferNoVideo = new BindableBool();
        private InputManager? containingInputManager;

        [Resolved]
        private BeatmapSetOverlay? beatmapSetOverlay { get; set; }

        [Resolved]
        private BeatmapModelDownloader? beatmaps { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        protected BeatmapCard(APIBeatmapSet beatmapSet, bool allowExpansion = true)
            : base(HoverSampleSet.Button)
        {
            Expanded = new BindableBool { Disabled = !allowExpansion };

            BeatmapSet = beatmapSet;
            FavouriteState = new Bindable<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount));
            DownloadTracker = new BeatmapDownloadTracker(beatmapSet);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.PreferNoVideo, preferNoVideo);

            AddInternal(DownloadTracker);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DownloadTracker.State.BindValueChanged(_ => UpdateState());
            Expanded.BindValueChanged(_ => UpdateState(), true);
            FinishTransforms(true);

            containingInputManager = GetContainingInputManager();

            Action = () =>
            {
                if (containingInputManager?.CurrentState.Keyboard.ShiftPressed == true)
                {
                    switch (DownloadTracker.State.Value)
                    {
                        case DownloadState.NotDownloaded:
                            if (!BeatmapSet.Availability.DownloadDisabled)
                                beatmaps?.Download(BeatmapSet, preferNoVideo.Value);
                            break;

                        case DownloadState.LocallyAvailable:
                            game?.PresentBeatmap(BeatmapSet);
                            break;
                    }
                }
                else
                    beatmapSetOverlay?.FetchAndShowBeatmapSet(BeatmapSet.OnlineID);
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateState();
            base.OnHoverLost(e);
        }

        protected virtual void UpdateState()
        {
            bool showProgress = DownloadTracker.State.Value == DownloadState.Downloading || DownloadTracker.State.Value == DownloadState.Importing;

            IdleContent.FadeTo(showProgress ? 0 : 1, TRANSITION_DURATION, Easing.OutQuint);
            DownloadInProgressContent.FadeTo(showProgress ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }

        /// <summary>
        /// Creates a beatmap card of the given <paramref name="size"/> for the supplied <paramref name="beatmapSet"/>.
        /// </summary>
        public static BeatmapCard Create(APIBeatmapSet beatmapSet, BeatmapCardSize size, bool allowExpansion = true)
        {
            switch (size)
            {
                case BeatmapCardSize.Nano:
                    return new BeatmapCardNano(beatmapSet);

                case BeatmapCardSize.Normal:
                    return new BeatmapCardNormal(beatmapSet, allowExpansion);

                case BeatmapCardSize.Extra:
                    return new BeatmapCardExtra(beatmapSet, allowExpansion);

                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, @"Unsupported card size");
            }
        }

        public virtual MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(ContextMenuStrings.ViewBeatmap, MenuItemType.Highlighted, Action),
        };
    }
}
