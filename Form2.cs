using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Dyscord.Properties;
using System.Xml.Linq;

// Class: SettingsForm
// Author: Yanzhi Wang
// Purpose: Represents a dialog box that allows the user to configure the application's settings, including the port number
// Restrictions: None

// Define the main form class
namespace Dyscord
{
    // Define a delegate for updating the conversation
    public delegate void UpdateConversationDelegate(string text);
    public partial class DyscordForm : Form
    {
        // Declare variables
        string targetUser = "";
        string targetIp = "";
        int targetPort;
        string myIp = "";
        int myPort = 2222;
        Thread thread;
        Socket listener;

        // Constructor
        public DyscordForm()
        {
            // Initialize the form
            InitializeComponent();

            // Show the form
            this.Show();

            // Show the settings form and get the selected port
            SettingsForm settingsForm = new SettingsForm(this, myPort);
            settingsForm.ShowDialog();
            this.myPort = settingsForm.myPort;

            // Start a thread to listen for incoming messages
            ThreadStart threadStart = new ThreadStart(Listen);
            thread = new Thread(threadStart);
            thread.Start();

            // Get the IP address of the current machine
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress iPAddress in ipHost.AddressList)
            {
                if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.myIp = iPAddress.ToString();
                    break;
                }
            }

            // Add event handlers for the buttons and web browser
            this.loginButton.Click += new EventHandler(LoginButton_Click);
            this.usersButton.Click += new EventHandler(UsersButton_Click);
            this.sendButton.Click += new EventHandler(SendButton_Click);
            this.exitButton.Click += new EventHandler(ExitButton_Click);
            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser1_DocumentCompleted);
        }

        // Handle the login button click event
        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (userTextBox.TextLength > 0)
            {
                // Navigate to the login page and disable the form controls
                webBrowser1.Navigate("http://people.rit.edu/dxsigm/php/login.php?login=" + userTextBox.Text + "&ip=" + myIp + +myPort);
                webBrowser1.Visible = false;
                userTextBox.Enabled = false;
                loginButton.Enabled = false;
            }
        }

        // Handle the users button click event
        private void UsersButton_Click(object sender, EventArgs e)
        {
            // Navigate to the users page and show the web browser
            webBrowser1.Navigate("http://people.rit.edu/dxsigm/php/login.php?logins");
            webBrowser1.Visible = true;
            convRichTextBox.SendToBack();
        }

        // Handle the web browser document completed event
        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Add event handlers for the buttons on the web page
            HtmlElementCollection htmlElementCollection;
            htmlElementCollection = webBrowser1.Document.GetElementsByTagName("button");
            foreach (HtmlElement htmlElement in htmlElementCollection)
            {
                htmlElement.Click += new HtmlElementEventHandler(HtmlElement_Click);
            }
        }

        // Handle the button click event for the buttons on the web page
        private void HtmlElement_Click(object sender, HtmlElementEventArgs e)
        {
            // Get the IP address, port, and username of the target user
            string title;
            string[] ipPort;
            HtmlElement htmlElement = (HtmlElement)sender;

            // Get the "title" attribute of the clicked HTML element, which contains the IP address and port
            title = htmlElement.GetAttribute("title");
            ipPort = title.Split(' ');

            // Store the target IP address and port in class variables
            this.targetIp = ipPort[0];
            this.targetPort = Int32.Parse(ipPort[1]);

            // Store the target username in a class variable and update the conversation group box
            this.targetUser = htmlElement.GetAttribute("name");
            this.groupBox1.Text = "Conversing with " + targetUser;

            // Hide the web browser control after selecting a user
            webBrowser1.Visible = false;
            webBrowser1.SendToBack();

        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (this.targetIp.Length > 0)
            {
                // Create an endpoint with the IP address and port of the target user
                IPAddress iPAddress = IPAddress.Parse(this.targetIp);
                IPEndPoint remoteEndPoint = new IPEndPoint(iPAddress, this.targetPort);

                // Connect to the target user's socket
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Connect(remoteEndPoint);

                // Send the message to the target user
                Stream netStream = new NetworkStream(server);
                StreamWriter writer = new StreamWriter(netStream);
                string msg = userTextBox.Text + ":" + msgRichTextBox.Text;
                writer.Write(msg.ToCharArray(), 0, msg.Length);
                writer.Close();
                netStream.Close();
                server.Close();

                // Add the message to the conversation text box
                this.convRichTextBox.Text += ">" + this.targetUser + ":" + msgRichTextBox.Text + "\n";
                msgRichTextBox.Clear();
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            // Close the listener socket and abort the thread
            listener.Close();
            thread.Abort();

            // Close the application
            Application.Exit();
        }

        // Delegate method used to update the conversation text box from the listening thread
        public void UpdateConversation(string text)
        {
            this.convRichTextBox.Text += text + "\n";
        }

        public void Listen()
        {
            // Create a delegate to update the conversation text box
            UpdateConversationDelegate updateConversationDelegate;
            updateConversationDelegate = new UpdateConversationDelegate(UpdateConversation);

            // Bind the listener socket to the local IP address and port
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, myPort);
            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(serverEndpoint);
            listener.Listen(300);

            while (true)
            {
                // Accept incoming connections from clients
                Socket client = listener.Accept();

                // Read the message from the client
                Stream netStream = new NetworkStream(client);
                StreamReader reader = new StreamReader(netStream);
                string result = reader.ReadToEnd();

                // Update the conversation text box with the received message
                Invoke(updateConversationDelegate, result);

                reader.Close();
                netStream.Close();
                client.Close();
            }
        }
    }
}