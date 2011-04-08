using System;
using System.Collections;
using jQueryApi;

namespace SharpUI.Examples
{
    public class Application
    {
        public Application(string placeholderId)
        {
            jQueryObject placeholder = jQuery.Select("#" + placeholderId);
            Form f = new Form();
            placeholder.Append(f.RootElement);
        }
    }
}
