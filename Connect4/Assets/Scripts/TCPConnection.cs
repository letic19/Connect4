using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;

public class TCPConnection : MonoBehaviour
{
    public TextMeshProUGUI connectionStatus;
    public TextMeshProUGUI messageText;
    public bool isServer = true;
    public string serverIP = "127.0.0.1";
    public int port = 7777;
    public BoardManager boardManager;

    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        boardManager.isMyTurn = isServer;

        if (isServer)
            StartServer();
        else
            ConnectToServer();
    }

    void StartServer()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        connectionStatus.text = "Aguardando conexão...";

        Debug.Log("Aguardando conexão...");

        listener.BeginAcceptTcpClient(OnClientConnected, null);
    }

    void OnClientConnected(System.IAsyncResult result)
    {
        client = listener.EndAcceptTcpClient(result);
        stream = client.GetStream();
        connectionStatus.text = "Cliente conectado!";

        Debug.Log("Cliente conectado!");

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        SendMessageToOther("OLA DO SERVIDOR");
    }

    void ConnectToServer()
    {
        client = new TcpClient();
        client.Connect(serverIP, port);

        stream = client.GetStream();
        connectionStatus.text = "Conectado ao servidor!";

        Debug.Log("Conectado ao servidor!");
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        SendMessageToOther("OLA DO CLIENTE");
    }

    public void SendMessageToOther(string message)
    {
        if (stream == null)
            return;

        byte[] data = Encoding.UTF8.GetBytes(message);

        stream.Write(data, 0, data.Length);
    }

    void ReceiveMessages()
    {

        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                messageText.text = message;

                if (message.StartsWith("MOVE:"))
                {
                    string[] parts = message.Split(':');

                    int column = int.Parse(parts[1]);
                    bool redTurn = parts[2] == "1";

                    boardManager.DropPieceNetwork(column, redTurn);
                    boardManager.isMyTurn = true;
                }

            }


        }
    }
}
