using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public QueueManager(CommunicationClass c)
        {
            InitializeComponent();
            commObj = c;
            initLists();
            this.Show();
        }

        private void initLists()
        {
            QueueList.ItemsSource = commObj.fetchQueueNames();
            manageQueueBox.ItemsSource = commObj.fetchQueueNames();
            TASList.ItemsSource = commObj.getTASNames();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            commObj.saveQueueList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToPrimaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.putInQueue(s, QueueList.SelectedItem.ToString(), true);
                moveListItem(s, TASList, PrimaryMemberList, false);
            }
        }

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

        private void AddToSecondaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.putInQueue(s, QueueList.SelectedItem.ToString(), false);
                moveListItem(s, TASList, SecondaryMemberList, false);
            }
        }

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

        private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrimaryMemberList.Items.Clear();
            SecondaryMemberList.Items.Clear();

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

        private void EditTASList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewQueueButton_Click(object sender, RoutedEventArgs e)
        {

        }

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

        private void manageQueueBox_TextInput(object sender, string e)
        {
            queueOwnerTextBox.Text = commObj.getQueueOwnerName(e.ToString());
            startTimeTextBox.Text = commObj.getQueueStartTime(e.ToString()).ToShortTimeString();
            endTimeTextBox.Text = commObj.getQueueEndTime(e.ToString()).ToShortTimeString();
            durationTextBox.Text = commObj.getQueueSlotDuration(e.ToString()).ToString();
        }

        private void manageQueueBox_TextInput(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                manageQueueBox_TextInput(sender, manageQueueBox.Text);
            }
        }

        private void queueEditSaveButton_Click(object sender, RoutedEventArgs e)
        {
            commObj.updateQueueList(manageQueueBox.Text, queueOwnerTextBox.Text, startTimeTextBox.Text, 
                endTimeTextBox.Text, durationTextBox.Text);
            manageQueueBox.SelectedIndex = -1;
            initLists();
        }
    }
}
