using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using Outlook = Microsoft.Office.Interop.Outlook;
using EWS = Microsoft.Exchange.WebServices.Data;

namespace WpfApplication1
{
    public class CommunicationClass
    {
        private string userName;
        //private string displayName;
        //private string queueName;
        private EWS.ExchangeService service;
        private EWS.GetUserAvailabilityResults freeBusy;
        private Random r; // for testing purposes


        public CommunicationClass(MainWindow dad)
        {
            //need to wrap a try catch here
            //give the user a couple chances to login properly
            LoginDialog dlg = new LoginDialog();
            dlg.Owner = dad;
            dlg.ShowDialog();
            if (dlg.DialogResult == true)
            {
                try
                {
                    // EWS temporarily blocked during development
                    //service = new EWS.ExchangeService(EWS.ExchangeVersion.Exchange2010_SP1);
                    service.Credentials = new EWS.WebCredentials(dlg.userNameBox.Text, dlg.passwordBox1.Password);
                    //service.AutodiscoverUrl(dlg.userNameBox.Text, RedirectionUrlValidationCallback);
                    userName = dlg.userNameBox.Text; //would be nice to get it from service...
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught: ", e);
                    throw(e);
                }
                r = new Random(); // for testing purposes
            }
            
        }


        // Create the callback to validate the redirection URL.
        static bool RedirectionUrlValidationCallback(String redirectionUrl)
        {
            // Perform validation.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            if (redirectionUri.Scheme == "https")
            {
                //illumina.com in url
                result = true;
            }

            return result;
        }

        //Get a user friendly name
        public string fetchDisplayName()
        {
            // do what is needed here to really get someone's name
            string result = "Billy Bob";
            return result;
        }

        //Get the queue information
        public string fetchQueueName()
        {
            // do what is needed here to really get some one's queue
            string result = "marketing";
            return result;
        }

        //overload method to get current user availability
        public bool getCal(DateTime start, int length)
        {
            List<string> names = new List<string>();
            names.Add(userName);
            return (this.getCal(names, start, length));
        }

        //get user availability
        public bool getCal(List<string> names, DateTime start, int length)
        {
            // Block off this code while testing
            //bool result = false;
            //try
            //{
            //    //Sending queue names one by one. Trying to preserve encapsulation for EWS
            //    //Maybe smarter & faster to break encapsulation and do the list all at once

            //    //Set up user list
            //    List<EWS.AttendeeInfo> whoList = new List<EWS.AttendeeInfo>();
            //    foreach (string who in names)
            //    {
            //        whoList.Add(new EWS.AttendeeInfo()
            //        {
            //            SmtpAddress = who,
            //            AttendeeType = EWS.MeetingAttendeeType.Required
            //        });
            //    }

            //    //fetch availability
            //    EWS.AvailabilityOptions opt = new EWS.AvailabilityOptions();
            //    opt.MeetingDuration = length;
            //    opt.RequestedFreeBusyView = EWS.FreeBusyViewType.FreeBusy;
            //    EWS.TimeWindow tw = new EWS.TimeWindow(start, start.AddDays(1));
            //    freeBusy = service.GetUserAvailability(whoList, tw, EWS.AvailabilityData.FreeBusy, opt);
            //    result = true;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("{0} Exception caught.", e);
            //    throw (e);
            //}
            //return result;

            // temporary testing code
            return true;
        }

        public bool isAvailable(DateTime shiftStart)
        {
            return this.isAvailable(shiftStart, 0);
        }

        public bool isAvailable(DateTime shiftStart, int member)
        {
            //Block use of EWS whil developing
            //try
            //{
            //    EWS.AttendeeAvailability avail = freeBusy.AttendeesAvailability.ElementAt(member);
            //    foreach (EWS.CalendarEvent cal in avail.CalendarEvents)
            //    {
            //        if ((cal.StartTime == shiftStart) &&
            //            ((cal.FreeBusyStatus == EWS.LegacyFreeBusyStatus.Busy) || (cal.FreeBusyStatus == EWS.LegacyFreeBusyStatus.OOF)))
            //        {
            //            return false;
            //        }
            //    }
            //}
            //catch
            //{
            //    throw new Exception("Couldn't access calendar from attendee list");
            //}
            //return true;

            //temporary testing code
            int i = r.Next(15);
            if (i < 10)
            {
                return true;
            }
            return false;
        }

        // OIO interface -- works, but would like to deprecate in favor of EWS
        //public Outlook.Items GetCalendar(DateTime picked)
        //{
        //    //Set up day filter, resetting start time to 7AM
        //    DateTime start = new DateTime(picked.Year, picked.Month, picked.Day, 7, 0, 0);
        //    DateTime end = start.AddHours(10);
        //    string filter = "[Start] <= '" + end.ToString("g") + "' AND [End] >= '" + start.ToString("g") + "'";

        //    try
        //    {
        //        // create all the outlook connections
        //        Outlook.Application oApp = new Outlook.Application();
        //        Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
        //        Outlook.Recipient oUser = oNS.CreateRecipient(userName);
        //        Outlook.Folder cFolder =
        //            oApp.Session.GetSharedDefaultFolder(oUser,
        //            Outlook.OlDefaultFolders.olFolderCalendar)
        //            as Outlook.Folder;

        //        //Get the calendar items for the day
        //        Outlook.Items cItems = cFolder.Items;
        //        cItems.IncludeRecurrences = true;
        //        cItems.Sort("[Start]", Type.Missing);
        //        Outlook.Items restrictItems = cItems.Restrict(filter);
        //        return restrictItems;
        //    }
        //    catch
        //    {
        //        throw (new Exception("Couldn't retrieve calendar for " + displayName));
        //    }
        //}
    }
}
