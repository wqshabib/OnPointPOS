using POSSUM.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace POSSUM.ApiModel
{
    public class JournalApi
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ItemId { get; set; }
        public int? TableId { get; set; }
        public int ActionId { get; set; }
        public DateTime Created { get; set; }
        [MaxLength(250)]
        public string LogMessage { get; set; }
        public Guid? TerminalId { get; set; }

        public static JournalApi ConvertModelToApiModel(Journal journal)
        {
            return new JournalApi
            {
                Id = journal.Id,
                UserId = journal.UserId,
                OrderId = journal.OrderId,
                ItemId = journal.ItemId,
                TableId = journal.TableId,
                ActionId = journal.ActionId,
                Created = journal.Created,
                LogMessage = journal.LogMessage,
                TerminalId = journal.TerminalId,
            };
        }

        public static Journal ConvertApiModelToModel(JournalApi journal)
        {
            return new Journal
            {
                //Id = journal.Id,
                UserId = journal.UserId,
                OrderId = journal.OrderId,
                ItemId = journal.ItemId,
                TableId = journal.TableId,
                ActionId = journal.ActionId,
                Created = journal.Created,
                LogMessage = journal.LogMessage,
                TerminalId = journal.TerminalId,
            };
        }

        public static Journal UpdateModel(Journal dbObject, JournalApi journal)
        {
            dbObject.UserId = journal.UserId;
            dbObject.OrderId = journal.OrderId;
            dbObject.ItemId = journal.ItemId;
            dbObject.TableId = journal.TableId;
            dbObject.ActionId = journal.ActionId;
            dbObject.Created = journal.Created;
            dbObject.LogMessage = journal.LogMessage;
            dbObject.TerminalId = journal.TerminalId;

            return dbObject;
        }
    }
}