using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
namespace SpleefResurgence.uh
{
    public class BridgeWebsocket
    {
        public enum OperationType
        {
            OP_HELLO,
            OP_HEARTBEAT,
            OP_AUTHENTICATE,
            OP_DISPATCH,
            OP_DISCONNECT
        }

        public class BridgePacket
        {
            [JsonPropertyName("op")]
            public int Op { get; set; }

            [JsonPropertyName("t")]
            public string? Type { get; set; }

            [JsonPropertyName("d")]
            public JsonElement Data { get; set; }

            [JsonPropertyName("hb_interval")]
            public int HeartbeatInterval { get; set; }

            [JsonPropertyName("ack")]
            public bool Ack { get; set; }

            [JsonPropertyName("auth")]
            public bool Authenticated { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }

        private class PlayerInfo
        {
            public string AccountName;
            public string Name;
            public bool isOnline;
            public int? Associated_uid;
            public bool Notify_others;
            public DateTimeOffset Last_login_since;
            private Stopwatch afkTimer = new();

            public PlayerInfo(string accountName, string name, bool online, DateTimeOffset last_login_since)
            {
                AccountName = accountName;
                Name = name;
                isOnline = online;
                Associated_uid = GetUid();
                Notify_others = GetNotify();
                Last_login_since = last_login_since;
            }

            private int? GetUid()
            {
                // discord uid code thign yeah
                return null;
            }

            private bool GetNotify()
            {
                // same here
                return true;
            }

            public JsonElement ConvertToJson()
            {
                var d = new
                {
                    username = AccountName,
                    associated_uid = Associated_uid,
                    online = isOnline,
                    last_login_since = Last_login_since.ToUnixTimeSeconds(),
                    display_name = Name,
                    notify_others = Notify_others
                };
                return JsonSerializer.SerializeToElement(d);
            }
        }

        private class AuthStuff
        {
            public string code { get; set; }
            public string username { get; set; }
            public int expires_at { get; set; }
        }

        private readonly Dictionary<int, PlayerInfo> trackedPlayers = new();

        private readonly Spleef pluginInstance;

        private readonly string url;
        private readonly string ApiKey;

        private ClientWebSocket? socket;

        private CancellationTokenSource? cancellation;

        private bool authenticated = false;
        private bool disposed = false;

        private int HeartbeatInterval = 30000;

        public bool IsConnected => socket?.State == WebSocketState.Open && authenticated;

        private Task? HeartbeatTask;
        private Task? ReceiveTask;

        private CancellationTokenSource? _reconnectCts;
        private Task? ReconnectTask;

        private int ReconnectAttempts = 0;

        private BridgeHttpClient http;

        public BridgeWebsocket(string ip, int port, string apiKey, Spleef plugin)
        {
            url = $"ws://{ip}:{port}/hello";
            ApiKey = apiKey;

            http = new(ip, port, apiKey);

            pluginInstance = plugin;

            Commands.ChatCommands.Add(new Command("bridge.connect", ConnectCommand, "connectbridge", "cb"));
            Commands.ChatCommands.Add(new Command("bridge.disconnect", DisconnectCommand, "disconnectbridge", "dc"));

            Commands.ChatCommands.Add(new Command("spleef.settings", ConnectAccount, "verify"));

            ServerApi.Hooks.ServerJoin.Register(pluginInstance, OnPlayerJoin);
            PlayerHooks.PlayerPostLogin += OnPlayerLogin;
            ServerApi.Hooks.ServerLeave.Register(pluginInstance, OnPlayerLeave);
            _reconnectCts = new CancellationTokenSource();
            ReconnectTask = TryReconnect(_reconnectCts.Token);
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            cancellation = new CancellationTokenSource();

            socket = new ClientWebSocket();

            try
            {
                TShock.Log.ConsoleInfo($"Connecting to WebSocket server at {url}...");
                await socket.ConnectAsync(new Uri(url), cancellation.Token);
                TShock.Log.ConsoleInfo("Connected to the WebSocket server.");

                await WaitForHello();

                await Authenticate();

                authenticated = true;
                TShock.Log.ConsoleInfo("Authenticated to the WebSocket server.");

                await SyncUsers();

                HeartbeatTask = HeartbeatLoop();
                ReceiveTask = ReceiveLoop();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to the WebSocket server: {ex.Message}");
            }
        }

        private async Task WaitForHello()
        {
            var packet = await ReceivePacket(cancellation!.Token);
            if (packet == null)
                throw new InvalidOperationException("Failed to receive hello packet from the bridge.");
            try
            {
                if (packet.Op != (int)OperationType.OP_HELLO)
                    throw new InvalidOperationException("Expected hello packet from the bridge.");
                if (packet.HeartbeatInterval <= 0)
                    throw new InvalidOperationException("Invalid heartbeat interval received from the bridge.");
                HeartbeatInterval = packet.HeartbeatInterval;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse hello packet from the bridge.", ex);
            }
        }

        private async Task Authenticate()
        {
            if (socket == null || socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected.");

            await SendPacket(new
            {
                op = OperationType.OP_AUTHENTICATE,
                auth_key = ApiKey,
                id = "server"
            });

            BridgePacket? response = await ReceivePacket(cancellation.Token);

            if (response == null)
                throw new InvalidOperationException("Failed to receive authentication response from the bridge.");
            try
            {
                if (response.Op != (int)OperationType.OP_AUTHENTICATE)
                    throw new InvalidOperationException("Expected authentication response from the bridge.");

                if (!response.Authenticated)
                    throw new InvalidOperationException("false :(");

                if (response.Id != "server")
                    throw new InvalidOperationException("what how is the id not server??");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse authentication response from the bridge.", ex);
            }
        }

        private async Task SendPacket(object packet)
        {
            if (socket == null || socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected.");
            var jsonMessage = JsonSerializer.Serialize(packet);
            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            var buffer = new ArraySegment<byte>(messageBytes);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation!.Token);
#if DEBUG
            TShock.Log.ConsoleInfo($"Sent packet: {jsonMessage}");
#endif
        }

        private async Task<BridgePacket?> ReceivePacket(CancellationToken cancellation)
        {
            if (socket == null)
                return null;

            byte[] buffer = new byte[1024];
            while (true)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    TShock.Log.ConsoleInfo($"Connection closed :c. Reason: {result.CloseStatusDescription}");
                    await StopAsync();
                    return null;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

#if DEBUG
                    TShock.Log.ConsoleInfo($"Received packet: {message}");
#endif
                    return JsonSerializer.Deserialize<BridgePacket>(message);

                }
            }
        }

        private async Task SendEvent(string eventType, JsonElement data)
        {
            try
            {
                await SendPacket(new
                {
                    op = OperationType.OP_DISPATCH,
                    t = eventType,
                    d = data
                });
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Failed to send event '{eventType}': {ex.Message}");
            }
        }

        private async Task HeartbeatLoop()
        {
            try
            {
                while (!cancellation!.Token.IsCancellationRequested)
                {
                    await Task.Delay(HeartbeatInterval, cancellation.Token);
                    DateTime now = DateTime.UtcNow;
                    await SendPacket(new
                    {
                        op = OperationType.OP_HEARTBEAT,
                    });
                    DateTime after = DateTime.UtcNow;
                    TShock.Log.ConsoleInfo($"{(after - now).TotalMilliseconds} ms");
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Heartbeat loop error: {ex.Message}");
            }
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (!cancellation!.Token.IsCancellationRequested)
                {
                    BridgePacket? packet = await ReceivePacket(cancellation.Token);
                    if (packet == null)
                        break;
                    await HandlePacket(packet);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive loop error: {ex.Message}");
                await StopAsync();
            }
            authenticated = false;
        }

        private async Task HandlePacket(BridgePacket packet)
        {
            switch (packet.Op)
            {
                case (int)OperationType.OP_HEARTBEAT:
                    break;
                case (int)OperationType.OP_DISPATCH:
                    // dispatch stuff :p
                    break;
                case (int)OperationType.OP_DISCONNECT:
                    TShock.Log.ConsoleInfo("Bridge requested a disconnect.");
                    await StopAsync();
                    break;
                default:
                    TShock.Log.ConsoleError($"Received unhandled packet: {JsonSerializer.Serialize(packet)}");
                    break;
            }
        }
        /*
        private async void ConnectAccount(CommandArgs args)
        {
            var player = args.Player;

            if (player == null || !player.Active)
                return;

            if (!player.IsLoggedIn)
            {
                player.SendErrorMessage("You are not logged in!");
                return;
            }

            var d = new
            {
                username = player.Account.Name
            };

            player.SendInfoMessage("Getting the auth code...");

            try
            {
                AuthStuff? authCode = await http.PostAsync<object, AuthStuff>("codes", d);

                if (authCode == null)
                {
                    player.SendErrorMessage("something went wrong yo");
                    return;
                }

                var expiresIn = DateTimeOffset.FromUnixTimeSeconds(authCode.expires_at) - DateTimeOffset.Now;


                player.SendInfoMessage($"Here's your code: [c/ffffff:{authCode.code}], dm it to the epic discord bot, it expires in {expiresIn.Minutes} minutes!");
            }
            catch (HttpRequestException ex)
            {
                TShock.Log.ConsoleError(
                    $"Failed to request auth code: {ex.Message}"
                );

                player.SendErrorMessage(
                    "Could not connect to the bridge."
                );
            }
            catch (JsonException ex)
            {
                TShock.Log.ConsoleError(
                    $"Failed to parse auth code response: {ex.Message}"
                );

                player.SendErrorMessage(
                    "The bridge returned an invalid response."
                );
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(
                    $"Error while connecting account: {ex}"
                );

                player.SendErrorMessage(
                    "An error occurred while connecting your account."
                );
            }
        }
        */
        private async void ConnectAccount(CommandArgs args)
        {
            var player = args.Player;

            if (player == null || !player.Active)
                return;

            if (!player.IsLoggedIn)
            {
                player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (args.Parameters.Count != 1)
            {
                player.SendErrorMessage("Usage: /verify <code>");
                return;
            }

            string code = args.Parameters[0];

            var d = new
            {
                username = player.Account.Name
            };

            player.SendInfoMessage("Verifying the code...");
            try
            {
                await http.PostAsync($"codes/{code}/redeem", d);

                player.SendInfoMessage($"Verification done! Your account ([c/ffffff:{player.Account.Name} is now connected to your discord one!");
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(
                    $"Error while connecting account: {ex}"
                );

                player.SendErrorMessage(
                    "An error occurred while connecting your account."
                );
            }
        }

        private void OnPlayerJoin(JoinEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (player == null)
                return;

            trackedPlayers[player.Index] = new PlayerInfo($"<GUEST> {player.Name}", player.Name, true, DateTimeOffset.Now);

            if (!IsConnected)
                return;

            var d = trackedPlayers[player.Index].ConvertToJson();

            _ = SendEvent("PLAYER_JOIN", d);
        }

        private void OnPlayerLogin(PlayerPostLoginEventArgs e)
        {
            var player = e.Player;
            PlayerInfo playerInfo;
            if (trackedPlayers.ContainsKey(player.Index))
            {
                trackedPlayers[player.Index].AccountName = player.Account.Name;
                //trackedPlayers[player.Index].Last_login_since = DateTimeOffset.Now;
            }
            else
            {
                TShock.Log.ConsoleInfo($"Player with index {player.Index} not found in tracked players.");
                // this shouldn't happen at all anyway
                return;
            }

            if (!IsConnected)
                return;

            var d_register = new
            {
                username = player.Account.Name,
                display_name = player.Name,
                online = true,
                last_login_since = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            _ = Task.Run(async () =>
            {
                await SendEvent("PLAYER_REGISTER", JsonSerializer.SerializeToElement(d_register));
            });
        }
        
        private void OnPlayerLeave(LeaveEventArgs args)
        {
            if (trackedPlayers.ContainsKey(args.Who))
            {
                trackedPlayers[args.Who].isOnline = false;
            }
            else
            {
                TShock.Log.ConsoleInfo($"Player with index {args.Who} not found in tracked players.");
                return;
            }

            var d = new
            {
                username = trackedPlayers[args.Who].AccountName,
                online = trackedPlayers[args.Who].isOnline
            };

            trackedPlayers.Remove(args.Who);

            if (!IsConnected)
                return;

            _ = SendEvent("PLAYER_LEAVE", JsonSerializer.SerializeToElement(d));
        }

        public async Task SyncUsers()
        {
            if (!IsConnected)
                return;
            foreach (var player in TShock.Players)
            {
                if (player == null || !player.Active)
                    continue;
                if (!trackedPlayers.ContainsKey(player.Index))
                {
                    if (!player.IsLoggedIn)
                        trackedPlayers[player.Index] = new PlayerInfo($"<GUEST> {player.Name}", player.Name, true, DateTimeOffset.Now);
                    else
                        trackedPlayers[player.Index] = new PlayerInfo(player.Account.Name, player.Name, true, DateTimeOffset.Now);
                }
            }
            List<JsonElement> playerjsons = trackedPlayers.Values.ToList().Select(p => p.ConvertToJson()).ToList();
            TShock.Log.ConsoleInfo(JsonSerializer.Serialize(playerjsons));
            _ = SendEvent("PLAYER_SYNC", JsonSerializer.SerializeToElement(playerjsons));
        }

        public async Task TryReconnect(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(10000, cancellationToken);
                    if (!IsConnected)
                    {
                        if (ReconnectAttempts > 5)
                        {
                            ReconnectAttempts = 0;
                            TShock.Log.ConsoleInfo("Too many attempts on trying to reconnect, the ws server is either offline or the info is incorrect. You will have to manually reconnect with /cb");
                            StopReconnect();
                        }
                        ReconnectAttempts++;
                        TShock.Log.ConsoleInfo("Trying to reconnect...");
                        await Task.Delay(1000, cancellationToken);
                        await ConnectAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                TShock.Log.ConsoleInfo("Automatic reconnect cancelled.");
            }
        }

        public void ConnectCommand(CommandArgs args)
        {
            if (IsConnected)
            {
                args.Player.SendInfoMessage("Already connected to the bridge.");
                return;
            }
            args.Player.SendInfoMessage("Attempting to reconnect to the bridge...");

            StartReconnect();

            _ = Task.Run(async () =>
            {
                await ConnectAsync();
            });
        }

        public void DisconnectCommand(CommandArgs args)
        {
            StopReconnect();

            if (!IsConnected)
            {
                args.Player.SendInfoMessage("Not connected to the bridge.");
                return;
            }
            args.Player.SendInfoMessage("Disconnecting from the bridge...");

            _ = Task.Run(async () =>
            {
                await StopAsync();
            });
        }

        private void StartReconnect()
        {
            if (_reconnectCts != null)
                return;

            _reconnectCts = new CancellationTokenSource();

            ReconnectTask = TryReconnect(_reconnectCts.Token);
        }

        private void StopReconnect()
        {
            _reconnectCts?.Cancel();
            _reconnectCts?.Dispose();

            _reconnectCts = null;
            ReconnectTask = null;
        }

        public async Task StopAsync()
        {
            authenticated = false;

            if (cancellation != null)
            {
                cancellation.Cancel();
            }
            TShock.Log.ConsoleInfo("WebSocket connection closed w.");
            if (socket != null && socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                TShock.Log.ConsoleInfo("WebSocket connection closed.");

                socket.Dispose();
                socket = null;
            }

            cancellation?.Dispose();
            cancellation = null;

            ReceiveTask = null;
            HeartbeatTask = null;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            StopAsync()
                .GetAwaiter()
                .GetResult();
        }
    }
}
