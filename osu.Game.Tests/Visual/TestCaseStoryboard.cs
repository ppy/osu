// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Storyboards.Drawables;
using OpenTK.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;
using osu.Framework.Input;
using osu.Framework.Logging;
using System.Collections.Generic;
using osu.Framework.Input.Bindings;
using osu.Framework.Audio.Track;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseStoryboard : OsuTestCase
    {
        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        private readonly Container<DrawableStoryboard> storyboardContainer;
        private DrawableStoryboard storyboard;

        private readonly MusicController musicController;

        private readonly SongProgress progress;

        private RulesetStore rulesetStore;

        //To override OnClick and OnPressed
        private class CustomContainer : KeyBindingContainer<CustomAction>, IKeyBindingHandler<CustomAction> {
            MusicController musicController;

            //Press space to pause song
            public override IEnumerable<KeyBinding> DefaultKeyBindings => new List<KeyBinding> { new KeyBinding(InputKey.Space ,  CustomAction.TogglePause) };

            public bool OnPressed(CustomAction action)
            {
                if (action == CustomAction.TogglePause)
                    musicController.TogglePause();

                return true;
            }

            public bool OnReleased(CustomAction action)  { return false; }

            public void SetMusicController(MusicController musicController) => this.musicController = musicController;

            protected override bool OnClick(InputState inputState)
            {
                musicController.Show();
                musicController.HidePlaylist();
                return true;
            }
        }

        public enum CustomAction { TogglePause }

        public TestCaseStoryboard()
        {
            Clock = new FramedClock();
            CustomContainer customContainer;

            Add(customContainer = new CustomContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    storyboardContainer = new Container<DrawableStoryboard>
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            });
            Add(musicController = new MusicController
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                State = Visibility.Visible
            });
            Add(progress = new SongProgress
            {
                RelativeSizeAxes = Axes.X,
                AudioClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true },
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                AllowSeeking = true,
            });

            customContainer.SetMusicController(musicController);

            AddStep("Restart", restart);
            AddToggleStep("Passing", passing => { if (storyboard != null) storyboard.Passing = passing; });
            AddStep("Pause (Shortcut: Space)", () => customContainer.OnPressed(CustomAction.TogglePause));
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, RulesetStore rulesetStore)
        {
            beatmapBacking.BindTo(game.Beatmap);
            this.rulesetStore = rulesetStore;

            //adding this event trigger causes EnqueueAction in TrackBass to hang indefinitely. Not sure why
            beatmapBacking.ValueChanged += beatmapChanged;
        }

        private void beatmapChanged(WorkingBeatmap working)
            => loadStoryboard(working);

        private void restart()
        {
            var track = beatmapBacking.Value.Track;

            track.Reset();
            loadStoryboard(beatmapBacking.Value);
            track.Start();
        }

        private void loadStoryboard(WorkingBeatmap working)
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard);

            if (working.GetType() != typeof(DummyWorkingBeatmap))
            {
                var decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };
                storyboardContainer.Clock = decoupledClock;

                storyboard = working.Storyboard.CreateDrawable(beatmapBacking);
                storyboard.Passing = false;

                storyboardContainer.Add(storyboard);
                decoupledClock.ChangeSource(working.Track); 

                int id = working.BeatmapInfo.RulesetID;
                progress.Objects = working.GetPlayableBeatmap(rulesetStore.GetRuleset(id)).HitObjects;
                progress.AudioClock = working.Track;
                progress.AllowSeeking = true;
                progress.OnSeek = pos => working.Track.Seek(pos);
            }
        }
    }
}
