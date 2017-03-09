﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Connection;
using Common.EventArg;

namespace MockGameMaster
{
    public class AsynchronousClient
    {
        private IConnection connection;
        private Socket socket;

        public AsynchronousClient(IConnection connection)
        {
            this.connection = connection;
            connection.OnConnection += OnConnection;
            connection.OnMessageRecieve += OnMessageReceive;
            connection.OnMessageSend += OnMessageSend;
        }

        public void Connect()
        {
            connection.StartClient();
        }

        private void OnConnection(object sender, ConnectEventArgs eventArgs)
        {
            //TODO extension method to get address from socket
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("Successful connection with address {0}", address.ToString());
            socket = eventArgs.Handler as Socket;
        }

        private void OnMessageReceive(object sender, MessageRecieveEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
            Console.WriteLine("New message received from {0}: {1}", address.ToString(), eventArgs.Message);
        }

        private void OnMessageSend(object sender, MessageSendEventArgs eventArgs)
        {
            var address = (eventArgs.Handler.RemoteEndPoint as IPEndPoint).Address;
             Console.WriteLine("New message sent to {0}", address.ToString());
            
        }

        public void Send(string data)
        {
            connection.Send(socket, data);
        }
                  


    }//class
}//namespace
