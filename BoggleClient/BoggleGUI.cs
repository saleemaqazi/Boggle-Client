using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoggleClient
{
    public partial class BoggleGUI : Form
    {
        /// <summary>
        /// Constructor for the BoggleGUI
        /// </summary>
        public BoggleGUI()
        {
            InitializeComponent();
        }


        public event Action RefreshEvent;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// This method locks some buttons to prevent bugs.
        /// </summary>
        public void LockButtons()
        {
            button1.Enabled = false;
            button3.Enabled = false;
            button5.Enabled = false;
        }

        /// <summary>
        /// This method updates the timer on the GUI.
        /// </summary>
        /// <param name="TimeLeft"></param>
        public void UpdateTimer(int TimeLeft)
        {
            label7.Text = "Time Remaining: " + TimeLeft;
        }

        public Timer timer;

        /// <summary>
        /// This method initializes the timer on the GUI.
        /// </summary>
        public void StartRefreshTimer()
        {
            timer = new Timer();
            timer.Interval = (1000); // 
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        /// <summary>
        /// This method calls sends a RefreshEvent to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshEvent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public event Action<string, string> RegisterEvent;

        /// <summary>
        /// This method events a RegisterEvent to the controller
        /// when a user wants to register.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            RegisterEvent(textBox1.Text, textBox2.Text);
        }

        public event Action<String> RequestEvent;

        /// <summary>
        /// This method sends a RequestEvent to the controller
        /// when a user requests a game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            
            RequestEvent(textBox3.Text);
            
        }

        /// <summary>
        /// This method sends a SubmitWordEvent to the controller
        /// when the user wishes to submit a word for scoring.
        /// </summary>
        public event Action<string> SubmitWordEvent;
        private void button6_Click(object sender, EventArgs e)
        {
            SubmitWordEvent(textBox5.Text);
            textBox5.Text = "";
        }

        /// <summary>
        /// This method initializes the board with the letters
        /// from the server.
        /// </summary>
        /// <param name="board"></param>
        public void DisplayBoard(string board)
        {
            button7.Enabled = false;
            button4.Enabled = true;
            char[] Letters = board.ToArray();
            richTextBox1.Text = Letters[0].ToString();
            richTextBox2.Text = Letters[1].ToString();
            richTextBox3.Text = Letters[2].ToString();
            richTextBox4.Text = Letters[3].ToString();
            richTextBox5.Text = Letters[4].ToString();
            richTextBox6.Text = Letters[5].ToString();
            richTextBox7.Text = Letters[6].ToString();
            richTextBox8.Text = Letters[7].ToString();
            richTextBox9.Text = Letters[8].ToString();
            richTextBox10.Text = Letters[9].ToString();
            richTextBox11.Text = Letters[10].ToString();
            richTextBox12.Text = Letters[11].ToString();
            richTextBox13.Text = Letters[12].ToString();
            richTextBox14.Text = Letters[13].ToString();
            richTextBox15.Text = Letters[14].ToString();
            richTextBox16.Text = Letters[15].ToString();
        }

        /// <summary>
        /// This method clears the board at the end of a game.
        /// </summary>
        public void ClearBoard()
        {
            button1.Enabled = true;
            button3.Enabled = true;
            button5.Enabled = true;
            button7.Enabled = true;
            button4.Enabled = false;

            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            richTextBox4.Text = "";
            richTextBox5.Text = "";
            richTextBox6.Text = "";
            richTextBox7.Text = "";
            richTextBox8.Text = "";
            richTextBox9.Text = "";
            richTextBox10.Text = "";
            richTextBox11.Text = "";
            richTextBox12.Text = "";
            richTextBox13.Text = "";
            richTextBox14.Text = "";
            richTextBox15.Text = "";
            richTextBox16.Text = "";

            UpdateOpponentName("");
            UpdatePlayerName("");
            label8.Text = "";
            label9.Text = "";
            label7.Text = "";
        }

        /// <summary>
        /// This method displays the players name on the board.
        /// </summary>
        /// <param name="name"></param>
        public void UpdatePlayerName(string name)
        {
            label5.Text = name;
        }

        /// <summary>
        /// This method displays the opponents name on the board.
        /// </summary>
        /// <param name="name"></param>
        public void UpdateOpponentName(string name)
        {
            label6.Text = name;
        }

        /// <summary>
        /// This method displays and updates the players score on the GUI.
        /// </summary>
        /// <param name="score"></param>
        public void UpdatePlayerScore(int score)
        {
            label8.Text = "score: " + score.ToString();
        }

        /// <summary>
        /// This method displays and updates the opponents score on the GUI.
        /// </summary>
        /// <param name="score"></param>
        public void UpdateOpponentScore(int score)
        {
            label9.Text = "score: " + score.ToString();
        }

        /// <summary>
        /// This method unlocks some buttons.
        /// </summary>
        public void EnableRequest()
        {
            textBox3.Visible = true;
            label3.Visible = true;
        }

        /// <summary>
        /// This method makes some buttons visible when necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button5.Visible = true;
            button7.Visible = true;
            
        }

        public event Action CancelRequest;
        /// <summary>
        /// This method sends a CancelRequest event to the controller
        /// when a user wants to cancel registration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            CancelRequest();
        }

        /// <summary>
        /// This method handles exiting the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            ClearBoard();
            if(timer != null)
            {
                timer.Stop();
            }
            
            
        }

        
        public event Action CancelPendingGame;
        /// <summary>
        /// This method handles cancelling a pending game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            if(timer != null)
            {
                timer.Stop();
            }

            button1.Enabled = true;
            button3.Enabled = true;
            button5.Enabled = true;

            CancelPendingGame();

        }

        /// <summary>
        /// This method handles displaying the help information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To register as a player:\n" +
                "Enter the server domain name (you do not need to include " +
                "\"BoggleService\\\") and a user name, then press \"register user\"\n" +
                "Once you are registered, a textbox will appear for you to specify\n" +
                " a game length. The registration request can be cancelled at any\n" +
                "point before this textbox appears by pressing \"cancel registration\"\n\n" +
                "To Start a game:\n" +
                "Enter the preffered game length and press \"request game\"\n" +
                "This request can be canceled before the board is filled by pressing \n" +
                "\"cancel request\" (you may have to hit this button twice the very\n" +
                "first time in order to re-enable the registration buttons...idk sorryyy)\n\n" +
                "During a game:\n" +
                "Play a word by entering it in the textbox under the board and\n" +
                "pressing \"submit\". Or exit the current game by pressing \"exit game\"\n\n" +
                "Once a game has finished:\n" +
                "You can request another game or register with a new server or name");
        }
    }
}
