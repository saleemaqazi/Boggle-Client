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

namespace BoggleClient
{
    class Control
    {

        private BoggleGUI window;

        private String UserToken;

        private string url;

        private CancellationTokenSource tokenSource;

        private string GameID;
        

        /// <summary>
        /// This is the Control constructor method.
        /// This hooks to the events, and connects them with the proper handlers.
        /// </summary>
        /// <param name="window"></param>
        public Control(BoggleGUI window)
        {
            this.window = window;
            window.RegisterEvent += RegisterEventHandler;
            window.RequestEvent += RequestEventHandler;
            window.RefreshEvent += RefreshEventHandler;
            window.SubmitWordEvent += SubmitEventHandler;
            window.CancelRequest += CancelRequestHandler;
            window.CancelPendingGame += CancelPendingHandler;
        
        }

        /// <summary>
        /// This method handles the event where a user wants to cancel a join game request.
        /// </summary>
        private async void CancelPendingHandler()
        {
            tokenSource.Cancel();

            using (HttpClient client = CreateClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(UserToken), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync("games", content);
            }

            
            GameID = null;
        }

        /// <summary>
        /// This method handles the event where a user wants to cancel registering.
        /// </summary>
        private async void CancelRequestHandler()
        {
            using (HttpClient client = CreateClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(UserToken), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync("games/", content);

                if (!response.IsSuccessStatusCode)
                {
                    tokenSource.Cancel();
                }
                else
                {
                    UserToken = null;
                }
                

            }

            GameID = null;
            
        }

        /// <summary>
        /// This method handles the event where a user wants to submit a word for scoring.
        /// </summary>
        /// <param name="word"></param>
        private async void SubmitEventHandler(string word)
        {
            using (HttpClient client = CreateClient())
            {
                // Compose and send the request.
                dynamic BodyInfo = new ExpandoObject();
                BodyInfo.UserToken = UserToken;
                BodyInfo.Word = word;
                StringContent content = new StringContent(JsonConvert.SerializeObject(BodyInfo), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("games/" + GameID, content);
            }
        }

        /// <summary>
        /// This method refreshes the boggle board frequently to display the proper
        /// score and time left in the game.
        /// </summary>
        private async void RefreshEventHandler()
        {
                using (HttpClient client = CreateClient())
                {
                
                    HttpResponseMessage response = await client.GetAsync("games/" + GameID + "/true");

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        dynamic GameInfo = JsonConvert.DeserializeObject(result);

                        if (GameInfo.GameState == "active")
                        {
                            window.UpdatePlayerScore((int)GameInfo.Player1.Score);
                            window.UpdateOpponentScore((int)GameInfo.Player2.Score);
                            window.UpdateTimer((int)GameInfo.TimeLeft);
                        }
                        if(GameInfo.GameState == "completed")
                        {
                            window.timer.Stop();
                            DisplayEndScreen();
                        }
                    }

                    else
                    {
                        MessageBox.Show("Error registering: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
        }


        /// <summary>
        /// This method displays the end of game screen with the score and word information.
        /// </summary>
        private async void DisplayEndScreen()
        {
            using (HttpClient client = CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("games/" + GameID + "/false");
                if (response.IsSuccessStatusCode)
                {
                    String result = await response.Content.ReadAsStringAsync();
                    dynamic GameInfo = JsonConvert.DeserializeObject(result);
                    string playerName = GameInfo.Player1.Nickname;
                    string OpponentName = GameInfo.Player2.Nickname;

                    int playerScore = GameInfo.Player1.Score;
                    int opponentScore = GameInfo.Player2.Score;

                    List<string> playerWords = new List<string>();
                    List<string> playerWords2 = new List<string>();

                    foreach(var word in GameInfo.Player1.WordsPlayed)
                    {
                        playerWords.Add((string)word.Word.ToString());
                    }

                    foreach (var word in GameInfo.Player2.WordsPlayed)
                    {
                        playerWords2.Add((string)word.Word.ToString());
                    }

                    var message = string.Join(Environment.NewLine, playerWords);
                    var message2 = string.Join(Environment.NewLine, playerWords2);

                    DialogResult Result = MessageBox.Show(playerName + " : " + playerScore + "\n" + message + "\n\n"
                        + OpponentName + " : " + opponentScore + "\n" + message2, "game over", MessageBoxButtons.OK);

                    if(Result == DialogResult.OK)
                    {
                        window.ClearBoard();
                    }

                    
                    
                }

                else
                {
                    MessageBox.Show("SERVER DOWN: " + response.StatusCode + "\n" + response.ReasonPhrase);
                }

            }
        }


        /// <summary>
        /// This method handles the event where a user wishes to request a game to join.
        /// </summary>
        /// <param name="TimeLeft"></param>
        private async void RequestEventHandler(string TimeLeft)
        {
            try
            {
                using (HttpClient client = CreateClient())
                {
                    // Compose and send the request.
                    tokenSource = new CancellationTokenSource();
                    dynamic request = new ExpandoObject();
                    request.UserToken = UserToken;
                    request.TimeLimit = TimeLeft;

                    StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("games", content, tokenSource.Token);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        window.LockButtons();
                        String result = await response.Content.ReadAsStringAsync();
                        dynamic GameInfo = JsonConvert.DeserializeObject(result);
                        GameID = GameInfo.GameID;

                        GetGameStatus(tokenSource);
                    }
                    else
                    {
                        MessageBox.Show("Error requesting game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // dunno
            }
        }

        /// <summary>
        /// This method checks the game status to determine if the user is still
        /// waiting to join a game, or if the game has started.
        /// </summary>
        /// <param name="tokenSource"></param>
        public async void GetGameStatus(CancellationTokenSource tokenSource)
        {
            try
            {
                bool IsPending = true;
                while (IsPending)
                {


                    using (HttpClient client = CreateClient())
                    {
                        HttpResponseMessage response = await client.GetAsync("games/" + GameID + "/true", tokenSource.Token);

                        // Deal with the response
                        if (response.IsSuccessStatusCode)
                        {
                            String result = await response.Content.ReadAsStringAsync();
                            dynamic GameInfo = JsonConvert.DeserializeObject(result);
                            IsPending = (GameInfo.GameState == "pending");
                        }
                        else
                        {
                            MessageBox.Show("get game status: " + response.StatusCode + "\n" + response.ReasonPhrase);
                        }
                    }
                }

                UpdateBoard();

            }


            catch (TaskCanceledException)
            {

            }
        }

        /// <summary>
        /// This method updates the board with relevant information.
        /// </summary>
        public async void UpdateBoard()
        {
            try
            {
                using (HttpClient client = CreateClient())
                {


                    // Compose and send the request.
                    tokenSource = new CancellationTokenSource();
                    HttpResponseMessage response = await client.GetAsync("games/" + GameID + "/false", tokenSource.Token);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        dynamic GameInfo = JsonConvert.DeserializeObject(result);

                        window.DisplayBoard((string)GameInfo.Board);
                        window.UpdatePlayerName((string)GameInfo.Player1.Nickname);
                        window.UpdateOpponentName((string)GameInfo.Player2.Nickname);
                        window.UpdatePlayerScore((int)GameInfo.Player1.Score);
                        window.UpdateOpponentScore((int)GameInfo.Player2.Score);
                        window.UpdateTimer((int)GameInfo.TimeLeft);
                        window.StartRefreshTimer();
                    }

                    else
                    {
                        MessageBox.Show("update board: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // dunno
            }
        }

        /// <summary>
        /// This method handles the event where a user wishes to register a new nickname.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        private async void RegisterEventHandler(string url, string name)
        {
            try
            {
                this.url = url;
                using (HttpClient client = CreateClient())
                {

                    
                    // Compose and send the request.
                    tokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(name), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("users", content, tokenSource.Token);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        UserToken = (string)JsonConvert.DeserializeObject(result);
                        window.EnableRequest();
                    }
                    else
                    {
                        MessageBox.Show("Error registering: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch
            {
                // dunno
            }
            
        }

        /// <summary>
        /// This method creates the client that the other methods use to 
        /// communicate with the server.
        /// </summary>
        /// <returns></returns>
        private HttpClient CreateClient()
        {
            try
            {
                // Create a client whose base address is the GitHub server
                HttpClient client = new HttpClient();

                //shortcut for us to get rid of b4 submission
                if (url == "1") client.BaseAddress = new Uri("http://ice.eng.utah.edu/BoggleService/");
                else client.BaseAddress = new Uri(url + "/BoggleService/");



                // Tell the server that the client will accept this particular type of response data
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                // There is more client configuration to do, depending on the request.
                return client;
            }
            catch
            {
                MessageBox.Show("Error registering");
                tokenSource.Cancel();
                return null;

            }
        }
    }
}
