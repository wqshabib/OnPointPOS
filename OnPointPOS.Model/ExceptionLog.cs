using System;

namespace POSSUM.Model
{
    public class ExceptionLog
    {
        public ExceptionLog()
        {
            CreatedOn = DateTime.Now;
            Synced = false;
        }

        public long Id { get; set; }
        public string ExceptiontText { get; set; }
        public  DateTime CreatedOn { get; set; }
        public  Guid TerminalId { get; set; }
        public bool Synced { get; set; }
    }
}