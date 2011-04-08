using System;
using System.Collections;
using System.Html;
using System.Runtime.CompilerServices;
using jQueryApi;
using SharpUI;

namespace SharpUI.Examples
{
    [PreserveName]
    internal partial class Form : TemplateControl
    {
        /*
         * ::Example Notes::
         * Elements in markup that have "xid" attributes
         * are automatically added by reference
         * to correspondingly named fields. Underscores
         * are optional in the automatic mapping, but
         * the PreserveName attribute is necessary
         * to prevent obfuscation from interfering.
         * */
        #region Elements
        [PreserveName]
        jQueryObject _btnGo = null;

        [PreserveName]
        jQueryObject _btnCancel = null;

        [PreserveName]
        NetworkStatus _networkStatus = null;
        #endregion

        #region Construction
        public Form()
        {
            this.AddedToDocument += new EventHandler(OnAddedToDocument);
            this.RemovedFromDocument += new EventHandler(OnRemovedFromDocument);
            this._btnGo.Click(OnBtnClickGo);
            this._btnCancel.Click(OnBtnClickCancel);
        }
        #endregion

        #region Event Handlers
        private void OnAddedToDocument(object sender, EventArgs e)
        {
        
        }

        private void OnRemovedFromDocument(object sender, EventArgs e)
        {
            
        }

        private void OnBtnClickGo(jQueryEvent e)
        {
            _networkStatus.ShowNetworkActivity(768);
        }

        private void OnBtnClickCancel(jQueryEvent e)
        {
            _networkStatus.ShowNetworkActivity(0);
        }
        #endregion
    }
}
