// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgress : SongProgress
    {
        private readonly SongProgressInfo info;
        private readonly ArgonSongProgressGraph graph;
        private readonly ArgonSongProgressBar bar;
        private readonly Container graphContainer;

        private const float bar_height = 10;

        public readonly Bindable<bool> AllowSeeking = new BindableBool();

        [SettingSource("Show difficulty graph", "Whether a graph displaying difficulty throughout the beatmap should be shown")]
        public Bindable<bool> ShowGraph { get; } = new BindableBool(true);

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        private IClock referenceClock => drawableRuleset?.FrameStableClock ?? GameplayClock;

        [Resolved]
        private Player? player { get; set; }

        public ArgonSongProgress()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;
            CornerRadius = 5;
            Children = new Drawable[]
            {
                info = new SongProgressInfo
                {
                    Origin = Anchor.TopLeft,
                    Name = "Info",
                    Anchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    ShowProgress = false
                },
                bar = new ArgonSongProgressBar(bar_height)
                {
                    Name = "Seek bar",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    OnSeek = time => player?.Seek(time),
                },
                graphContainer = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Masking = true,
                    CornerRadius = 5,
                    Child = graph = new ArgonSongProgressGraph
                    {
                        Name = "Difficulty graph",
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive
                    },
                    RelativeSizeAxes = Axes.X,
                },
            };
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            base.LoadComplete();

            if (drawableRuleset != null)
            {
                if (player?.Configuration.AllowUserInteraction == true)
                    ((IBindable<bool>)AllowSeeking).BindTo(drawableRuleset.HasReplayLoaded);
            }

            info.ShowProgress = false;
            info.TextColour = Colour4.White;
            info.Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold);
        }

        protected override void LoadComplete()
        {
            AllowSeeking.BindValueChanged(_ => updateBarVisibility(), true);
            ShowGraph.BindValueChanged(_ => updateGraphVisibility(), true);
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
            graph.Objects = objects;

            info.StartTime = bar.StartTime = FirstHitTime;
            info.EndTime = bar.EndTime = LastHitTime;
        }

        private void updateBarVisibility()
        {
            bar.Interactive = AllowSeeking.Value;
        }

        private void updateGraphVisibility()
        {
            graph.FadeTo(ShowGraph.Value ? 1 : 0, 200, Easing.In);
            bar.ShowBackground = !ShowGraph.Value;
        }

        protected override void Update()
        {
            base.Update();
            Height = bar.Height + bar_height + info.Height;
            graphContainer.Height = bar.Height;
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
            bar.ReferenceTime = GameplayClock.CurrentTime;

            if (isIntro)
                bar.CurrentTime = 0;
            else
                bar.CurrentTime = referenceClock.CurrentTime;
        }
    }
}
