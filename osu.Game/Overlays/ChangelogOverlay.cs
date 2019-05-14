// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog;
using System;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : FullscreenOverlay
    {
        private ChangelogHeader header;

        private ChangelogBadges badges;

        private ChangelogContent listing;
        private ChangelogContent content;

        private ScrollContainer scroll;

        private SampleChannel sampleBack;

        private bool isAtListing;
        private float savedScrollPosition;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colour)
        {
            // these possibly need adjusting?
            Waves.FirstWaveColour = colour.Violet;
            Waves.SecondWaveColour = OsuColour.FromHex(@"8F03BF");
            Waves.ThirdWaveColour = OsuColour.FromHex(@"600280");
            Waves.FourthWaveColour = OsuColour.FromHex(@"300140");

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(49, 36, 54, 255),
                },
                scroll = new ScrollContainer
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
                            listing = new ChangelogContent(),
                            content = new ChangelogContent()
                        },
                    },
                },
            };
            header.ListingSelected += ShowListing;
            badges.Selected += onBuildSelected;
            listing.BuildSelected += onBuildSelected;
            content.BuildSelected += onBuildSelected;

            sampleBack = audio.Sample.Get(@"UI/generic-select-soft"); // @"UI/screen-back" feels non-fitting here
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            fetchListing();
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
                    {
                        if (scroll.Current > scroll.GetChildPosInContent(listing))
                        {
                            scroll.ScrollTo(0);
                            sampleBack?.Play();
                        }
                        else
                            State = Visibility.Hidden;
                    }
                    else
                    {
                        ShowListing();
                        sampleBack?.Play();
                    }

                    return true;
            }

            return false;
        }

        private void onBuildSelected(APIChangelogBuild build, EventArgs e) => FetchAndShowBuild(build);

        private void fetchListing()
        {
            header.ShowListing();

            if (isAtListing)
                return;

            isAtListing = true;
            var req = new GetChangelogRequest();
            badges.SelectNone();
            req.Success += res =>
            {
                listing.ShowListing(res.Builds);
                badges.Populate(res.Streams);
            };

            API.Queue(req);
        }

        public void ShowListing()
        {
            header.ShowListing();

            if (isAtListing)
                return;

            isAtListing = true;
            content.Hide();
            listing.Show();
            badges.SelectNone();
            listing.Show();
            scroll.ScrollTo(savedScrollPosition);
        }

        /// <summary>
        /// Fetches and shows a specific build from a specific update stream.
        /// </summary>
        /// <param name="build">Must contain at least <see cref="APIUpdateStream.Name"/> and
        /// <see cref="APIChangelogBuild.Version"/>. If <see cref="APIUpdateStream.DisplayName"/> and
        /// <see cref="APIChangelogBuild.DisplayVersion"/> are specified, the header will instantly display them.</param>
        /// <param name="updateBadges">Whether to update badges. Should be set to false in case
        /// the function is called by selecting a badge, to avoid an infinite loop.</param>
        public void FetchAndShowBuild(APIChangelogBuild build, bool updateBadges = true)
        {
            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);

            if (build.UpdateStream.DisplayName != null && build.DisplayVersion != null)
                header.ShowBuild(build.UpdateStream.DisplayName, build.DisplayVersion);
            else
                req.Success += res => header.ShowBuild(res.UpdateStream.DisplayName, res.DisplayVersion);

            if (updateBadges)
                badges.SelectUpdateStream(build.UpdateStream.Name);

            req.Success += apiChangelog =>
            {
                listing.Hide();
                content.Show();
                content.ShowBuild(apiChangelog);
                if (scroll.Current > scroll.GetChildPosInContent(content))
                    scroll.ScrollTo(content);
                if (isAtListing)
                    savedScrollPosition = scroll.Current;
                isAtListing = false;
            };

            API.Queue(req);
        }
    }
}
