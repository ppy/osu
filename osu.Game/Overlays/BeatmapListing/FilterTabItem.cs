// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class FilterTabItem<T> : TabItem<T>
    {
        [Resolved]
        protected OverlayColourProvider ColourProvider { get; private set; }

        private OsuSpriteText text;

        public FilterTabItem(T value)
            : base(value)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            AddRangeInternal(new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 13, weight: FontWeight.Regular),
                    Text = LabelFor(Value)
                },
                new HoverSounds(HoverSampleSet.TabSelect)
            });

            Enabled.Value = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UpdateState();
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            UpdateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            UpdateState();
        }

        protected override void OnActivated() => UpdateState();

        protected override void OnDeactivated() => UpdateState();

        /// <summary>
        /// Returns the label text to be used for the supplied <paramref name="value"/>.
        /// </summary>
        protected virtual LocalisableString LabelFor(T value) => (value as Enum)?.GetLocalisableDescription() ?? value.ToString();

        protected virtual bool HighlightOnHoverWhenActive => false;

        protected virtual void UpdateState()
        {
            bool highlightHover = IsHovered && (!Active.Value || HighlightOnHoverWhenActive);

            text.FadeColour(highlightHover ? ColourProvider.Content2 : GetStateColour(), 200, Easing.OutQuint);
            text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Regular);
        }

        protected virtual Color4 GetStateColour() => Active.Value ? ColourProvider.Content1 : ColourProvider.Light2;
    }
}
