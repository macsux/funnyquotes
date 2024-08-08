using System;
using System.Web.UI;
using FunnyQuotesCommon;

namespace FunnyQuotesUIForms
{
    public partial class Quotes : Page
    {
        public IFunnyQuoteService FunnyQuoteService { get; set; } = null!;


        protected void Page_Load(object sender, EventArgs e)
        {
            lblCookieProvider.Text = FunnyQuoteService.GetType().ToString();
        }

        protected async void btnGetCookie_OnClick(object sender, EventArgs e)
        {
            lblCookie.Text = await FunnyQuoteService.GetQuoteAsync();
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