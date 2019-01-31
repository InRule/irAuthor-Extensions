using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Controls
{
    public class HighlightedTextBlock : Control
    {
        public class PatternMatchResult
        {
            private static readonly List<int> _emptyIntList = new List<int>();
            private readonly List<int> _matchStarts;
            public string Pattern { get; private set; }
            private string SourceText { get; set; }
            public bool HasMatches { get; private set; }


            public PatternMatchResult(string pattern, string text, int maxMatchCount)
            {
                Pattern = pattern ?? string.Empty;
                SourceText = text ?? string.Empty;

                if (Pattern.Length == 0) { return; }
                if (SourceText.Length == 0) { return; }

                int currPos = 0;
                do
                {
                    int matchPos = SourceText.IndexOf(Pattern, currPos, StringComparison.OrdinalIgnoreCase);

                    if (matchPos == -1)
                    {
                        break;
                    }

                    if (!HasMatches)
                    {
                        HasMatches = true;
                        _matchStarts = new List<int>();
                    }

                    _matchStarts.Add(matchPos);

                    if (_matchStarts.Count >= maxMatchCount)
                    {
                        break;
                    }

                    currPos = matchPos + Pattern.Length;

                    if (currPos >= SourceText.Length - 1)
                    {
                        break;
                    }
                } while (true);
            }

            public bool StartsWithAMatch { get { return HasMatches ? _matchStarts[0] == 0 : false; } }
            public bool MatchesWholeString { get { return HasMatches ? Pattern.Length == SourceText.Length : false; } }
            public string GetTextBefore(int matchIndex)
            {
                if (!HasMatches) return SourceText;
                int matchStart = _matchStarts[matchIndex];
                return SourceText.Substring(0, matchStart);
            }
            public string GetTextFromBeginningOfMatchOn(int matchIndex)
            {
                if (!HasMatches) return "";

                int textStart = _matchStarts[matchIndex];
                return SourceText.Substring(textStart, SourceText.Length - textStart);
            }
            public int GetMatchLength(int matchIndex)
            {
                if (!HasMatches) return 0;
                return Pattern.Length;
            }
            public string GetTextAfter(int matchIndex)
            {
                if (!HasMatches) return "";

                int textStart = _matchStarts[matchIndex] + Pattern.Length;
                return SourceText.Substring(textStart, SourceText.Length - textStart);
            }
            public List<int> MatchStarts { get { return HasMatches ? _matchStarts : _emptyIntList; } }
        }


        /// <summary>
        /// Attempts to get the usable bounds in the control, returns false if no render space found
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private bool TryGetTextRenderBounds(out Rect bounds)
        {
            var left = Margin.Left + Padding.Left;
            var top = Margin.Top + Padding.Top;
            var bottom = Margin.Bottom + Padding.Bottom;
            var right = Margin.Right + Padding.Right;
            var height = ActualHeight - bottom - top;
            var width = ActualWidth - right - left;
            if (height <= 0 || width <= 0)
            {
                bounds = new Rect(0, 0, 0, 0);
                return false;
            }

            bounds = new Rect(left, top, ActualWidth - right - left, ActualHeight);
            return true;
        }

        private Brush GetHighlightBrush()
        {
            Color highlight = HighlightColor;
            return new SolidColorBrush(Color.FromScRgb(.7f, highlight.ScR, highlight.ScG, highlight.ScB));
        }

        protected override void OnRender(DrawingContext dc)
        {
            RenderBackground(dc);

            var sourceText = Text;
            if (String.IsNullOrEmpty(sourceText)) return;

            Rect bounds;
            if (!TryGetTextRenderBounds(out bounds)) { return; }

            var matchingText = new PatternMatchResult(MatchPattern, sourceText, 100);
            var allText = FormatText(sourceText, bounds.Height);
            if (!matchingText.HasMatches || !HighlightMatches)
            {
                // If not matches or not highlighting matches, render normally
                allText.MaxTextWidth = bounds.Width;
                dc.DrawText(allText, bounds.TopLeft);
                return;
            }

            if (matchingText.MatchesWholeString)
            {
                allText.MaxTextWidth = bounds.Width;
                var geo = allText.BuildHighlightGeometry(bounds.TopLeft, 0, sourceText.Length);
                var highlightBrush = GetHighlightBrush();
                dc.DrawGeometry(highlightBrush, null, geo);
                dc.DrawText(allText, bounds.TopLeft);
                return;
            }

            if (!UseLeftTrimming || matchingText.StartsWithAMatch || allText.Width <= bounds.Width)
            {
                // No left elipsis needed
                RenderWithNoLeftElipsis(dc, matchingText, bounds, allText, 0);
                return;
            }

            RenderLeftRightTrimmedText(dc, bounds, matchingText, allText);
        }

        private void RenderLeftRightTrimmedText(DrawingContext dc, Rect bounds, PatternMatchResult matchingText, FormattedText allText)
        {
            var leadingElipsisText = FormatText("...", bounds.Height);
            var leadingElipsisGeo = leadingElipsisText.BuildHighlightGeometry(bounds.TopLeft);
            var leadingGeo = allText.BuildHighlightGeometry(bounds.TopLeft, 0, matchingText.MatchStarts[0]);
            var highlightGeo = allText.BuildHighlightGeometry(bounds.TopLeft, matchingText.MatchStarts[0], matchingText.Pattern.Length);

            // We know it does not fit and we know it does not start with a match
            if (leadingGeo == null
                || highlightGeo == null
                || leadingElipsisGeo == null
                || (leadingGeo.Bounds.Width < leadingElipsisGeo.Bounds.Width))
            {
                // the leading text is shorter than the elipsis text
                // Just write the leading text, the match, and then the trailtext w/elipsis
                // No left elipsis needed
                RenderWithNoLeftElipsis(dc, matchingText, bounds, allText, 0);
                return;
            }

            var availableWidth = bounds.Width;
            availableWidth -= leadingElipsisGeo.Bounds.Width * 2;
            availableWidth -= highlightGeo.Bounds.Width;

            var leadingTextSource = matchingText.GetTextBefore(0);
            var trailingTextSource = matchingText.GetTextAfter(0);

            var leadingText = "";
            var trailingText = "";

            var shrunkTrailing = FormatText(trailingText, bounds.Height);
            var shrunkLeading = FormatText(leadingText, bounds.Height);
            var initialLeadingSize = FormatText(leadingTextSource, bounds.Height).WidthIncludingTrailingWhitespace;
            var appendLettersPassCount = 0;
            var leadingPassPadding = (matchingText.Pattern.Length) / 2;

            while ((shrunkLeading.WidthIncludingTrailingWhitespace + shrunkTrailing.WidthIncludingTrailingWhitespace) < availableWidth)
            {
                // Add characters until the width is as wide as the column
                // This roughly centers the highlighted text between the leading and trailing parts
                if (appendLettersPassCount > leadingPassPadding)
                {
                    if (leadingText.Length < leadingTextSource.Length)
                    {
                        leadingText = leadingTextSource.Substring(leadingTextSource.Length - leadingText.Length - 1,
                                                                  leadingText.Length + 1);
                        shrunkLeading = FormatText(leadingText, bounds.Height);
                    }

                    if (shrunkLeading.WidthIncludingTrailingWhitespace + shrunkTrailing.WidthIncludingTrailingWhitespace >=
                        availableWidth)
                    {
                        break;
                    }
                }

                if (trailingText.Length < trailingTextSource.Length)
                {
                    trailingText = trailingTextSource.Substring(0, trailingText.Length + 1);
                    shrunkTrailing = FormatText(trailingText, bounds.Height);
                }

                if (leadingText == leadingTextSource && trailingText == trailingTextSource)
                {
                    // Somehow it managed to fit
                    break;
                }

                appendLettersPassCount++;
            }


            if (leadingText == leadingTextSource || initialLeadingSize - shrunkLeading.WidthIncludingTrailingWhitespace < leadingElipsisText.WidthIncludingTrailingWhitespace)
            {
                // The whole leading text is there, no elipsis required
                RenderWithNoLeftElipsis(dc, matchingText, bounds, allText, 0);
                return;
            }

            // Draw the elipsis
            dc.DrawText(leadingElipsisText, bounds.TopLeft);

            var shrunkText = leadingText + matchingText.GetTextFromBeginningOfMatchOn(0);
            // Draw the remainder as a new match
            //var remainingMatch = new PatternMatchResult(matchingText.Pattern, shrunkText, 100);

            if (bounds.Width - leadingElipsisText.Width > 0)
            {
                var remainingBounds = new Rect(bounds.Left + leadingElipsisText.Width, bounds.Top,
                                               bounds.Width - leadingElipsisText.Width, bounds.Height);

                int matchPosOffset = leadingTextSource.Length - leadingText.Length;
                RenderWithNoLeftElipsis(dc, matchingText, remainingBounds, FormatText(shrunkText, bounds.Height), matchPosOffset);
            }
        }

        private void RenderWithNoLeftElipsis(DrawingContext dc, PatternMatchResult matchingText, Rect bounds, FormattedText allText, int matchPosRightOffset)
        {
            var highlightBrush = GetHighlightBrush();

            allText.Trimming = TextTrimming.CharacterEllipsis;
            allText.MaxTextWidth = bounds.Width;
            foreach (var matchPos in matchingText.MatchStarts)
            {
                // The offset position is used when the formatted text does not start at the beginning of the string
                var offsetPos = matchPos - matchPosRightOffset;
                var length = matchingText.Pattern.Length;
                if (offsetPos < 0)
                {
                    length -= offsetPos;
                    offsetPos = 0;
                }

                if (length > 0)
                {
                    var geo = allText.BuildHighlightGeometry(bounds.TopLeft, offsetPos, length);
                    dc.DrawGeometry(highlightBrush, null, geo);

                    if (HighlightForegroundBrush != null)
                    {
                        allText.SetForegroundBrush(HighlightForegroundBrush, offsetPos, length);
                    }
                }
            }
            dc.DrawText(allText, bounds.TopLeft);
        }

        private void RenderBackground(DrawingContext dc)
        {
            var backHeight = ActualHeight - Margin.Top - Margin.Bottom;
            var backWidth = ActualWidth - Margin.Left - Margin.Right;
            if (backWidth > 0 && backHeight > 0)
            {
                dc.DrawRectangle(Background, null, new Rect(Margin.Left, Margin.Top, backWidth, backHeight));
            }
        }

        private FormattedText FormatText(string text, double maxHeight)
        {
            var txtOut = FormatText(text);
            txtOut.MaxTextHeight = maxHeight;
            return txtOut;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            try
            {
                var hpadSpace = Padding.Left + Padding.Right + Margin.Left + Margin.Right;
                FormattedText txtOut = FormatText(Text);
                var measuredWidth = txtOut.WidthIncludingTrailingWhitespace + hpadSpace + 1;
                var measuredHeight = txtOut.Height + Padding.Top + Padding.Bottom + Margin.Top + Margin.Bottom;
                var width = Math.Min(constraint.Width, measuredWidth);
                var height = Math.Min(constraint.Height, measuredHeight);
                return new Size(width, height);
            }
            catch (Exception)
            {
                return new Size(0, 0);
            }

        }

        private FormattedText FormatText(string text)
        {
            if (text == null) text = "";
            var txtOut = new FormattedText(text, CultureInfo.CurrentCulture
                                           , FlowDirection.LeftToRight
                                           , Typeface
                                           , FontSize
                                           , Foreground);
            txtOut.Trimming = TextTrimming;
            txtOut.TextAlignment = TextAlignment;
            txtOut.MaxLineCount = 1;

            return txtOut;
        }

        private Typeface Typeface { get { return new Typeface(FontFamily, FontStyle, FontWeight, FontStretch); } }

        #region DependencyProperties

        private static Type thisType = typeof(HighlightedTextBlock);

        #region Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            thisType,
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }
        #endregion

        #region TextAlignment
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment",
            typeof(TextAlignment),
            thisType,
            new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public TextAlignment TextAlignment { get { return (TextAlignment)GetValue(TextAlignmentProperty); } set { SetValue(TextAlignmentProperty, value); } }
        #endregion

        #region TextTrimming
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming",
            typeof(TextTrimming),
            thisType,
            new FrameworkPropertyMetadata(TextTrimming.CharacterEllipsis, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public TextTrimming TextTrimming { get { return (TextTrimming)GetValue(TextTrimmingProperty); } set { SetValue(TextTrimmingProperty, value); } }
        #endregion

        #region HighlightColor
        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register(
            "HighlightColor",
            typeof(Color),
            thisType,
            new FrameworkPropertyMetadata(Colors.Yellow, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public Color HighlightColor { get { return (Color)GetValue(HighlightColorProperty); } set { SetValue(HighlightColorProperty, value); } }
        #endregion

        #region HighlightForegroundBrush
        public static readonly DependencyProperty HighlightForegroundBrushProperty = DependencyProperty.Register(
            "HighlightForegroundBrush",
            typeof(Brush),
            thisType,
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public Brush HighlightForegroundBrush { get { return (Brush)GetValue(HighlightForegroundBrushProperty); } set { SetValue(HighlightForegroundBrushProperty, value); } }
        #endregion

        #region MatchPattern
        public static readonly DependencyProperty MatchPatternProperty = DependencyProperty.Register(
            "MatchPattern",
            typeof(string),
            thisType,
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public string MatchPattern
        {
            get { return (string)GetValue(MatchPatternProperty); }
            set { SetValue(MatchPatternProperty, value); }
        }
        #endregion

        #region UseLeftTrimming
        public static readonly DependencyProperty UseLeftTrimmingProperty = DependencyProperty.Register(
            "UseLeftTrimming",
            typeof(bool),
            thisType,
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public bool UseLeftTrimming
        {
            get { return (bool)GetValue(UseLeftTrimmingProperty); }
            set { SetValue(UseLeftTrimmingProperty, value); }
        }
        #endregion

        #region HighlightMatches
        public static readonly DependencyProperty HighlightMatchesProperty = DependencyProperty.Register(
            "HighlightMatches",
            typeof(bool),
            thisType,
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public bool HighlightMatches
        {
            get { return (bool)GetValue(HighlightMatchesProperty); }
            set { SetValue(HighlightMatchesProperty, value); }
        }
        #endregion
        #endregion DependencyProperties
    }
}
