using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets
{
    [Serializable]
    public class Message
    {
        private byte[] _guid { get; set; }
        public string Action { get; set; }
        public bool Result { get; set; }
        public object Value { get; set; }

        public Message() { }

        public Message(Guid msgID)
        {
            this._guid = msgID.ToByteArray();
        }

        public void SetGuid(Guid gid)
        {
            _guid = gid.ToByteArray();
        }

        public Guid GetGuid()
        {
            if(_guid != null)
                return new Guid(_guid);
            return Guid.Empty;
        }
    }
}
