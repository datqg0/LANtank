using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace gametankz.Network
{
    public class NetworkClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private GameState currentState = new();
        private bool isConnected = false;
        private Thread receiveThread;

        public GameState CurrentState => currentState;
        public bool IsConnected => isConnected;

        public NetworkClient(string ip = "127.0.0.1", int port = 3636)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                isConnected = true;

                // Bắt đầu thread nhận dữ liệu
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                isConnected = false;
            }
        }

        public void SendInput(string input)
        {
            if (!isConnected || stream == null) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(input);
                stream.Write(data, 0, data.Length);
            }
            catch
            {
                isConnected = false;
            }
        }

        private void ReceiveLoopbackup()
        {
            byte[] buffer = new byte[8192];
            StringBuilder sb = new();

            try
            {
                while (isConnected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        isConnected = false;
                        break;
                    }

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // Tìm JSON hoàn chỉnh
                    string dataStr = sb.ToString();
                    int start = dataStr.IndexOf('{');
                    int end = dataStr.LastIndexOf('}');

                    if (start >= 0 && end >= start)
                    {
                        string json = dataStr.Substring(start, end - start + 1);
                        sb.Remove(0, end + 1);

                        try
                        {
                            var state = JsonSerializer.Deserialize<GameState>(json);
                            if (state != null)
                            {
                                currentState = state;
                            }
                        }
                        catch
                        {
                            // JSON parse error
                        }
                    }
                }
            }
            catch
            {
                isConnected = false;
            }
        }

        private void ReceiveLoop()
        {
            byte[] buffer = new byte[8192];
            StringBuilder sb = new();

            try
            {
                while (isConnected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        isConnected = false;
                        break;
                    }

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    while (true)
                    {
                        string dataStr = sb.ToString();
                        int newlineIndex = dataStr.IndexOf('\n');

                        if (newlineIndex < 0)
                            break; // chưa đủ 1 gói

                        // Lấy 1 JSON hoàn chỉnh
                        string json = dataStr.Substring(0, newlineIndex);
                        sb.Remove(0, newlineIndex + 1);

                        try
                        {
                            var state = JsonSerializer.Deserialize<GameState>(json);
                            if (state != null)
                            {
                                currentState = state;
                            }
                        }
                        catch
                        {
                            // parse fail (có thể log khi debug)
                        }
                    }
                }
            }
            catch
            {
                isConnected = false;
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            client?.Close();
        }
    }
}
