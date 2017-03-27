using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Message
{
    /// <summary>
    /// Interface for message handlers
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    public interface IMessageHandler<T>
    {
        void HandleMessage(T msg);
    }
}
