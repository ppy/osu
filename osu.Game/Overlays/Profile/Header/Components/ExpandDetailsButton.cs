// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ExpandDetailsButton : ProfileHeaderButton
    {
        public readonly BindableBool DetailsVisible = new BindableBool();

        public override LocalisableString TooltipText => DetailsVisible.Value ? CommonStrings.ButtonsCollapse : CommonStrings.ButtonsExpand;

        private SpriteIcon icon;
        private Sample sampleOpen;
        private Sample sampleClose;

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverClickSounds();

        public ExpandDetailsButton()
        {
            Action = () =>
            {
                DetailsVisible.Toggle();
                (DetailsVisible.Value ? sampleOpen : sampleClose)?.Play();
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, AudioManager audio)
        {
            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background2.Lighten(0.2f);

            sampleOpen = audio.Samples.Get(@"UI/dropdown-open");
            sampleClose = audio.Samples.Get(@"UI/dropdown-close");

            Child = icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(20, 12)
            };

            DetailsVisible.BindValueChanged(visible => updateState(visible.NewValue), true);
        }

        private void updateState(bool detailsVisible) => icon.Icon = detailsVisible ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
    }
}
