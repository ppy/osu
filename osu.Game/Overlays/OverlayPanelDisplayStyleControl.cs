// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osuTK.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Framework.Extensions;

namespace osu.Game.Overlays
{
    public partial class OverlayPanelDisplayStyleControl : OsuTabControl<OverlayPanelDisplayStyle>
    {
        protected override Dropdown<OverlayPanelDisplayStyle> CreateDropdown() => null;

        protected override TabItem<OverlayPanelDisplayStyle> CreateTabItem(OverlayPanelDisplayStyle value) => new PanelDisplayTabItem(value);

        protected override bool AddEnumEntriesAutomatically => false;

        public OverlayPanelDisplayStyleControl()
        {
            AutoSizeAxes = Axes.Both;

            AddTabItem(new PanelDisplayTabItem(OverlayPanelDisplayStyle.Card)
            {
                Icon = FontAwesome.Solid.Square
            });
            AddTabItem(new PanelDisplayTabItem(OverlayPanelDisplayStyle.List)
            {
                Icon = FontAwesome.Solid.Bars
            });
            AddTabItem(new PanelDisplayTabItem(OverlayPanelDisplayStyle.Brick)
            {
                Icon = FontAwesome.Solid.Th
            });
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal
        };

        private partial class PanelDisplayTabItem : TabItem<OverlayPanelDisplayStyle>, IHasTooltip
        {
            public IconUsage Icon
            {
                set => icon.Icon = value;
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            public LocalisableString TooltipText => Value.GetLocalisableDescription();

            private readonly SpriteIcon icon;

            private Sample selectSample = null!;

            public PanelDisplayTabItem(OverlayPanelDisplayStyle value)
                : base(value)
            {
                Size = new Vector2(11);
                AddRange(new Drawable[]
                {
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit
                    },
                    new HoverSounds(HoverSampleSet.TabSelect)
                });
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                selectSample = audio.Samples.Get(@"UI/tabselect-select");
            }

            protected override void OnActivated() => updateState();

            protected override void OnDeactivated() => updateState();

            protected override void OnActivatedByUser() => selectSample.Play();

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private void updateState() => icon.Colour = Active.Value || IsHovered ? colourProvider.Light1 : Color4.White;
        }
    }

    public enum OverlayPanelDisplayStyle
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ViewModeCard))]
        Card,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ViewModeList))]
        List,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ViewModeBrick))]
        Brick
    }
}
