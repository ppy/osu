// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ToggleCoverButton : ProfileHeaderButton
    {
        public readonly BindableBool CoverVisible = new BindableBool();

        public override LocalisableString TooltipText => CoverVisible.Value ? UsersStrings.ShowCoverTo0 : UsersStrings.ShowCoverTo1;

        private SpriteIcon icon = null!;
        private Sample? sampleOpen;
        private Sample? sampleClose;

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverClickSounds();

        public ToggleCoverButton()
        {
            Action = () =>
            {
                CoverVisible.Toggle();
                (CoverVisible.Value ? sampleOpen : sampleClose)?.Play();
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, AudioManager audio)
        {
            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;

            sampleOpen = audio.Samples.Get(@"UI/dropdown-open");
            sampleClose = audio.Samples.Get(@"UI/dropdown-close");

            AutoSizeAxes = Axes.None;
            Size = new Vector2(30);
            Child = icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(10.5f, 12)
            };

            CoverVisible.BindValueChanged(visible => updateState(visible.NewValue), true);
        }

        private void updateState(bool detailsVisible) => icon.Icon = detailsVisible ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
    }
}
