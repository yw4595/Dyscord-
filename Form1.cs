using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Namespace: Dyscord
namespace Dyscord
{
    // Class: SettingsForm
    // Author: Yanzhi Wang
    // Purpose: Represents a dialog box that allows the user to configure the application's settings, including the port number
    // Restrictions: None
    public partial class SettingsForm : Form
    {
        // Field: myPort
        // Purpose: Stores the current port number selected by the user
        public int myPort;

        // Constructor: SettingsForm
        // Purpose: Initializes a new instance of the SettingsForm class with the specified owner form and port number
        public SettingsForm(Form owner, int nPort)
        {
            InitializeComponent();

            // Set the properties of the form
            this.Owner = owner;
            this.CenterToParent();
            this.myPort = nPort;
            this.portTextBox.Text = nPort.ToString();

            // Attach event handlers for the form controls
            this.startButton.Click += new EventHandler(StartButton_Click);
            this.portTextBox.KeyPress += new KeyPressEventHandler(PortTextBox_KeyPress);
        }

        // Method: StartButton_Click
        // Purpose: Handles the Click event for the Start button
        private void StartButton_Click(object sender, EventArgs e)
        {
            // Update the myPort field with the value entered by the user in the portTextBox
            this.myPort = Int32.Parse(this.portTextBox.Text);

            // Close the dialog box
            this.Close();
        }

        // Method: PortTextBox_KeyPress
        // Purpose: Handles the KeyPress event for the portTextBox
        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the pressed key is a digit or the backspace key, allow the input
            if (Char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
            {
                e.Handled = false;
            }
            else
            {
                // Otherwise, reject the input
                e.Handled = true;
            }
        }
    }
}
