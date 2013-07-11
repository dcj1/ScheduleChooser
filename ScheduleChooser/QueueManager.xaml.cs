using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for QueueManager.xaml
    /// </summary>
    public partial class QueueManager : Window
    {
        CommunicationClass commObj;
        List<ComboBox> secondaryQueueList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c">Communication class object</param>
        public QueueManager(CommunicationClass c)
        {
            InitializeComponent();
            commObj = c;
            initLists();
            this.Show();
        }

        /// <summary>
        /// Instantiate internal lists
        /// </summary>
        private void initLists()
        {
            commObj.reloadQueueList();
            QueueList.ItemsSource = commObj.fetchQueueNames();
            manageQueueBox.ItemsSource = commObj.fetchQueueNames();
            TASList.ItemsSource = commObj.getTASNames();
            manageTASList.ItemsSource = commObj.getTASNames();
            primaryQueueChooser.ItemsSource = commObj.fetchQueueNames();
            secondaryQueueList = new List<ComboBox>();
            secondaryQueueList.Add(secondaryQueueChooser1);
            secondaryQueueList.Add(secondaryQueueChooser2);
            secondaryQueueList.Add(secondaryQueueChooser3);
            secondaryQueueList.Add(secondaryQueueChooser4);
            secondaryQueueList.Add(secondaryQueueChooser5);
            foreach (ComboBox cb in secondaryQueueList)
            {
                cb.ItemsSource = commObj.fetchQueueNames();
            }
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            commObj.saveTASList();
            UserWarning verity = new UserWarning();
            verity.WarningTextBlock.Text = "Saved TAS names";
            verity.Show();
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            initLists();
            QueueList.SelectedIndex = -1;

        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToPrimaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                try
                {
                    commObj.putInQueue(s, QueueList.SelectedItem.ToString(), true);
                    moveListItem(s, TASList, PrimaryMemberList, false);
                }
                catch (Exception qe)
                {
                    //put up a modal dialog, continue
                    UserWarning uw = new UserWarning();
                    uw.setMessage(qe.Message);
                    uw.Show();
                }
            }
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFromPrimaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in PrimaryMemberList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.removeFromQueue(s, QueueList.SelectedItem.ToString());
                moveListItem(s, PrimaryMemberList, TASList, true);
            }
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToSecondaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                try
                {
                    commObj.putInQueue(s, QueueList.SelectedItem.ToString(), false);
                    moveListItem(s, TASList, SecondaryMemberList, false);
                }
                catch (Exception qe)
                {
                    //put up a modal dialog, continue
                    UserWarning uw = new UserWarning();
                    uw.setMessage(qe.Message);
                    uw.Show();
                }
            }
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFromSecondaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in SecondaryMemberList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.removeFromQueue(s, QueueList.SelectedItem.ToString()); 
                moveListItem(s, SecondaryMemberList, TASList, true);
            }
        }

        /// <summary>
        /// event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrimaryMemberList.Items.Clear();
            SecondaryMemberList.Items.Clear();

            if (QueueList.SelectedIndex != -1)
            {
                //Get primary queue names
                List<string> names = commObj.getTASNames(QueueList.SelectedValue.ToString(), true);
                foreach (string s in names)
                {
                    PrimaryMemberList.Items.Add(s);
                }

                //Get secondary queue names
                names = commObj.getTASNames(QueueList.SelectedValue.ToString(), false);
                foreach (string s in names)
                {
                    SecondaryMemberList.Items.Add(s);
                }
            }

        }

        /// <summary>
        /// shift an item from one listview to another
        /// </summary>
        /// <param name="s">The ListItem label</param>
        /// <param name="source">The original ListView</param>
        /// <param name="dest">The destination ListView</param>
        /// <param name="del">True will delete the ListItem from the originating ListView</param>
        private void moveListItem(string s, ListView source, ListView dest, bool del)
        {
            string s2 = s.Clone() as string;
            if (del)
            {
                source.Items.Remove(s);
            }
            else
            {
                dest.Items.Add(s2);
            }
        }

        /// <summary>
        /// NO OP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditTASList_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// NO OP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewQueueButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Update queue information based upon selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageQueueBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                queueOwnerTextBox.Text = commObj.getQueueOwnerName(manageQueueBox.SelectedValue.ToString());
                startTimeTextBox.Text = commObj.getQueueStartTime(manageQueueBox.SelectedValue.ToString()).ToShortTimeString();
                endTimeTextBox.Text = commObj.getQueueEndTime(manageQueueBox.SelectedValue.ToString()).ToShortTimeString();
                durationTextBox.Text = commObj.getQueueSlotDuration(manageQueueBox.SelectedValue.ToString()).ToString();
            }
            catch
            {
                //we've _probably_ reset the manageQueueBox to null
                queueOwnerTextBox.Text = "";
                startTimeTextBox.Text = "";
                endTimeTextBox.Text = "";
                durationTextBox.Text = "";
            }

        }

        /// <summary>
        /// Populate queue information with default values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageQueueBox_TextInput(object sender, string e)
        {
            queueOwnerTextBox.Text = commObj.getQueueOwnerName(e.ToString());
            startTimeTextBox.Text = commObj.getQueueStartTime(e.ToString()).ToShortTimeString();
            endTimeTextBox.Text = commObj.getQueueEndTime(e.ToString()).ToShortTimeString();
            durationTextBox.Text = commObj.getQueueSlotDuration(e.ToString()).ToString();
        }

        /// <summary>
        /// capture the enter key to trigger update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageQueueBox_TextInput(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                manageQueueBox_TextInput(sender, manageQueueBox.Text);
            }
        }

        /// <summary>
        /// Save queue edits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void queueEditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            commObj.updateQueueList(manageQueueBox.Text, queueOwnerTextBox.Text, startTimeTextBox.Text, 
                endTimeTextBox.Text, durationTextBox.Text);
            manageQueueBox.SelectedIndex = -1;
            initLists();

        }

        /// <summary>
        /// Capture when the TAS names change and populate queue information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void manageTASList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (manageTASList.SelectedIndex == -1)
            {
                //reset everything to null
                primaryQueueChooser.SelectedIndex = -1;
                foreach (ComboBox cb in secondaryQueueList)
                {
                    cb.SelectedIndex = -1;
                }
            }
            else
            {
                //get the needed information
                primaryQueueChooser.SelectedItem = commObj.getPrimaryQueue(manageTASList.SelectedValue.ToString());
                int index = 0;
                foreach (string s in commObj.getSecondaryQueues(manageTASList.SelectedValue.ToString()))
                {
                    if (index < secondaryQueueList.Count)
                    {
                        //guarding against data corruption -- we'll through away any queues beyond what we have room for
                        secondaryQueueList[index].SelectedItem = s;
                    }
                    index++;
                }
                while (index < secondaryQueueList.Count)
                {
                    secondaryQueueList[index].SelectedIndex = -1;
                    index++;
                }
            }
        }

        /// <summary>
        /// update primary queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void primaryQueueChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (primaryQueueChooser.SelectedIndex != -1)
            {
                //remove the primary queue from the secondary queues
                foreach (ComboBox cb in secondaryQueueList)
                {
                    if (cb.SelectedIndex != -1 && cb.SelectedItem.ToString().Equals(primaryQueueChooser.SelectedItem.ToString()))
                    {
                        cb.SelectedIndex = -1;
                    }
                }
            }
        }

        //private void secondaryQueueChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //}

        /// <summary>
        /// cancel TAS changes and reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelTASChanges_Click(object sender, RoutedEventArgs e)
        {
            initLists();
            manageTASList.SelectedIndex = -1;
        }

        /// <summary>
        /// Propogate the changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveTASChanges_Click(object sender, RoutedEventArgs e)
        {
            UserWarning warn = new UserWarning();
            //if (manageTASList.SelectedIndex == -1)
            //{
            //    warn.setMessage("Please select or enter a name");
            //    warn.Show();
            //}
            //else 
                if (primaryQueueChooser.SelectedIndex == -1)
            {
                warn.setMessage("Please choose a primary queue");
                warn.Show();
            }
            else
            {
                List<string> sQueues = new List<string>();
                foreach (ComboBox cb in secondaryQueueList)
                {
                    if (cb.SelectedIndex != -1)
                    {
                        sQueues.Add(cb.Text);
                    }
                }
                commObj.updateTASList(manageTASList.Text, primaryQueueChooser.Text, sQueues);
                initLists();
                manageTASList.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Alter the secondary queues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void secondaryQueueChooser_SelectionChanged(object sender, EventArgs e)
        {
            ComboBox src = sender as ComboBox;
            UserWarning warn = new UserWarning();
            if (primaryQueueChooser.SelectedIndex != -1 && src.SelectedItem.ToString().Equals(primaryQueueChooser.SelectedItem.ToString()))
            {
                warn.setMessage("User is already in " + src.SelectedItem.ToString() + " as their primary queue.");
                warn.Show();
                src.SelectedIndex = -1;
            }
            else
            {
                string srcString = src.SelectedItem.ToString(); 
                foreach (ComboBox cb in secondaryQueueList)
                {
                    if (!cb.Equals(src))
                    {
                        if (cb.SelectedIndex != -1 && cb.SelectedItem.ToString().Equals(srcString))
                        {
                            warn.setMessage("User is already in " + src.SelectedItem.ToString() + " as a secondary queue.");
                            warn.Show();
                            src.SelectedIndex = -1;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// this should be deprecated
        /// </summary>
        /// <param name="c"></param>
        private void flashBackground(ComboBox c)
        {
            for (int i = 0; i < 5; i++)
            {
                c.IsEnabled = false;
                System.Threading.Thread.Sleep(50);
                c.IsEnabled = true;
                System.Threading.Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Bail out of queue management and restore defaults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void queueEditCancelButton_Click(object sender, RoutedEventArgs e)
        {
            initLists();
            manageQueueBox.SelectedIndex = -1;
        }
            
     }
}
