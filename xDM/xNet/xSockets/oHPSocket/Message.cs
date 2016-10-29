using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.oHPSocket
{
    [Serializable]
    public class Message
    {
        public string Sender { get; set; }
        public string MessageGuid { get; set; }
        public DateTime Time { get; set; }
        public string Action { get; set; }
        public bool Result { get; set; }
        public string Value { get; set; }

        public Message()
        {
            this.Action = "";
            this.Result = false;
            this.Value = "";
            this.Sender = "";
            this.MessageGuid = Guid.NewGuid().ToString("N");
            this.Time = DateTime.MinValue;
        }
        public Message(Guid msgID)
        {
            this.Action = "";
            this.Result = false;
            this.Value = "";
            this.Sender = "";
            this.MessageGuid = msgID.ToString("N");
            this.Time = DateTime.MinValue;
        }
    }
}
