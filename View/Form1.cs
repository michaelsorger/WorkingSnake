using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnakeGame;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace View
{
    public partial class Form1 : Form
    {
        World ourWorld;
        public Form1()
        {
            InitializeComponent();
            drawingPanel1.SetWorld(ourWorld);

        }

        /// <summary>
        /// Event to try to connect to server and start call back functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if(serverAddress.Text == "")
            {
                MessageBox.Show("Please put in a server address/URL");
                return;
            }
            ConnectButton.Enabled = false;
            serverAddress.Enabled = false;
            //Need to fix case where address doesn't work.

            NetworkController.Network.ConnectToServer(FirstContact, serverAddress.Text);
            this.Focus();
        }
        private bool EnterNameIsClicked = false;
        private bool AbleToSendDirections = false;
        private NetworkController.SocketState stateToDealWithMyIssues = new NetworkController.SocketState();
        private Dictionary<string, int> namesAndScores = new Dictionary<string, int>();
        private HashSet<string> namesAlreadyWritten = new HashSet<string>();
        private int labelNumber = 0;
        private Dictionary<string, Label> labelDict = new Dictionary<string, Label>();

        private void FirstContact(NetworkController.SocketState state)
        {
            state.callMe = ReceiveStartup;
            this.Invoke(new MethodInvoker(() => this.nameBox.Enabled = true));
            
            this.Invoke(new MethodInvoker(() => this.EnterName.Enabled = true));
            stateToDealWithMyIssues = state;
            while (!EnterNameIsClicked)
            {

            }
            if (nameBox.Text == "")
            {
                MessageBox.Show("Please enter a name of more than one character or digit");
                EnterNameIsClicked = false;
                FirstContact(state);
            }
            else
            {
                NetworkController.Network.Send(state, nameBox.Text + "\n");
            }
        }
        private void ReceiveStartup(NetworkController.SocketState state)
        {
            this.Invoke(new MethodInvoker(() => this.nameBox.Enabled = false));

            this.Invoke(new MethodInvoker(() => this.EnterName.Enabled = false));
            state.callMe = ReceiveWorld;

            stateToDealWithMyIssues = state;
            AbleToSendDirections = true;
            this.Invoke(new MethodInvoker(() => this.Focus()));

            int startupCase = 0;
            int worldStartWidth = 0;
            int worldStartHeight = 0;
            //Code to extract initial ourWorld data
            string totalData = state.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;
                //only happens if we need to deal with \n
                switch (startupCase)
                {
                    case 0:
                        Snake ourSnake = new Snake(p);
                        startupCase++;
                        break;
                    case 1:
                        Int32.TryParse(p, out worldStartWidth);
                        startupCase++;
                        break;
                    case 2:
                        Int32.TryParse(p, out worldStartHeight);
                        startupCase++;
                        break;
                    default:
                        break;
                }
                //method invoker?
                state.sb.Remove(0, p.Length);
            }
            ourWorld = new World(worldStartWidth, worldStartHeight);
            this.Invoke((MethodInvoker)delegate
            {
                Size = new System.Drawing.Size(worldStartWidth * ourWorld.pixelsPerCell + 200, worldStartHeight * ourWorld.pixelsPerCell + 100);
                drawingPanel1.SetWorld(ourWorld);
                // Set the size of the drawing panel to match the ourWorld
                drawingPanel1.Width = ourWorld.width * ourWorld.pixelsPerCell;
                drawingPanel1.Height = ourWorld.height * ourWorld.pixelsPerCell;
                drawingPanel1.Invalidate();
            });
            state.callMe = ReceiveWorld;
            stateToDealWithMyIssues = state;
            NetworkController.Network.GetData(state);
        }
        private void ReceiveWorld(NetworkController.SocketState state)
        {
            stateToDealWithMyIssues = state;

            //Parsing JSON stuff and updating ourWorld
            string totalData = state.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                //deserialize
                //some random ip 128.110.21.208
                //check to see if parts is a snake or food Json string
                // Parser the JSON string so we can examine it to determine what type of object it represents.
                JObject obj = JObject.Parse(p);
                JToken snakeProp = obj["vertices"];
                JToken foodProp = obj["loc"];
                JToken ID = obj["ID"];

                if (snakeProp != null)
                {
                    Snake s = JsonConvert.DeserializeObject<Snake>(p);
                    namesAndScores[s.getName()] = s.snakeScore(s);
                    if (!ourWorld.snakeDict.ContainsKey(s.getID()))
                    {
                        Label label = new Label();
                        this.Invoke((MethodInvoker)delegate
                        {
                            label.Name = "label" + labelNumber;
                            label.Location = new System.Drawing.Point(ourWorld.width * ourWorld.pixelsPerCell + 20, 100 + labelNumber * 22);
                            label.Size = new System.Drawing.Size(180, 20);
                            label.Parent = this;
                            label.ForeColor = s.getColor(); 
                            label.Text = s.getName() + ": " + s.snakeScore(s);
                            s.myLabelNumber = labelNumber;
                            labelNumber++;
                            labelDict[label.Name] = label;
                            this.Controls.Add(label);
                        });
                    }
                    else
                    {
                        this.Invoke(new MethodInvoker(() => labelDict["label" + s.myLabelNumber].ForeColor = s.getColor()));
                        this.Invoke(new MethodInvoker(() => labelDict["label" + s.myLabelNumber].Text = s.getName() + ": " + s.snakeScore(s)));
                    }

                    lock (ourWorld.snakeDict)
                    {
                        ourWorld.snakeDict[s.getID()] = s;
                        if (s.SnakeIsDead(s))
                        {
                            ourWorld.snakeDict.Remove(s.getID());
                            this.Invoke(new MethodInvoker(() => labelDict["label" + s.myLabelNumber].Text = s.getName() + ": 0"));
                        }                       
                    }

                }

                if (foodProp != null)
                {
                    Food f = JsonConvert.DeserializeObject<Food>(p);

                    lock (ourWorld.foodDict)
                    {
                        ourWorld.foodDict[f.getID()] = f;
                        if (f.FoodIsDead(f))
                        {
                            ourWorld.foodDict.Remove(f.getID());
                        }
                    }
                }
                state.sb.Remove(0, p.Length);
            }
            try
            {
                this.Invoke(new MethodInvoker(() => drawingPanel1.Invalidate()));                
                
            }
            catch(Exception e)
            {
                //form should end now
            }
                        
            NetworkController.Network.GetData(state);
        }
        private void nameBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void EnterName_Click(object sender, EventArgs e)
        {
            EnterNameIsClicked = true;
            this.Focus();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!AbleToSendDirections)
            {
                return;
            }

            if (e.KeyChar == 'w' || e.KeyChar <= 'a' || e.KeyChar == 's' || e.KeyChar == 'd')
            {
                //1: UP, 2: RIGHT, 3: DOWN, 4: LEFT.
                switch (e.KeyChar)
                {

                    case 'w': //w  = 1  
                        NetworkController.Network.Send(stateToDealWithMyIssues, "(1)");
                        break;                   
                    case 'a': //a  = 4
                        NetworkController.Network.Send(stateToDealWithMyIssues, "(4)");
                        break;
                    case 's': //s = 3
                        NetworkController.Network.Send(stateToDealWithMyIssues, "(3)");
                        break;
                    case 'd': //d = 2
                        NetworkController.Network.Send(stateToDealWithMyIssues, "(2)");
                        break;
                }
            }
        }
    }
}
