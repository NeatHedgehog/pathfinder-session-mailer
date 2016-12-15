using System.IO;
using System;
using System.Collections.Generic;

namespace AutoMailerApp
{
    public class AttendanceLog
    {
        public string configPath;
        public AttendanceRecord[] attendanceRecords;

        public AttendanceLog(string _configPath)
        {
            if (!File.Exists(_configPath))
            {
                File.CreateText(_configPath).Dispose();
            }

            configPath = _configPath;

            string[] masterLog = File.ReadAllLines(configPath);
            List<AttendanceRecord> tempLogs = new List<AttendanceRecord>();

            //0: SessionGuid
            //1: RSVPGUID
            //2: MemberEmail
            //3: GCC6
            //4: SN
            //5: CancelReason
            //6: MemberName

            for (int i = 0; i < masterLog.Length; i++)
            {
                string[] logDetails = masterLog[i].Split('|');

                char sn = Convert.ToChar(logDetails[4]);

                tempLogs.Add(new AttendanceRecord(Guid.Parse(logDetails[0]), Guid.Parse(logDetails[1]), logDetails[2], logDetails[6], logDetails[3], sn, logDetails[5]));
            }

            attendanceRecords = tempLogs.ToArray();
        }

        public Guid CreateAttendanceLogEntry(string _memberEmail, string _memberName, Guid _sessionID)
        {
            Guid rsvpGuid = Guid.NewGuid();

            AttendanceRecord newRecord = new AttendanceRecord(_sessionID, rsvpGuid, _memberEmail, _memberName, "NR", 'X', "X");
            string newRecordString = _sessionID.ToString() + "|" + rsvpGuid.ToString() + "|" + _memberEmail + "|NR|X|X|" + _memberName;

            //Write the new logs to the log file
            File.AppendAllLines(configPath, new List<string>(){newRecordString});

            //Write the new RSVP logs to the logs in memory
            List<AttendanceRecord> _tempRecords = new List<AttendanceRecord>(attendanceRecords);
            _tempRecords.Add(newRecord);
            attendanceRecords = _tempRecords.ToArray();

            return rsvpGuid;
        }
    }

    public class AttendanceRecord
    {
        public Guid sessionID;
        public Guid rsvpGUID;
        public string memberName;
        public string memberEmail;
        /// <summary>
        /// going, canceled, canceled in under 6hrs,
        /// </summary>
        public string gcc6;
        /// <summary>
        /// show,noshow
        /// </summary>
        public char sn;
        public string cancelReason;

        public AttendanceRecord(Guid _sessionID, Guid _rsvpGUID, string _memberEmail, string _memberName, string _gcc6, char _sn, string _cancelReason)
        {
            sessionID = _sessionID;
            rsvpGUID = _rsvpGUID;
            memberEmail = _memberEmail;
            memberName = _memberName;
            gcc6 = _gcc6;
            sn = _sn;
            cancelReason = _cancelReason;
        }
    }

}