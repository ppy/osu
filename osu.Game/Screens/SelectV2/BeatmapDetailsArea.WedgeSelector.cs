// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDetailsArea
    {
        public partial class WedgeSelector<T> : TabControl<T>
            where T : struct, Enum
        {
            private Circle strip = null!;

            protected override Dropdown<T>? CreateDropdown() => null;

            protected override TabItem<T> CreateTabItem(T value) => new TabItem(value);

            protected new TabItem SelectedTab => (TabItem)base.SelectedTab;

            public WedgeSelector(float spacing)
            {
                TabContainer.Spacing = new Vector2(spacing, 0f);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                AddInternal(strip = new Circle
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = 2,
                    Colour = colourProvider.Highlight1,
                });

                foreach (var type in Enum.GetValues<T>())
                    AddItem(type);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(_ => updateDisplay());

                ScheduleAfterChildren(() =>
                {
                    updateDisplay();
                    FinishTransforms(true);
                });
            }

            private void updateDisplay()
            {
                strip.MoveToX(SelectedTab.Text.ToSpaceOfOtherDrawable(Vector2.Zero, this).X, 300, Easing.OutQuint);
                strip.ResizeWidthTo(SelectedTab.Text.Width, 0, Easing.OutQuint);
            }

            protected partial class TabItem : TabItem<T>
            {
                private Sample? selectSample;

                [Resolved]
                private OverlayColourProvider colourProvider { get; set; } = null!;

                public readonly OsuSpriteText Text;

                public TabItem(T value)
                    : base(value)
                {
                    AutoSizeAxes = Axes.Both;

                    Children = new Drawable[]
                    {
                        Text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = value.GetLocalisableDescription(),
                            Font = OsuFont.Style.Body,
                        },
                        new HoverSounds(HoverSampleSet.TabSelect)
                    };
                }

                [BackgroundDependencyLoader]
                private void load(AudioManager audio)
                {
                    selectSample = audio.Samples.Get(@"UI/tabselect-select");
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    updateDisplay();
                }

                protected override void OnActivatedByUser() => selectSample?.Play();

                protected override void OnActivated() => updateDisplay();

                protected override void OnDeactivated() => updateDisplay();

                protected override bool OnHover(HoverEvent e)
                {
                    updateDisplay();
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e) => updateDisplay();

                private void updateDisplay()
                {
                    if (Active.Value || IsHovered)
                        Text.FadeColour(colourProvider.Content1, 300, Easing.OutQuint);
                    else
                        Text.FadeColour(colourProvider.Content2, 300, Easing.OutQuint);

                    Text.Font = Text.Font.With(weight: Active.Value ? FontWeight.SemiBold : FontWeight.Regular);
                }
            }
        }
    }
}
