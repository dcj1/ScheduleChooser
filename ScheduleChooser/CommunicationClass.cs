using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
        private EWS.ExchangeService service;
        private EWS.GetUserAvailabilityResults freeBusy;
        private Random r; // for testing purposes
        private Hashtable queueDefList;
        private List<Membership> tasNames;
        private List<Schedule> schedule;

        //temporary names for local xml files
        string configFile;
        string tasFile;
        string scheduleFile;

        /// <summary>
        /// Internal data structure that knows about queues
        /// </summary>
        public class QueueDef
        {
            public string queueName { get; set; }
            public string owner { get; set; }
            public int slotDuration { get; set; }
            public DateTime startTime { get; set; }
            public DateTime endTime { get; set; }
        }

        /// <summary>
        /// Internal data structure that knows about people
        /// </summary>
        public class Membership
        {
            public string queueName { get; set; }
            public string userName { get; set; }
            public bool primary { get; set; }
        }

        /// <summary>
        /// Internal data structure that represents a schedule slot
        /// </summary>
        public class Schedule
        {
            public string slot { get; set; }
            public string userName { get; set; }
            public string queue { get; set; }
            public DateTime time { get; set; }
            public bool approved { get; set; }
            public bool published { get; set; }
        }

        /// <summary>
        /// An exception class wrapper for when we get fancier
        /// </summary>
        public class QueueException : Exception, ISerializable
        {
            public QueueException()
                :base() {}
            public QueueException(String message)
                : base(message) { }
            public QueueException(string message, Exception inner)
                : base(message, inner) { }
            public QueueException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
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
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught: ", e);
            }

            // for testing purposes we define some local files and other stuff
            configFile = "config.xml";
            tasFile = "tas.xml";
            scheduleFile = "schedule.xml";
            r = new Random();

            //fire up the internal data structures
            InitQueueList();
            initQueueMembers();
            //initSchedule();
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
        /// Initialize the queue member list
        /// </summary>
        private void initQueueMembers()
        {
            ////Trial code to initialize without external data
            //tasNames = new List<Membership>();
            //List<string> tempNames = new List<string>();
            //tempNames.Add("hnguyen2@illumina.com");
            //tempNames.Add("ctetzlaff@illumina.com");
            //tempNames.Add("iwallace@illumina.com");
            //tempNames.Add("clarson@illumina.com");
            //tempNames.Add("jrogers@illumina.com");
            //foreach (string name in tempNames)
            //{
            //    Membership tn = new Membership();
            //    tn.userName = name;
            //    tn.queueName = "Bioinformatics";
            //    tn.primary = true;
            //    tasNames.Add(tn);
            //}

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Membership>));
                serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
                FileStream fs = new FileStream(tasFile, FileMode.Open);
                tasNames = (List<Membership>)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Initialize the queue list
        /// </summary>
        private void InitQueueList()
        {
            // //Trial code to initialize without external data
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
            }
        }

        /// <summary>
        /// Initialize the schedule with default dates
        /// </summary>
        private void initSchedule()
        {
            //initSchedule(DateTime.Today, DateTime.Today.AddDays(1));
        }

        /// <summary>
        /// Initialize the schedule with specific dates
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        private void initSchedule(DateTime begin, DateTime end)
        {
            //try
            //{
            //    XmlSerializer serializer = new XmlSerializer(typeof(List<Schedule>));
            //    serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            //    serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            //    FileStream fs = new FileStream(configFile, FileMode.Open);
            //    List<Schedule> full = (List<Schedule>)serializer.Deserialize(fs);
            //    foreach (Schedule s in full)
            //    {
            //        schedule.Add(s);
            //    }
            //    fs.Close();
            //}
            //catch (Exception e)
            //{
            //    System.Console.WriteLine(e);
            //}

        }

        /// <summary>
        /// Serializer error trap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serializer_UnknownNode (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        /// <summary>
        /// Serializer error trap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }

        /// <summary>
        /// Save the current queue structure to storage
        /// </summary>
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

        /// <summary>
        /// Save the current queue member list
        /// </summary>
        public void saveTASList()
        { 
            try
            {
                XmlSerializer serializer = new XmlSerializer(tasNames.GetType());
                TextWriter outFile = new StreamWriter(tasFile);
                serializer.Serialize(outFile, tasNames);
                outFile.Close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("a generic unprintable XML error occured");
                System.Console.WriteLine(e);
            }
        }

        /// <summary>
        /// external facing trigger to refresh the queue lists from storage
        /// </summary>
        public void reloadQueueList()
        {
            //capitalization conventions are for weenies
            //(post-hoc rationalization for being a sloppy idiot and not refactoring)
            InitQueueList();
            initQueueMembers();
            initSchedule();
        }

        /// <summary>
        /// return the current list of queue names
        /// </summary>
        /// <returns></returns>
        public List<string> fetchQueueNames()
        {
            List<string> res = new List<string>();
            foreach (String s in queueDefList.Keys)
            {
                res.Add(s);
            }
            return res;
        }

        /// <summary>
        /// update the current queue list
        /// </summary>
        /// <param name="name">Member name</param>
        /// <param name="queue">Queue name</param>
        /// <param name="isPrimary">Primary queue is true, secondary is false</param>
        public void putInQueue(string name, string queue, bool isPrimary)
        {
            Membership tn = new Membership();
            tn.userName = name;
            tn.queueName = queue;
            tn.primary = isPrimary;
            if (uniqueTAS(tn))
            {
                tasNames.Add(tn);
            }
            else
            {
                //if this is a duplicate, maybe we just quietly ignore it?
                throw new Exception(name + " already exists in queue " + queue);
            }
         }

        /// <summary>
        /// update all membership items for a name
        /// </summary>
        /// <param name="name">Member name</param>
        /// <param name="pQueue">Primary queue name</param>
        /// <param name="sQueues">Secondary queue name list</param>
        public void updateTASList(string name, string pQueue, List<String> sQueues)
        {
            Membership m = new Membership();
            m.userName = name;
            m.queueName = pQueue;
            m.primary = true;
            //if (!uniqueTAS(m))
            //{
                wipeOutMember(name);
            //}
            tasNames.Add(m);
            foreach (string s in sQueues)
            {
                m = new Membership();
                m.userName = name;
                m.queueName = s;
                m.primary = false;
                tasNames.Add(m);
            }
            saveTASList();
            reloadQueueList();
        }

        /// <summary>
        /// Clean out all mentions of a member
        /// </summary>
        /// <param name="name"></param>
        private void wipeOutMember(string name)
        {
            for (int i = tasNames.Count - 1; i >= 0; i--)
            {
                if (tasNames[i].userName.Equals(name))
                {
                    tasNames.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Test to see if the name/queue/primary tuple already exists
        /// </summary>
        /// <param name="m">Membership record</param>
        /// <returns></returns>
        private bool uniqueTAS(Membership m)
        {
            bool retval = true;
            int i = 0;
            while (retval && (i<tasNames.Count))
            {
                if (tasNames.ElementAt(i).userName.ToString().Equals(m.userName.ToString()) &&
                    tasNames.ElementAt(i).queueName.ToString().Equals(m.queueName.ToString()))
                {
                    retval = false;
                }
                i++;
            }
            return retval;
        }

        /// <summary>
        /// Remove a member from the queue
        /// </summary>
        /// <param name="name">Member name</param>
        /// <param name="queue">Queue name</param>
        public void removeFromQueue(string name, string queue)
        {
            tasNames.RemoveAll(m => (m.userName == name && m.queueName == queue));
        }

        /// <summary>
        /// Find out what the primary queue for this person is
        /// </summary>
        /// <param name="name">Member name</param>
        /// <returns>String representing the queue name</returns>
        public string getPrimaryQueue (string name)
        {
            foreach (Membership m in tasNames) 
            {
                if ((m.primary == true) && (m.userName.Equals(name)))
                {
                    //Implicit assumption that there is only one primary queue because of our
                    //previous integrity checks. If those integrity checks failed, we are only
                    //going to return the first queue encountered. 
                    return m.queueName;
                }
            }
            return "";
        }

        /// <summary>
        /// Find out what the secondary queues for this person are
        /// </summary>
        /// <param name="name">Member name</param>
        /// <returns>List of strings representing the queue names</returns>
        public List<string> getSecondaryQueues(string name)
        {
            List<string> retval = new List<string>();
            foreach (Membership m in tasNames)
            {
                if ((m.primary == false) && (m.userName.Equals(name)))
                {
                    retval.Add(m.queueName);
                }
            }
            return retval;
        }

        /// <summary>
        /// Find out who is in the queue filtered by primary or secondary responsibility
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <param name="isPrimary">true for primary queue members, false for secondary queue members</param>
        /// <returns>List of strings representing the queue members</returns>
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

        /// <summary>
        /// Find out who is associated with the queue in any manner
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <returns>List of strings representing the queue members</returns>
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

        /// <summary>
        /// get all the peoples!
        /// </summary>
        /// <returns>List of strings representing members</returns>
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

        /// <summary>
        /// How long are the shifts?
        /// </summary>
        /// <param name="q">Queue name</param>
        /// <returns>Integer representing the <em>minutes</em> the shift lasts</returns>
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

        /// <summary>
        /// When do the shifts start?
        /// </summary>
        /// <param name="q">Queue name</param>
        /// <returns>DateTime representing the shift start, only utilizing the hours & minutes field</returns>
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

        /// <summary>
        /// When do the shifts end?
        /// </summary>
        /// <param name="q">Queue name</param>
        /// <returns>DateTime representing the shift end, only utilizing the hours & minutes field</returns>
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

        /// <summary>
        /// Who owns this queue?
        /// </summary>
        /// <param name="p">Queue name</param>
        /// <returns>String representing queue owner name</returns>
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
            return getPrimaryQueue(userName);
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
                string queueName = fetchQueueName();
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
                string queueName = fetchQueueName();
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

        /// <summary>
        /// Sets all the queue values. Will create the queue record if it doesn't exist.
        /// </summary>
        /// <param name="p1">Queue name as string</param>
        /// <param name="p2">Owner name as string</param>
        /// <param name="p3">Start time as string in h:mm tt format</param>
        /// <param name="p4">End time as string in h:mm tt format</param>
        /// <param name="p5">duration time as string</param>
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
            saveQueueList();
        }

        /// <summary>
        /// Internal method to convert a string to an integer
        /// </summary>
        /// <param name="p5">String to convert</param>
        /// <returns>integer, with a guaranteed result return of 60</returns>
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

        /// <summary>
        /// Internal method to convert a string to a DateTime
        /// </summary>
        /// <param name="p3">String in h:mm tt format to convert</param>
        /// <returns>DateTime, with a guaranteed result of 1:00 AM</returns>
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



