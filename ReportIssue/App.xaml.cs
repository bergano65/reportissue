using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Identity.Client;


namespace ReportIssue
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Below are the clientId (Application Id) of your app registration and the tenant information.
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - the content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use `organizations`
        //   - for any Work or School accounts, or Microsoft personal account, use `common`
        //   - for Microsoft Personal account, use consumers
        private static string ClientId = "20047d37-a1d7-466b-9e2a-ece6fe5b4d0a";
        private static string Password = "14mqem2ISQ0.E.E6-xV6p-YtHluR-FDn4R";
        private static string Tenant = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        private static IPublicClientApplication _clientApp;

        private static string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        //Set the scope for API call to user.read
        private static string[] scopes = new string[] { "user.read"};

        public static IPublicClientApplication PublicClientApp
        {
            get
            { return _clientApp;
            }
        }

        static App()
        {
            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                                    .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
                                    .WithDefaultRedirectUri()
                                    .Build();
            Login();


        }

        private static void Login()
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;
            Task<IEnumerable<IAccount>> accTsk = app.GetAccountsAsync();
            accTsk.Wait();
            var accounts = accTsk.Result;
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                Task<AuthenticationResult> authResultTsk = app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync();
                authResultTsk.Wait();
                authResult = authResultTsk.Result;

            }
            catch (Exception ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    Task<AuthenticationResult> authResultTsk = app.AcquireTokenInteractive(scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();

                    authResultTsk.Wait();
                    authResult = authResultTsk.Result;
                }
                catch (Exception ex1)
                {
                }
           } // end of catch   exception getting interactively

            if (authResult != null)
            {
            }
        }
    }
}
