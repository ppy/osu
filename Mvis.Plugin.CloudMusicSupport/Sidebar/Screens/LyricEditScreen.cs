// Partial copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See Any function comment with "From" started for what is copyrighted by ppy Pty Ltd.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.CloudMusicSupport.Misc;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Screens
{
    public class LyricEditScreen : LyricScreen
    {
        private List<Lyric> localList = new List<Lyric>();

        protected override DrawableLyric CreateDrawableLyric(Lyric lyric)
        {
            var piece = new EditableLyricPiece(lyric)
            {
                OnDeleted = () => this.Delay(1).Schedule(applyChanges)
            };

            piece.OnAdjust = () => adjustPieceTime(piece);

            return piece;
        }

        private void adjustPieceTime(DrawableLyric drawableLyric)
        {
            LyricFlow.Remove(drawableLyric);
            sortPiece(drawableLyric);
        }

        public override IconButton[] Entries => new[]
        {
            new IconButton
            {
                Icon = FontAwesome.Solid.IceCream,
                TooltipText = CloudMusicStrings.InsertNewLine,
                Size = new Vector2(45),
                Action = () => addNewLyricAt(Plugin.GetCurrentTrack().CurrentTime)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Backward,
                TooltipText = CloudMusicStrings.SeekToPrev,
                Action = () => seek(-1),
                Size = new Vector2(45)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Forward,
                TooltipText = CloudMusicStrings.SeekToNext,
                Action = () => seek(1),
                Size = new Vector2(45)
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.AngleDown,
                Size = new Vector2(45),
                TooltipText = CloudMusicStrings.ScrollToCurrent,
                Action = ScrollToCurrent
            },
            new IconButton
            {
                Icon = FontAwesome.Solid.Save,
                TooltipText = CloudMusicStrings.Save,
                Action = applyChanges,
                Size = new Vector2(45)
            }
        };

        private void applyChanges()
        {
            var list = new List<Lyric>();

            foreach (var drawableLyric in LyricFlow.Children)
            {
                list.Add(drawableLyric.Value);
            }

            localList = list;

            performSave(false);
        }

        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            localList = Plugin.Lyrics;
            RefreshLrcInfo(localList);
        }

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

        private void performSave(bool saveToDisk)
        {
            Plugin.ReplaceLyricWith(localList, saveToDisk);
        }

        private void addNewLyricAt(double time)
        {
            //新建lyric
            var lrc = new Lyric
            {
                Time = (int)time
            };

            //添加歌词至localList
            localList.Add(lrc);

            //创建与lrc对应的drawable
            var piece = CreateDrawableLyric(lrc);

            sortPiece(piece);
        }

        private void sortPiece(DrawableLyric piece)
        {
            var lrc = piece.Value;

            //将歌词按时间排序，并同步至localList中。
            localList = localList.OrderBy(l => l.Time).ToList();

            //获取在歌词列表中的index
            var targetIndex = localList.IndexOf(lrc);

            //遍历LyricFlow，找到任何LyricFlow中Index大于等于目标Index的片，并将他们的Index+1以实现靠后
            foreach (var d in LyricFlow)
            {
                //获取在LyricFlow中的Index
                //drawableIndex不是目标index
                var drawableIndex = LyricFlow.IndexOf(d);

                //如果这个片比目标Index大，则将该片的Index+1
                if (drawableIndex >= targetIndex)
                    LyricFlow.SetLayoutPosition(d, drawableIndex + 1);
            }

            //插入
            LyricFlow.Insert(targetIndex, piece);

            //临时保存
            performSave(false);
        }

        protected override void UpdateStatus(LyricPlugin.Status status)
        {
        }

        protected override void RefreshLrcInfo(List<Lyric> lyrics)
        {
            LyricFlow.Clear();

            int index = 0;

            foreach (var t in lyrics)
            {
                LyricFlow.Insert(index, CreateDrawableLyric(t));
                index++;
            }
        }

        public override void OnEntering(IScreen last)
        {
            this.MoveToX(0, 200, Easing.OutQuint).FadeInFromZero(200, Easing.OutQuint);
            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            Plugin.IsEditing = false;
            performSave(true);
            this.MoveToX(10, 200, Easing.OutQuint).FadeOut(200, Easing.OutQuint);
            return base.OnExiting(next);
        }
    }
}
