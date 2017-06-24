// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class SectionTrackOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION_FOR_EFFECT / 2;

        private List<BreakPeriod> breaks = new List<BreakPeriod>();

        private readonly BindableDouble healthBindable = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1
        };

        private int currentBreakIndex;
        private double imageAppearTime;
        private bool isBreak;
        private bool imageHasBeenShown;
        private double health;

        private SampleChannel samplePass;
        private SampleChannel sampleFail;

        private readonly TextAwesome resultIcon;

        private IClock audioClock;
        public IClock AudioClock { set { audioClock = value; } }

        public List<BreakPeriod> Breaks { set { breaks = value; } }

        public Action BreakPeriodIn;
        public Action BreakPeriodOut;

        public SectionTrackOverlay()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Add(resultIcon = new TextAwesome
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 100,
                Alpha = 0,
                AlwaysPresent = true,
            });

            healthBindable.ValueChanged += newValue => { health = newValue; };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            samplePass = audio.Sample.Get(@"SectionResult/sectionpass");
            sampleFail = audio.Sample.Get(@"SectionResult/sectionfail");
        }

        public void BindHealth(BindableDouble health) => healthBindable.BindTo(health);

        protected override void Update()
        {
            if (breaks == null) return;
            if (breaks.Count == 0) return;
            if (currentBreakIndex == breaks.Count) return;

            double currentTime = audioClock?.CurrentTime ?? Time.Current;

            var currentBreak = breaks[currentBreakIndex];

            if (!isBreak)
            {
                if (currentTime > currentBreak.StartTime)
                {
                    isBreak = true;
                    imageHasBeenShown = false;
                    imageAppearTime = currentTime + (currentBreak.EndTime - currentBreak.StartTime) / 2;
                    BreakPeriodIn?.Invoke();
                }
            }
            else
            {
                if (currentTime > imageAppearTime && !imageHasBeenShown && currentBreak.HasEffect)
                {
                    if (currentBreak.HasPeriodResult)
                    {
                        // Show icon depends on HP
                        if(health < 0.3)
                        {
                            resultIcon.Icon = FontAwesome.fa_close;
                            sampleFail.Play();
                        }
                        else
                        {
                            resultIcon.Icon = FontAwesome.fa_check;
                            samplePass.Play();
                        }

                        resultIcon.FadeTo(1);
                        Delay(100);
                        Schedule(() => resultIcon.FadeTo(0));
                        Delay(100);
                        Schedule(() => resultIcon.FadeTo(1));
                        Delay(1000);
                        Schedule(() => resultIcon.FadeTo(0, 200));

                        imageHasBeenShown = true;
                    }
                }

                if (currentBreak.EndTime - currentTime < fade_duration)
                {
                    // Exit from break
                    BreakPeriodOut?.Invoke();
                    currentBreakIndex++;
                    isBreak = false;
                }
            }
        }
    }
}
