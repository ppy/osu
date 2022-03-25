// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Online.API;
using System;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Menu
{
    public class MenuSideFlashes : BeatSyncedContainer
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private Box leftBox;
        private Box rightBox;

        private const float amplitude_dead_zone = 0.25f;
        private const float alpha_multiplier = (1 - amplitude_dead_zone) / 0.55f;
        private const float kiai_multiplier = (1 - amplitude_dead_zone * 0.95f) / 0.8f;

        private const int box_max_alpha = 200;
        private const double box_fade_in_time = 65;
        private const int box_width = 200;

        private IBindable<APIUser> user;
        private Bindable<Skin> skin;

        [Resolved]
        private OsuColour colours { get; set; }

        public MenuSideFlashes()
        {
            EarlyActivationMilliseconds = box_fade_in_time;

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, IAPIProvider api, SkinManager skinManager)
        {
            this.beatmap.BindTo(beatmap);

            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            Children = new Drawable[]
            {
                leftBox = new Box
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width * 2,
                    Height = 1.5f,
                    // align off-screen to make sure our edges don't become visible during parallax.
                    X = -box_width,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive
                },
                rightBox = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Width = box_width * 2,
                    Height = 1.5f,
                    X = box_width,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive
                }
            };

            user.ValueChanged += _ => updateColour();
            skin.BindValueChanged(_ => updateColour(), true);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (beatIndex < 0)
                return;

            if (effectPoint.KiaiMode ? beatIndex % 2 == 0 : beatIndex % timingPoint.TimeSignature.Numerator == 0)
                flash(leftBox, timingPoint.BeatLength, effectPoint.KiaiMode, amplitudes);
            if (effectPoint.KiaiMode ? beatIndex % 2 == 1 : beatIndex % timingPoint.TimeSignature.Numerator == 0)
                flash(rightBox, timingPoint.BeatLength, effectPoint.KiaiMode, amplitudes);
        }

        private void flash(Drawable d, double beatLength, bool kiai, ChannelAmplitudes amplitudes)
        {
            d.FadeTo(Math.Max(0, ((ReferenceEquals(d, leftBox) ? amplitudes.LeftChannel : amplitudes.RightChannel) - amplitude_dead_zone) / (kiai ? kiai_multiplier : alpha_multiplier)), box_fade_in_time)
             .Then()
             .FadeOut(beatLength, Easing.In);
        }

        private void updateColour()
        {
            Color4 baseColour = colours.Blue;

            if (user.Value?.IsSupporter ?? false)
                baseColour = skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? baseColour;

            // linear colour looks better in this case, so let's use it for now.
            Color4 gradientDark = baseColour.Opacity(0).ToLinear();
            Color4 gradientLight = baseColour.Opacity(0.6f).ToLinear();

            leftBox.Colour = ColourInfo.GradientHorizontal(gradientLight, gradientDark);
            rightBox.Colour = ColourInfo.GradientHorizontal(gradientDark, gradientLight);
        }
    }
}
