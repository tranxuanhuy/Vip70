using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TutorialConnectToAccessDB
{
    public class TcpForwarderSlim
    {
        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint remote;

        public IPEndPoint Remote
        {
            get { return remote; }
            set { remote = value; }
        }

        public TcpForwarderSlim(IPEndPoint iPEndPoint)
        {
            // TODO: Complete member initialization
            this.remote = iPEndPoint;
        }

        public TcpForwarderSlim()
        {
            // TODO: Complete member initialization
        }
        public void Start(IPEndPoint local)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(200);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TcpForwarderSlim();
                var state = new State(source, destination._mainSocket);
                do
                {
                    if (destination.Connect(remote, source))
                    {
                        break;
                    }
                } while (true);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }

        private bool Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            try
            {
                _mainSocket.Connect(remoteEndpoint);
                _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
                return true;
            }
            catch (Exception)
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
                return false;
            }
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }
    }
}
