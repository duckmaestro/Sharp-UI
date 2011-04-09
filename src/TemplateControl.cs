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
    public enum Position
    {
        Unspecified = 0,
        Absolute = 1,
        Relative = 2,
        Fixed = 3,
    }

    public enum MouseCaptureState { Begin = 0, Move = 1, End = 2 }
    public delegate void MouseCaptureHandler(MouseCaptureState state, jQueryPosition positionInDocument);

    public abstract class TemplateControl
    {
        #region Constants
        private const string AttributeNameLocalId = "xid";
        private const string AttributeNameControlClass = "control";
        private const string DataNameControl = "templateControl";
        public const string CssClassNameControl = "templateControl";
        public const string CssClassNameControlUnadded = "templateControlUnadded";
        private const string IdPrefixAutoRewrite = "auto_";
        private const string CssClassNamePrefixAutoRewrite = "css_auto_";
        #endregion

        private jQueryObject _jqRootElement;
        public jQueryObject RootElement { get { return (jQueryObject)_jqRootElement; } }

        #region Construction
        private string _strInstanceId;
        private static bool _bStaticConstructionFinished = false;
        /// <remarks>
        /// Proper static construct declaration conflicting with extern instance constructor.
        /// This is workaround.
        /// </remarks>
        private static void StaticConstructor()
        {
#if DEBUG
            if (_bStaticConstructionFinished)
            {
                throw new Exception("Static construction already finished.");
            }
#endif
            InitDocumentDetection();
            InitDocumentMouseTracking();
            _bStaticConstructionFinished = true;
        }
        [AlternateSignature]
        public extern TemplateControl();
        [AlternateSignature]
        public extern TemplateControl(string strTemplate);
        public TemplateControl(object oTemplate)
        {
            // init static stuff
            if (!_bStaticConstructionFinished)
            {
                StaticConstructor();
            }

            // continue instance setup..
            this._strInstanceId = GenerateNewInstanceId();

            string strTemplate;

            // grab template
            if (Script.IsNullOrUndefined(oTemplate))
            {
                strTemplate = FindTemplate(this);
#if DEBUG
                if (string.IsNullOrEmpty(strTemplate))
                {
                    throw new Exception(this.GetType().FullName + " is missing a Template member and no template was provided.");
                }
#endif
            }
            else
            {
                if (oTemplate is string)
                {
                    strTemplate = (string)oTemplate;
                }
                else
                {
                    jQueryObject jqTemplate = jQuery.FromObject(oTemplate);
                    strTemplate = "<" + jqTemplate[0].TagName + ">" + jqTemplate.GetHtml() + "</" + jqTemplate[0].TagName + ">";
                }
            }

            _hash_oNamedChildControls = new Dictionary();
            _hash_oNamedChildElements = new Dictionary();

            jQueryObject jqHead = jQuery.Select("head");

            // grab template
            if (Script.IsNullOrUndefined(strTemplate))
            {
                strTemplate = (string)Type.GetField(this, "template");
            }
#if DEBUG
            if (string.IsNullOrEmpty(strTemplate))
            {
                throw new Exception(this.GetType().FullName + " is missing a Template member.");
            }
#endif
            // generate an absolute id for this control
            string strNewId = GenerateNewAutoId();

#if DEBUG
            // is there a conflict in the auto gen'd id?
            {
                int iNumOtherControlsWithId = jQuery.Select("#" + strNewId).Length;
                if (iNumOtherControlsWithId != 0)
                {
                    throw new Exception("Auto generated id conflict.");
                }
            }
#endif

            // parse template
            jQueryObject jqContent = jQuery.FromHtml(strTemplate.Replace("`", "\""));

            // are there style tags?
            string strStyleRules = string.Empty;
            jqContent.Filter("style").Each(delegate(int i, Element e)
            {
                jQueryObject jqElement = jQuery.FromElement(e);
                strStyleRules += jqElement.GetHtml();
                jqElement.Remove();
            });

            // proceed with non-style tags
            jqContent = jqContent.Not("style").Remove();

            // remove attribute id if any
#if DEBUG
            if (!string.IsNullOrEmpty(jqContent.GetAttribute("id")))
            {
                throw new Exception("Global ID's not permitted. Element with ID \"" + jqContent.GetAttribute("id") + "\" found.");
            }
#endif
            jqContent.RemoveAttr("id");

            // store reference
            _jqRootElement = jqContent;

            // store reference from element back to this control
            _jqRootElement.Data(DataNameControl, this);

            // identify locally-named elements. this runs before parsing children because we don't want to add children's locally-named html elements.
            {
                jqContent.Find("*[" + AttributeNameLocalId + "]").Each(
                    delegate(int i, Element e)
                    {
                        jQueryObject jqElement = jQuery.FromElement(e);
                        if (!string.IsNullOrEmpty(jqElement.GetAttribute(AttributeNameControlClass)))
                        {
                            return;
                        }

                        string strLocalId = GetLocalId(jqElement);
                        if (strLocalId == null)
                        {
                            return;
                        }

                        // store record of it
                        this._hash_oNamedChildElements[strLocalId] = jqElement;

                        // remove absolute id if any
#if DEBUG
                        if (!string.IsNullOrEmpty(jqElement.GetAttribute("id")))
                        {
                            throw new Exception("Global ID's not permitted. Element with ID \"" + jqElement.GetAttribute("id") + "\" found.");
                        }
#endif
                        jqElement.RemoveAttr("id");
                    })
                ;
            }

            // calculcate search namespace
            Type currentType = this.GetType();
            string currentTopLevelNamespace
                = currentType
                .FullName
                .Substr(0, currentType.FullName.IndexOf('.'))
            ;

            // add class for identifying this as a template control
            _jqRootElement.AddClass(CssClassNameControl);

            // add class for identifying this as a newly added template control
            _jqRootElement.AddClass(CssClassNameControlUnadded);

            // recurse into child controls
            jqContent.Find("div[" + AttributeNameControlClass + "]").Each(
                delegate(int index, Element element)
                {
                    jQueryObject jqElement = jQuery.FromElement(element);
                    string strChildTypeName = jqElement.GetAttribute(AttributeNameControlClass);

                    string strChildTypeNameResolved
                        = ResolveTypeName(
                            strChildTypeName,
                            currentTopLevelNamespace
                        )
                    ;
                    Type oChildType = Type.GetType(strChildTypeNameResolved);

                    if (Script.IsNullOrUndefined(oChildType))
                    {
#if DEBUG
                        throw new Exception("Could not locate type \"" + (strChildTypeNameResolved ?? strChildTypeName) + "\"");
#else
                        return;
#endif
                    }

                    TemplateControl childControl = Type.CreateInstance(oChildType, null) as TemplateControl;

                    // grab local id if any
                    string strLocalId = GetLocalId(jqElement) ?? GenerateNewAutoId();

                    // store named control
                    if (strLocalId != null)
                    {
                        this._hash_oNamedChildControls[strLocalId] = childControl;
                    }

                    // merge style and class attributes
                    string strClass = jqElement.GetAttribute("class");
                    string strStyle = jqElement.GetAttribute("style");
                    if (!string.IsNullOrEmpty(strClass))
                    {
                        string strClassFromTemplate = childControl.RootElement.GetAttribute("class") ?? string.Empty;
                        childControl.RootElement.Attribute("class", strClassFromTemplate + " " + strClass);
                    }
                    if (!string.IsNullOrEmpty(strStyle))
                    {
                        string strStyleFromTemplate = childControl.RootElement.GetAttribute("style") ?? string.Empty;
                        childControl.RootElement.Attribute("style", strStyleFromTemplate + " " + strStyle);
                    }

                    // preserve other attributes
                    for (int i = 0, m = jqElement[0].Attributes.Length; i < m; ++i)
                    {
                        ElementAttribute a = (ElementAttribute)Type.GetField(jqElement[0].Attributes, (string)(object)i);
                        if (jQuery.Browser.Version == "7.0" && jQuery.Browser.MSIE && !a.Specified)
                        {
                            continue;
                        }
                        string attributeName = a.Name.ToLowerCase();
                        switch (attributeName)
                        {
                            case "id":
                            case "xid":
                            case "class":
                            case "style":
                            case "control":
                                break;
                            default:
                                childControl.RootElement.Attribute(a.Name, a.Value);
                                break;
                        }
                    }

                    // replace the placeholder element with the new control.
                    jqElement.RemoveAttr("id").After(childControl.RootElement).Remove();

                    // preserve local id & control type name
                    if (strLocalId != null)
                    {
                        childControl.RootElement.Attribute("xid", strLocalId);
                    }
                    childControl.RootElement.Attribute("control", jqElement.GetAttribute(AttributeNameControlClass));

                    // any children content?
                    jQueryObject jqChildContent = jqElement.Find(">*");
                    if (jqChildContent.Length > 0)
                    {
                        childControl.ProcessChildContent(jqChildContent);
                    }
                }
            );

            // rewrite radio input groups
            {
                jQueryObject jqRadioInputs = _jqRootElement.Find("input[type=radio]");
                Dictionary hash_rewrittenGroupNames = new Dictionary();
                jqRadioInputs.Each(delegate(int index, Element element)
                {
                    jQueryObject jqRadio = jQuery.FromElement(element);

                    // rewrite name attribute
                    {
                        string strGroupName = jqRadio.GetAttribute("name");
                        if (string.IsNullOrEmpty(strGroupName))
                        {
                            return;
                        }
                        // need a new name?
                        string strNewGroupName;
                        if (hash_rewrittenGroupNames.ContainsKey(strGroupName))
                        {
                            strNewGroupName = (string)hash_rewrittenGroupNames[strGroupName];
                        }
                        else
                        {
                            hash_rewrittenGroupNames[strGroupName] = strNewGroupName = GenerateNewAutoId();
                        }
                        jqRadio.Attribute("name", strNewGroupName);
                    }

                    // make sure the element has an id, for label elements to use
                    if (string.IsNullOrEmpty(jqRadio.GetAttribute("id")))
                    {
                        jqRadio.Attribute("id", GenerateNewAutoId());
                    }
                });
                this._hash_rewrittenGroupNames = hash_rewrittenGroupNames;
            }

            // rewrite label elements
            {
                jQueryObject jqLabels = _jqRootElement.Find("label[for]");
                jqLabels.Each(delegate(int index, Element element)
                {
                    jQueryObject jqLabelElement = jQuery.FromElement(element);
                    string strForId = jqLabelElement.GetAttribute("for");
                    // is this element rewritten?
                    jQueryObject jqTargetElement = TryGetElement(strForId);
                    if (jqTargetElement == null)
                    {
                        return;
                    }
                    string strTargetElementNewId = jqTargetElement.GetAttribute("id");
                    // make sure the "for" element has an id
                    if (string.IsNullOrEmpty(strTargetElementNewId))
                    {
                        jqTargetElement.Attribute("id", strTargetElementNewId = GenerateNewAutoId());
                    }
                    jqLabelElement.Attribute("for", strTargetElementNewId);

                    return;
                });
            }

            // fixup css rules & add to head
            if (strStyleRules.Length != 0)
            {
                ProcessCss(this, strStyleRules);
            }

            // auto fill members that point to elements/controls
            AutoFillMemberFields();
        }
        /// <summary>
        /// If this control instance had content placed inside its declaration, 
        /// this method is called with said content.
        /// </summary>
        /// <param name="jqChildContent"></param>
        protected virtual void ProcessChildContent(jQueryObject jqChildContent)
        {

        }
        /// <summary>
        /// Cache of html templates for each known type.
        /// </summary>
        private static Dictionary/*<typename, templatestring>*/ _hash_templateCache = new Dictionary();
        /// <summary>
        /// For the given template control, retrieve its html template.
        /// </summary>
        /// <param name="templateControl"></param>
        /// <returns></returns>
        private static string FindTemplate(TemplateControl templateControl)
        {
            string templateTypeName = templateControl.GetType().FullName;
            if (!_hash_templateCache.ContainsKey(templateTypeName))
            {
                string strTemplate = (string)Type.GetField(templateControl, "template");
                if (!string.IsNullOrEmpty(strTemplate))
                {
                    _hash_templateCache[templateTypeName] = strTemplate;
                }
                else
                {
                    // search for a private/obfuscated field
                    foreach (DictionaryEntry kvp in Dictionary.GetDictionary(templateControl))
                    {
                        if (!(kvp.Value is string))
                        {
                            continue;
                        }
                        if (!kvp.Key.StartsWith("$"))
                        {
                            continue;
                        }
                        string strTmp = ((string)kvp.Value).Trim(); // todo: avoid a trim.
                        if (strTmp.StartsWith("<"))
                        {
                            _hash_templateCache[templateTypeName] = kvp.Value;
                        }
                    }
                }
            }

            return (string)(_hash_templateCache[templateTypeName] ?? null);
        }
        /// <summary>
        /// Helper method. Retrieves the TemplateControl instance from its jQuery root element.
        /// </summary>
        /// <returns></returns>
        internal static TemplateControl FromRootElement(object elem)
        {
#if DEBUG
            if (elem == null)
            {
                throw new Exception("Element null.");
            }
#endif
            jQueryObject jqElem = jQuery.FromElement((Element)elem);
            TemplateControl tc = jqElem.GetDataValue(DataNameControl) as TemplateControl;
            if (tc == null)
            {
                throw new Exception("Provided element is not the root of a Template Control.");
            }
            return tc;
        }
        #endregion

        #region CSS Rewriting
        private static Dictionary/*<typename,Dictionary<xid,cssClass>>*/ _hash_processedCss = new Dictionary();
        private static void ProcessCss(TemplateControl rootControl, string strRawCss)
        {
            // has this template been processed before?
            string strControlType = rootControl.GetType().FullName;
            bool bIsNewStyleSet = false;
            Dictionary hash_xidsToCssClasses = (Dictionary)_hash_processedCss[strControlType] ?? null;
            if (hash_xidsToCssClasses == null)
            {
                hash_xidsToCssClasses = new Dictionary();
                bIsNewStyleSet = true;
            }

            //////
            // rewrite css rules if this is the first time loading this control.
            if (bIsNewStyleSet)
            {
                string strProcessedCss = strRawCss.ReplaceRegex(
                    new RegularExpression(@"#[a-zA-Z]\w*", "g"),
                    delegate(string s)
                    {
                        string sSub = s.Substr(1);

                        // if #this
                        if (sSub == "this")
                        {
                            hash_xidsToCssClasses[sSub] = (string)hash_xidsToCssClasses[sSub] ?? GenerateNewAutoCssClass();
#if DEBUG
                            return "." + (string)hash_xidsToCssClasses[sSub] + "/* " + s + " */";
#else
                            return "." + (string)hash_xidsToCssClasses[sSub];
#endif
                        }

                        // if #someLocalId (element)
                        jQueryObject jqElement = (jQueryObject)rootControl._hash_oNamedChildElements[sSub] ?? null;
                        if (jqElement != null)
                        {
                            hash_xidsToCssClasses[sSub] = (string)hash_xidsToCssClasses[sSub] ?? GenerateNewAutoCssClass();
#if DEBUG
                            return "." + (string)hash_xidsToCssClasses[sSub] + "/* " + s + " */";
#else
                            return "." + (string)hash_xidsToCssClasses[sSub];
#endif
                        }

                        // if #someLocalId (control)
                        TemplateControl oControl = (TemplateControl)rootControl._hash_oNamedChildControls[sSub] ?? null;
                        if (oControl != null)
                        {
                            hash_xidsToCssClasses[sSub] = (string)hash_xidsToCssClasses[sSub] ?? GenerateNewAutoCssClass();
#if DEBUG
                            return "." + (string)hash_xidsToCssClasses[sSub] + "/* " + s + " */";
#else
                            return "." + (string)hash_xidsToCssClasses[sSub];
#endif
                        }

                        // unknown. keep.
                        return s;
                    }
                );

                // add style head
                jQueryObject jqStyle;
                if (jQuery.Browser.MSIE)
                {
                    // setting inner html does not work on IE8. so we do a string concat here instead.
                    jqStyle = jQuery.FromHtml(@"<style type=""text/css"">" + strProcessedCss + @"</style>");
                }
                else
                {
                    jqStyle = jQuery.FromHtml(@"<style type=""text/css""></style>");
                    jqStyle.Html(strProcessedCss);
                }

                jQuery.Select("head").Append(jqStyle);

                // save rewrite rules
                _hash_processedCss[strControlType] = hash_xidsToCssClasses;
            }

            //////
            // apply classes to named elements
            foreach (DictionaryEntry kvp in hash_xidsToCssClasses)
            {
                string key = kvp.Key;
                if (key == "this")
                {
                    rootControl._jqRootElement.AddClass((string)kvp.Value);
                    continue;
                }

                // is it an element?
                jQueryObject jqElement = (jQueryObject)rootControl._hash_oNamedChildElements[key] ?? null;
                if (jqElement != null)
                {
                    jqElement.AddClass((string)kvp.Value);
                    continue;
                }

                // is it a control?
                TemplateControl oControl = (TemplateControl)rootControl._hash_oNamedChildControls[key] ?? null;
                if (oControl != null)
                {
                    oControl.RootElement.AddClass((string)kvp.Value);
                    continue;
                }

#if DEBUG
                throw new Exception("CSS rule found for no corresponding element/control.");
#endif
            }
        }
        #endregion

        #region Document Detection
        private const int DocumentTreeCheckInterval = 200;

        protected event EventHandler AddedToDocument;
        protected event EventHandler RemovedFromDocument;
        protected event EventHandler Presented;
        private bool _bPresented;
        /// <remarks>
        /// according to (url) issued id's must be greater than 0. 
        /// url: http://www.whatwg.org/specs/web-apps/current-work/multipage/timers.html#timers
        /// </remarks>
        private static int _iCheckParentIntervalId = 0;
        private static Dictionary/*<*/ _hash_strControlIdsKnownInDocument = new Dictionary();

        private static void InitDocumentDetection()
        {
            if (_iCheckParentIntervalId == 0)
            {
                _iCheckParentIntervalId = Window.SetInterval(OnIntervalCheckParent, DocumentTreeCheckInterval);
            }
        }
        private static void OnIntervalCheckParent()
        {
            ArrayList arr_controlsToNotifyAdded = new ArrayList();

            // search for missing controls or newly presented controls
            {
                foreach (DictionaryEntry kvp in _hash_strControlIdsKnownInDocument)
                {
                    string strInstanceId = kvp.Key;
                    TemplateControl control = (TemplateControl)kvp.Value;

                    //if (!hash_strControlsFound.ContainsKey(strInstanceId))
                    if (!control.IsInDocument)
                    {
                        // remove from known controls
                        _hash_strControlIdsKnownInDocument.Remove(strInstanceId);

                        // notify removed.
                        control.NotifyRemovedFromDocument();

                        // mark as unadded
                        control.RootElement.AddClass(CssClassNameControlUnadded);
                    }

                    // newly presented?
                    if (!control._bPresented && control.Presented != null)
                    {
                        if (control.RootElement.Is(":visible"))
                        {
                            control.NotifyPresented();
                            control._bPresented = true;
                        }
                    }
                }
            }

            // search for new controls
            Dictionary hash_strControlsFound = new Dictionary();
            jQueryObject newControls = jQuery.Select("." + CssClassNameControlUnadded);
            newControls.Each(delegate(int i, Element e)
            {
                jQueryObject rootElement = jQuery.FromElement(e);
                TemplateControl control = (TemplateControl)rootElement.GetDataValue(DataNameControl);

                if (control == null)
                {
#if DEBUG
                    throw new Exception("Control root element missing Control data. Did you use jQuery empty() or remove() by mistake?");
#else
                    return;
#endif
                }

                string strInstanceId = control._strInstanceId;
#if DEBUG
                if (string.IsNullOrEmpty(strInstanceId.Trim()))
                {
                    throw new Exception("Found control with empty instance id.");
                }
#endif
                hash_strControlsFound[strInstanceId] = null;
                if (!_hash_strControlIdsKnownInDocument.ContainsKey(strInstanceId))
                {
                    // add to known controls
                    _hash_strControlIdsKnownInDocument[strInstanceId] = control;

                    // notify it was added
                    arr_controlsToNotifyAdded.Add(control);

                    // mark as added
                    rootElement.RemoveClass(CssClassNameControlUnadded);
                }
            });

            // notify controls that were added.
            for (int i = arr_controlsToNotifyAdded.Count - 1; i >= 0; --i)
            {
                TemplateControl controlToNotify = (TemplateControl)arr_controlsToNotifyAdded[i];
                controlToNotify.NotifyAddedToDocument();
            }
            arr_controlsToNotifyAdded.Clear();
        }
        private void NotifyAddedToDocument()
        {
            if (AddedToDocument != null)
            {
                AddedToDocument(this, null);
            }
        }
        private void NotifyRemovedFromDocument()
        {
            if (RemovedFromDocument != null)
            {
                RemovedFromDocument(this, null);
            }
        }
        private void NotifyPresented()
        {
            if (Presented != null)
            {
                Presented(this, null);
            }
        }
        public jQueryObject DocumentBody
        {
            get
            {
                if (IsInDocument)
                {
                    return jQuery.FromObject(Window.Document.Body);
                }
                else
                {
                    return null;
                }
            }
        }
        private bool IsInDocument
        {
            get
            {
                Element e = this.RootElement[0];
                while (true)
                {
                    e = e.ParentNode;
                    if (e == null)
                    {
                        return false;
                    }
                    if (e.NodeType == ElementType.Document)
                    {
                        return true;
                    }
                }
            }
        }
        #endregion

        #region Naming & Type Helpers
        private static Dictionary _hash_cachedTypeNameResolves = new Dictionary();
        private static string ResolveTypeName(string strShortName, string startingNamespace)
        {
#if DEBUG
            if (Script.IsNullOrUndefined(startingNamespace))
            {
                throw new Exception("Missing starting namespace.");
            }
#endif

            if (_hash_cachedTypeNameResolves.ContainsKey(strShortName))
            {
                return (string)_hash_cachedTypeNameResolves[strShortName];
            }

            // search using short name as is
            string strResolvedTypeName = startingNamespace + "." + strShortName;
            if (Type.GetType(strResolvedTypeName) != null)
            {
                return (string)(_hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
            }


            strResolvedTypeName = startingNamespace + "." + "_" + strShortName.Substr(0, 1).ToLowerCase() + strShortName.Substr(1);
            if (Type.GetType(strResolvedTypeName) != null)
            {
                return (string)(_hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
            }

            // search sub namespaces
            Dictionary d = Dictionary.GetDictionary(Type.GetType(startingNamespace));
            foreach (DictionaryEntry kvp in d)
            {
                if (!Type.IsNamespace(kvp.Value))
                {
                    continue;
                }
                string namespaceName = (string)Type.InvokeMethod(kvp.Value, "getName");
                strResolvedTypeName = ResolveTypeName(strShortName, namespaceName);

                if (strResolvedTypeName != null)
                {
                    return (string)(_hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
                }
            }
            return null;
        }

        private static int _iAutoIdGeneratorCounter = 0;
        private static string GenerateNewAutoId()
        {
            return IdPrefixAutoRewrite + _iAutoIdGeneratorCounter++;
        }
        private static string GenerateNewAutoCssClass()
        {
            return CssClassNamePrefixAutoRewrite + _iAutoIdGeneratorCounter++;
        }
        private static string GetLocalId(jQueryObject jqElement)
        {
            string strLocalId = jqElement.GetAttribute(AttributeNameLocalId);
            if (string.IsNullOrEmpty(strLocalId))
            {
                return null;
            }
            strLocalId = strLocalId.Trim();
            if (strLocalId.Length == 0)
            {
                return null;
            }

            return strLocalId;
        }
        private static int _iInstanceIdGeneratorCounter = 1;
        private static string GenerateNewInstanceId()
        {
            return (_iInstanceIdGeneratorCounter++).ToString();
        }
        #endregion

        #region Named Elements/Controls
        protected Dictionary _hash_oNamedChildElements;
        protected jQueryObject GetElement(string strId)
        {
            jQueryObject o = (jQueryObject)this._hash_oNamedChildElements[strId];
            if (Script.IsNullOrUndefined(o))
            {
                throw new Exception("Element by id \"" + strId + "\" not found.");
            }
            return o;
        }
        protected jQueryObject TryGetElement(string strId)
        {
            jQueryObject o = (jQueryObject)this._hash_oNamedChildElements[strId];
            return o ?? null;
        }

        protected Dictionary _hash_oNamedChildControls;
        protected TemplateControl GetControl(string strId)
        {
            TemplateControl o = (TemplateControl)this._hash_oNamedChildControls[strId];
            if (Script.IsNullOrUndefined(o))
            {
                throw new Exception("Control by id \"" + strId + "\" not found.");
            }
            return o;
        }
        private Dictionary _hash_rewrittenGroupNames;
        protected jQueryObject GetGroup(string formFieldGroupname)
        {
            string rewrittenName = (string)_hash_rewrittenGroupNames[formFieldGroupname];
            if (Script.IsNullOrUndefined(rewrittenName))
            {
                throw new Exception("Group by name \"" + formFieldGroupname + "\" not found.");
            }
            return (jQueryObject)this.RootElement.Find("*[name=" + rewrittenName + "]");
        }

        /// <summary>
        /// Each entry KVP is a type=>Dictionary. each inner dictionary is a xid(string)=>field(string).
        /// </summary>
        private static Dictionary _hash_controlFieldMappingByControl = new Dictionary();
        private void AutoFillMemberFields()
        {
            // is this a new type?
            string typeNameThis = this.GetType().FullName;
            if (!_hash_controlFieldMappingByControl.ContainsKey(typeNameThis))
            {
                // generate the mapping
                Dictionary newMapping = new Dictionary();
                Dictionary thisAsDictionary = Dictionary.GetDictionary(this);

                // for each named child element
                foreach (DictionaryEntry kvpElement in this._hash_oNamedChildElements)
                {
                    // for each variation in naming
                    int loopCount = 0;
                    string strFieldNameTemp = null;
                    while (loopCount >= 0)
                    {
                        switch (loopCount)
                        {
                            case 0:
                                strFieldNameTemp = "_" + kvpElement.Key;
                                break;
                            case 1:
                                strFieldNameTemp = "_jq" + kvpElement.Key;
                                break;
                            case 2:
                                if (kvpElement.Key.Length <= 1)
                                {
                                    loopCount = -10;
                                    break;
                                }
                                strFieldNameTemp = "_" + kvpElement.Key.Substr(0, 1).ToLowerCase() + kvpElement.Key.Substr(1);
                                break;
                            case 3:
                                strFieldNameTemp = "_jq" + kvpElement.Key.Substr(0, 1).ToUpperCase() + kvpElement.Key.Substr(1);
                                break;
                            default:
                                loopCount = -10;
                                break;
                        }

                        // for each field in this class.  TODO: this can be optimized with a custom lookup.
                        foreach (DictionaryEntry kvpField in thisAsDictionary)
                        {
                            if (kvpField.Key.StartsWith(strFieldNameTemp) && Math.Abs(kvpField.Key.Length - strFieldNameTemp.Length) <= 2)
                            {
                                // only store the mapping if the found field is null (making it likely that it's a subclass field pointing to a control or element).
                                if (kvpField.Value == null)
                                {
                                    // save mapping
                                    newMapping[kvpElement.Key] = kvpField.Key;
                                    loopCount = -10;
                                }
                            }
                        }

                        ++loopCount;
                    }
                }
                // for each named child control
                foreach (DictionaryEntry kvpControl in this._hash_oNamedChildControls)
                {
                    // for each variation in naming
                    int loopCount = 0;
                    string strFieldNameTemp = null;
                    while (loopCount >= 0)
                    {
                        switch (loopCount)
                        {
                            case 0:
                                strFieldNameTemp = "_" + kvpControl.Key;
                                break;
                            case 1:
                                strFieldNameTemp = "_o" + kvpControl.Key;
                                break;
                            case 2:
                                if (kvpControl.Key.Length <= 1)
                                {
                                    loopCount = -10;
                                    break;
                                }
                                strFieldNameTemp = "_" + kvpControl.Key.Substr(0, 1).ToLowerCase() + kvpControl.Key.Substr(1);
                                break;
                            case 3:
                                strFieldNameTemp = "_o" + kvpControl.Key.Substr(0, 1).ToUpperCase() + kvpControl.Key.Substr(1);
                                break;
                            default:
                                loopCount = -10;
                                break;
                        }

                        // for each field in this class.  TODO: this can be optimized with a custom lookup.
                        foreach (DictionaryEntry kvpField in thisAsDictionary)
                        {
                            if (kvpField.Key.StartsWith(strFieldNameTemp) && Math.Abs(kvpField.Key.Length - strFieldNameTemp.Length) <= 2)
                            {
                                // only store the mapping if the found field is null (making it likely that it's a subclass field pointing to a control or element).
                                if (kvpField.Value == null)
                                {
                                    // save mapping
                                    newMapping[kvpControl.Key] = kvpField.Key;
                                    loopCount = -10;
                                }
                            }
                        }

                        ++loopCount;
                    }
                }
                _hash_controlFieldMappingByControl[typeNameThis] = newMapping;
            }

            // perform the auto-fill
            Dictionary mapping = (Dictionary)_hash_controlFieldMappingByControl[typeNameThis];

            foreach (DictionaryEntry kvp in this._hash_oNamedChildElements)
            {
                if (mapping.ContainsKey(kvp.Key))
                {
                    Type.SetField(this, (string)mapping[kvp.Key], kvp.Value);
                }
            }
            foreach (DictionaryEntry kvp in this._hash_oNamedChildControls)
            {
                if (mapping.ContainsKey(kvp.Key))
                {
                    Type.SetField(this, (string)mapping[kvp.Key], kvp.Value);
                }
            }
        }
        #endregion

        #region Wrapping Properties
        public double ActualWidth
        {
            get
            {
#if DEBUG
                if (!IsInDocument)
                {
                    throw new Exception("Control not added to document yet.");
                }
#endif
                return RootElement.GetOuterWidth(false);
            }
        }
        public double ActualHeight
        {
            get
            {
#if DEBUG
                if (!IsInDocument)
                {
                    throw new Exception("Control not added to document yet.");
                }
#endif
                return RootElement.GetOuterHeight(false);
            }
        }
        public double PercentWidth
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                RootElement.Width(Math.Round(value).ToString() + "%");
            }
        }
        public double PercentHeight
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                RootElement.Height(Math.Round(value).ToString() + "%");
            }
        }
        public double PixelWidth
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                RootElement.Width(Math.Round(value).ToString() + "px");
            }
        }
        public double PixelHeight
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                RootElement.Height(Math.Round(value).ToString() + "px");
            }
        }
        public double PixelLeft
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                this.RootElement.CSS("left", Math.Round(value).ToString() + "px");
            }
        }
        public double PixelTop
        {
            private get
            {
#if DEBUG
                throw new Exception("Getter not supported.");
#else
                return Number.NaN;
#endif
            }
            set
            {
                this.RootElement.CSS("top", Math.Round(value).ToString() + "px");
            }
        }
        #endregion

        #region Global Mouse
        private static MouseCaptureHandler _mouseCaptureHandler;
        private static jQueryObject _jqMouseCaptureGlassBarrier;
        [AlternateSignature]
        extern internal void CaptureMouse(jQueryEvent mouseEvent, MouseCaptureHandler h);
        internal void CaptureMouse(jQueryEvent mouseEvent, MouseCaptureHandler h, string cssMouseCursor)
        {
            // coalesce cursor to null if undefined.
            cssMouseCursor = cssMouseCursor ?? null;

            if (_mouseCaptureHandler != null)
            {
#if DEBUG
                throw new Exception("Mouse already being captured.");
#else
                return;
#endif
            }
#if DEBUG
            if (h == null || mouseEvent == null)
            {
                throw new Exception("Argument(s) were null.");
            }
            if (mouseEvent.Type != "mousedown"
                && mouseEvent.Type != "mousemove")
            {
                throw new Exception("Event must be a 'mousedown' or 'mousemove' type.");
            }
#endif

            // invoke begin
            {
                jQueryPosition pos = MakeJQueryPosition(mouseEvent.PageX, mouseEvent.PageY);
                h(MouseCaptureState.Begin, pos);
            }

            // save
            _mouseCaptureHandler = h;

            // set barrier cursor
            {
                _jqMouseCaptureGlassBarrier.CSS("cursor", cssMouseCursor ?? "");
            }

            // turn on barrier
#if !DEBUG_MOUSE_CAPTURE
            _jqMouseCaptureGlassBarrier.Show();
#else
            _jqMouseCaptureGlassBarrier.FadeTo(0, 0.25f);
#endif

        }
        private static void InitDocumentMouseTracking()
        {
            // grab document
            jQueryObject jqDocument = jQuery.FromObject(Window.Document);

            // setup barrier
            {
                jQueryObject jqBarrier
                    = jQuery.FromHtml(
                        "<div></div>"
                    )
                ;
                jqBarrier.CSS(new Dictionary(
                    "position", "fixed",
                    "left", "0px",
                    "top", "0px",
                    "width", "100%",
                    "height", "100%",
                    "z-index", "500"
                ));
#if DEBUG_MOUSE_CAPTURE
                jqBarrier.CSS("background-color", "red");
#endif
                jqBarrier.Hide();
                _jqMouseCaptureGlassBarrier = jqBarrier;
                jqDocument.Append(jqBarrier);
            }

            // setup mouse listeners
            {
                jqDocument.MouseMove(OnMouseMoveDocument);
                jqDocument.MouseDown(OnMouseDownDocument);
                jqDocument.MouseUp(OnMouseUpDocument);
            }
        }
        private static void OnMouseDownDocument(jQueryEvent e)
        {
        }
        private static void OnMouseMoveDocument(jQueryEvent e)
        {
            if (_mouseCaptureHandler == null)
            {
                return;
            }
            _mouseCaptureHandler(MouseCaptureState.Move, MakeJQueryPosition(e.PageX, e.PageY));
            e.PreventDefault(); // todo: avoid using this. prevents capturing mouse beyond window in IE.
            e.StopPropagation();
        }
        private static void OnMouseUpDocument(jQueryEvent e)
        {
            if (_mouseCaptureHandler == null)
            {
                return;
            }
            _mouseCaptureHandler(MouseCaptureState.End, MakeJQueryPosition(e.PageX, e.PageY));
            _mouseCaptureHandler = null;
            e.PreventDefault();
            e.StopPropagation();
            _jqMouseCaptureGlassBarrier.Hide();
        }
        private static jQueryPosition MakeJQueryPosition(double left, double top)
        {
            return (jQueryPosition)(Object)(new Dictionary("left", left, "top", top));
        }
        #endregion

        #region Layout
        public int ZIndex
        {
            get
            {
                int zIndex;
                try
                {
                    zIndex = Math.Round(Number.ParseFloat(RootElement.GetCSS("z-index")));
                }
                catch
                {
                    zIndex = 0;
                }
                return zIndex;
            }
            set
            {
                RootElement.CSS("z-index", value.ToString());
            }
        }

        private Position _layoutPosition = Position.Unspecified;
        public Position LayoutPosition
        {
            get
            {
                return _layoutPosition;
            }
            set
            {
                string cssValue;
                switch (value)
                {
                    default:
                    case Position.Unspecified:
                        cssValue = string.Empty;
                        break;
                    case Position.Absolute:
                        cssValue = "absolute";
                        break;
                    case Position.Relative:
                        cssValue = "relative";
                        break;
                    case Position.Fixed:
                        cssValue = "fixed";
                        break;
                }
                RootElement.CSS("position", cssValue);
                _layoutPosition = value;
            }
        }
        #endregion
    }
}
