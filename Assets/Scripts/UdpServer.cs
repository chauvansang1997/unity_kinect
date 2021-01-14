using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpServer
{
    private UdpClient client;

    public string strMessage = string.Empty;

    private int port;

    public event Action<string> ServerMessage = null;

    Thread receiveThread;
    public UdpServer(int port)
    {
        this.port = port;
    }

    public void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveMessage));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        //ReceiveMessage();
    }

    IPEndPoint anyIP;
    void ReceiveMessage()
    {
        client = new UdpClient(port);
        anyIP = new IPEndPoint(IPAddress.Any, 0);
        //  byte[] data = client.Receive(ref anyIP);
        // string message = Encoding.UTF8.GetString(data);
        // Start an asynchronous read invoking DoRead to avoid lagging the user interface.
        client.BeginReceive(new AsyncCallback(DoRead), null);
    }

    private void DoRead(IAsyncResult ar)
    {
       
        try
        {
            // Finish asynchronous read into readBuffer and return number of bytes read.
            byte[] bytes = client.EndReceive(ar, ref anyIP);

            // Convert the byte array the message was saved into, minus two for the
            // Chr(13) and Chr(10)
            if (bytes.Length < 1)
            {
                // if no bytes were read server has close.
                client.BeginReceive(new AsyncCallback(DoRead), null);
                return;
            }
            strMessage = Encoding.ASCII.GetString(bytes);
            ProcessCommands(strMessage);
            // Start a new asynchronous read into readBuffer.
            client.BeginReceive(new AsyncCallback(DoRead), null);
        }
        catch
        {
            //Debug.LogWarning("Disconnected");
        }
    }



    public void Disconnect()
    {
        try
        {
            client.Close();
            receiveThread.Abort();
            
        }
        catch { }
    }


    // Process the command received from the server, and send it back to listener.
    private void ProcessCommands(string strMessage)
    {
        if (ServerMessage != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ServerMessage(strMessage);
            });
        }
    }

    // Use a StreamWriter to send a message to server.


}
