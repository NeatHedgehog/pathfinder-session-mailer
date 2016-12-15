using System;
using System.Collections.Generic;
using System.IO;

namespace AutoMailerApp
{
    public class ScheduleEntry
    {
        public DateTime sessionDate;
        /// <summary>
        /// Active, Canceled, Done
        /// </summary>
        public char acd;
        public Guid id;
        public string[] sessionMembers;
        public string[] sessionMemberEmails;
        public bool reminderSent;

        public ScheduleEntry()
        {
            sessionDate = new DateTime();
            acd = 'a';
            id = Guid.NewGuid();
            sessionMembers = null;
        }

        public ScheduleEntry(DateTime _sessionDate, char _acd, string[] _sessionMembers, string[] _sessionMemberEmails, Guid _id, bool _reminderSent)
        {
            sessionMembers = new string[_sessionMembers.Length];
            sessionMemberEmails = new string[_sessionMemberEmails.Length];

            sessionDate = _sessionDate;
            acd = _acd;
            _sessionMembers.CopyTo(sessionMembers, 0);
            _sessionMemberEmails.CopyTo(sessionMemberEmails, 0);
            id = _id;
            reminderSent = _reminderSent;
        }

        public void Cancel()
        {
            acd = 'c';
        }

        public void Activate()
        {
            acd = 'a';
        }

        public void Finish()
        {
            acd = 'd';
        }

        public void Reschedule(DateTime _sessionDate)
        {
            sessionDate = _sessionDate;
        }

        public void RemoveMember(string _member, string _memberEmail)
        {
            List<string> _tempMembers = new List<string>(sessionMembers);
            List<string> _tempMemberEmails = new List<string>(sessionMemberEmails);

            _tempMembers.Remove(_member);
            _tempMemberEmails.Remove(_memberEmail);

            sessionMembers = _tempMembers.ToArray();
            sessionMemberEmails = _tempMemberEmails.ToArray();
        }

        public void AddMember(string _member, string _memberEmail)
        {
            List<string> _tempMembers = new List<string>(sessionMembers);
            List<string> _tempMemberEmails = new List<string>(sessionMemberEmails);

            _tempMembers.Add(_member);
            _tempMemberEmails.Add(_memberEmail);

            sessionMembers = _tempMembers.ToArray();
            sessionMemberEmails = _tempMemberEmails.ToArray();
        }

        public string GetMembers
        {
            get
            {
                string _members = "";

                for (int i = 0; i < sessionMembers.Length; i++)
                {
                    _members += "," + sessionMembers[0];
                }

                _members = _members.Substring(1);

                return _members;
            }
        }

        public string GetEmails
        {
            get
            {
                string _emails = "";

                for (int i = 0; i < sessionMemberEmails.Length; i++)
                {
                    _emails += "," + sessionMemberEmails[0];
                }

                _emails = _emails.Substring(1);

                return _emails;
            }
        }
    }

    public class Schedule
    {
        public ScheduleEntry[] scheduleEntries;
        private string configPath;

        public void RemoveSession(Guid _id)
        {
            List<ScheduleEntry> _tempSchedule = new List<ScheduleEntry>(scheduleEntries);

            _tempSchedule.RemoveAt(_tempSchedule.FindIndex(a => a.id == _id));

            scheduleEntries = _tempSchedule.ToArray();
        }

        public Schedule(string _configPath)
        {
            if (!File.Exists(_configPath))
            {
                File.CreateText(_configPath).Dispose();
            }

            configPath = _configPath;
            scheduleEntries = null;

            string[] masterSchedule = File.ReadAllLines(configPath);
            List<ScheduleEntry> tempSchedule = new List<ScheduleEntry>();

            for (int i = 0; i < masterSchedule.Length; i++)
            {
                    //0: GUID
                    //1: DateTime
                    //2: ACD (Active Canceled Done)
                    //3: Names
                    //4: Emails
                    //5: ReminderSent

                    string[] entryDetails = masterSchedule[i].Split('|');

                    string[] memberNames = entryDetails[3].Split(',');
                    string[] memberEmails = entryDetails[4].Split(',');

                    char acd = Convert.ToChar(entryDetails[2]);

                    bool reminderSent = false;

                    if (entryDetails[5] == "y")
                    {
                        reminderSent = true;
                    }

                    tempSchedule.Add(new ScheduleEntry(Convert.ToDateTime(entryDetails[1]), acd, memberNames, memberEmails, Guid.Parse(entryDetails[0]), reminderSent));
            }

            scheduleEntries = tempSchedule.ToArray();
        }

        public ScheduleEntry[] GetActiveScheduleEntries
        {
            get
            {
                List<ScheduleEntry> _tempResults = new List<ScheduleEntry>();

                for (int i = 0; i < scheduleEntries.Length; i++)
                {
                    if (scheduleEntries[i].acd == 'a')
                    {
                        _tempResults.Add(scheduleEntries[i]);
                    }
                }

                return _tempResults.ToArray();
            }
        }

        public void WriteNewEntry(ScheduleEntry _newEntry)
        {
            string members = "";
            string emails = "";

            for (int i = 0; i < _newEntry.sessionMembers.Length; i++)
            {
                members += "," + _newEntry.sessionMembers[0];
                emails += "," + _newEntry.sessionMemberEmails[0];
            }

            members = members.Substring(1);
            emails = emails.Substring(1);

            char sentReminder = 'n';

            if (_newEntry.reminderSent)
            {
                sentReminder = 'y';
            }

            string newEntry = _newEntry.id.ToString() + "|" + _newEntry.sessionDate.ToString("g") + "|" + _newEntry.acd.ToString() + "|" + members + "|" + emails + "|" + sentReminder.ToString();

            // Write the new entry to the config file
            File.AppendAllLines(configPath, new List<string>() {newEntry});

            // Add the new entry to the copy of the config file in memory
            List<ScheduleEntry> _tempSchedule = new List<ScheduleEntry>(scheduleEntries);
            _tempSchedule.Add(_newEntry);
            scheduleEntries = _tempSchedule.ToArray();
        }

        public void SetReminderSent(Guid _sessionID)
        {
            //update schedule file
            string[] masterSchedule = File.ReadAllLines(configPath);
            List<ScheduleEntry> tempSchedule = new List<ScheduleEntry>();

            for (int i = 0; i < masterSchedule.Length; i++)
            {
                    //0: GUID
                    //1: DateTime
                    //2: ACD (Active Canceled Done)
                    //3: Names
                    //4: Emails
                    //5: ReminderSent

                    string[] entryDetails = masterSchedule[i].Split('|');

                    string[] memberNames = entryDetails[3].Split(',');
                    string[] memberEmails = entryDetails[4].Split(',');

                    char acd = Convert.ToChar(entryDetails[2]);

                    bool reminderSent = false;

                    if (entryDetails[5] == "y")
                    {
                        reminderSent = true;
                    }

                    if (Guid.Parse(entryDetails[0]) != _sessionID)
                    {
                        tempSchedule.Add(new ScheduleEntry(Convert.ToDateTime(entryDetails[1]), acd, memberNames, memberEmails, Guid.Parse(entryDetails[0]), reminderSent));
                    }

                    else
                    {
                        tempSchedule.Add(new ScheduleEntry(Convert.ToDateTime(entryDetails[1]), acd, memberNames, memberEmails, Guid.Parse(entryDetails[0]), true));
                    }
            }

            File.WriteAllText(configPath,String.Empty);

            foreach (ScheduleEntry s in tempSchedule)
            {
                WriteNewEntry(s);
            }

            scheduleEntries = tempSchedule.ToArray();
        }
    }
}