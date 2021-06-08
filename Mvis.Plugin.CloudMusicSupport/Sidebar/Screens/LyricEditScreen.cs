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
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public class LyricEditScreen : LyricScreen<EditableLyricPiece>
    {
        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        protected override EditableLyricPiece CreateDrawableLyric(Lyric lyric)
        {
            var piece = new EditableLyricPiece(lyric);

            piece.OnDeleted += () =>
            {
                Logger.Log("ONDELETE");
                lyrics.Remove(piece.Value);

                Logger.Log($"COUNT 1: {AvaliableDrawableLyrics.Count}");
                var p = AvaliableDrawableLyrics.Find(drawableLyric => drawableLyric.Value.Equals(piece.Value));
                AvaliableDrawableLyrics.Remove(p);
                Logger.Log($"COUNT 2: {AvaliableDrawableLyrics.Count}");
            };

            piece.OnAdjust += () =>
            {
                lyrics = lyrics.OrderBy(l => l.Time).ToList();

                var height = piece.FinalHeight();

                foreach (var drawableLyric in AvaliableDrawableLyrics)
                {
                    if (AvaliableDrawableLyrics.IndexOf(drawableLyric) >= lyrics.IndexOf(piece.Value))
                        drawableLyric.CurrentY += height;
                }
            };

            return piece;
        }

        public override IconButton[] Entries => new[]
        {
            new IconButton
            {
                Icon = FontAwesome.Solid.IceCream,
                TooltipText = "在当前时间添加新的歌词",
                Size = new Vector2(45),
                Action = () => addNewLyricAtTime(Plugin.GetCurrentTrack().CurrentTime)
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
                Icon = FontAwesome.Solid.AngleDown,
                Size = new Vector2(45),
                TooltipText = "滚动到当前歌词",
                Action = ScrollToCurrent
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Save,
                TooltipText = "保存",
                Action = () => performSave(true),
                Size = new Vector2(45)
            }
        };

        private List<Lyric> lyrics;

        [BackgroundDependencyLoader]
        private void load()
        {
            lyrics = Plugin.Lyrics;
        }

        private void addNewLyricAtTime(double currentTime)
        {
            Lyric lrc;
            lyrics.Add(lrc = new Lyric
            {
                Time = currentTime
            });

            lyrics = lyrics.OrderBy(l => l.Time).ToList();

            AvaliableDrawableLyrics.Insert(lyrics.IndexOf(lrc), CreateDrawableLyric(lrc));
        }

        private void performSave(bool saveToDisk) =>
            Plugin.ReplaceLyricWith(lyrics, saveToDisk);

        //From EditorClock.cs
        private void seek(int direction)
        {
            var track = Plugin.GetCurrentTrack();
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
                Plugin.Seek(seekTime);
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
            Plugin.Seek(seekTime);
        }

        public override void OnEntering(IScreen last)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeInFromZero(200, Easing.OutQuint);
            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            Plugin.IsEditing = false;
            this.MoveToX(10, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
            return base.OnExiting(next);
        }
    }
}
