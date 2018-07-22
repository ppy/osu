// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Changelog;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : WaveOverlayContainer
    {
        private readonly ChangelogHeader header;
        private readonly ChangelogBadges badges;
        private readonly ChangelogChart chart;
        private readonly ChangelogContent content;

        private readonly Color4 purple = new Color4(191, 4, 255, 255);

        private APIAccess api;

        private bool isAtListing;

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
            //    content.ShowListing();
            //    if (!Streams.IsHovered)
            //        foreach (StreamBadge item in Streams.BadgesContainer.Children)
            //            item.Activate(true);
            //    else
            //        foreach (StreamBadge item in Streams.BadgesContainer.Children)
            //            item.Deactivate();
        }

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    if (isAtListing)
                        State = Visibility.Hidden;
                    else
                        FetchAndShowListing();
                    return true;
            }

            return false;
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

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        /// <summary>
        /// Fetches and shows changelog listing.
        /// </summary>
        public void FetchAndShowListing()
        {
            var req = new GetChangelogLatestBuildsRequest();
            header.ShowListing();
            badges.SelectNone();
            chart.ShowAllUpdateStreams();
            req.Success += content.ShowListing;
            api.Queue(req);
        }

        /// <summary>
        /// Fetches and shows a specific build from a specific update stream.
        /// </summary>
        /// <param name="sentByBadges">If true, will select fetched build's update stream badge.</param>
        public void FetchAndShowBuild(string updateStream, string version)
        {
            var req = new GetChangelogBuildRequest(updateStream, version);
            header.ShowBuild(updateStream, version);
            badges.SelectBadge(updateStream);
            chart.ShowUpdateStream(updateStream);
            req.Success += content.ShowBuild;
            api.Queue(req);
        }
    }
}
