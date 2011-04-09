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

    [IgnoreNamespace]
    [Imported]
    [ScriptName("Object")]
    internal class AdvancedLayoutState
    {
        public Thickness Padding;
        public Thickness Margin;
        public float Width;
        public float Height;
        public VerticalAlignment VerticalAlignment;
        public HorizontalAlignment HorizontalAlignment;
        //public float ParentLastWidth;
        //public float ParentLastHeight;
    }

    [IgnoreNamespace]
    [Imported]
    [ScriptName("Object")]
    internal class DimensionsAndPadding
    {
        public float ClientWidth;
        public float ClientHeight;
        public float PaddingTop;
        public float PaddingRight;
        public float PaddingBottom;
        public float PaddingLeft;
    }

    public static class AdvancedLayout
    {
        public const string CssClassNameAdvancedLayout = "advancedLayout";
        private const int LayoutEnforcementInterval = 500;
        public const string AttributeNamePrefix = "al:";

        private static int _layoutEnforcementTimerId;
        private static jQueryObject _frameDetector;

        static AdvancedLayout()
        {
            InitLayoutEnforcement();
        }

        private static void InitLayoutEnforcement()
        {
            _frameDetector = jQuery.FromHtml("<span></span");
            _frameDetector.CSS(new Dictionary("position", "absolute", "height", "0px", "width", "0px", "margin", "0px", "padding", "0px", "border", "none", "display", "block"));

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
            // does element have its al state parsed?
            AdvancedLayoutState elementState = (AdvancedLayoutState)element.GetDataValue("__als");
            if (elementState == null)
            {
                element.Data("__als", elementState = ParseAdvancedLayout(element));
            }

            // grab parents
            jQueryObject parent = element.Parent();
            jQueryObject offsetParent = element.OffsetParent();
            if (offsetParent.Length == 0 || parent.Length == 0)
            {
                return;
            }

            bool parentIsOffsetParent = offsetParent[0] == parent[0];
#if DEBUG
            if (!parentIsOffsetParent && element.Is(":visible"))
            {
                throw new Exception("Parent must use position:absolute|fixed|relative;.");
            }
#endif
            if (!parentIsOffsetParent)
            {
                return;
            }

            // gather parent padding and client dimensions
            DimensionsAndPadding parentDimensions = null;
            parentDimensions = GetDimensionsAndPadding(parent);

            // detect parent's coordinates in offset-parent frame.
            float contentStartInOffsetSpaceX, contentStartInOffsetSpaceY;
            {
                if (parentIsOffsetParent)
                { // parent is offset parent. we know our local frame.
                    contentStartInOffsetSpaceX = 0;
                    contentStartInOffsetSpaceY = 0;
                }
                else
                { // experimental support for staticly positioned parent.

                    parent.Prepend(_frameDetector);

                    jQueryPosition parentContentFrameInDocumentSpace = _frameDetector.GetOffset();
                    jQueryPosition offsetParentFrameInDocumentSpace = offsetParent.GetOffset();
                    if (parentContentFrameInDocumentSpace != null && offsetParentFrameInDocumentSpace != null)
                    {
                        contentStartInOffsetSpaceX = parentContentFrameInDocumentSpace.Left - offsetParentFrameInDocumentSpace.Left - parentDimensions.PaddingLeft;
                        contentStartInOffsetSpaceY = parentContentFrameInDocumentSpace.Top - offsetParentFrameInDocumentSpace.Top - parentDimensions.PaddingTop;
                    }
                    else
                    {
                        jQueryPosition contentStartInOffsetSpace = _frameDetector.Position();
                        if (contentStartInOffsetSpace != null)
                        {
                            contentStartInOffsetSpaceX = contentStartInOffsetSpace.Left - parentDimensions.PaddingLeft;
                            contentStartInOffsetSpaceY = contentStartInOffsetSpace.Top - parentDimensions.PaddingTop;
                        }
                        else
                        {
                            contentStartInOffsetSpaceX = 0;
                            contentStartInOffsetSpaceY = 0;
                        }
                    }

                    _frameDetector.Remove();
                }

            }

            double topBoundary = contentStartInOffsetSpaceY + parentDimensions.PaddingTop + elementState.Margin.Top;
            double bottomBoundary = contentStartInOffsetSpaceY + parentDimensions.ClientHeight - parentDimensions.PaddingBottom - elementState.Margin.Bottom;
            double leftBoundary = contentStartInOffsetSpaceX + parentDimensions.PaddingLeft + elementState.Margin.Left;
            double rightBoundary = contentStartInOffsetSpaceX + parentDimensions.ClientWidth - parentDimensions.PaddingRight - elementState.Margin.Right;


            // determine where to position
            int top = 0;
            int left = 0;
            int width = 0;
            int height = 0;
            switch (elementState.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    height = Math.Round(elementState.Height - elementState.Padding.Top - elementState.Padding.Bottom);
                    top = Math.Round(topBoundary);
                    break;

                case VerticalAlignment.Center:
                    height = Math.Round(elementState.Height - elementState.Padding.Top - elementState.Padding.Bottom);
                    top = Math.Round(topBoundary * 0.5 + bottomBoundary * 0.5 - height * 0.5);
                    break;

                case VerticalAlignment.Bottom:
                    height = Math.Round(elementState.Height - elementState.Padding.Top - elementState.Padding.Bottom);
                    top = Math.Round(contentStartInOffsetSpaceY + parentDimensions.ClientHeight - parentDimensions.PaddingBottom - elementState.Margin.Bottom - elementState.Height);
                    break;

                case VerticalAlignment.Stretch:
                    height = Math.Round(bottomBoundary - topBoundary - elementState.Padding.Top - elementState.Padding.Bottom);
                    top = Math.Round(topBoundary);
                    break;

            }
            switch (elementState.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    width = Math.Round(elementState.Width - elementState.Padding.Left - elementState.Padding.Right);
                    left = Math.Round(leftBoundary);
                    break;

                case HorizontalAlignment.Center:
                    width = Math.Round(elementState.Width - elementState.Padding.Left - elementState.Padding.Right);
                    left = Math.Round(leftBoundary * 0.5 + rightBoundary * 0.5 - width * 0.5);
                    break;

                case HorizontalAlignment.Right:
                    width = Math.Round(elementState.Width - elementState.Padding.Left - elementState.Padding.Right);
                    left = Math.Round(contentStartInOffsetSpaceX + parentDimensions.ClientWidth - parentDimensions.PaddingRight - elementState.Margin.Right - elementState.Width);
                    break;

                case HorizontalAlignment.Stretch:
                    width = Math.Round(rightBoundary - leftBoundary - elementState.Padding.Left - elementState.Padding.Right);
                    left = Math.Round(leftBoundary);
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

            element.CSS(
                new Dictionary(
                    "position", "absolute",
                    "top", top,
                    "left", left,
                    "width", width,
                    "height", height,
                    "padding-top", elementState.Padding.Top,
                    "padding-right", elementState.Padding.Right,
                    "padding-bottom", elementState.Padding.Bottom,
                    "padding-left", elementState.Padding.Left
                )
            );
        }
        private static DimensionsAndPadding GetDimensionsAndPadding(jQueryObject element)
        {
            DimensionsAndPadding d = new DimensionsAndPadding();

            if (Type.HasMethod(typeof(Window), "getComputedStyle"))
            {
                Object computedStyle = Type.InvokeMethod(typeof(Window), "getComputedStyle", element[0]);
                if (Type.HasField(computedStyle, "width"))
                {
                    d.ClientWidth = Number.Parse((string)Type.GetField(computedStyle, "width"));
                    d.ClientHeight = Number.Parse((string)Type.GetField(computedStyle, "height"));
                    d.PaddingTop = Number.Parse((string)Type.GetField(computedStyle, "paddingTop"));
                    d.PaddingRight = Number.Parse((string)Type.GetField(computedStyle, "paddingRight"));
                    d.PaddingBottom = Number.Parse((string)Type.GetField(computedStyle, "paddingBottom"));
                    d.PaddingLeft = Number.Parse((string)Type.GetField(computedStyle, "paddingLeft"));
                }
                else
                {
                    d.ClientWidth = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "width"));
                    d.ClientHeight = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "height"));
                    d.PaddingTop = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "padding-top"));
                    d.PaddingRight = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "padding-right"));
                    d.PaddingBottom = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "padding-bottom"));
                    d.PaddingLeft = Number.Parse((string)Type.InvokeMethod(computedStyle, "getPropertyValue", "padding-left"));
                }
                d.ClientWidth += d.PaddingLeft + d.PaddingRight;
                d.ClientHeight += d.PaddingTop + d.PaddingBottom;
                return d;
            }
            else
            {
                // gather parent padding
                d.ClientWidth = element.GetInnerWidth();
                d.ClientHeight = element.GetInnerHeight();
                d.PaddingTop = Number.Parse(element.GetCSS("padding-top"));
                d.PaddingRight = Number.Parse(element.GetCSS("padding-right"));
                d.PaddingBottom = Number.Parse(element.GetCSS("padding-bottom"));
                d.PaddingLeft = Number.Parse(element.GetCSS("padding-left"));
                return d;
            }
        }
        private static AdvancedLayoutState ParseAdvancedLayout(jQueryObject element)
        {
            // gather margins
            float marginTop, marginRight, marginBottom, marginLeft;
            {
                string margin = element.GetAttribute(AttributeNamePrefix + "margin");
                if (!string.IsNullOrEmpty(margin))
                {
                    string[] split = margin.Trim().Split(" ");
                    marginTop = float.Parse(split[0]);
                    marginRight = float.Parse(split[1]);
                    marginBottom = float.Parse(split[2]);
                    marginLeft = float.Parse(split[3]);
                }
                else
                {
                    marginTop = 0;
                    marginRight = 0;
                    marginBottom = 0;
                    marginLeft = 0;
                }
            }

            // gather padding
            float paddingTop, paddingRight, paddingBottom, paddingLeft;
            {
                string padding = element.GetAttribute(AttributeNamePrefix + "padding");
                if (!string.IsNullOrEmpty(padding))
                {
                    string[] split = padding.Trim().Split(" ");
                    paddingTop = float.Parse(split[0]);
                    paddingRight = float.Parse(split[1]);
                    paddingBottom = float.Parse(split[2]);
                    paddingLeft = float.Parse(split[3]);
                }
                else
                {
                    paddingTop = 0;
                    paddingRight = 0;
                    paddingBottom = 0;
                    paddingLeft = 0;
                }
            }

            // gather dimensions
            Number advancedWidth, advancedHeight;
            {
                advancedWidth = Number.Parse(element.GetAttribute(AttributeNamePrefix + "width"));
                advancedHeight = Number.Parse(element.GetAttribute(AttributeNamePrefix + "height"));
            }

            // gather vertical alignment
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

            // gather horizontal alignment
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
            if (verticalAlignment != VerticalAlignment.Stretch && Number.IsNaN(advancedHeight))
            {
                verticalAlignment = VerticalAlignment.Stretch;
            }
            if (horizontalAlignment != HorizontalAlignment.Stretch && Number.IsNaN(advancedWidth))
            {
                horizontalAlignment = HorizontalAlignment.Stretch;
            }

            AdvancedLayoutState state = new AdvancedLayoutState();
            state.Margin = new Thickness();
            state.Padding = new Thickness();

            state.Height = advancedHeight;
            state.Width = advancedWidth;
            state.VerticalAlignment = verticalAlignment;
            state.HorizontalAlignment = horizontalAlignment;
            state.Margin.Top = marginTop;
            state.Margin.Right = marginRight;
            state.Margin.Bottom = marginBottom;
            state.Margin.Left = marginLeft;
            state.Padding.Top = paddingTop;
            state.Padding.Right = paddingRight;
            state.Padding.Bottom = paddingBottom;
            state.Padding.Left = paddingLeft;

            return state;
        }
    }
}
