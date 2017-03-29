using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.DebugUtils;
using Common.Message;
using Common.Schema;

namespace GameMaster.Net
{
    class BehaviorChooser/*: IMessageHandler<ConfirmGameRegistration>*/
    {
        

        public static void HandleMessage(ConfirmGameRegistration message)
        {
            ConsoleDebug.Warning("BehaviorChooser.ConfirmGameRegistation Here IMPLEMENTE ME");
        }



        public static void HandleMessage(object message)
        {
            ConsoleDebug.Warning("Unknown Type");
        }

        
    }
}
