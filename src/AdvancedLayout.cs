/*
Copyright (c) 2010, 2011 Clifford Champion

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Html;
using System.Linq;
using System.Runtime.CompilerServices;
using jQueryApi;

namespace SharpUI
{
    public class Thickness
    {
        public double Bottom;
        public double Left;
        public double Top;
        public double Right;
    }

    public enum VerticalAlignment
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Stretch = 3,
    }

    public enum HorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
    }

    public static class AdvancedLayout
    {
        #region Layout
        public const string CssClassNameAdvancedLayout = "advancedLayout";
        private const int LayoutEnforcementInterval = 250;
        public const string AttributeNamePrefix = "al-";
        
        private static int _layoutEnforcementTimerId;
        private static jQueryObject _frameDetector;

        static AdvancedLayout()
        {
            InitLayoutEnforcement();
        }

        private static void InitLayoutEnforcement()
        {
            _frameDetector = jQuery.FromHtml("<span></span");
            _frameDetector.CSS(new Dictionary("position", "absolute", "height", "0px", "width","0px", "margin","0px", "padding","0px", "border", "none", "display", "block"));

            if (_layoutEnforcementTimerId == 0)
            {
                _layoutEnforcementTimerId = Window.SetInterval(OnLayoutEnforcement, LayoutEnforcementInterval);
            }
        }
        private static void OnLayoutEnforcement()
        {
            jQueryObject controlsInDocument = jQuery.Select("." + CssClassNameAdvancedLayout);

            for (int i = 0, m = controlsInDocument.Length; i < m; ++i)
            {
                UpdateLayout(jQuery.FromElement(controlsInDocument[i]));
            }
        }
        private static void UpdateLayout(jQueryObject element)
        {
            Measure(element);
            Arrange(element);
        }
        private static void Measure(jQueryObject element)
        {

        }
        private static void Arrange(jQueryObject element)
        {
#if DEBUG
            if (!element.Is("." + CssClassNameAdvancedLayout))
            {
                throw new Exception("Element not marked for advanced layout.");
            }
#endif

            jQueryObject offsetParent = element.OffsetParent();
            jQueryObject parent = element.Parent();

            if (offsetParent.Length == 0 || parent.Length == 0)
            {
                return;
            }

            bool parentIsOffsetParent = offsetParent[0] == parent[0];

            int parentInnerWidth = parent.GetInnerWidth();
            int parentInnerHeight = parent.GetInnerHeight();
            int elementWidth = element.GetInnerWidth();
            int elementHeight = element.GetInnerHeight();

            // gather parent padding
            int parentPaddingTop, parentPaddingRight, parentPaddingBottom, parentPaddingLeft;
            {
                parentPaddingTop = Number.Parse(parent.GetCSS("padding-top"));
                parentPaddingRight = Number.Parse(parent.GetCSS("padding-right"));
                parentPaddingBottom = Number.Parse(parent.GetCSS("padding-bottom"));
                parentPaddingLeft = Number.Parse(parent.GetCSS("padding-left"));
            }

            // detect parent's coordinates in offset-parent frame.
            parent.Prepend(_frameDetector);
            jQueryPosition parentPosition = _frameDetector.Position();
            _frameDetector.Remove();


            // gather advanced margins
            int marginTop, marginRight, marginBottom, marginLeft;
            {
                string margin = element.GetAttribute(AttributeNamePrefix + "margin");
                if (!string.IsNullOrEmpty(margin))
                {
                    string[] marginSplit = margin.Trim().Split(" ");
                    marginTop = int.Parse(marginSplit[0]);
                    marginRight = int.Parse(marginSplit[1]);
                    marginBottom = int.Parse(marginSplit[2]);
                    marginLeft = int.Parse(marginSplit[3]);
                }
                else
                {
                    marginTop = 0;
                    marginRight = 0;
                    marginBottom = 0;
                    marginLeft = 0;
                }
            }

            // gather advanced dimensions
            Number advancedWidth, advancedHeight;
            {
                advancedWidth = Number.Parse(element.GetAttribute(AttributeNamePrefix + "width"));
                advancedHeight = Number.Parse(element.GetAttribute(AttributeNamePrefix + "height"));
            }

            // gather advanced vertical alignment
            VerticalAlignment verticalAlignment;
            switch (element.GetAttribute(AttributeNamePrefix + "vertical-alignment"))
            {
                case "top":
                case "Top":
                    verticalAlignment = VerticalAlignment.Top;
                    break;
                case "center":
                case "Center":
                    verticalAlignment = VerticalAlignment.Center;
                    break;
                case "bottom":
                case "Bottom":
                    verticalAlignment = VerticalAlignment.Bottom;
                    break;
                default:
                case "stretch":
                case "Stretch":
                    verticalAlignment = VerticalAlignment.Stretch;
                    break;
            }

            // gather advanced horizontal alignment
            HorizontalAlignment horizontalAlignment;
            switch (element.GetAttribute(AttributeNamePrefix + "horizontal-alignment"))
            {
                case "left":
                case "Left":
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
                case "center":
                case "Center":
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;
                case "right":
                case "Right":
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;
                default:
                case "stretch":
                case "Stretch":
                    horizontalAlignment = HorizontalAlignment.Stretch;
                    break;
            }

            // override alignments
            if(verticalAlignment != VerticalAlignment.Stretch && Number.IsNaN(advancedHeight))
            {
                verticalAlignment = VerticalAlignment.Stretch;
            }
            if (horizontalAlignment != HorizontalAlignment.Stretch && Number.IsNaN(advancedWidth))
            {
                horizontalAlignment = HorizontalAlignment.Stretch;
            }


            // determine where
            int top = 0;
            int left = 0;
            int width = 0;
            int height = 0;
            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    top = Math.Round(marginTop + parentPosition.Top);
                    height = advancedHeight;
                    break;

                case VerticalAlignment.Bottom:
                    top = Math.Round(parentPosition.Top + parentInnerHeight - parentPaddingBottom - marginBottom - advancedHeight);
                    height = Math.Round(advancedHeight);
                    break;

                case VerticalAlignment.Stretch:
                    top = Math.Round(marginTop + parentPosition.Top);
                    height = Math.Round(parentInnerHeight - marginBottom - marginTop - parentPaddingTop - parentPaddingBottom);
                    break;

            }
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    left = Math.Round(marginLeft + parentPosition.Left);
                    width = Math.Round(advancedWidth);
                    break;

                case HorizontalAlignment.Right:
                    left = Math.Round(parentPosition.Left + parentInnerWidth - parentPaddingRight - marginRight - advancedWidth);
                    width = Math.Round(advancedWidth);
                    break;

                case HorizontalAlignment.Stretch:
                    left = Math.Round(marginLeft + parentPosition.Left);
                    width = Math.Round(parentInnerWidth - marginRight - marginLeft - parentPaddingLeft - parentPaddingRight);
                    break;
            }
            if (width <= 0)
            {
                width = 0;
            }
            if (height <= 0)
            {
                height = 0;
            }

            element.CSS("position", "absolute");
            element.CSS("top", top + "px");
            element.CSS("left", left + "px");
            element.CSS("width", width + "px");
            element.CSS("height", height + "px");

        }
        #endregion
    }
}
