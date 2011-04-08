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
        
        private static int _layoutEnforcementTimerId;

        private static void InitLayoutEnforcement()
        {
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

            jQueryPosition parentPosition = parent.Position();
            int parentWidth = parent.GetInnerWidth();
            int parentHeight = parent.GetInnerHeight();
            int elementWidth = element.GetInnerWidth();
            int elementHeight = element.GetInnerHeight();

            int marginTop = int.Parse(element.GetCSS("margin-top").Split("px")[0]);
            int marginRight = int.Parse(element.GetCSS("margin-right").Split("px")[0]);
            int marginBottom = int.Parse(element.GetCSS("margin-bottom").Split("px")[0]);
            int marginLeft = int.Parse(element.GetCSS("margin-left").Split("px")[0]);

            VerticalAlignment verticalAlignment;
            switch (element.GetAttribute("VerticalAlignment"))
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


            int top = 0;
            int left = 0;
            int width = 0;
            int height = 0;
            switch(verticalAlignment)
            {
                case VerticalAlignment.Stretch:
                    top = Math.Round(parentPosition.Top + marginTop);
                    height = Math.Round(parentHeight - marginBottom);
                    break;
                //case SharpUI.VerticalAlignment.Bottom:
                    
            }
            //switch (_horizontalAlignment)
            //{
            //    case HorizontalAlignment.Stretch:
            //        left = Math.Round(parentPosition.Left + marginLeft);
            //        width = Math.Round(parentWidth - marginRight);
            //        break;
            //}
                       

            element.CSS("top", top + "px");
            //element.CSS("left", left + "px");
            //element.CSS("width", width + "px");
            element.CSS("height", height + "px");
        }
        #endregion
    }
}
