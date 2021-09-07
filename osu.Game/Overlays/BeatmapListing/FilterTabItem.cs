// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class FilterTabItem<T> : TabItem<T>
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private OsuSpriteText text;

        public FilterTabItem(T value)
            : base(value)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            AddRangeInternal(new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 13, weight: FontWeight.Regular),
                    Text = LabelFor(Value)
                },
                new HoverClickSounds()
            });

            Enabled.Value = true;
            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        /// <summary>
        /// Returns the label text to be used for the supplied <paramref name="value"/>.
        /// </summary>
        protected virtual LocalisableString LabelFor(T value) => (value as Enum)?.GetLocalisableDescription() ?? value.ToString();

        private void updateState()
        {
            text.FadeColour(IsHovered ? colourProvider.Light1 : GetStateColour(), 200, Easing.OutQuint);
            text.Font = text.Font.With(weight: Active.Value ? FontWeight.SemiBold : FontWeight.Regular);
        }

        protected virtual Color4 GetStateColour() => Active.Value ? colourProvider.Content1 : colourProvider.Light2;
    }
}
