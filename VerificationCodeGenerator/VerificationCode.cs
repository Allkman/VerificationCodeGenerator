using System;

namespace VerificationCodeGenerator
{
    internal class VerificationCode
    {
        public Guid ID { get; set; }
        public int ThreadID { get; set; }
        public DateTime Time { get; set; }
        public string Data { get; set; }
    }
}
