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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private CommunicationClass commObj;

        //this need to be much more clever in the setup -- can we be dynamic?
        public Page1(CommunicationClass c)
        {
            InitializeComponent();
            commObj = c;
            TASName.Content = commObj.fetchDisplayName();
            QueueName.Content = commObj.fetchQueueName();
        }

        private void datePicker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            updateCalendars(datePicker1.SelectedDate);
        }

        public void updateCalendars(DateTime? day)
        {
            DateTime qDay;
            if (day != null)
            {
                qDay = (DateTime)day; //cast to overcome MS null value stupidity
 
                //Get personal calendar
                if (!commObj.getCal(qDay, 60))
                {
                    throw new Exception ("Couldn't get calendar for some unknown reason");
                }
                //Need to be much more clever to make more sustainable code
                //7AM
                slotRequest1.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year,qDay.Month,qDay.Day,7,0,0)))
                {
                    slotRequest1.setStatus(slotRequest1.OPEN);
                } 
                else 
                {
                    slotRequest1.setStatus(slotRequest1.BUSY);
                }
                //8AM
                slotRequest2.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 8, 0, 0)))
                {
                    slotRequest2.setStatus(slotRequest2.OPEN);
                }
                else
                {
                    slotRequest2.setStatus(slotRequest2.BUSY);
                }
                //9AM
                slotRequest3.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 9, 0, 0)))
                {
                    slotRequest3.setStatus(slotRequest3.OPEN);
                }
                else
                {
                    slotRequest3.setStatus(slotRequest3.BUSY);
                }
                //10
                slotRequest4.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 10, 0, 0)))
                {
                    slotRequest4.setStatus(slotRequest4.OPEN);
                }
                else
                {
                    slotRequest4.setStatus(slotRequest4.BUSY);
                }
                //11
                slotRequest5.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 11, 0, 0)))
                {
                    slotRequest5.setStatus(slotRequest5.OPEN);
                }
                else
                {
                    slotRequest5.setStatus(slotRequest5.BUSY);
                }
                //12
                slotRequest6.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 12, 0, 0)))
                {
                    slotRequest6.setStatus(slotRequest6.OPEN);
                }
                else
                {
                    slotRequest6.setStatus(slotRequest6.BUSY);
                }
                //1
                slotRequest7.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 13, 0, 0)))
                {
                    slotRequest7.setStatus(slotRequest7.OPEN);
                }
                else
                {
                    slotRequest7.setStatus(slotRequest7.BUSY);
                }
                //2
                slotRequest8.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 14, 0, 0)))
                {
                    slotRequest8.setStatus(slotRequest8.OPEN);
                }
                else
                {
                    slotRequest8.setStatus(slotRequest8.BUSY);
                }
                //3
                slotRequest9.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 15, 0, 0)))
                {
                    slotRequest9.setStatus(slotRequest9.OPEN);
                }
                else
                {
                    slotRequest9.setStatus(slotRequest9.BUSY);
                }
                //4

                slotRequest10.reset();
                if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 16, 0, 0)))
                {
                    slotRequest10.setStatus(slotRequest10.OPEN);
                }
                else
                {
                    slotRequest10.setStatus(slotRequest10.BUSY);
                }

                //Get queue calendars
                List<string> whom = new List<string>(); //replace with real code
                whom.Add("clarson@illumina.com");
                whom.Add("hnguyen2@illumina.com");
                whom.Add("iwallace@illumina.com");
                if (!commObj.getCal(whom, qDay, 60))
                {
                    throw new Exception("Couldn't get calendar for some unknown reason");
                }

                List<UserControl1> slots = new List<UserControl1>();
                slots.Add(userControl11);
                slots.Add(userControl12);
                slots.Add(userControl13);
                for (int i = 0; i < whom.Count; i++)
                {
                    if (commObj.isAvailable(new DateTime(qDay.Year,qDay.Month,qDay.Day,7,0,0,i))) {
                        slots[i].slotStatus1.setStatus(slots[i].slotStatus1.OPEN);
                    } else {
                        slots[i].slotStatus1.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 8, 0, 0, i)))
                    {
                        slots[i].slotStatus2.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus2.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 9, 0, 0, i)))
                    {
                        slots[i].slotStatus3.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus3.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 10, 0, 0, i)))
                    {
                        slots[i].slotStatus4.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus4.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 11, 0, 0, i)))
                    {
                        slots[i].slotStatus5.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus5.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 12, 0, 0, i)))
                    {
                        slots[i].slotStatus6.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus6.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 13, 0, 0, i)))
                    {
                        slots[i].slotStatus7.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus7.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 14, 0, 0, i)))
                    {
                        slots[i].slotStatus8.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus8.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 15, 0, 0, i)))
                    {
                        slots[i].slotStatus9.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus9.setStatus(slots[i].slotStatus1.BUSY);
                    }
                    if (commObj.isAvailable(new DateTime(qDay.Year, qDay.Month, qDay.Day, 16, 0, 0, i)))
                    {
                        slots[i].slotStatus10.setStatus(slots[i].slotStatus1.OPEN);
                    }
                    else
                    {
                        slots[i].slotStatus10.setStatus(slots[i].slotStatus1.BUSY);
                    }
                }
            }
        }
    }
}
