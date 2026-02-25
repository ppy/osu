// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Screens.Select
{
    public partial class FilterControl
    {
        public partial class ScopedBeatmapSetDisplay : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
        {
            public IBindable<BeatmapSetInfo?> ScopedBeatmapSet { get; } = new Bindable<BeatmapSetInfo?>();

            private Box flashLayer = null!;
            private Container content = null!;
            private OsuTextFlowContainer text = null!;

            private const float transition_duration = 300;

            public ScopedBeatmapSetDisplay()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius = 8f;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(ISongSelect? songSelect, OverlayColourProvider colourProvider)
            {
                Content.AutoSizeEasing = Easing.OutQuint;
                Content.AutoSizeDuration = transition_duration;

                AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Highlight1,
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        BypassAutoSizeAxes = Axes.Y,
                        Shear = -OsuGame.SHEAR,
                        Padding = new MarginPadding
                        {
                            Horizontal = 6,
                            Vertical = 2,
                        },
                        Children = new Drawable[]
                        {
                            text = new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Body)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Colour = colourProvider.Background6,
                                Padding = new MarginPadding { Right = 80, Vertical = 5 }
                            },
                            new ShearedButton(80)
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Text = CommonStrings.Back,
                                RelativeSizeAxes = Axes.Y,
                                Height = 1,
                                Action = () => Action?.Invoke(),
                            }
                        }
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                });
                Action = () => songSelect?.UnscopeBeatmapSet();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScopedBeatmapSet.BindValueChanged(_ => updateState(), true);
            }

            private void updateState()
            {
                if (ScopedBeatmapSet.Value != null)
                {
                    content.BypassAutoSizeAxes = Axes.None;
                    text.Clear();
                    text.AddText(SongSelectStrings.TemporarilyShowingAllBeatmapsIn);
                    text.AddText(@" ");
                    text.AddText(ScopedBeatmapSet.Value.Metadata.GetDisplayTitleRomanisable(), t => t.Font = OsuFont.Style.Body.With(weight: FontWeight.Bold));
                }
                else
                {
                    flashLayer.FadeOutFromOne(transition_duration, Easing.OutQuint);
                    content.BypassAutoSizeAxes = Axes.Y;
                }
            }

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (ScopedBeatmapSet.Value != null && e.Action == GlobalAction.Back && !e.Repeat)
                {
                    TriggerClick();
                    return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }
    }
}
