using System;
using System.Collections;
using System.Html;
using System.Runtime.CompilerServices;
using jQueryApi;
using SharpUI;

namespace SharpUI.Examples
{
    [PreserveName]
    internal partial class NetworkStatus : TemplateControl
    {
        [PreserveName]
        jQueryObject _imgAnimatedSpinner = null;
        [PreserveName]
        jQueryObject _txtBytesTransferred = null;

        public NetworkStatus()
        {
            this.AddedToDocument += new EventHandler(OnAddedToDocument);
            this.RemovedFromDocument += new EventHandler(OnRemovedFromDocument);
        }

        private void OnAddedToDocument(object sender, EventArgs e) { }

        private void OnRemovedFromDocument(object sender, EventArgs e) { }

        /// <summary>
        /// Animate the spinner image, and update the bytes transferred.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        public void ShowNetworkActivity(int bytesTransferred)
        {
            _txtBytesTransferred.Text(bytesTransferred.ToString());
            _imgAnimatedSpinner.FadeIn().FadeOut();
        }
    }
}
