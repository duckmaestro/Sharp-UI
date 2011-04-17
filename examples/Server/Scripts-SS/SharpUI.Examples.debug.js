//! SharpUI.Examples.debug.js
//

(function($) {

Type.registerNamespace('SharpUI');

////////////////////////////////////////////////////////////////////////////////
// SharpUI.VerticalAlignment

SharpUI.VerticalAlignment = function() { };
SharpUI.VerticalAlignment.prototype = {
    top: 0, 
    center: 1, 
    bottom: 2, 
    stretch: 3
}
SharpUI.VerticalAlignment.registerEnum('SharpUI.VerticalAlignment', false);


////////////////////////////////////////////////////////////////////////////////
// SharpUI.HorizontalAlignment

SharpUI.HorizontalAlignment = function() { };
SharpUI.HorizontalAlignment.prototype = {
    left: 0, 
    center: 1, 
    right: 2, 
    stretch: 3
}
SharpUI.HorizontalAlignment.registerEnum('SharpUI.HorizontalAlignment', false);


////////////////////////////////////////////////////////////////////////////////
// SharpUI.Position

SharpUI.Position = function() { };
SharpUI.Position.prototype = {
    unspecified: 0, 
    absolute: 1, 
    relative: 2, 
    fixed: 3
}
SharpUI.Position.registerEnum('SharpUI.Position', false);


////////////////////////////////////////////////////////////////////////////////
// SharpUI.MouseCaptureState

SharpUI.MouseCaptureState = function() { };
SharpUI.MouseCaptureState.prototype = {
    begin: 0, 
    move: 1, 
    end: 2
}
SharpUI.MouseCaptureState.registerEnum('SharpUI.MouseCaptureState', false);


////////////////////////////////////////////////////////////////////////////////
// SharpUI.Thickness

SharpUI.Thickness = function SharpUI_Thickness() {
}
SharpUI.Thickness.prototype = {
    bottom: 0,
    left: 0,
    top: 0,
    right: 0
}


////////////////////////////////////////////////////////////////////////////////
// SharpUI.AdvancedLayout

SharpUI.AdvancedLayout = function SharpUI_AdvancedLayout() {
}
SharpUI.AdvancedLayout._initLayoutEnforcement = function SharpUI_AdvancedLayout$_initLayoutEnforcement() {
    SharpUI.AdvancedLayout._frameDetector = $('<span></span');
    SharpUI.AdvancedLayout._frameDetector.css({ position: 'absolute', height: '0px', width: '0px', margin: '0px', padding: '0px', border: 'none', display: 'block' });
    if (SharpUI.AdvancedLayout._layoutEnforcementTimerId === 0) {
        SharpUI.AdvancedLayout._layoutEnforcementTimerId = window.setInterval(SharpUI.AdvancedLayout._onLayoutEnforcement, SharpUI.AdvancedLayout._layoutEnforcementInterval);
    }
}
SharpUI.AdvancedLayout._onLayoutEnforcement = function SharpUI_AdvancedLayout$_onLayoutEnforcement() {
    var controlsInDocument = $('.' + SharpUI.AdvancedLayout.cssClassNameAdvancedLayout);
    for (var i = 0, m = controlsInDocument.length; i < m; ++i) {
        SharpUI.AdvancedLayout._updateLayout($(controlsInDocument[i]));
    }
}
SharpUI.AdvancedLayout._updateLayout = function SharpUI_AdvancedLayout$_updateLayout(element) {
    SharpUI.AdvancedLayout._measure(element);
    SharpUI.AdvancedLayout._arrange(element);
}
SharpUI.AdvancedLayout._measure = function SharpUI_AdvancedLayout$_measure(element) {
}
SharpUI.AdvancedLayout._arrange = function SharpUI_AdvancedLayout$_arrange(element) {
    if (!element.is('.' + SharpUI.AdvancedLayout.cssClassNameAdvancedLayout)) {
        throw new Error('Element not marked for advanced layout.');
    }
    var elementState = element.data('__als');
    if (elementState == null) {
        element.data('__als', elementState = SharpUI.AdvancedLayout._parseAdvancedLayout(element));
    }
    var parent = element.parent();
    var offsetParent = element.offsetParent();
    if (offsetParent.length === 0 || parent.length === 0) {
        return;
    }
    var parentIsOffsetParent = offsetParent[0] === parent[0];
    if (!parentIsOffsetParent && element.is(':visible')) {
        throw new Error('Parent must use position:absolute|fixed|relative;.');
    }
    if (!parentIsOffsetParent) {
        return;
    }
    var parentDimensions = null;
    parentDimensions = SharpUI.AdvancedLayout._getDimensionsAndPadding(parent);
    var contentStartInOffsetSpaceX, contentStartInOffsetSpaceY;
    if (parentIsOffsetParent) {
        contentStartInOffsetSpaceX = 0;
        contentStartInOffsetSpaceY = 0;
    }
    else {
        parent.prepend(SharpUI.AdvancedLayout._frameDetector);
        var parentContentFrameInDocumentSpace = SharpUI.AdvancedLayout._frameDetector.offset();
        var offsetParentFrameInDocumentSpace = offsetParent.offset();
        if (parentContentFrameInDocumentSpace != null && offsetParentFrameInDocumentSpace != null) {
            contentStartInOffsetSpaceX = parentContentFrameInDocumentSpace.left - offsetParentFrameInDocumentSpace.left - parentDimensions.paddingLeft;
            contentStartInOffsetSpaceY = parentContentFrameInDocumentSpace.top - offsetParentFrameInDocumentSpace.top - parentDimensions.paddingTop;
        }
        else {
            var contentStartInOffsetSpace = SharpUI.AdvancedLayout._frameDetector.position();
            if (contentStartInOffsetSpace != null) {
                contentStartInOffsetSpaceX = contentStartInOffsetSpace.left - parentDimensions.paddingLeft;
                contentStartInOffsetSpaceY = contentStartInOffsetSpace.top - parentDimensions.paddingTop;
            }
            else {
                contentStartInOffsetSpaceX = 0;
                contentStartInOffsetSpaceY = 0;
            }
        }
        SharpUI.AdvancedLayout._frameDetector.remove();
    }
    var topBoundary = contentStartInOffsetSpaceY + parentDimensions.paddingTop + elementState.margin.top;
    var bottomBoundary = contentStartInOffsetSpaceY + parentDimensions.clientHeight - parentDimensions.paddingBottom - elementState.margin.bottom;
    var leftBoundary = contentStartInOffsetSpaceX + parentDimensions.paddingLeft + elementState.margin.left;
    var rightBoundary = contentStartInOffsetSpaceX + parentDimensions.clientWidth - parentDimensions.paddingRight - elementState.margin.right;
    var top = 0;
    var left = 0;
    var width = 0;
    var height = 0;
    switch (elementState.verticalAlignment) {
        case SharpUI.VerticalAlignment.top:
            height = Math.round(elementState.height - elementState.padding.top - elementState.padding.bottom);
            top = Math.round(topBoundary);
            break;
        case SharpUI.VerticalAlignment.center:
            height = Math.round(elementState.height - elementState.padding.top - elementState.padding.bottom);
            top = Math.round(topBoundary * 0.5 + bottomBoundary * 0.5 - height * 0.5);
            break;
        case SharpUI.VerticalAlignment.bottom:
            height = Math.round(elementState.height - elementState.padding.top - elementState.padding.bottom);
            top = Math.round(contentStartInOffsetSpaceY + parentDimensions.clientHeight - parentDimensions.paddingBottom - elementState.margin.bottom - elementState.height);
            break;
        case SharpUI.VerticalAlignment.stretch:
            height = Math.round(bottomBoundary - topBoundary - elementState.padding.top - elementState.padding.bottom);
            top = Math.round(topBoundary);
            break;
    }
    switch (elementState.horizontalAlignment) {
        case SharpUI.HorizontalAlignment.left:
            width = Math.round(elementState.width - elementState.padding.left - elementState.padding.right);
            left = Math.round(leftBoundary);
            break;
        case SharpUI.HorizontalAlignment.center:
            width = Math.round(elementState.width - elementState.padding.left - elementState.padding.right);
            left = Math.round(leftBoundary * 0.5 + rightBoundary * 0.5 - width * 0.5);
            break;
        case SharpUI.HorizontalAlignment.right:
            width = Math.round(elementState.width - elementState.padding.left - elementState.padding.right);
            left = Math.round(contentStartInOffsetSpaceX + parentDimensions.clientWidth - parentDimensions.paddingRight - elementState.margin.right - elementState.width);
            break;
        case SharpUI.HorizontalAlignment.stretch:
            width = Math.round(rightBoundary - leftBoundary - elementState.padding.left - elementState.padding.right);
            left = Math.round(leftBoundary);
            break;
    }
    if (width <= 0) {
        width = 0;
    }
    if (height <= 0) {
        height = 0;
    }
    element.css({ position: 'absolute', top: top, left: left, width: width, height: height, 'padding-top': elementState.padding.top, 'padding-right': elementState.padding.right, 'padding-bottom': elementState.padding.bottom, 'padding-left': elementState.padding.left });
}
SharpUI.AdvancedLayout._getDimensionsAndPadding = function SharpUI_AdvancedLayout$_getDimensionsAndPadding(element) {
    var d = {};
    if ((typeof(window.getComputedStyle) === 'function')) {
        var computedStyle = window.getComputedStyle(element[0]);
        if (('width' in computedStyle)) {
            d.clientWidth = Number.parse(computedStyle.width);
            d.clientHeight = Number.parse(computedStyle.height);
            d.paddingTop = Number.parse(computedStyle.paddingTop);
            d.paddingRight = Number.parse(computedStyle.paddingRight);
            d.paddingBottom = Number.parse(computedStyle.paddingBottom);
            d.paddingLeft = Number.parse(computedStyle.paddingLeft);
        }
        else {
            d.clientWidth = Number.parse(computedStyle.getPropertyValue('width'));
            d.clientHeight = Number.parse(computedStyle.getPropertyValue('height'));
            d.paddingTop = Number.parse(computedStyle.getPropertyValue('padding-top'));
            d.paddingRight = Number.parse(computedStyle.getPropertyValue('padding-right'));
            d.paddingBottom = Number.parse(computedStyle.getPropertyValue('padding-bottom'));
            d.paddingLeft = Number.parse(computedStyle.getPropertyValue('padding-left'));
        }
        d.clientWidth += d.paddingLeft + d.paddingRight;
        d.clientHeight += d.paddingTop + d.paddingBottom;
        return d;
    }
    else {
        d.clientWidth = element.innerWidth();
        d.clientHeight = element.innerHeight();
        d.paddingTop = Number.parse(element.css('padding-top'));
        d.paddingRight = Number.parse(element.css('padding-right'));
        d.paddingBottom = Number.parse(element.css('padding-bottom'));
        d.paddingLeft = Number.parse(element.css('padding-left'));
        return d;
    }
}
SharpUI.AdvancedLayout._parseAdvancedLayout = function SharpUI_AdvancedLayout$_parseAdvancedLayout(element) {
    var marginTop, marginRight, marginBottom, marginLeft;
    var margin = element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'margin');
    if (!String.isNullOrEmpty(margin)) {
        var split = margin.trim().split(' ');
        marginTop = parseFloat(split[0]);
        marginRight = parseFloat(split[1]);
        marginBottom = parseFloat(split[2]);
        marginLeft = parseFloat(split[3]);
    }
    else {
        marginTop = 0;
        marginRight = 0;
        marginBottom = 0;
        marginLeft = 0;
    }
    var paddingTop, paddingRight, paddingBottom, paddingLeft;
    var padding = element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'padding');
    if (!String.isNullOrEmpty(padding)) {
        var split = padding.trim().split(' ');
        paddingTop = parseFloat(split[0]);
        paddingRight = parseFloat(split[1]);
        paddingBottom = parseFloat(split[2]);
        paddingLeft = parseFloat(split[3]);
    }
    else {
        paddingTop = 0;
        paddingRight = 0;
        paddingBottom = 0;
        paddingLeft = 0;
    }
    var advancedWidth, advancedHeight;
    advancedWidth = Number.parse(element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'width'));
    advancedHeight = Number.parse(element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'height'));
    var verticalAlignment;
    switch (element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'vertical-alignment')) {
        case 'top':
        case 'Top':
            verticalAlignment = SharpUI.VerticalAlignment.top;
            break;
        case 'center':
        case 'Center':
            verticalAlignment = SharpUI.VerticalAlignment.center;
            break;
        case 'bottom':
        case 'Bottom':
            verticalAlignment = SharpUI.VerticalAlignment.bottom;
            break;
        case 'stretch':
        case 'Stretch':
        default:
            verticalAlignment = SharpUI.VerticalAlignment.stretch;
            break;
    }
    var horizontalAlignment;
    switch (element.attr(SharpUI.AdvancedLayout.attributeNamePrefix + 'horizontal-alignment')) {
        case 'left':
        case 'Left':
            horizontalAlignment = SharpUI.HorizontalAlignment.left;
            break;
        case 'center':
        case 'Center':
            horizontalAlignment = SharpUI.HorizontalAlignment.center;
            break;
        case 'right':
        case 'Right':
            horizontalAlignment = SharpUI.HorizontalAlignment.right;
            break;
        case 'stretch':
        case 'Stretch':
        default:
            horizontalAlignment = SharpUI.HorizontalAlignment.stretch;
            break;
    }
    if (verticalAlignment !== SharpUI.VerticalAlignment.stretch && isNaN(advancedHeight)) {
        verticalAlignment = SharpUI.VerticalAlignment.stretch;
    }
    if (horizontalAlignment !== SharpUI.HorizontalAlignment.stretch && isNaN(advancedWidth)) {
        horizontalAlignment = SharpUI.HorizontalAlignment.stretch;
    }
    var state = {};
    state.margin = new SharpUI.Thickness();
    state.padding = new SharpUI.Thickness();
    state.height = advancedHeight;
    state.width = advancedWidth;
    state.verticalAlignment = verticalAlignment;
    state.horizontalAlignment = horizontalAlignment;
    state.margin.top = marginTop;
    state.margin.right = marginRight;
    state.margin.bottom = marginBottom;
    state.margin.left = marginLeft;
    state.padding.top = paddingTop;
    state.padding.right = paddingRight;
    state.padding.bottom = paddingBottom;
    state.padding.left = paddingLeft;
    return state;
}


////////////////////////////////////////////////////////////////////////////////
// SharpUI.TemplateControl

SharpUI.TemplateControl = function SharpUI_TemplateControl(oTemplate) {
    this._layoutPosition = SharpUI.Position.unspecified;
    if (!SharpUI.TemplateControl._bStaticConstructionFinished) {
        SharpUI.TemplateControl._staticConstructor();
    }
    this._strInstanceId = SharpUI.TemplateControl._generateNewInstanceId();
    var strTemplate;
    if (ss.isNullOrUndefined(oTemplate)) {
        strTemplate = SharpUI.TemplateControl._findTemplate(this);
        if (String.isNullOrEmpty(strTemplate)) {
            throw new Error(Type.getInstanceType(this).get_fullName() + ' is missing a Template member and no template was provided.');
        }
    }
    else {
        if (Type.canCast(oTemplate, String)) {
            strTemplate = oTemplate;
        }
        else {
            var jqTemplate = $(oTemplate);
            strTemplate = '<' + jqTemplate[0].tagName + '>' + jqTemplate.html() + '</' + jqTemplate[0].tagName + '>';
        }
    }
    this._hash_oNamedChildControls = {};
    this._hash_oNamedChildElements = {};
    var jqHead = $('head');
    if (ss.isNullOrUndefined(strTemplate)) {
        strTemplate = this.template;
    }
    if (String.isNullOrEmpty(strTemplate)) {
        throw new Error(Type.getInstanceType(this).get_fullName() + ' is missing a Template member.');
    }
    var strNewId = SharpUI.TemplateControl._generateNewAutoId();
    var iNumOtherControlsWithId = $('#' + strNewId).length;
    if (iNumOtherControlsWithId !== 0) {
        throw new Error('Auto generated id conflict.');
    }
    var jqContent = $(strTemplate.replaceAll('`', '\"'));
    var strStyleRules = String.Empty;
    jqContent.filter('style').each(function(i, e) {
        var jqElement = $(e);
        strStyleRules += jqElement.html();
        jqElement.remove();
    });
    jqContent = jqContent.not('style').remove();
    if (!String.isNullOrEmpty(jqContent.attr('id'))) {
        throw new Error('Global ID\'s not permitted. Element with ID \"' + jqContent.attr('id') + '\" found.');
    }
    jqContent.removeAttr('id');
    this._jqRootElement = jqContent;
    this._jqRootElement.data(SharpUI.TemplateControl._dataNameControl, this);
    jqContent.find('*[' + SharpUI.TemplateControl._attributeNameLocalId + ']').each(ss.Delegate.create(this, function(i, e) {
        var jqElement = $(e);
        if (!String.isNullOrEmpty(jqElement.attr(SharpUI.TemplateControl._attributeNameControlClass))) {
            return;
        }
        var strLocalId = SharpUI.TemplateControl._getLocalId(jqElement);
        if (strLocalId == null) {
            return;
        }
        this._hash_oNamedChildElements[strLocalId] = jqElement;
        if (!String.isNullOrEmpty(jqElement.attr('id'))) {
            throw new Error('Global ID\'s not permitted. Element with ID \"' + jqElement.attr('id') + '\" found.');
        }
        jqElement.removeAttr('id');
    }));
    var currentType = Type.getInstanceType(this);
    var currentTopLevelNamespace = currentType.get_fullName().substr(0, currentType.get_fullName().indexOf('.'));
    this._jqRootElement.addClass(SharpUI.TemplateControl.cssClassNameControl);
    this._jqRootElement.addClass(SharpUI.TemplateControl.cssClassNameControlUnadded);
    jqContent.find('div[' + SharpUI.TemplateControl._attributeNameControlClass + ']').each(ss.Delegate.create(this, function(index, element) {
        var jqElement = $(element);
        var strChildTypeName = jqElement.attr(SharpUI.TemplateControl._attributeNameControlClass);
        var strChildTypeNameResolved = SharpUI.TemplateControl._resolveTypeName(strChildTypeName, currentTopLevelNamespace);
        var oChildType = Type.getType(strChildTypeNameResolved);
        if (ss.isNullOrUndefined(oChildType)) {
            throw new Error('Could not locate type \"' + (strChildTypeNameResolved || strChildTypeName) + '\"');
        }
        var childControl = Type.safeCast(new oChildType(null), SharpUI.TemplateControl);
        var strLocalId = SharpUI.TemplateControl._getLocalId(jqElement) || SharpUI.TemplateControl._generateNewAutoId();
        if (strLocalId != null) {
            this._hash_oNamedChildControls[strLocalId] = childControl;
        }
        var strClass = jqElement.attr('class');
        var strStyle = jqElement.attr('style');
        if (!String.isNullOrEmpty(strClass)) {
            var strClassFromTemplate = childControl.get_rootElement().attr('class') || String.Empty;
            childControl.get_rootElement().attr('class', strClassFromTemplate + ' ' + strClass);
        }
        if (!String.isNullOrEmpty(strStyle)) {
            var strStyleFromTemplate = childControl.get_rootElement().attr('style') || String.Empty;
            childControl.get_rootElement().attr('style', strStyleFromTemplate + ' ' + strStyle);
        }
        for (var i = 0, m = jqElement[0].attributes.length; i < m; ++i) {
            var a = jqElement[0].attributes[i];
            if ($.browser.version === '7.0' && $.browser.msie && !a.specified) {
                continue;
            }
            var attributeName = a.name.toLowerCase();
            switch (attributeName) {
                case 'id':
                case 'xid':
                case 'class':
                case 'style':
                case 'control':
                    break;
                default:
                    childControl.get_rootElement().attr(a.name, a.value);
                    break;
            }
        }
        jqElement.removeAttr('id').after(childControl.get_rootElement()).remove();
        if (strLocalId != null) {
            childControl.get_rootElement().attr('xid', strLocalId);
        }
        childControl.get_rootElement().attr('control', jqElement.attr(SharpUI.TemplateControl._attributeNameControlClass));
        var jqChildContent = jqElement.find('>*');
        if (jqChildContent.length > 0) {
            childControl.processChildContent(jqChildContent);
        }
    }));
    var jqRadioInputs = this._jqRootElement.find('input[type=radio]');
    var hash_rewrittenGroupNames = {};
    jqRadioInputs.each(function(index, element) {
        var jqRadio = $(element);
        var strGroupName = jqRadio.attr('name');
        if (String.isNullOrEmpty(strGroupName)) {
            return;
        }
        var strNewGroupName;
        if (Object.keyExists(hash_rewrittenGroupNames, strGroupName)) {
            strNewGroupName = hash_rewrittenGroupNames[strGroupName];
        }
        else {
            hash_rewrittenGroupNames[strGroupName] = strNewGroupName = SharpUI.TemplateControl._generateNewAutoId();
        }
        jqRadio.attr('name', strNewGroupName);
        if (String.isNullOrEmpty(jqRadio.attr('id'))) {
            jqRadio.attr('id', SharpUI.TemplateControl._generateNewAutoId());
        }
    });
    this._hash_rewrittenGroupNames = hash_rewrittenGroupNames;
    var jqLabels = this._jqRootElement.find('label[for]');
    jqLabels.each(function(index, element) {
        var jqLabelElement = $(element);
        var strForId = jqLabelElement.attr('for');
        var jqTargetElement = this.tryGetElement(strForId);
        if (jqTargetElement == null) {
            return;
        }
        var strTargetElementNewId = jqTargetElement.attr('id');
        if (String.isNullOrEmpty(strTargetElementNewId)) {
            jqTargetElement.attr('id', strTargetElementNewId = SharpUI.TemplateControl._generateNewAutoId());
        }
        jqLabelElement.attr('for', strTargetElementNewId);
        return;
    });
    if (strStyleRules.length !== 0) {
        SharpUI.TemplateControl._processCss(this, strStyleRules);
    }
    this._autoFillMemberFields();
}
SharpUI.TemplateControl._staticConstructor = function SharpUI_TemplateControl$_staticConstructor() {
    if (SharpUI.TemplateControl._bStaticConstructionFinished) {
        throw new Error('Static construction already finished.');
    }
    SharpUI.TemplateControl._initDocumentDetection();
    SharpUI.TemplateControl._initDocumentMouseTracking();
    SharpUI.TemplateControl._bStaticConstructionFinished = true;
}
SharpUI.TemplateControl._findTemplate = function SharpUI_TemplateControl$_findTemplate(templateControl) {
    var templateTypeName = Type.getInstanceType(templateControl).get_fullName();
    if (!Object.keyExists(SharpUI.TemplateControl._hash_templateCache, templateTypeName)) {
        var strTemplate = templateControl.template;
        if (!String.isNullOrEmpty(strTemplate)) {
            SharpUI.TemplateControl._hash_templateCache[templateTypeName] = strTemplate;
        }
        else {
            var $dict1 = templateControl;
            for (var $key2 in $dict1) {
                var kvp = { key: $key2, value: $dict1[$key2] };
                if (!(Type.canCast(kvp.value, String))) {
                    continue;
                }
                if (!kvp.key.startsWith('$')) {
                    continue;
                }
                var strTmp = (kvp.value).trim();
                if (strTmp.startsWith('<')) {
                    SharpUI.TemplateControl._hash_templateCache[templateTypeName] = kvp.value;
                }
            }
        }
    }
    return (SharpUI.TemplateControl._hash_templateCache[templateTypeName] || null);
}
SharpUI.TemplateControl._fromRootElement = function SharpUI_TemplateControl$_fromRootElement(elem) {
    if (elem == null) {
        throw new Error('Element null.');
    }
    var jqElem = $(elem);
    var tc = Type.safeCast(jqElem.data(SharpUI.TemplateControl._dataNameControl), SharpUI.TemplateControl);
    if (tc == null) {
        throw new Error('Provided element is not the root of a Template Control.');
    }
    return tc;
}
SharpUI.TemplateControl._processCss = function SharpUI_TemplateControl$_processCss(rootControl, strRawCss) {
    var strControlType = Type.getInstanceType(rootControl).get_fullName();
    var bIsNewStyleSet = false;
    var hash_xidsToCssClasses = SharpUI.TemplateControl._hash_processedCss[strControlType] || null;
    if (hash_xidsToCssClasses == null) {
        hash_xidsToCssClasses = {};
        bIsNewStyleSet = true;
    }
    if (bIsNewStyleSet) {
        var strProcessedCss = strRawCss.replace(new RegExp('#[a-zA-Z]\\w*', 'g'), function(s) {
            var sSub = s.substr(1);
            if (sSub === 'this') {
                hash_xidsToCssClasses[sSub] = hash_xidsToCssClasses[sSub] || SharpUI.TemplateControl._generateNewAutoCssClass();
                return '.' + hash_xidsToCssClasses[sSub] + '/* ' + s + ' */';
            }
            var jqElement = rootControl._hash_oNamedChildElements[sSub] || null;
            if (jqElement != null) {
                hash_xidsToCssClasses[sSub] = hash_xidsToCssClasses[sSub] || SharpUI.TemplateControl._generateNewAutoCssClass();
                return '.' + hash_xidsToCssClasses[sSub] + '/* ' + s + ' */';
            }
            var oControl = rootControl._hash_oNamedChildControls[sSub] || null;
            if (oControl != null) {
                hash_xidsToCssClasses[sSub] = hash_xidsToCssClasses[sSub] || SharpUI.TemplateControl._generateNewAutoCssClass();
                return '.' + hash_xidsToCssClasses[sSub] + '/* ' + s + ' */';
            }
            return s;
        });
        var jqStyle;
        if ($.browser.msie) {
            jqStyle = $('<style type=\"text/css\">' + strProcessedCss + '</style>');
        }
        else {
            jqStyle = $('<style type=\"text/css\"></style>');
            jqStyle.html(strProcessedCss);
        }
        $('head').append(jqStyle);
        SharpUI.TemplateControl._hash_processedCss[strControlType] = hash_xidsToCssClasses;
    }
    var $dict1 = hash_xidsToCssClasses;
    for (var $key2 in $dict1) {
        var kvp = { key: $key2, value: $dict1[$key2] };
        var key = kvp.key;
        if (key === 'this') {
            rootControl._jqRootElement.addClass(kvp.value);
            continue;
        }
        var jqElement = rootControl._hash_oNamedChildElements[key] || null;
        if (jqElement != null) {
            jqElement.addClass(kvp.value);
            continue;
        }
        var oControl = rootControl._hash_oNamedChildControls[key] || null;
        if (oControl != null) {
            oControl.get_rootElement().addClass(kvp.value);
            continue;
        }
        throw new Error('CSS rule found for no corresponding element/control.');
    }
}
SharpUI.TemplateControl._initDocumentDetection = function SharpUI_TemplateControl$_initDocumentDetection() {
    if (SharpUI.TemplateControl._iCheckParentIntervalId === 0) {
        SharpUI.TemplateControl._iCheckParentIntervalId = window.setInterval(SharpUI.TemplateControl._onIntervalCheckParent, SharpUI.TemplateControl._documentTreeCheckInterval);
    }
}
SharpUI.TemplateControl._onIntervalCheckParent = function SharpUI_TemplateControl$_onIntervalCheckParent() {
    var arr_controlsToNotifyAdded = [];
    var $dict1 = SharpUI.TemplateControl._hash_strControlIdsKnownInDocument;
    for (var $key2 in $dict1) {
        var kvp = { key: $key2, value: $dict1[$key2] };
        var strInstanceId = kvp.key;
        var control = kvp.value;
        if (!control.get__isInDocument()) {
            delete SharpUI.TemplateControl._hash_strControlIdsKnownInDocument[strInstanceId];
            control._notifyRemovedFromDocument();
            control.get_rootElement().addClass(SharpUI.TemplateControl.cssClassNameControlUnadded);
        }
        if (!control._bPresented && control.__presented != null) {
            if (control.get_rootElement().is(':visible')) {
                control._notifyPresented();
                control._bPresented = true;
            }
        }
    }
    var hash_strControlsFound = {};
    var newControls = $('.' + SharpUI.TemplateControl.cssClassNameControlUnadded);
    newControls.each(function(i, e) {
        var rootElement = $(e);
        var control = rootElement.data(SharpUI.TemplateControl._dataNameControl);
        if (control == null) {
            throw new Error('Control root element missing Control data. Did you use jQuery empty() or remove() by mistake?');
        }
        var strInstanceId = control._strInstanceId;
        if (String.isNullOrEmpty(strInstanceId.trim())) {
            throw new Error('Found control with empty instance id.');
        }
        hash_strControlsFound[strInstanceId] = null;
        if (!Object.keyExists(SharpUI.TemplateControl._hash_strControlIdsKnownInDocument, strInstanceId)) {
            SharpUI.TemplateControl._hash_strControlIdsKnownInDocument[strInstanceId] = control;
            arr_controlsToNotifyAdded.add(control);
            rootElement.removeClass(SharpUI.TemplateControl.cssClassNameControlUnadded);
        }
    });
    for (var i = arr_controlsToNotifyAdded.length - 1; i >= 0; --i) {
        var controlToNotify = arr_controlsToNotifyAdded[i];
        controlToNotify._notifyAddedToDocument();
    }
    arr_controlsToNotifyAdded.clear();
}
SharpUI.TemplateControl._resolveTypeName = function SharpUI_TemplateControl$_resolveTypeName(strShortName, startingNamespace) {
    if (ss.isNullOrUndefined(startingNamespace)) {
        throw new Error('Missing starting namespace.');
    }
    if (Object.keyExists(SharpUI.TemplateControl._hash_cachedTypeNameResolves, strShortName)) {
        return SharpUI.TemplateControl._hash_cachedTypeNameResolves[strShortName];
    }
    var strResolvedTypeName = startingNamespace + '.' + strShortName;
    if (Type.getType(strResolvedTypeName) != null) {
        return (SharpUI.TemplateControl._hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
    }
    strResolvedTypeName = startingNamespace + '.' + '_' + strShortName.substr(0, 1).toLowerCase() + strShortName.substr(1);
    if (Type.getType(strResolvedTypeName) != null) {
        return (SharpUI.TemplateControl._hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
    }
    var d = Type.getType(startingNamespace);
    var $dict1 = d;
    for (var $key2 in $dict1) {
        var kvp = { key: $key2, value: $dict1[$key2] };
        if (!Type.isNamespace(kvp.value)) {
            continue;
        }
        var namespaceName = kvp.value.getName();
        strResolvedTypeName = SharpUI.TemplateControl._resolveTypeName(strShortName, namespaceName);
        if (strResolvedTypeName != null) {
            return (SharpUI.TemplateControl._hash_cachedTypeNameResolves[strShortName] = strResolvedTypeName);
        }
    }
    return null;
}
SharpUI.TemplateControl._generateNewAutoId = function SharpUI_TemplateControl$_generateNewAutoId() {
    return SharpUI.TemplateControl._idPrefixAutoRewrite + SharpUI.TemplateControl._iAutoIdGeneratorCounter++;
}
SharpUI.TemplateControl._generateNewAutoCssClass = function SharpUI_TemplateControl$_generateNewAutoCssClass() {
    return SharpUI.TemplateControl._cssClassNamePrefixAutoRewrite + SharpUI.TemplateControl._iAutoIdGeneratorCounter++;
}
SharpUI.TemplateControl._getLocalId = function SharpUI_TemplateControl$_getLocalId(jqElement) {
    var strLocalId = jqElement.attr(SharpUI.TemplateControl._attributeNameLocalId);
    if (String.isNullOrEmpty(strLocalId)) {
        return null;
    }
    strLocalId = strLocalId.trim();
    if (strLocalId.length === 0) {
        return null;
    }
    return strLocalId;
}
SharpUI.TemplateControl._generateNewInstanceId = function SharpUI_TemplateControl$_generateNewInstanceId() {
    return (SharpUI.TemplateControl._iInstanceIdGeneratorCounter++).toString();
}
SharpUI.TemplateControl._initDocumentMouseTracking = function SharpUI_TemplateControl$_initDocumentMouseTracking() {
    var jqDocument = $(window.document);
    var jqBarrier = $('<div></div>');
    jqBarrier.css({ position: 'fixed', left: '0px', top: '0px', width: '100%', height: '100%', 'z-index': '500' });
    jqBarrier.hide();
    SharpUI.TemplateControl._jqMouseCaptureGlassBarrier = jqBarrier;
    jqDocument.append(jqBarrier);
    jqDocument.mousemove(SharpUI.TemplateControl._onMouseMoveDocument);
    jqDocument.mousedown(SharpUI.TemplateControl._onMouseDownDocument);
    jqDocument.mouseup(SharpUI.TemplateControl._onMouseUpDocument);
}
SharpUI.TemplateControl._onMouseDownDocument = function SharpUI_TemplateControl$_onMouseDownDocument(e) {
}
SharpUI.TemplateControl._onMouseMoveDocument = function SharpUI_TemplateControl$_onMouseMoveDocument(e) {
    if (SharpUI.TemplateControl._mouseCaptureHandler == null) {
        return;
    }
    SharpUI.TemplateControl._mouseCaptureHandler(SharpUI.MouseCaptureState.move, SharpUI.TemplateControl._makeJQueryPosition(e.pageX, e.pageY));
    e.preventDefault();
    e.stopPropagation();
}
SharpUI.TemplateControl._onMouseUpDocument = function SharpUI_TemplateControl$_onMouseUpDocument(e) {
    if (SharpUI.TemplateControl._mouseCaptureHandler == null) {
        return;
    }
    SharpUI.TemplateControl._mouseCaptureHandler(SharpUI.MouseCaptureState.end, SharpUI.TemplateControl._makeJQueryPosition(e.pageX, e.pageY));
    SharpUI.TemplateControl._mouseCaptureHandler = null;
    e.preventDefault();
    e.stopPropagation();
    SharpUI.TemplateControl._jqMouseCaptureGlassBarrier.hide();
}
SharpUI.TemplateControl._makeJQueryPosition = function SharpUI_TemplateControl$_makeJQueryPosition(left, top) {
    return { left: left, top: top };
}
SharpUI.TemplateControl.prototype = {
    _jqRootElement: null,
    
    get_rootElement: function SharpUI_TemplateControl$get_rootElement() {
        return this._jqRootElement;
    },
    
    _strInstanceId: null,
    
    processChildContent: function SharpUI_TemplateControl$processChildContent(jqChildContent) {
    },
    
    add_addedToDocument: function SharpUI_TemplateControl$add_addedToDocument(value) {
        this.__addedToDocument = ss.Delegate.combine(this.__addedToDocument, value);
    },
    remove_addedToDocument: function SharpUI_TemplateControl$remove_addedToDocument(value) {
        this.__addedToDocument = ss.Delegate.remove(this.__addedToDocument, value);
    },
    
    __addedToDocument: null,
    
    add_removedFromDocument: function SharpUI_TemplateControl$add_removedFromDocument(value) {
        this.__removedFromDocument = ss.Delegate.combine(this.__removedFromDocument, value);
    },
    remove_removedFromDocument: function SharpUI_TemplateControl$remove_removedFromDocument(value) {
        this.__removedFromDocument = ss.Delegate.remove(this.__removedFromDocument, value);
    },
    
    __removedFromDocument: null,
    
    add_presented: function SharpUI_TemplateControl$add_presented(value) {
        this.__presented = ss.Delegate.combine(this.__presented, value);
    },
    remove_presented: function SharpUI_TemplateControl$remove_presented(value) {
        this.__presented = ss.Delegate.remove(this.__presented, value);
    },
    
    __presented: null,
    _bPresented: false,
    
    _notifyAddedToDocument: function SharpUI_TemplateControl$_notifyAddedToDocument() {
        if (this.__addedToDocument != null) {
            this.__addedToDocument(this, null);
        }
    },
    
    _notifyRemovedFromDocument: function SharpUI_TemplateControl$_notifyRemovedFromDocument() {
        if (this.__removedFromDocument != null) {
            this.__removedFromDocument(this, null);
        }
    },
    
    _notifyPresented: function SharpUI_TemplateControl$_notifyPresented() {
        if (this.__presented != null) {
            this.__presented(this, null);
        }
    },
    
    get_documentBody: function SharpUI_TemplateControl$get_documentBody() {
        if (this.get__isInDocument()) {
            return $(window.document.body);
        }
        else {
            return null;
        }
    },
    
    get__isInDocument: function SharpUI_TemplateControl$get__isInDocument() {
        var e = this.get_rootElement()[0];
        while (true) {
            e = e.parentNode;
            if (e == null) {
                return false;
            }
            if (e.nodeType === 9) {
                return true;
            }
        }
    },
    
    _hash_oNamedChildElements: null,
    
    getElement: function SharpUI_TemplateControl$getElement(strId) {
        var o = this._hash_oNamedChildElements[strId];
        if (ss.isNullOrUndefined(o)) {
            throw new Error('Element by id \"' + strId + '\" not found.');
        }
        return o;
    },
    
    tryGetElement: function SharpUI_TemplateControl$tryGetElement(strId) {
        var o = this._hash_oNamedChildElements[strId];
        return o || null;
    },
    
    _hash_oNamedChildControls: null,
    
    getControl: function SharpUI_TemplateControl$getControl(strId) {
        var o = this._hash_oNamedChildControls[strId];
        if (ss.isNullOrUndefined(o)) {
            throw new Error('Control by id \"' + strId + '\" not found.');
        }
        return o;
    },
    
    _hash_rewrittenGroupNames: null,
    
    getGroup: function SharpUI_TemplateControl$getGroup(formFieldGroupname) {
        var rewrittenName = this._hash_rewrittenGroupNames[formFieldGroupname];
        if (ss.isNullOrUndefined(rewrittenName)) {
            throw new Error('Group by name \"' + formFieldGroupname + '\" not found.');
        }
        return this.get_rootElement().find('*[name=' + rewrittenName + ']');
    },
    
    _autoFillMemberFields: function SharpUI_TemplateControl$_autoFillMemberFields() {
        var typeNameThis = Type.getInstanceType(this).get_fullName();
        if (!Object.keyExists(SharpUI.TemplateControl._hash_controlFieldMappingByControl, typeNameThis)) {
            var newMapping = {};
            var thisAsDictionary = this;
            var $dict1 = this._hash_oNamedChildElements;
            for (var $key2 in $dict1) {
                var kvpElement = { key: $key2, value: $dict1[$key2] };
                var loopCount = 0;
                var strFieldNameTemp = null;
                while (loopCount >= 0) {
                    switch (loopCount) {
                        case 0:
                            strFieldNameTemp = '_' + kvpElement.key;
                            break;
                        case 1:
                            strFieldNameTemp = '_jq' + kvpElement.key;
                            break;
                        case 2:
                            if (kvpElement.key.length <= 1) {
                                loopCount = -10;
                                break;
                            }
                            strFieldNameTemp = '_' + kvpElement.key.substr(0, 1).toLowerCase() + kvpElement.key.substr(1);
                            break;
                        case 3:
                            strFieldNameTemp = '_jq' + kvpElement.key.substr(0, 1).toUpperCase() + kvpElement.key.substr(1);
                            break;
                        default:
                            loopCount = -10;
                            break;
                    }
                    var $dict3 = thisAsDictionary;
                    for (var $key4 in $dict3) {
                        var kvpField = { key: $key4, value: $dict3[$key4] };
                        if (kvpField.key.startsWith(strFieldNameTemp) && Math.abs(kvpField.key.length - strFieldNameTemp.length) <= 2) {
                            if (kvpField.value == null) {
                                newMapping[kvpElement.key] = kvpField.key;
                                loopCount = -10;
                            }
                        }
                    }
                    ++loopCount;
                }
            }
            var $dict5 = this._hash_oNamedChildControls;
            for (var $key6 in $dict5) {
                var kvpControl = { key: $key6, value: $dict5[$key6] };
                var loopCount = 0;
                var strFieldNameTemp = null;
                while (loopCount >= 0) {
                    switch (loopCount) {
                        case 0:
                            strFieldNameTemp = '_' + kvpControl.key;
                            break;
                        case 1:
                            strFieldNameTemp = '_o' + kvpControl.key;
                            break;
                        case 2:
                            if (kvpControl.key.length <= 1) {
                                loopCount = -10;
                                break;
                            }
                            strFieldNameTemp = '_' + kvpControl.key.substr(0, 1).toLowerCase() + kvpControl.key.substr(1);
                            break;
                        case 3:
                            strFieldNameTemp = '_o' + kvpControl.key.substr(0, 1).toUpperCase() + kvpControl.key.substr(1);
                            break;
                        default:
                            loopCount = -10;
                            break;
                    }
                    var $dict7 = thisAsDictionary;
                    for (var $key8 in $dict7) {
                        var kvpField = { key: $key8, value: $dict7[$key8] };
                        if (kvpField.key.startsWith(strFieldNameTemp) && Math.abs(kvpField.key.length - strFieldNameTemp.length) <= 2) {
                            if (kvpField.value == null) {
                                newMapping[kvpControl.key] = kvpField.key;
                                loopCount = -10;
                            }
                        }
                    }
                    ++loopCount;
                }
            }
            SharpUI.TemplateControl._hash_controlFieldMappingByControl[typeNameThis] = newMapping;
        }
        var mapping = SharpUI.TemplateControl._hash_controlFieldMappingByControl[typeNameThis];
        var $dict9 = this._hash_oNamedChildElements;
        for (var $key10 in $dict9) {
            var kvp = { key: $key10, value: $dict9[$key10] };
            if (Object.keyExists(mapping, kvp.key)) {
                this[mapping[kvp.key]] = kvp.value;
            }
        }
        var $dict11 = this._hash_oNamedChildControls;
        for (var $key12 in $dict11) {
            var kvp = { key: $key12, value: $dict11[$key12] };
            if (Object.keyExists(mapping, kvp.key)) {
                this[mapping[kvp.key]] = kvp.value;
            }
        }
    },
    
    get_actualWidth: function SharpUI_TemplateControl$get_actualWidth() {
        if (!this.get__isInDocument()) {
            throw new Error('Control not added to document yet.');
        }
        return this.get_rootElement().outerWidth(false);
    },
    
    get_actualHeight: function SharpUI_TemplateControl$get_actualHeight() {
        if (!this.get__isInDocument()) {
            throw new Error('Control not added to document yet.');
        }
        return this.get_rootElement().outerHeight(false);
    },
    
    get_percentWidth: function SharpUI_TemplateControl$get_percentWidth() {
        throw new Error('Getter not supported.');
    },
    set_percentWidth: function SharpUI_TemplateControl$set_percentWidth(value) {
        this.get_rootElement().width(Math.round(value).toString() + '%');
        return value;
    },
    
    get_percentHeight: function SharpUI_TemplateControl$get_percentHeight() {
        throw new Error('Getter not supported.');
    },
    set_percentHeight: function SharpUI_TemplateControl$set_percentHeight(value) {
        this.get_rootElement().height(Math.round(value).toString() + '%');
        return value;
    },
    
    get_pixelWidth: function SharpUI_TemplateControl$get_pixelWidth() {
        throw new Error('Getter not supported.');
    },
    set_pixelWidth: function SharpUI_TemplateControl$set_pixelWidth(value) {
        this.get_rootElement().width(Math.round(value).toString() + 'px');
        return value;
    },
    
    get_pixelHeight: function SharpUI_TemplateControl$get_pixelHeight() {
        throw new Error('Getter not supported.');
    },
    set_pixelHeight: function SharpUI_TemplateControl$set_pixelHeight(value) {
        this.get_rootElement().height(Math.round(value).toString() + 'px');
        return value;
    },
    
    get_pixelLeft: function SharpUI_TemplateControl$get_pixelLeft() {
        throw new Error('Getter not supported.');
    },
    set_pixelLeft: function SharpUI_TemplateControl$set_pixelLeft(value) {
        this.get_rootElement().css('left', Math.round(value).toString() + 'px');
        return value;
    },
    
    get_pixelTop: function SharpUI_TemplateControl$get_pixelTop() {
        throw new Error('Getter not supported.');
    },
    set_pixelTop: function SharpUI_TemplateControl$set_pixelTop(value) {
        this.get_rootElement().css('top', Math.round(value).toString() + 'px');
        return value;
    },
    
    _captureMouse: function SharpUI_TemplateControl$_captureMouse(mouseEvent, h, cssMouseCursor) {
        cssMouseCursor = cssMouseCursor || null;
        if (SharpUI.TemplateControl._mouseCaptureHandler != null) {
            throw new Error('Mouse already being captured.');
        }
        if (h == null || mouseEvent == null) {
            throw new Error('Argument(s) were null.');
        }
        if (mouseEvent.type !== 'mousedown' && mouseEvent.type !== 'mousemove') {
            throw new Error('Event must be a \'mousedown\' or \'mousemove\' type.');
        }
        var pos = SharpUI.TemplateControl._makeJQueryPosition(mouseEvent.pageX, mouseEvent.pageY);
        h(SharpUI.MouseCaptureState.begin, pos);
        SharpUI.TemplateControl._mouseCaptureHandler = h;
        SharpUI.TemplateControl._jqMouseCaptureGlassBarrier.css('cursor', cssMouseCursor || '');
        SharpUI.TemplateControl._jqMouseCaptureGlassBarrier.show();
    },
    
    get_zIndex: function SharpUI_TemplateControl$get_zIndex() {
        var zIndex;
        try {
            zIndex = Math.round(parseFloat(this.get_rootElement().css('z-index')));
        }
        catch ($e1) {
            zIndex = 0;
        }
        return zIndex;
    },
    set_zIndex: function SharpUI_TemplateControl$set_zIndex(value) {
        this.get_rootElement().css('z-index', value.toString());
        return value;
    },
    
    get_layoutPosition: function SharpUI_TemplateControl$get_layoutPosition() {
        return this._layoutPosition;
    },
    set_layoutPosition: function SharpUI_TemplateControl$set_layoutPosition(value) {
        var cssValue;
        switch (value) {
            case SharpUI.Position.unspecified:
            default:
                cssValue = String.Empty;
                break;
            case SharpUI.Position.absolute:
                cssValue = 'absolute';
                break;
            case SharpUI.Position.relative:
                cssValue = 'relative';
                break;
            case SharpUI.Position.fixed:
                cssValue = 'fixed';
                break;
        }
        this.get_rootElement().css('position', cssValue);
        this._layoutPosition = value;
        return value;
    }
}


Type.registerNamespace('SharpUI.Examples');

Type.registerNamespace('Examples');

////////////////////////////////////////////////////////////////////////////////
// SharpUI.Examples._advancedLayout

SharpUI.Examples._advancedLayout = function SharpUI_Examples__advancedLayout() {
    SharpUI.Examples.AdvancedLayout.initializeBase(this);
    this.add_addedToDocument(ss.Delegate.create(this, this._onAddedToDocument$1));
    this.add_removedFromDocument(ss.Delegate.create(this, this._onRemovedFromDocument$1));
}
SharpUI.Examples._advancedLayout.prototype = {
    template: '\r\n\r\n<style>\r\n\r\n</style>\r\n\r\n<div>\r\n    <h2>Advanced Layout Example</h2>\r\n\r\n    <div style=\'width:500px;height:250px;\'>\r\n\t\t<div style=\'background-color:Red;\' class=\'advancedLayout\' al:horizontal-alignment=\'stretch\' al:vertical-alignment=\'stretch\'></div>\r\n\r\n\t\t<div style=\'color:white; background-color:blue;\' class=\'advancedLayout\' al:margin=\'16 16 16 16\' al:horizontal-alignment=\'right\' al:width=\'32\' al:vertical-alignment=\'bottom\' al:height=\'32\'>\r\n\t\t\tHello, World.\r\n\t\t</div>\r\n\t</div>\r\n</div>\r\n\r\n',
    
    _onAddedToDocument$1: function SharpUI_Examples_AdvancedLayout$_onAddedToDocument$1(sender, e) {
    },
    
    _onRemovedFromDocument$1: function SharpUI_Examples_AdvancedLayout$_onRemovedFromDocument$1(sender, e) {
    }
}


////////////////////////////////////////////////////////////////////////////////
// Examples.Application

Examples.Application = function Examples_Application(placeholderId) {
    var placeholder = $('#' + placeholderId);
    var f = new SharpUI.Examples._form();
    placeholder.append(f.get_rootElement());
}


////////////////////////////////////////////////////////////////////////////////
// SharpUI.Examples._networkStatus

SharpUI.Examples._networkStatus = function SharpUI_Examples__networkStatus() {
    SharpUI.Examples._networkStatus.initializeBase(this);
    this.add_addedToDocument(ss.Delegate.create(this, this._onAddedToDocument$1));
    this.add_removedFromDocument(ss.Delegate.create(this, this._onRemovedFromDocument$1));
}
SharpUI.Examples._networkStatus.prototype = {
    _imgAnimatedSpinner$1: null,
    _txtBytesTransferred$1: null,
    
    _onAddedToDocument$1: function SharpUI_Examples__networkStatus$_onAddedToDocument$1(sender, e) {
    },
    
    _onRemovedFromDocument$1: function SharpUI_Examples__networkStatus$_onRemovedFromDocument$1(sender, e) {
    },
    
    showNetworkActivity: function SharpUI_Examples__networkStatus$showNetworkActivity(bytesTransferred) {
        this._txtBytesTransferred$1.text(bytesTransferred.toString());
        this._imgAnimatedSpinner$1.fadeIn().fadeOut();
    },
    
    template: '\r\n\r\n<style>\r\n\t#this \r\n\t{\r\n\t\tposition:relative;\r\n\t\tmin-width:80px;\r\n\t\tmin-height:50px;\r\n\t}\r\n\t\t\r\n\t#imgAnimatedSpinner\r\n\t{\r\n\t\tdisplay:none;\r\n\t}\r\n\t\r\n\t#txtBytesTransferred\r\n\t{\r\n\t\tfont-size:10px;\r\n\t}\r\n\r\n</style>\r\n\r\n<div>\r\n\t<img xid=\'imgAnimatedSpinner\' src=\'/Images/ajax-loader.gif\' />\r\n\t<span xid=\'txtBytesTransferred\'></span>\r\n</div>\r\n\r\n'
}


////////////////////////////////////////////////////////////////////////////////
// SharpUI.Examples._form

SharpUI.Examples._form = function SharpUI_Examples__form() {
    SharpUI.Examples._form.initializeBase(this);
    this.add_addedToDocument(ss.Delegate.create(this, this._onAddedToDocument$1));
    this.add_removedFromDocument(ss.Delegate.create(this, this._onRemovedFromDocument$1));
    this._btnGo$1.click(ss.Delegate.create(this, this._onBtnClickGo$1));
    this._btnCancel$1.click(ss.Delegate.create(this, this._onBtnClickCancel$1));
}
SharpUI.Examples._form.prototype = {
    _btnGo$1: null,
    _btnCancel$1: null,
    _networkStatus$1: null,
    
    _onAddedToDocument$1: function SharpUI_Examples__form$_onAddedToDocument$1(sender, e) {
    },
    
    _onRemovedFromDocument$1: function SharpUI_Examples__form$_onRemovedFromDocument$1(sender, e) {
    },
    
    _onBtnClickGo$1: function SharpUI_Examples__form$_onBtnClickGo$1(e) {
        this._networkStatus$1.showNetworkActivity(768);
    },
    
    _onBtnClickCancel$1: function SharpUI_Examples__form$_onBtnClickCancel$1(e) {
        this._networkStatus$1.showNetworkActivity(0);
    },
    
    template: '\r\n\r\n<!--\r\n\t::Example Notes::\r\n\tStyle blocks are automatically rewritten and reused\r\n\tacross multiple instances of the control. Any rules\r\n\tfeaturing \'#this\' are rewritten to refer\r\n\tto the root div of the control. Rules featuring #someId\r\n\tare rewritten and assumed to point to a locally named control\r\n\tor element with corresponding xid attribute.\r\n-->\r\n\r\n<style>\r\n\t#this \r\n\t{\r\n\t\tposition:relative;\r\n\t\twidth:400px;\r\n\t\theight:250px;\r\n\t}\r\n\t\r\n\t#this h2\r\n\t{\r\n\t\tfont-family:Arial;\r\n\t}\r\n\t\r\n\t#this .normalText\r\n\t{\r\n\t\tfont-family:Courier New;\r\n\t\tmargin:10px 10px 10px 10px;\r\n\t}\r\n\t\r\n\t#networkStatus\r\n\t{\r\n\t\tfloat:right;\r\n\t\twidth:150px;\r\n\t\theight:80px;\r\n\t}\r\n\r\n</style>\r\n\r\n<div>\r\n\t<!--\r\n\t\t::Example Notes::\r\n\t\tElements and controls may have local identifiers via\r\n\t\tthe \'xid\' attribute. Any such elements will be\r\n\t\tautomatically bound to correspondingly named\r\n\t\tfields in the class, and will have a unique (to the document)\r\n\t\tid written for it, avoiding any global id clashes.\r\n\t-->\r\n\t<h2 xid=\'heading\'>Sharp UI Example</h2>\r\n\t\r\n\t<!--\r\n\t\t::Example Notes::\r\n\t\tIf you wish to declare an instance of another control, \r\n\t\tsimply declare a div element, give it an optional xid,\r\n\t\tand specify the desired control\'s type name via the\r\n\t\t\'control\' attribute.\r\n\t-->\r\n\t<div xid=\'networkStatus\' control=\'NetworkStatus\'></div>\r\n\r\n\t<div class=\'normalText\'>\r\n\t\tLorem Ipsum Lorem Ipsum.\r\n\t</div>\r\n\r\n\t<input xid=\'btnGo\' type=\'button\' value=\'Go\' />\r\n\t<input xid=\'btnCancel\' type=\'button\' value=\'Cancel\' />\r\n</div>\r\n\r\n'
}


SharpUI.Thickness.registerClass('SharpUI.Thickness');
SharpUI.AdvancedLayout.registerClass('SharpUI.AdvancedLayout');
SharpUI.TemplateControl.registerClass('SharpUI.TemplateControl');
SharpUI.Examples._advancedLayout.registerClass('SharpUI.Examples._advancedLayout', SharpUI.TemplateControl);
Examples.Application.registerClass('Examples.Application');
SharpUI.Examples._networkStatus.registerClass('SharpUI.Examples._networkStatus', SharpUI.TemplateControl);
SharpUI.Examples._form.registerClass('SharpUI.Examples._form', SharpUI.TemplateControl);
SharpUI.AdvancedLayout.cssClassNameAdvancedLayout = 'advancedLayout';
SharpUI.AdvancedLayout._layoutEnforcementInterval = 500;
SharpUI.AdvancedLayout.attributeNamePrefix = 'al:';
SharpUI.AdvancedLayout._layoutEnforcementTimerId = 0;
SharpUI.AdvancedLayout._frameDetector = null;
(function () {
    SharpUI.AdvancedLayout._initLayoutEnforcement();
})();
SharpUI.TemplateControl._attributeNameLocalId = 'xid';
SharpUI.TemplateControl._attributeNameControlClass = 'control';
SharpUI.TemplateControl._dataNameControl = 'templateControl';
SharpUI.TemplateControl.cssClassNameControl = 'templateControl';
SharpUI.TemplateControl.cssClassNameControlUnadded = 'templateControlUnadded';
SharpUI.TemplateControl._idPrefixAutoRewrite = 'auto_';
SharpUI.TemplateControl._cssClassNamePrefixAutoRewrite = 'css_auto_';
SharpUI.TemplateControl._bStaticConstructionFinished = false;
SharpUI.TemplateControl._hash_templateCache = {};
SharpUI.TemplateControl._hash_processedCss = {};
SharpUI.TemplateControl._documentTreeCheckInterval = 200;
SharpUI.TemplateControl._iCheckParentIntervalId = 0;
SharpUI.TemplateControl._hash_strControlIdsKnownInDocument = {};
SharpUI.TemplateControl._hash_cachedTypeNameResolves = {};
SharpUI.TemplateControl._iAutoIdGeneratorCounter = 0;
SharpUI.TemplateControl._iInstanceIdGeneratorCounter = 1;
SharpUI.TemplateControl._hash_controlFieldMappingByControl = {};
SharpUI.TemplateControl._mouseCaptureHandler = null;
SharpUI.TemplateControl._jqMouseCaptureGlassBarrier = null;
})(jQuery);

//! This script was generated using Script# v0.7.0.0
