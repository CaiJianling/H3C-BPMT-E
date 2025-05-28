using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3C_BPMT_E.Models
{
    public class Log
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public required DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
