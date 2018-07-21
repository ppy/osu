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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Changelog;

namespace osu.Game.Overlays
{
    public class ChangelogOverlay : WaveOverlayContainer
    {
        private readonly ChangelogHeader header;
        public readonly ChangelogStreams Streams;
        private readonly ChangelogChart chart;
        private APIChangelog changelogEntry;

        private APIAccess api;

        protected readonly Color4 Purple = new Color4(191, 4, 255, 255);

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

            ChangelogContent content; // told by appveyor to convert to local variable..

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
                    Colour = new Color4(20, 18, 23, 255)
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
                            Streams = new ChangelogStreams(),
                            chart = new ChangelogChart(),
                            content = new ChangelogContent(),
                        },
                    },
                },
            };
            OnLoadComplete += d => FetchChangelog();
            Streams.OnSelection = () =>
            {
                if (Streams.SelectedRelease != null)
                {
                    header.ChangelogEntry = Streams.SelectedRelease;
                }
                header.ShowReleaseStream();
                content.ShowBuild(Streams.SelectedRelease);
                chart.ShowChart(Streams.SelectedRelease);
            };
            header.OnListingActivated += () =>
            {
                Streams.SelectedRelease = null;
                content.Clear();
                // should add listing to content here
                if (!Streams.IsHovered)
                    foreach (StreamBadge item in Streams.BadgesContainer.Children)
                        item.Activate(true);
                else
                    foreach (StreamBadge item in Streams.BadgesContainer.Children)
                        item.Deactivate();
                chart.ShowChart();
            };
            content.OnBuildChanged = () =>
            {
                header.ChangelogEntry = content.CurrentBuild;
                header.ShowReleaseStream();
            };
        }

        public void ActivateListing() => header.ActivateListing();

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    if (header.IsListingActivated())
                        State = Visibility.Hidden;
                    else
                        header.ActivateListing();
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

        public void FetchChangelog()
        {
            var req = new GetChangelogLatestBuildsRequest();
            req.Success += res =>
            {
                Streams.BadgesContainer.Clear();
                foreach (APIChangelog item in res)
                    Streams.BadgesContainer.Add(new StreamBadge(item));
                chart.ShowChart();
            };
            api.Queue(req);
        }
    }
}
