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

namespace osu.Game.Screens.SelectV2
{
    public partial class FilterControl
    {
        public partial class ScopedBeatmapSetDisplay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
        {
            public Bindable<BeatmapSetInfo?> ScopedBeatmapSet
            {
                get => scopedBeatmapSet.Current;
                set => scopedBeatmapSet.Current = value;
            }

            private readonly BindableWithCurrent<BeatmapSetInfo?> scopedBeatmapSet = new BindableWithCurrent<BeatmapSetInfo?>();
            private Container content = null!;
            private OsuTextFlowContainer text = null!;
            private ShearedButton goBackButton = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                AutoSizeEasing = Easing.OutQuint;
                AutoSizeDuration = 200;
                CornerRadius = 8f;
                Masking = true;
                InternalChildren = new Drawable[]
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
                            goBackButton = new ShearedButton(80)
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Text = CommonStrings.Back,
                                RelativeSizeAxes = Axes.Y,
                                Height = 1,
                                Action = () => scopedBeatmapSet.Value = null,
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                scopedBeatmapSet.BindValueChanged(_ => updateState(), true);
            }

            private void updateState()
            {
                content.BypassAutoSizeAxes = scopedBeatmapSet.Value != null ? Axes.None : Axes.Y;

                if (scopedBeatmapSet.Value != null)
                {
                    text.Clear();
                    text.AddText(SongSelectStrings.TemporarilyShowingAllBeatmapsIn);
                    text.AddText(@" ");
                    text.AddText(scopedBeatmapSet.Value.Metadata.GetDisplayTitleRomanisable(), t => t.Font = OsuFont.Style.Body.With(weight: FontWeight.Bold));
                }
            }

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (scopedBeatmapSet.Value != null && e.Action == GlobalAction.Back && !e.Repeat)
                {
                    goBackButton.TriggerClick();
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
