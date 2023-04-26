// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Cursor;
using osu.Game.Overlays;
using osu.Game.Screens.Utility.SampleComponents;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    [Cached]
    public partial class LatencyArea : CompositeDrawable, IProvideCursor
    {
        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public Action? ReportUserBest { get; set; }

        private Drawable? background;

        private readonly Key key;

        private Container visualContent = null!;

        public readonly int? TargetFrameRate;

        public readonly BindableBool IsActiveArea = new BindableBool();

        public readonly Bindable<LatencyVisualMode> VisualMode = new Bindable<LatencyVisualMode>();

        public CursorContainer? Cursor { get; private set; }

        public bool ProvidingUserCursor => IsActiveArea.Value;

        public LatencyArea(Key key, int? targetFrameRate)
        {
            this.key = key;
            TargetFrameRate = targetFrameRate;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new[]
            {
                background = new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                new ButtonWithKeyBind(key)
                {
                    Text = "Feels better",
                    Y = 20,
                    Width = 0.8f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Action = () => ReportUserBest?.Invoke(),
                },
                visualContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            IsActiveArea.BindValueChanged(active =>
            {
                background.FadeColour(active.NewValue ? overlayColourProvider.Background4 : overlayColourProvider.Background6, 200, Easing.OutQuint);
            }, true);

            VisualMode.BindValueChanged(mode =>
            {
                switch (mode.NewValue)
                {
                    case LatencyVisualMode.Simple:
                        visualContent.Children = new Drawable[]
                        {
                            new LatencyMovableBox
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            Cursor = new LatencyCursorContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        };
                        break;

                    case LatencyVisualMode.CircleGameplay:
                        visualContent.Children = new Drawable[]
                        {
                            new CircleGameplay
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            Cursor = new LatencyCursorContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        };
                        break;

                    case LatencyVisualMode.ScrollingGameplay:
                        visualContent.Children = new Drawable[]
                        {
                            new ScrollingGameplay
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            Cursor = new LatencyCursorContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        };
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, true);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            IsActiveArea.Value = true;
            return base.OnMouseMove(e);
        }

        private double lastFrameTime;

        public override bool UpdateSubTree()
        {
            double elapsed = Clock.CurrentTime - lastFrameTime;
            if (TargetFrameRate.HasValue && elapsed < 1000.0 / TargetFrameRate)
                return false;

            lastFrameTime = Clock.CurrentTime;

            return base.UpdateSubTree();
        }
    }
}
