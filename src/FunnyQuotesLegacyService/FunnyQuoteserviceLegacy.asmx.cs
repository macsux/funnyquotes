using System;
using System.ComponentModel;
using System.Data;
using System.Web.Services;
using FunnyQuotesCookieDatabase;
using MySql.Data.MySqlClient;

namespace FunnyQuotesLegacyService
{
    /// <summary>
    ///     Summary description for FunnyQuoteserviceLegacy
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class FunnyQuoteserviceLegacy : WebService
    {
        [WebMethod]
        public string GetCookie()
        {
//            return new LocalFunnyQuoteService().GetQuote();
            var connection = Global.DatabaseFactory() as MySqlConnection;

            var adapter = new MySqlDataAdapter("select * from FunnyQuotes", connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            var randomCookieIndex = new Random().Next(0, dataTable.Rows.Count - 1);
            var cookie = (string) dataTable.Rows[randomCookieIndex][nameof(FunnyQuote.Quote)];
            return cookie;
        }
    }
}