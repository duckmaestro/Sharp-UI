using System;
using System.Collections;
using System.Runtime.CompilerServices;
using jQueryApi;
 
namespace SharpUI.Examples
{
    [ScriptNamespace("Examples")]
    [ScriptName("Application")]
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
