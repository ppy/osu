﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using System;
using osu.Game.Resources.Localisation.Web;
using osu.Framework.Extensions;

namespace osu.Game.Overlays
{
    public class OverlayPanelDisplayStyleControl : OsuTabControl<OverlayPanelDisplayStyle>
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

        private class PanelDisplayTabItem : TabItem<OverlayPanelDisplayStyle>, IHasTooltip
        {
            public IconUsage Icon
            {
                set => icon.Icon = value;
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            public LocalisableString TooltipText => Value.GetLocalisableDescription();

            private readonly SpriteIcon icon;

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
                    new HoverClickSounds()
                });
            }

            protected override void OnActivated() => updateState();

            protected override void OnDeactivated() => updateState();

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

    [LocalisableEnum(typeof(OverlayPanelDisplayStyleEnumLocalisationMapper))]
    public enum OverlayPanelDisplayStyle
    {
        Card,
        List,
        Brick
    }

    public class OverlayPanelDisplayStyleEnumLocalisationMapper : EnumLocalisationMapper<OverlayPanelDisplayStyle>
    {
        public override LocalisableString Map(OverlayPanelDisplayStyle value)
        {
            switch (value)
            {
                case OverlayPanelDisplayStyle.Card:
                    return UsersStrings.ViewModeCard;

                case OverlayPanelDisplayStyle.List:
                    return UsersStrings.ViewModeList;

                case OverlayPanelDisplayStyle.Brick:
                    return UsersStrings.ViewModeBrick;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
