using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EWS = Microsoft.Exchange.WebServices.Data;
using System.DirectoryServices.AccountManagement;

namespace WpfApplication1
{
    /// <summary>
    /// Wrapper class to handle data source connections
    /// <para>This class provides a wrapper for both the database and MS Exchange communications. The database contains information specific to the queues, while the Exchange Server is used to fetch free/busy status from the main Exchange Server.</para>
    /// </summary>
    public class CommunicationClass
    {
        private string userName;
        private string displayName;
        private string queueName;
        private List<String> queueMembers;
        private int shiftDuration;
        private EWS.ExchangeService service;
        private EWS.GetUserAvailabilityResults freeBusy;
        private Random r; // for testing purposes

        /// <summary>
        /// Constructor for the communication class.
        /// <para>The user name is established either automatically if within the domain, or via a dialog box if outside.</para>
        /// </summary>
        /// <param name="dad">Pointer back to main window</param>
        public CommunicationClass(MainWindow dad)
        {
            try
            {
                service = new EWS.ExchangeService(EWS.ExchangeVersion.Exchange2010_SP1);
                if (Environment.UserDomainName.Equals("ILLUMINA"))
                {
                    //Inside, so we'll use our local login credentials
                    service.UseDefaultCredentials = true;
                    userName = Environment.UserName + "@illumina.com"; //Hmmm. Wonder why I have to put this on?
                    service.AutodiscoverUrl(userName, RedirectionUrlValidationCallback);
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                    UserPrincipal u = UserPrincipal.Current;
                    displayName = u.DisplayName;
                    initQueue();
                } 
                else
                {
                    //outside, so we'll do a login
                    LoginDialog dlg = new LoginDialog();
                    dlg.Owner = dad;
                    dlg.ShowDialog();
                    if (dlg.DialogResult == true) 
                    {
                        service.Credentials = new EWS.WebCredentials(dlg.userNameBox.Text, dlg.passwordBox1.Password);
                        service.AutodiscoverUrl(dlg.userNameBox.Text, RedirectionUrlValidationCallback);
                        userName = dlg.userNameBox.Text; //would be nice to get it from service...
                        displayName = userName;
                        initQueue();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught: ", e);
            }
            r = new Random(); // for testing purposes     
        }


        /// <summary>
        /// Callback for redirection validation
        /// </summary>
        /// <param name="redirectionUrl">URL we are validating</param>
        /// <returns></returns>
        static bool RedirectionUrlValidationCallback(String redirectionUrl)
        {
            // Perform validation.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            //we want to both use https and have illumina in url
            if ((redirectionUri.Scheme == "https") && (redirectionUrl.Contains("illumina")))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Get the display name of the current user
        /// </summary>
        /// <returns>A string with the name</returns>
        public string fetchDisplayName()
        {
            return displayName;
        }

        /// <summary>
        /// Private method to initialize the internally held information about the queue.
        /// </summary>
        private void initQueue()
        {
            //We'll fix this later
            queueName = "Bioinformatics";
            queueMembers = new List<string>();
            queueMembers.Add("hnguyen2@illumina.com");
            queueMembers.Add("ctetzlaff@illumina.com");
            queueMembers.Add("iwallace@illumina.com");
            queueMembers.Add("clarson@illumina.com");
            queueMembers.Add("jrogers@illumina.com");
            shiftDuration = 60;
        }

        /// <summary>
        /// Get the name of the user's queue.
        /// </summary>
        /// <returns>String value of the queue</returns>
        public string fetchQueueName()
        {
            return queueName;
        }

        /// <summary>
        /// Get a count of how many other people are in the user's queue. <emphasis>Does not include the user in the total!</emphasis>
        /// </summary>
        /// <returns>Integer count of queue other members</returns>
        public int queueMemberCount()
        {
            return queueMembers.Count;
        }

        /// <summary>
        /// Get the Outlook calendars for the queue.
        /// <para>This method fetches the calendars for the current user and the rest of the queue members. Only the day specified is retrieved. The calendars are stored in the communication object until <c>getCal</c> is called again.</para>
        /// </summary>
        /// <param name="start">The <see cref=" DateTime"/> value for the day to fetch the calendars</param>
        /// <returns>True or False depending upon the success of finding the calendar.</returns>
        public bool getCal(DateTime start)
        {
            bool result = false;
            try
            {
                //Set up user list
                List<EWS.AttendeeInfo> whoList = new List<EWS.AttendeeInfo>();
                whoList.Add(new EWS.AttendeeInfo()
                {
                    SmtpAddress = userName,
                    AttendeeType = EWS.MeetingAttendeeType.Required
                });

                foreach (string who in queueMembers)
                {
                    whoList.Add(new EWS.AttendeeInfo()
                    {
                        SmtpAddress = who,
                        AttendeeType = EWS.MeetingAttendeeType.Required
                    });
                }

                //fetch availability
                EWS.AvailabilityOptions opt = new EWS.AvailabilityOptions();
                opt.MeetingDuration = shiftDuration;
                opt.RequestedFreeBusyView = EWS.FreeBusyViewType.FreeBusy;
                EWS.TimeWindow tw = new EWS.TimeWindow(start, start.AddDays(1));
                freeBusy = service.GetUserAvailability(whoList, tw, EWS.AvailabilityData.FreeBusy, opt);
                foreach (EWS.AttendeeAvailability a in freeBusy.AttendeesAvailability)
                {
                    Console.WriteLine(a.CalendarEvents.Count);
                }
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                throw (e);
            }
            return result;

        }

        /// <summary>
        /// Check to see if the current user is available at a specific time.
        /// </summary>
        /// <param name="shiftStart">DateTime format for starting time.</param>
        /// <returns>True if user is available, false if not available.</returns>
        public bool isAvailable(DateTime shiftStart)
        {
            return this.isAvailable(shiftStart, -1);
        }

        /// <summary>
        /// Check to see if a specific user is available.
        /// <para>Queue members calendars are stored in a 0-based integer list. Use <see cref=" queueMemberCount"/> to determine number of members.</para>
        /// </summary>
        /// <param name="shiftStart">DateTime format for starting time.</param>
        /// <param name="member">Integer value of member to check</param>
        /// <returns></returns>
        public bool isAvailable(DateTime shiftStart, int member)
        {
            bool result = true;
            try
            {
                //Okay, I lied. The group members are 1-based, not 0-based. The 0th element is actually the user, so we need to offset by 1.
                EWS.AttendeeAvailability avail = freeBusy.AttendeesAvailability.ElementAt(member+1);
                foreach (EWS.CalendarEvent cal in avail.CalendarEvents)
                {
                    //look for conditions where the calendar should be busy.
                    DateTime shiftEnd = shiftStart.AddMinutes(shiftDuration);
                    if (((shiftStart == cal.StartTime) ||
                        ((shiftStart < cal.EndTime) && (cal.EndTime <= shiftEnd)) || 
                        ((shiftStart <= cal.StartTime) && (cal.StartTime < shiftEnd)) ||
                        ((cal.StartTime < shiftStart) && (cal.EndTime > shiftStart))) &&
                        ((cal.FreeBusyStatus == EWS.LegacyFreeBusyStatus.Busy) || 
                        (cal.FreeBusyStatus == EWS.LegacyFreeBusyStatus.OOF)))
                    {
                        result = false;
                    }
                }
            }
            catch
            {
                throw new Exception("Couldn't access calendar from attendee list");
            }
            return result;
        }

    }
}
