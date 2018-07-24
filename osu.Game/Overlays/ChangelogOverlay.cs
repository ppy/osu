// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog;
using System;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : WaveOverlayContainer
    {
        private readonly ChangelogHeader header;
        private readonly ChangelogBadges badges;
        private readonly ChangelogChart chart;
        private readonly ChangelogContent content;

        private readonly Color4 purple = new Color4(191, 4, 255, 255);

        private SampleChannel sampleBack;

        private APIAccess api;

        private bool isAtListing;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        public ChangelogOverlay()
        {
            // these possibly need adjusting?
            Waves.FirstWaveColour = OsuColour.FromHex(@"bf04ff");
            Waves.SecondWaveColour = OsuColour.FromHex(@"8F03BF");
            Waves.ThirdWaveColour = OsuColour.FromHex(@"600280");
            Waves.FourthWaveColour = OsuColour.FromHex(@"300140");

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
            Width = 0.85f;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(49, 36, 54, 255),
                },
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new ChangelogHeader(),
                            badges = new ChangelogBadges(),
                            chart = new ChangelogChart(),
                            content = new ChangelogContent()
                        },
                    },
                },
            };
            header.ListingSelected += FetchAndShowListing;
            badges.Selected += onBuildSelected;
            content.BuildSelected += onBuildSelected;
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, AudioManager audio)
        {
            this.api = api;
            sampleBack = audio.Sample.Get(@"UI/generic-select-soft"); // @"UI/screen-back" feels non-fitting here
        }

        protected override void LoadComplete()
        {
            var req = new GetChangelogLatestBuildsRequest();
            req.Success += badges.Populate;
            api.Queue(req);
            FetchAndShowListing();
            base.LoadComplete();
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.25f, WaveContainer.APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, WaveContainer.DISAPPEAR_DURATION, Easing.Out);
        }

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    if (isAtListing)
                        State = Visibility.Hidden;
                    else
                    {
                        FetchAndShowListing();
                        sampleBack?.Play();
                    }
                    return true;
            }

            return false;
        }

        private void onBuildSelected(APIChangelog build, EventArgs e) => FetchAndShowBuild(build);

        /// <summary>
        /// If we're not already at it, fetches and shows changelog listing.
        /// </summary>
        public void FetchAndShowListing()
        {
            header.ShowListing();
            if (isAtListing)
                return;
            isAtListing = true;
            var req = new GetChangelogRequest();
            badges.SelectNone();
            chart.ShowAllUpdateStreams();
            req.Success += content.ShowListing;
            api.Queue(req);
        }

        /// <summary>
        /// Fetches and shows a specific build from a specific update stream.
        /// </summary>
        public void FetchAndShowBuild(APIChangelog build, bool sentByBadges = false)
        {
            isAtListing = false;
            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);

            if (build.UpdateStream.DisplayName != null && build.DisplayVersion != null)
                header.ShowBuild(build.UpdateStream.DisplayName, build.DisplayVersion);
            else
                req.Success += res => header.ShowBuild(res.UpdateStream.DisplayName, res.DisplayVersion);

            if (!sentByBadges)
                badges.SelectUpdateStream(build.UpdateStream.Name);

            chart.ShowUpdateStream(build.UpdateStream.Name);
            req.Success += content.ShowBuild;
            api.Queue(req);
        }
    }
}
