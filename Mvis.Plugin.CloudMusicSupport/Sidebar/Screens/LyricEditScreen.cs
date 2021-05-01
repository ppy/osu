// Partial copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See Any function comment with "From" started for what is copyrighted by ppy Pty Ltd.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public class LyricEditScreen : LyricScreen
    {
        protected override DrawableLyric CreateDrawableLyric(Lyric lyric)
            => new EditableLyricPiece(lyric);

        public override Drawable[] Entries => new Drawable[]
        {
            new IconButton
            {
                Icon = FontAwesome.Solid.IceCream,
                TooltipText = "在当前时间添加新的歌词",
                Size = new Vector2(45),
                Action = () => addNewLyricAt(plugin.GetCurrentTrack().CurrentTime)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Backward,
                TooltipText = "Seek至上一拍",
                Action = () => seek(-1),
                Size = new Vector2(45)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Forward,
                TooltipText = "Seek至下一拍",
                Action = () => seek(1),
                Size = new Vector2(45)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Save,
                TooltipText = "保存",
                Action = saveChanges,
                Size = new Vector2(45)
            }
        };

        private void saveChanges()
        {
            var list = new List<Lyric>();

            foreach (var drawableLyric in LyricFlow)
            {
                list.Add(drawableLyric.Value);
            }

            plugin.ReplaceLyricWith(list);
        }

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        [Resolved]
        private LyricPlugin plugin { get; set; }

        //From EditorClock.cs
        private void seek(int direction)
        {
            var track = plugin.GetCurrentTrack();
            double current = track.CurrentTime;

            var controlPointInfo = mvisScreen.Beatmap.Value.Beatmap.ControlPointInfo;
            var timingPoint = controlPointInfo.TimingPointAt(current);

            if (direction < 0 && timingPoint.Time == current)
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                timingPoint = controlPointInfo.TimingPointAt(current - 1);

            double seekAmount = timingPoint.BeatLength * 1;
            double seekTime = current + seekAmount * direction;

            if (controlPointInfo.TimingPoints.Count == 0)
            {
                plugin.Seek(seekTime);
                return;
            }

            // We will be snapping to beats within timingPoint
            seekTime -= timingPoint.Time;

            // Determine the index from timingPoint of the closest beat to seekTime, accounting for scrolling direction
            int closestBeat;
            if (direction > 0)
                closestBeat = (int)Math.Floor(seekTime / seekAmount);
            else
                closestBeat = (int)Math.Ceiling(seekTime / seekAmount);

            seekTime = timingPoint.Time + closestBeat * seekAmount;

            // limit forward seeking to only up to the next timing point's start time.
            var nextTimingPoint = controlPointInfo.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            // Due to the rounding above, we may end up on the current beat. This will effectively cause 0 seeking to happen, but we don't want this.
            // Instead, we'll go to the next beat in the direction when this is the case
            if (Precision.AlmostEquals(current, seekTime, 0.5f))
            {
                closestBeat += direction > 0 ? 1 : -1;
                seekTime = timingPoint.Time + closestBeat * seekAmount;
            }

            if (seekTime < timingPoint.Time && timingPoint != controlPointInfo.TimingPoints.First())
                seekTime = timingPoint.Time;

            // Ensure the sought point is within the boundaries
            seekTime = Math.Clamp(seekTime, 0, track.Length);
            plugin.Seek(seekTime);
        }

        private void addNewLyricAt(double time)
        {
            plugin.Lyrics.Add(new Lyric
            {
                Time = (int)time
            });

            RefreshLrcInfo(plugin.Lyrics.OrderBy(l => l.Time).ToList());
        }

        protected override void UpdateStatus(LyricPlugin.Status status)
        {
        }

        protected override void RefreshLrcInfo(List<Lyric> lyrics)
        {
            LyricFlow.Clear();

            foreach (var t in lyrics)
            {
                LyricFlow.Add(CreateDrawableLyric(t));
            }
        }

        public override void OnEntering(IScreen last)
        {
            RefreshLrcInfo(plugin.Lyrics);
            this.MoveToX(0, 200, Easing.OutQuint).FadeInFromZero(200, Easing.OutQuint);
            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            this.MoveToX(10, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
            return base.OnExiting(next);
        }
    }
}
