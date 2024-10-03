using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{

    public class Transaction
    {
        public int? TransactionId { get; set; }
        public int? M_Id { get; set; }
        public EventType? EventType { get; set; }
        public DateTime? Timestamp { get; set; }
        public TransactionStatus? Status { get; set; }

    }

    public enum TransactionStatus
    {
        Success,
        Failure
    }

    public enum EventType
    {
        Ping,
        TelNet
    }
}