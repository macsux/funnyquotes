using System;
using System.Web.UI;
using FortunesCommon;

namespace FortunesUIForms
{
    public partial class Cookies : Page
    {
        public IFortuneService FortuneService { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            lblCookieProvider.Text = FortuneService.GetType().ToString();
        }

        protected async void btnGetCookie_OnClick(object sender, EventArgs e)
        {
            lblCookie.Text = await FortuneService.GetCookieAsync();
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