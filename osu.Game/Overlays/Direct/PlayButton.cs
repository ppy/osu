// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        public string TrackUrl;

        public Bindable<bool> Playing;

        public Track Track;
        private Bindable<WorkingBeatmap> gameBeatmap;
        private AudioManager audio;

        private Color4 hoverColour;
        private readonly SpriteIcon icon;

        public PlayButton(Bindable<bool> playing)
        {
            Playing = playing;
            Add(icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.fa_play,
            });

            Playing.ValueChanged += newValue => icon.Icon = newValue ? (Track == null ? FontAwesome.fa_spinner : FontAwesome.fa_pause) : FontAwesome.fa_play;

            Playing.ValueChanged += newValue =>
            {
                if (newValue)
                    Track?.Start();
                else
                    Track?.Stop();
            };

            Playing.ValueChanged += newValue => icon.FadeColour(newValue || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, OsuGameBase game, AudioManager audio)
        {
            hoverColour = colour.Yellow;
            gameBeatmap = game.Beatmap;
            this.audio = audio;
        }

        private Task loadTask;

        protected override bool OnClick(InputState state)
        {
            gameBeatmap.Value.Track.Stop();

            Playing.Value = !Playing.Value;

            if (loadTask == null)
            {
                icon.Spin(2000, RotationDirection.Clockwise);

                loadTask = Task.Run(() =>
                {
                    Track = audio.Track.Get(TrackUrl);
                    Track.Looping = true;
                    if (Playing)
                        Track.Start();

                    icon.ClearTransforms();
                    icon.Rotation = 0;
                    Playing.TriggerChange();
                });
            }

            return true;
        }

        protected override bool OnHover(InputState state)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if(!Playing)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(state);
        }
    }
}
