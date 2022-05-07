using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using VoiceEngine.Network.Abstractions.Packets;
using VoiceEngine.Network.Abstractions.Server;

namespace VoiceEngine.Network.Server
{
    public class Broadcaster : IBroadcaster
    {
        private readonly IConnectionManager _connectionManager;

        public Broadcaster(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task ToAllExcept(string exceptUserAccountId, Packet packet)
        {
            var otherRecipents = _connectionManager.Get()
               .Where(x => x.AccountId != exceptUserAccountId)
               .ToList();

            foreach (var recipient in otherRecipents)
            {
                try
                {
                    await recipient.Socket.Send(packet);
                }
                catch(WebSocketException)
                {
                    _connectionManager.Remove(recipient.AccountId);
                    continue;
                }
            }
        }

        public async Task ToAllExcept(string exceptUserAccountId, byte[] packet)
        {
            var otherRecipents = _connectionManager.Get()
              .Where(x => x.AccountId != exceptUserAccountId)
              .ToList();

            foreach (var recipient in otherRecipents)
            {
                try
                {
                    await recipient.Socket.Send(packet);
                }
                catch (WebSocketException)
                {
                    _connectionManager.Remove(recipient.AccountId);
                    continue;
                }
            }
        }

        public Task ToChannel(string channelId, Packet packet)
        {
            throw new NotImplementedException();
        }

        public Task ToChannel(string channelId, byte[] packet)
        {
            throw new NotImplementedException();
        }

        public async Task ToUser(string userAccountId, Packet packet)
        {
            var targetConnection = _connectionManager.Get(userAccountId);

            try
            {
                await targetConnection.Socket.Send(packet);
            }
            catch (WebSocketException)
            {
                _connectionManager.Remove(targetConnection.AccountId);
            }
        }

        public Task ToUser(string userAccountId, byte[] packet)
        {
            var targetConnection = _connectionManager.Get(userAccountId);

            return targetConnection.Socket.Send(packet);
        }
    }
}
