using System;
using System.Collections;
using System.Html;
using System.Runtime.CompilerServices;
using jQueryApi;
using SharpUI;

namespace SharpUI.Examples
{
    [PreserveName]
    internal partial class AdvancedLayout : TemplateControl
    {
        public AdvancedLayout()
        {
            this.AddedToDocument += new EventHandler(OnAddedToDocument);
            this.RemovedFromDocument += new EventHandler(OnRemovedFromDocument);
        }

        private void OnAddedToDocument(object sender, EventArgs e) { }

        private void OnRemovedFromDocument(object sender, EventArgs e) { }
    }
}
