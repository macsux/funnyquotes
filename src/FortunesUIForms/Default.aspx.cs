using System;
using System.Web.UI;

namespace FortunesUIForms
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("Cookies.aspx");
        }
    }
}