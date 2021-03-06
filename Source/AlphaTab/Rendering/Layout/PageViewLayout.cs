﻿/*
 * This file is part of alphaTab.
 * Copyright (c) 2014, Daniel Kuschny and Contributors, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or at your option any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */
using System;
using AlphaTab.Collections;
using AlphaTab.Model;
using AlphaTab.Platform.Model;
using AlphaTab.Rendering.Staves;
using AlphaTab.Rendering.Utils;

namespace AlphaTab.Rendering.Layout
{
    /// <summary>
    /// This layout arranges the bars into a fixed width and dynamic height region. 
    /// </summary>
    public class PageViewLayout : ScoreLayout
    {
        // left top right bottom
        public static readonly float[] PagePadding = { 40, 40, 40, 40 };
        public const float WidthOn100 = 950;
        public const float GroupSpacing = 20;

        private FastList<StaveGroup> _groups;

        public PageViewLayout(ScoreRenderer renderer)
            : base(renderer)
        {
            _groups = new FastList<StaveGroup>();
        }

        public override void DoLayout()
        {
            _groups = new FastList<StaveGroup>();

            var score = Renderer.Score;

            var startIndex = Renderer.Settings.Layout.Get("start", 1);
            startIndex--; // map to array index
            startIndex = Math.Min(score.MasterBars.Count - 1, Math.Max(0, startIndex));
            var currentBarIndex = startIndex;

            var endBarIndex = Renderer.Settings.Layout.Get("count", score.MasterBars.Count);
            if (endBarIndex < 0) endBarIndex = score.MasterBars.Count;
            endBarIndex = startIndex + endBarIndex - 1; // map count to array index
            endBarIndex = Math.Min(score.MasterBars.Count - 1, Math.Max(0, endBarIndex));


            var x = PagePadding[0];
            var y = PagePadding[1];

            y = DoScoreInfoLayout(y);

            var autoSize = Renderer.Settings.Layout.Get("autoSize", true);
            if (autoSize || Renderer.Settings.Width <= 0)
            {
                Width = WidthOn100 * Scale;
            }
            else
            {
                Width = Renderer.Settings.Width;
            }

            if (Renderer.Settings.Staves.Count > 0)
            {
                while (currentBarIndex <= endBarIndex)
                {
                    var group = CreateStaveGroup(currentBarIndex, endBarIndex);
                    _groups.Add(group);

                    group.X = x;
                    group.Y = y;

                    FitGroup(group);
                    group.FinalizeGroup(this);

                    y += group.Height + (GroupSpacing * Scale);

                    currentBarIndex = group.LastBarIndex + 1;
                }
            }

            Height = y + PagePadding[3];
        }

        private float DoScoreInfoLayout(float y)
        {
            // TODO: Check if it's a good choice to provide the complete flags as setting
            HeaderFooterElements flags = Renderer.Settings.Layout.Get("hideInfo", false) ? HeaderFooterElements.None : HeaderFooterElements.All;
            Score score = Renderer.Score;
            float scale = Scale;

            if (!string.IsNullOrEmpty(score.Title) && (flags & HeaderFooterElements.Title) != 0)
            {
                y += (35 * scale);
            }
            if (!string.IsNullOrEmpty(score.SubTitle) && (flags & HeaderFooterElements.SubTitle) != 0)
            {
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Artist) && (flags & HeaderFooterElements.Artist) != 0)
            {
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Album) && (flags & HeaderFooterElements.Album) != 0)
            {
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Music) && score.Music == score.Words && (flags & HeaderFooterElements.WordsAndMusic) != 0)
            {
                y += (20 * scale);
            }
            else
            {
                if (!string.IsNullOrEmpty(score.Music) && (flags & HeaderFooterElements.Music) != 0)
                {
                    y += (20 * scale);
                }
                if (!string.IsNullOrEmpty(score.Words) && (flags & HeaderFooterElements.Words) != 0)
                {
                    y += (20 * scale);
                }
            }

            y += (20 * scale);

            // tuning info
            if (Renderer.Tracks.Length == 1 && !Renderer.Tracks[0].IsPercussion)
            {
                var tuning = Tuning.FindTuning(Renderer.Tracks[0].Tuning);
                if (tuning != null)
                {
                    // Name
                    y += (15 * scale);

                    if (!tuning.IsStandard)
                    {
                        // Strings
                        var stringsPerColumn = (int)Math.Ceiling(Renderer.Tracks[0].Tuning.Count / 2.0);
                        y += (stringsPerColumn * (15 * scale));
                    }

                    y += (15 * scale);
                }
            }

            y += (40 * scale);

            return y;
        }

        public override void PaintScore()
        {
            var x = PagePadding[0];
            var y = PagePadding[1];

            y = PaintScoreInfo(x, y);

            Renderer.Canvas.Color = Renderer.RenderingResources.MainGlyphColor;
            Renderer.Canvas.TextAlign = TextAlign.Left;
            for (int i = 0, j = _groups.Count; i < j; i++)
            {
                _groups[i].Paint(0, 0, Renderer.Canvas);
            }
        }

        private void DrawCentered(string text, Font font, float y)
        {
            Renderer.Canvas.Font = font;
            Renderer.Canvas.FillText(text, Width / 2.0f, y);
        }

        private float PaintScoreInfo(float x, float y)
        {
            HeaderFooterElements flags = Renderer.Settings.Layout.Get("hideInfo", false) ? HeaderFooterElements.None : HeaderFooterElements.All;
            var score = Renderer.Score;
            var scale = Scale;

            var canvas = Renderer.Canvas;
            var res = Renderer.RenderingResources;

            canvas.Color = res.ScoreInfoColor;
            canvas.TextAlign = TextAlign.Center;

            string str;
            if (!string.IsNullOrEmpty(score.Title) && (flags & HeaderFooterElements.Title) != 0)
            {
                DrawCentered(score.Title, res.TitleFont, y);
                y += (35 * scale);
            }
            if (!string.IsNullOrEmpty(score.SubTitle) && (flags & HeaderFooterElements.SubTitle) != 0)
            {
                DrawCentered(score.SubTitle, res.SubTitleFont, y);
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Artist) && (flags & HeaderFooterElements.Artist) != 0)
            {
                DrawCentered(score.Artist, res.SubTitleFont, y);
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Album) && (flags & HeaderFooterElements.Album) != 0)
            {
                DrawCentered(score.Album, res.SubTitleFont, y);
                y += (20 * scale);
            }
            if (!string.IsNullOrEmpty(score.Music) && score.Music == score.Words && (flags & HeaderFooterElements.WordsAndMusic) != 0)
            {
                DrawCentered("Music and Words by " + score.Words, res.WordsFont, y);
                y += (20 * scale);
            }
            else
            {
                canvas.Font = res.WordsFont;
                if (!string.IsNullOrEmpty(score.Music) && (flags & HeaderFooterElements.Music) != 0)
                {
                    canvas.TextAlign = TextAlign.Right;
                    canvas.FillText("Music by " + score.Music, Width - PagePadding[2], y);
                }
                if (!string.IsNullOrEmpty(score.Words) && (flags & HeaderFooterElements.Words) != 0)
                {
                    canvas.TextAlign = TextAlign.Left;
                    canvas.FillText("Words by " + score.Music, x, y);
                }
                y += (20 * scale);
            }

            y += (20 * scale);

            // tuning info
            if (Renderer.Tracks.Length == 1 && !Renderer.Tracks[0].IsPercussion)
            {
                canvas.TextAlign = TextAlign.Left;
                var tuning = Tuning.FindTuning(Renderer.Tracks[0].Tuning);
                if (tuning != null)
                {
                    // Name
                    canvas.Font = res.EffectFont;
                    canvas.FillText(tuning.Name, x, y);

                    y += (15 * scale);

                    if (!tuning.IsStandard)
                    {
                        // Strings
                        var stringsPerColumn = (int)Math.Ceiling(Renderer.Tracks[0].Tuning.Count / 2.0);

                        var currentX = x;
                        var currentY = y;

                        for (int i = 0, j = Renderer.Tracks[0].Tuning.Count; i < j; i++)
                        {
                            str = "(" + (i + 1) + ") = " + Tuning.GetTextForTuning(Renderer.Tracks[0].Tuning[i], false);
                            canvas.FillText(str, currentX, currentY);
                            currentY += (15 * scale);
                            if (i == stringsPerColumn - 1)
                            {
                                currentY = y;
                                currentX += (43 * scale);
                            }
                        }

                        y += (stringsPerColumn * (15 * scale));
                    }
                }
            }
            y += 25 * scale;
            return y;
        }

        /// <summary>
        /// Realignes the bars in this line according to the available space
        /// </summary>
        private void FitGroup(StaveGroup group)
        {
            // calculate additional space for each bar (can be negative!)
            float barSpace = 0f;
            float freeSpace = MaxWidth - group.Width;

            if (freeSpace != 0 && group.MasterBars.Count > 0)
            {
                barSpace = freeSpace / group.MasterBars.Count;
            }

            if (group.IsFull || barSpace < 0)
            {
                // add it to the measures
                group.ApplyBarSpacing(barSpace);
            }

            Width = Math.Max(Width, group.Width);
        }

        private StaveGroup CreateStaveGroup(int currentBarIndex, int endIndex)
        {
            var group = CreateEmptyStaveGroup();
            group.Index = _groups.Count;

            var barsPerRow = Renderer.Settings.Layout.Get("barsPerRow", -1);

            var maxWidth = MaxWidth;
            var end = endIndex + 1;
            for (int i = currentBarIndex; i < end; i++)
            {
                group.AddBars(Renderer.Tracks, i);

                var groupIsFull = false;

                // can bar placed in this line?
                if (barsPerRow == -1 && ((group.Width) >= maxWidth && group.MasterBars.Count != 0))
                {
                    groupIsFull = true;
                }
                else if (group.MasterBars.Count == barsPerRow + 1)
                {
                    groupIsFull = true;
                }

                if (groupIsFull)
                {
                    group.RevertLastBar();
                    group.IsFull = true;
                    return group;
                }

                group.X = 0;
            }

            return group;
        }

        private float MaxWidth
        {
            get
            {
                var autoSize = Renderer.Settings.Layout.Get("autoSize", true);
                var width = autoSize ? SheetWidth : Renderer.Settings.Width;
                return width - PagePadding[0] - PagePadding[2];
            }
        }


        private float SheetWidth
        {
            get
            {
                return (WidthOn100 * Scale);
            }
        }

        public override void BuildBoundingsLookup(BoundingsLookup lookup)
        {
            for (int i = 0, j = _groups.Count; i < j; i++)
            {
                _groups[i].BuildBoundingsLookup(lookup);
            }
        }
    }
}
