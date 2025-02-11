// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
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

        protected OsuSpriteText Text;

        protected Sample SelectSample { get; private set; } = null!;

        public FilterTabItem(T value)
            : base(value)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AutoSizeAxes = Axes.Both;
            AddRangeInternal(new Drawable[]
            {
                Text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 13, weight: FontWeight.Regular),
                    Text = LabelFor(Value)
                },
                new HoverSounds(HoverSampleSet.TabSelect)
            });

            Enabled.Value = true;

            SelectSample = audio.Samples.Get(@"UI/tabselect-select");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(_ => UpdateState());
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

        protected override void OnActivatedByUser() => SelectSample.Play();

        /// <summary>
        /// Returns the label text to be used for the supplied <paramref name="value"/>.
        /// </summary>
        protected virtual LocalisableString LabelFor(T value) => (value as Enum)?.GetLocalisableDescription() ?? value.ToString();

        protected virtual Color4 ColourActive => ColourProvider.Content1;
        protected virtual Color4 ColourNormal => ColourProvider.Light2;

        protected virtual void UpdateState()
        {
            Color4 colour = Active.Value ? ColourActive : ColourNormal;

            if (IsHovered)
                colour = colour.Lighten(0.2f);

            Text.FadeColour(colour, 200, Easing.OutQuint);
            Text.Font = Text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Regular);
        }
    }
}
