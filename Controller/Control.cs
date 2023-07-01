using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoggleClient;
using System.Windows.Forms;

namespace Controller
{
    class Control
    {

        private BoggleGUI window;

        private String PlayerName;

        private String OpponentName;

        private String UserToken;

        private int PlayerScore;

        private int OpponentScore;

        private int TimeLeft;

        private CancellationTokenSource tokenSource;


        public Control(BoggleGUI window)
        {
            this.window = window;
            window.RegisterEvent += RegisterEventHandler;
            window.RequestEvent += RequestEventHandler;
            window.RefreshEvent += RefreshEventHandler;
        }


        private async void RefreshEventHandler()
        {
            
        }

        private async void RequestEventHandler(string TimeLeft, string OponentName)
        {

        }

        private async void RegisterEventHandler(string url, string name)
        {
            try
            {
                using (HttpClient client = CreateClient(url))
                {
                    

                    // Compose and send the request.
                    tokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(name), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("RegisterUser", content, tokenSource.Token);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        UserToken = (string)JsonConvert.DeserializeObject(result);
                    }
                    else
                    {
                        MessageBox.Show("Error registering: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                
            }
        }


        private static HttpClient CreateClient(string url)
        {
            // Create a client whose base address is the GitHub server
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            // Tell the server that the client will accept this particular type of response data
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            // There is more client configuration to do, depending on the request.
            return client;
        }
    }
}
