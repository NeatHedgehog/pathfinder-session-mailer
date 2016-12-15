using System;

namespace AutoMailerApp
{
    public class Program
    {
        public static Schedule sessionSchedule;
        public static AttendanceLog attendanceLogs;

        public static void Main(string[] args)
        {
            sessionSchedule = new Schedule("/home/dockeruser/AutoMailerSystemFiles/masterSchedule.txt");
            attendanceLogs = new AttendanceLog("/home/dockeruser/AutoMailerSystemFiles/masterAttendance.txt");

            if (args.Length > 0)
            {
                if (args[0] == "SendMail")
                {
                    SendMail();
                }

                if (args[0] == "NewEntry")
                {
                    NewEntry();
                }

                if (args[0] == "RSVP")
                {
                    RSVP(Guid.Parse(args[1]), args[2]);
                }
            }

            else
            {
                //NewEntry();
                SendMail();
            }
        }

        public static void NewEntry()
        {
            string[] memberNames = new string[] { "Carl" };
            string[] memberEmails = new string[] {"zacdalton@gmail.com"};

            ScheduleEntry _example = new ScheduleEntry(Convert.ToDateTime("12/9/2016 1:30 PM"), 'a', memberNames, memberEmails, Guid.NewGuid(), false);
            sessionSchedule.WriteNewEntry(_example);
        }

        public static void SendMail()
        {
            ScheduleEntry[] activeEntries = sessionSchedule.GetActiveScheduleEntries;

            for (int i = 0; i < activeEntries.Length; i++)
            {
                if (!activeEntries[i].reminderSent
                    && activeEntries[i].sessionDate.AddDays(-5) < DateTime.Now)
                {
                    //Console.WriteLine("Hello World! I'm sending some mail!");

                    for (int i2 = 0; i2 < activeEntries[i].sessionMemberEmails.Length; i2++)
                    {
                        Guid rsvpGuid = attendanceLogs.CreateAttendanceLogEntry(activeEntries[i].sessionMemberEmails[i2], activeEntries[i].sessionMembers[i2], activeEntries[i].id);

                        AutoMail.SendMail(activeEntries[i].sessionMembers[i2], activeEntries[i].sessionMemberEmails[i2], activeEntries[i].sessionDate.ToString("g"), rsvpGuid);
                    }

                    sessionSchedule.SetReminderSent(activeEntries[i].id);
                }
            }
        }

        public static void RSVP(Guid _sessionGuid, string _memberEmail)
        {

        }
    }
}
