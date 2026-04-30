using System;
using System.Collections.Generic;
using System.Text;

namespace LNUBookShare.Domain.Entities
{
    public static class TransactionStatuses
    {
        public const string Active = "active";
        public const string Returned = "returned";
        public const string Overdue = "overdue";
    }
}
