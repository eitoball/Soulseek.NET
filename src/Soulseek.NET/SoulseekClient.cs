﻿namespace Soulseek.NET
{
    using Soulseek.NET.Messaging;
    using Soulseek.NET.Messaging.Requests;
    using Soulseek.NET.Messaging.Responses;
    using Soulseek.NET.Tcp;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SoulseekClient
    {
        public SoulseekClient(string address = "server.slsknet.org", int port = 2242)
        {
            Address = address;
            Port = port;

            Connection = new Connection(ConnectionType.Server, Address, Port);
            Connection.StateChanged += OnConnectionStateChanged;
            Connection.DataReceived += OnConnectionDataReceived;
        }

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<MessageReceivedEventArgs> UnknownMessageRecieved;
        public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;
        public event EventHandler<SearchResultReceivedEventArgs> SearchResultReceived;

        public string Address { get; private set; }
        public Connection Connection { get; private set; }
        public int Port { get; private set; }

        public IEnumerable<Room> Rooms { get; private set; }
        public int ParentMinSpeed { get; private set; }
        public int ParentSpeedRatio { get; private set; }
        public int WishlistInterval { get; private set; }
        public IEnumerable<string> PrivilegedUsers { get; private set; }

        private MessageWaiter MessageWaiter { get; set; } = new MessageWaiter();

        private List<Connection> PeerConnections { get; set; } = new List<Connection>();
        
        public async Task ConnectAsync()
        {
            await Connection.ConnectAsync();
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var request = new LoginRequest(username, password);

            var login = MessageWaiter.Wait(MessageCode.ServerLogin).Task;
            var roomList = MessageWaiter.Wait(MessageCode.ServerRoomList).Task;
            var parentMinSpeed = MessageWaiter.Wait(MessageCode.ServerParentMinSpeed).Task;
            var parentSpeedRatio = MessageWaiter.Wait(MessageCode.ServerParentSpeedRatio).Task;
            var wishlistInterval = MessageWaiter.Wait(MessageCode.ServerWishlistInterval).Task;
            var privilegedUsers = MessageWaiter.Wait(MessageCode.ServerPrivilegedUsers).Task;

            await Connection.SendAsync(request.ToMessage().ToByteArray());

            Task.WaitAll(login, roomList, parentMinSpeed, parentSpeedRatio, wishlistInterval, privilegedUsers);

            Rooms = ((RoomListResponse)roomList.Result).Rooms;
            ParentMinSpeed = ((IntegerResponse)parentMinSpeed.Result).Value;
            ParentSpeedRatio = ((IntegerResponse)parentSpeedRatio.Result).Value;
            WishlistInterval = ((IntegerResponse)wishlistInterval.Result).Value;
            PrivilegedUsers = ((PrivilegedUsersResponse)privilegedUsers.Result).PrivilegedUsers;

            return (LoginResponse)login.Result;
        }

        public async Task SearchAsync(string searchText)
        {
            var request = new SearchRequest(searchText, 1);
            Console.WriteLine($"Searching for {searchText}...");
            await Connection.SendAsync(request.ToMessage().ToByteArray());
        }

        private async void OnConnectionDataReceived(object sender, DataReceivedEventArgs e)
        {
            Task.Run(() => DataReceived?.Invoke(this, e)).Forget();
            await HandleMessage(new Message(e.Data));
        }

        private async Task HandleMessage(Message message)
        {
            Task.Run(() => MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Message = message })).Forget();
            
            if (new MessageMapper().TryMapResponse(message, out var mappedResponse))
            {
                MessageWaiter.Complete(message.Code, mappedResponse);

                var eventArgs = new ResponseReceivedEventArgs()
                {
                    Message = message,
                    ResponseType = mappedResponse.GetType(),
                    Response = mappedResponse,
                };
                
                Task.Run(() => ResponseReceived?.Invoke(this, eventArgs)).Forget();
            }
            else
            {
                Task.Run(() => UnknownMessageRecieved?.Invoke(this, new MessageReceivedEventArgs() { Message = message })).Forget();
            }

            if (mappedResponse is ConnectToPeerResponse connectToPeerResponse)
            {
                await HandleConnectToPeerResponse(connectToPeerResponse);
            }

            if (mappedResponse is PeerSearchReplyResponse peerSearchReplyResponse)
            {
                await HandlePeerSearchReplyResponse(peerSearchReplyResponse);
            }
        }

        private async Task HandlePeerSearchReplyResponse(PeerSearchReplyResponse peerSearchReplyResponse)
        {
            if (peerSearchReplyResponse.FileCount > 0)
            {
                var eventArgs = new SearchResultReceivedEventArgs() { Response = peerSearchReplyResponse };
                Task.Run(() => SearchResultReceived?.Invoke(this, eventArgs)).Forget();
            }
        }

        private async Task HandleConnectToPeerResponse(ConnectToPeerResponse connectToPeerResponse)
        {
            var connection = new Connection(ConnectionType.Peer, connectToPeerResponse.IPAddress.ToString(), connectToPeerResponse.Port);
            PeerConnections.Add(connection);

            connection.DataReceived += OnConnectionDataReceived;
            connection.StateChanged += OnPeerConnectionStateChanged;

            try
            {
                await connection.ConnectAsync();

                var request = new PierceFirewallRequest(connectToPeerResponse.Token);
                await connection.SendAsync(request.ToByteArray(), suppressCodeNormalization: true);
            }
            catch (ConnectionException ex)
            {
                Console.WriteLine($"Failed to connect to Peer {connectToPeerResponse.Username}@{connectToPeerResponse.IPAddress}: {ex.Message}");
            }
        }

        private void OnPeerConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Console.WriteLine($"\tPeer Connection State Changed: {e.State} ({e.Message ?? "Unknown"})");
        }

        private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            Console.WriteLine($"Connection State Changed: {e.State} ({e.Message ?? "Unknown"})");
            ConnectionStateChanged?.Invoke(this, e);
        }
    }
}