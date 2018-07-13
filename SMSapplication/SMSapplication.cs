/*
 * Created by: Syeda Anila Nusrat. *
 * Date: 1st August 2009
 * Time: 2:54 PM 
 * Enhanced by: Ranjan Dailata. *
 * Date: 3rd March 2015
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using MySql.Data.MySqlClient;

namespace SMSapplication
{
    public partial class SMSapplication : Form
    {

        #region Constructor
        public SMSapplication()
        {
            InitializeComponent();
            rbReadAll.Checked = true;
            lvwMessages.Columns[1].Width = 130;
            lvwMessages.Columns[3].Width = 400;
        }
        #endregion

        #region Private Variables
        SerialPort port = new SerialPort();
        SmsHelper smsHelper = new SmsHelper();
        //USSDHelper ussdHelper = new USSDHelper();

        ShortMessageCollection objShortMessageCollection = new ShortMessageCollection();
        #endregion

        #region Private Methods

        #region Write StatusBar
        private void WriteStatusBar(string status)
        {
            try
            {
                statusBar1.Text = "Message: " + status;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ex on:" + ex.Message);
            }
        }
        #endregion

        #endregion

        #region Private Events

        private void SMSapplication_Load(object sender, EventArgs e)
        {
            try
            {
                #region Display all available COM Ports
                string[] ports = SerialPort.GetPortNames();

                // Add all port names to the combo box:
                foreach (string port in ports)
                {
                    this.cboPortName.Items.Add(port);
                }
                #endregion

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);
                this.tabSMSapplication.TabPages.Remove(tbCallUSSD);

                this.btnDisconnect.Enabled = false;

                MySqlConnection conn = DBUtils.GetDBConnection();

                
                //Console.WriteLine("Openning Connection ...");

                conn.Open();

                //Console.WriteLine("Connection successful!");
                

                Console.Read();
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //Open communication port 
                this.port = smsHelper.OpenPort(this.cboPortName.Text, Convert.ToInt32(this.cboBaudRate.Text),
                                               Convert.ToInt32(this.cboDataBits.Text), Convert.ToInt32(this.txtReadTimeOut.Text),
                                               Convert.ToInt32(this.txtWriteTimeOut.Text));

                if (this.port != null)
                {
                    this.gboPortSettings.Enabled = false;

                    //MessageBox.Show("Modem is connected at PORT " + this.cboPortName.Text);
                    this.statusBar1.Text = "Modem is connected at PORT " + this.cboPortName.Text;

                    //Add tab pages
                    this.tabSMSapplication.TabPages.Add(tbSendSMS);
                    this.tabSMSapplication.TabPages.Add(tbReadSMS);
                    this.tabSMSapplication.TabPages.Add(tbDeleteSMS);
                    this.tabSMSapplication.TabPages.Add(tbCallUSSD);

                    this.lblConnectionStatus.Text = "Connected at " + this.cboPortName.Text;
                    this.btnDisconnect.Enabled = true;
                }
                else
                {
                    //MessageBox.Show("Invalid port settings");
                    this.statusBar1.Text = "Invalid port settings";
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.gboPortSettings.Enabled = true;
                smsHelper.ClosePort(this.port);

                //Remove tab pages
                this.tabSMSapplication.TabPages.Remove(tbSendSMS);
                this.tabSMSapplication.TabPages.Remove(tbReadSMS);
                this.tabSMSapplication.TabPages.Remove(tbDeleteSMS);
                this.tabSMSapplication.TabPages.Remove(tbCallUSSD);

                this.lblConnectionStatus.Text = "Not Connected";
                this.btnDisconnect.Enabled = false;

            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void btnSendSMS_Click(object sender, EventArgs e)
        {
            this.statusBar1.Text = "";
            //.............................................. Send SMS ....................................................
            try
            {

                if (smsHelper.SendMessage(this.port, this.txtSIM.Text, this.txtMessage.Text))
                {
                    //MessageBox.Show("Message has sent successfully");
                    this.statusBar1.Text = "Message has sent successfully";
                }
                else
                {
                    //MessageBox.Show("Failed to send message");
                    this.statusBar1.Text = "Failed to send message";
                }

            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }
        private void btnReadSMS_Click(object sender, EventArgs e)
        {
            this.statusBar1.Text = "";
            try
            {
                //count SMS 
                int uCountSMS = smsHelper.CountSmsMessages(this.port);
                if (uCountSMS > 0)
                {

                    #region Command
                    string strCommand = "AT+CMGL=\"ALL\"";

                    if (this.rbReadAll.Checked)
                    {
                        strCommand = "AT+CMGL=\"ALL\"";
                    }
                    else if (this.rbReadUnRead.Checked)
                    {
                        strCommand = "AT+CMGL=\"REC UNREAD\"";
                    }
                    else if (this.rbReadStoreSent.Checked)
                    {
                        strCommand = "AT+CMGL=\"STO SENT\"";
                    }
                    else if (this.rbReadStoreUnSent.Checked)
                    {
                        strCommand = "AT+CMGL=\"STO UNSENT\"";
                    }
                    #endregion

                    // If SMS exist then read SMS
                    #region Read SMS
                    //.............................................. Read all SMS ....................................................
                    objShortMessageCollection = smsHelper.ReadSMS(this.port, strCommand);

                    RemoveMessagesFromListView();

                    foreach (ShortMessage msg in objShortMessageCollection)
                    {
                        ListViewItem item = new ListViewItem(new string[] { msg.Index, msg.Sent, msg.Sender, msg.Message });
                        item.Tag = msg;
                        lvwMessages.Items.Add(item);
                    }
                    #endregion
                }
                else
                {
                    RemoveMessagesFromListView();
                    this.statusBar1.Text = "There is no message in SIM";
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        private void RemoveMessagesFromListView()
        {
            for (int i = 0; i < lvwMessages.Items.Count; i++)
            {
                lvwMessages.Items.RemoveAt(i);
            }
        }

        private void btnDeleteSMS_Click(object sender, EventArgs e)
        {
            this.statusBar1.Text = "";
            try
            {
                //Count SMS 
                int uCountSMS = smsHelper.CountSmsMessages(this.port);
                if (uCountSMS > 0)
                {
                    DialogResult dr = MessageBox.Show("Are u sure u want to delete the SMS?", "Delete confirmation", MessageBoxButtons.YesNo);

                    if (dr.ToString() == "Yes")
                    {
                        #region Delete SMS

                        if (this.rbDeleteAllSMS.Checked)
                        {
                            //...............................................Delete all SMS ....................................................

                            #region Delete all SMS
                            string strCommand = "AT+CMGD=1,4";
                            if (smsHelper.DeleteMessage(this.port, strCommand))
                            {
                                //MessageBox.Show("Messages has deleted successfuly ");
                                this.statusBar1.Text = "Messages has deleted successfuly";
                            }
                            else
                            {
                                //MessageBox.Show("Failed to delete messages ");
                                this.statusBar1.Text = "Failed to delete messages";
                            }
                            #endregion

                        }
                        else if (this.rbDeleteReadSMS.Checked)
                        {
                            //...............................................Delete Read SMS ....................................................
                            #region Delete Read SMS
                            string strCommand = "AT+CMGD=1,3";
                            if (smsHelper.DeleteMessage(this.port, strCommand))
                            {
                                //MessageBox.Show("Messages has deleted successfuly");
                                this.statusBar1.Text = "Messages has deleted successfuly";
                            }
                            else
                            {
                                //MessageBox.Show("Failed to delete messages ");
                                this.statusBar1.Text = "Failed to delete messages";
                            }
                            #endregion

                        }
                        else if (this.rbDeleteByIndex.Checked)
                        {
                            //...............................................Delete SMS By Index ....................................................
                            #region Delete Read SMS
                            if (txtDeleteIndex.Text.Equals(""))
                            {
                                MessageBox.Show("Please Specify Delete Message Index");
                                return;
                            }
                            string strCommand = "AT+CMGD="+ txtDeleteIndex.Text.Trim();
                            if (smsHelper.DeleteMessage(this.port, strCommand))
                            {
                                this.statusBar1.Text = "Messages has deleted successfuly";
                            }
                            else
                            {
                                this.statusBar1.Text = "Failed to delete messages";
                            }
                            #endregion
                        }

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }

        }
        private void btnCountSMS_Click(object sender, EventArgs e)
        {
            this.statusBar1.Text = "";
            try
            {
                //Count SMS
                int uCountSMS = smsHelper.CountSmsMessages(this.port);
                this.txtCountSMS.Text = uCountSMS.ToString();
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
            }
        }

        #endregion

        #region Error Log
        public void ErrorLog(string Message)
        {
            StreamWriter sw = null;

            try
            {
                WriteStatusBar(Message);

                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                //string sPathName = @"E:\";
                string sPathName = @"SMSapplicationErrorLog_";

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                sw = new StreamWriter(sPathName + sErrorTime + ".txt", true);

                sw.WriteLine(sLogFormat + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }

        }
        #endregion

        private void lvwMessages_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = lvwMessages.HitTest(e.X, e.Y);
            ListViewItem item = info.Item;
            ShortMessage shortMessage = (ShortMessage)item.Tag;

            if (item != null)
            {
                MessageBox.Show(shortMessage.Message);
            }
            else
            {
                this.lvwMessages.SelectedItems.Clear();
                MessageBox.Show("No Item is selected");
            }
        }

        private void btnCallUSSD_Click(object sender, EventArgs e)
        {
            string receivedData = smsHelper.callUSSD101(this.port);
            lblMessageUSSD.Text = receivedData;
        }

        private void btnNapThe_Click(object sender, EventArgs e)
        {
            if (txtSerial.Text != null && txtSerial.Text != String.Empty)
            {
                string receivedData = smsHelper.callUSSD100(this.port, txtSerial.Text);
                MessageBox.Show(receivedData);
            }
            else {
                MessageBox.Show("Serial is not empty");
            }
        }
    }
}