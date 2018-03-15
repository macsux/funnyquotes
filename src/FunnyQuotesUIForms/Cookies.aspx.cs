using System;
using System.Web.UI;
using FunnyQuotesCommon;

namespace FunnyQuotesUIForms
{
    public partial class Cookies : Page
    {
        public IFunnyQuoteservice FunnyQuoteservice { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            lblCookieProvider.Text = FunnyQuoteservice.GetType().ToString();
        }

        protected async void btnGetCookie_OnClick(object sender, EventArgs e)
        {
            lblCookie.Text = await FunnyQuoteservice.GetCookieAsync();
        }

        protected void btnKill_OnClick(object sender, EventArgs e)
        {
            try
            {
                throw new DivideByZeroException();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                Environment.Exit(-1);
            }
        }
    }
}