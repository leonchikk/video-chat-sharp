using System;
using System.Threading.Tasks;
using System.Windows.Input;
using VoiceChat.Desktop.Commands;
using VoiceChat.Desktop.Stores;
using VoiceEngine.Network.Abstractions;
using VoiceEngine.Network.Abstractions.Clients;
using VoiceEngine.Network.Abstractions.EventArgs;
using VoiceEngine.Network.Abstractions.Services;

namespace VoiceChat.Desktop.ViewModels
{
    public class SplashViewModel : ViewModelBase
    {
        private ISocketClient _socketClient;
        private IRestClient _restClient;
        private ITokenService _tokenService;

        public event Action OnInitHandshake;

        public SplashViewModel(
            ISocketClient socketClient,
            IRestClient restClient,
            ITokenService tokenService)
        {
            _socketClient = socketClient;
            _restClient = restClient;
            _tokenService = tokenService;
            _socketClient.OnMessage += SocketClient_OnMessage;
        }

        private void SocketClient_OnMessage(NetworkMessageReceivedEventArgs e)
        {
            switch (e.PacketType)
            {
                case PacketTypeEnum.InitHandshake:

                    OnInitHandshake?.Invoke();

                    break;

                default:
                    break;
            }
        }

        public async Task Connect()
        {
            string token = await _restClient.GetAuthorizationToken();

            UserStore.AccountId = _tokenService.GetAccountId(token);

            await _socketClient.Connect(token);
        }

        public new void Dispose()
        {
            _socketClient.OnMessage -= SocketClient_OnMessage;
        }
    }
}
