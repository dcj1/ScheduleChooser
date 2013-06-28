using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
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
        private EWS.ExchangeService service;
        private EWS.GetUserAvailabilityResults freeBusy;
        private Random r; // for testing purposes
        private Hashtable queueDefList;
        private List<Membership> tasNames;
        string configFile;

        public class QueueDef
        {
            public string queueName { get; set; }
            public string owner { get; set; }
            public int slotDuration { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
        }

        public class Membership
        {
            public string queueName { get; set; }
            public string userName { get; set; }
            public bool primary { get; set; }
        }

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
                    configFile = "config.xml";
                    InitQueueList();
                    initQueueMembers();
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
        }

        private void initQueueMembers()
        {
            tasNames = new List<Membership>();
            List<string> tempNames = new List<string>();
            tempNames.Add("hnguyen2@illumina.com");
            tempNames.Add("ctetzlaff@illumina.com");
            tempNames.Add("iwallace@illumina.com");
            tempNames.Add("clarson@illumina.com");
            tempNames.Add("jrogers@illumina.com");
            foreach (string name in tempNames)
            {
                Membership tn = new Membership();
                tn.userName = name;
                tn.queueName = "Bioinformatics";
                tn.primary = true;
                tasNames.Add(tn);
            }
        }

        private void InitQueueList()
        {
            //List<string> qs = new List<string>();
            //qs.Add("Bioinformatics");
            //qs.Add("SamplePrep");
            //queueDefList = new Hashtable();
            //foreach (string s in qs)
            //{
            //    QueueDef qd = new QueueDef();
            //    qd.queueName = s;
            //    qd.owner = "cjamison@illumina.com";
            //    qd.slotDuration = 60;
            //    qd.startTime = new DateTime(2013, 1, 1, 7, 0, 0);
            //    qd.endTime = new DateTime(2013, 1, 1, 17, 0, 0);
            //    queueDefList.Add(s, qd);
            //}

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<QueueDef>));
                serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
                FileStream fs = new FileStream(configFile, FileMode.Open);
                List<QueueDef> q = (List<QueueDef>)serializer.Deserialize(fs);
                queueDefList = new Hashtable();
                foreach (QueueDef qd in q)
                {
                    queueDefList.Add(qd.queueName, qd);
                }
                fs.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }

        private void serializer_UnknownNode (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }


        public void saveQueueList()
        {
            try
            {
                List<QueueDef> q = new List<QueueDef>();
                foreach (string s in queueDefList.Keys)
                {
                    q.Add((QueueDef)queueDefList[s]);
                }
                XmlSerializer serializer = new XmlSerializer(q.GetType());
                TextWriter outFile = new StreamWriter(configFile);
                serializer.Serialize(outFile, q);
                outFile.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("a generic unprintable XML error occured");
                System.Console.WriteLine(e);
            }
        }

        public List<string> fetchQueueNames()
        {
            List<string> res = new List<string>();
            foreach (String s in queueDefList.Keys)
            {
                res.Add(s);
            }
            return res;
        }

        public void putInQueue(string name, string queue, bool isPrimary)
        {
            Membership tn = new Membership();
            tn.userName = name;
            tn.queueName = queue;
            tn.primary = isPrimary;
            tasNames.Add(tn);
        }

        public void removeFromQueue(string name, string queue)
        {
            tasNames.RemoveAll(m => (m.userName == name && m.queueName == queue));
        }


        public List<string> getTASNames(string queueName, bool isPrimary)
        {
            List<string> retval = new List<string>();
            foreach (Membership member in tasNames)
            {
                if (member.queueName.Equals(queueName) && member.primary.Equals(isPrimary))
                {
                    retval.Add(member.userName);
                }
            }
            return retval;
        }

        public List<string> getTASNames(string queueName)
        {
            List<string> retval = new List<string>();
            foreach (Membership member in tasNames)
            {
                if (member.queueName.Equals(queueName))
                {
                    retval.Add(member.userName);
                }
            }
            return retval;
        }

        public List<string> getTASNames()
        {
            List<string> retval = new List<string>();
            foreach (Membership member in tasNames)
            {
                if (!retval.Contains(member.userName))
                {
                    retval.Add(member.userName);
                }
            }
            return retval;
        }

        public int getQueueSlotDuration(string q)  
        {
            try 
            {
                int retVal = ((QueueDef)queueDefList[q]).slotDuration;
                return retVal;
            } 
            catch 
            {
                return 60;
            }
        }

        public DateTime getQueueStartTime(string q) 
        {
            try
            {
                DateTime retval = ((QueueDef)queueDefList[q]).startTime;
                return retval;
            }
            catch
            {
                return new DateTime(2013, 1, 1, 7, 0, 0);
            }
        }

        public DateTime getQueueEndTime(string q) 
        {
            try
            {
                DateTime retval = ((QueueDef)queueDefList[q]).endTime;
                return retval;
            }
            catch
            {
                return new DateTime(2013, 1, 1, 17, 0, 0);
            }
        }

        internal string getQueueOwnerName(string p)
        {
            try
            {
                string retval = ((QueueDef)queueDefList[p]).owner;
                return retval;
            }
            catch
            {
                return userName;
            }
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
            return tasNames.Count;
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

                foreach (string who in getTASNames())
                {
                    whoList.Add(new EWS.AttendeeInfo()
                    {
                        SmtpAddress = who,
                        AttendeeType = EWS.MeetingAttendeeType.Required
                    });
                }

                //fetch availability
                EWS.AvailabilityOptions opt = new EWS.AvailabilityOptions();
                opt.MeetingDuration = ((QueueDef)queueDefList[queueName]).slotDuration;
                opt.RequestedFreeBusyView = EWS.FreeBusyViewType.FreeBusy;
                EWS.TimeWindow tw = new EWS.TimeWindow(start, start.AddDays(1));
                freeBusy = service.GetUserAvailability(whoList, tw, EWS.AvailabilityData.FreeBusy, opt);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                //throw (e);
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
                    DateTime shiftEnd = shiftStart.AddMinutes(((QueueDef)queueDefList[queueName]).slotDuration);
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

        internal void updateQueueList(string p1, string p2, string p3, string p4, string p5)
        {
            try
            {
                //See if we can add the new information to an existing record
                ((QueueDef)queueDefList[p1]).owner = p2;
                ((QueueDef)queueDefList[p1]).startTime = timeStringToDate(p3);
                ((QueueDef)queueDefList[p1]).endTime = timeStringToDate(p4);
                ((QueueDef)queueDefList[p1]).slotDuration = stringToInt(p5);
            }
            catch
            {
                //We can't add, so we create
                QueueDef temp = new QueueDef
                {
                    queueName = p1,
                    owner = p2,
                    startTime = timeStringToDate(p3),
                    endTime = timeStringToDate(p4),
                    slotDuration = stringToInt(p5)
                };
                queueDefList.Add(p1, temp);
            }
        }

        private int stringToInt(string p5)
        {
            try
            {
                return Convert.ToInt32(p5);
            }
            catch
            {
                return 60;
            }
        }

        private DateTime timeStringToDate(string p3)
        {
            try
            {
                return DateTime.ParseExact(p3, "h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return new DateTime(2013, 1, 1, 1, 0, 0);
            }
        }
    }
}



