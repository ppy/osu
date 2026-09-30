// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Threading;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    [Cached]
    public partial class CardDetailsOverlayContainer : Container
    {
        public double HideDelay { get; set; } = 1000;

        protected override Container<Drawable> Content { get; }

        private readonly CardDetailsOverlay overlay;

        public CardDetailsOverlayContainer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren =
            [
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                overlay = new CardDetailsOverlay
                {
                    Alpha = 0,
                }
            ];
        }

        private ScheduledDelegate? hideDelegate;

        public void ShowCardDetails(Drawable targetDrawable, APIBeatmap beatmap)
        {
            // TODO: remove this once there's more than just tags in the overlay
            if (beatmap.GetTopUserTags().Length == 0)
                return;

            hideDelegate?.Cancel();
            hideDelegate = Scheduler.AddDelayed(overlay.Hide, HideDelay);

            overlay.TargetDrawable = targetDrawable;
            overlay.Beatmap.Value = beatmap;
            overlay.Show();
        }

        private partial class CardDetailsOverlay : VisibilityContainer
        {
            public readonly Bindable<APIBeatmap> Beatmap = new Bindable<APIBeatmap>();

            public Drawable? TargetDrawable;

            private Container content = null!;
            private UserTagSection tagSection = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Width = 200;
                Origin = Anchor.CentreRight;

                InternalChild = content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 6,
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                            Alpha = 0.85f,
                        },
                        tagSection = new UserTagSection()
                    ]
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Beatmap.BindValueChanged(e =>
                {
                    if (e.NewValue != null)
                        populateContent(e.NewValue);
                }, true);
            }

            private void populateContent(APIBeatmap beatmap)
            {
                tagSection.Tags = beatmap.GetTopUserTags().Select(it => new UserTag(it.Tag) { VoteCount = { Value = it.VoteCount } });
            }

            private Vector2 targetPosition => TargetDrawable is { } drawable
                ? Parent!.ToLocalSpace(drawable.ScreenSpaceDrawQuad.TopLeft) + new Vector2(-20, 0)
                // this results essentially a no-op when there's no valid target
                : Position;

            private readonly Vector2Spring position = new Vector2Spring
            {
                NaturalFrequency = 2f,
                Response = 0.25f,
                Damping = 0.85f
            };

            protected override void Update()
            {
                base.Update();

                // Workaround for AutoSizeAxes not working due to content being able to move
                Height = content.Height;

                Position = position.Update(Time.Elapsed, targetPosition);
            }

            protected override void PopIn()
            {
                this.FadeIn(300);

                content.MoveToX(-50)
                       .MoveToX(0, 400, Easing.OutExpo);

                position.Current = position.PreviousTarget = targetPosition;
            }

            protected override void PopOut() => this.FadeOut(300);
        }
    }
}
