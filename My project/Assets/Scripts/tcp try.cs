using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpServerTest : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpThread;
    private TcpClient client;
    private NetworkStream stream;
    public int port = 5005;

    void Start()
    {
        Debug.Log("Starting TCP thread...");
        tcpThread = new Thread(StartServer);
        tcpThread.IsBackground = true;
        tcpThread.Start();
    }

    void StartServer()
    {
        try
        {
            IPAddress ip = IPAddress.Any;
            tcpListener = new TcpListener(ip, port);
            tcpListener.Start();
            Debug.Log("TCP Server started on port " + port);

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = tcpListener.AcceptTcpClient();
                stream = client.GetStream();
                Debug.Log("Client connected: " + client.Client.RemoteEndPoint);

                // Echo loop
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log("Received: " + received);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("TCP Server error: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        tcpListener?.Stop();
        stream?.Close();
        client?.Close();
        if (tcpThread != null && tcpThread.IsAlive)
            tcpThread.Abort();
    }
}
