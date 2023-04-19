﻿using System.Threading.Tasks;
using BossBase;
using Helper;
using Logger;

namespace BossCommand
{
    public class BCSendMail: ABossCommand
    {
        public BossMail BossMail { get; set; }

        public BCSendMail(IMessageChannel iMessageChannel): base(iMessageChannel)
        {
        }

        public override async Task<object> DoAsync()
        {
            this.CommandString = string.Format("send_mail --json {0} ",
                    MongoHelper.ToJson(this.BossMail));
            Log.Trace(this.CommandString);
            this.SendMessage(new CMSG_Boss_Gm { Message = this.CommandString });
            var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
            return smsgBossCommandResponse.ErrorCode;
        }
    }
}