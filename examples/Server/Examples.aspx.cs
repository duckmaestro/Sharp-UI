using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Server
{
    public partial class Examples : System.Web.UI.Page
    {
        protected bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}