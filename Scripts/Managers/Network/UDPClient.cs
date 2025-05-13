using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    public string playerName = "Jugador";
    public string serverIP = "127.0.0.1";
    public ushort serverPort = 9000;

    private NetworkDriver driver;
    private NetworkConnection connection;
    private bool isConnected = false;
    public Transform chatContent;
    public GameObject chatMessagePrefab;
    public TMP_InputField chatInputField;


    void Start()
    {
        playerName = PlayerPrefs.GetString("jugador_nombre", "Jugador");
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.Parse(serverIP, serverPort);
        connection = driver.Connect(endpoint);
    }

    void OnDestroy()
    {
        if (driver.IsCreated) driver.Dispose();
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        if (!isConnected && connection.IsCreated && connection.GetState(driver) == NetworkConnection.State.Connected)
        {
            isConnected = true;
            SendRegister();
        }

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out var reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                HandleMessage(reader);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }
    }

    public void OnSendMessage()
    {
        string message = chatInputField.text;

        if (!string.IsNullOrEmpty(message))
        {
            SendChat(message);
            chatInputField.text = "";
            chatInputField.ActivateInputField();
        }
    }

    void SendRegister()
    {
        driver.BeginSend(connection, out var writer);
        writer.WriteByte((byte)MessageType.Register);
        writer.WriteFixedString64(playerName);
        driver.EndSend(writer);
    }

    public void SendChat(string message)
    {
        if (driver.IsCreated && connection.IsCreated)
        {
            driver.BeginSend(connection, out var writer);
            
            if (message.StartsWith("/"))
            {
                writer.WriteByte((byte)MessageType.Command);
                writer.WriteFixedString512(message);
            }
            else
            {
                writer.WriteByte((byte)MessageType.Chat);
                writer.WriteFixedString512(message);
            }
            
            driver.EndSend(writer);

            Debug.Log($"Sent message: {message}");
        }
    }

    void HandleMessage(DataStreamReader reader)
    {
        var type = (MessageType)reader.ReadByte();
        var sender = reader.ReadFixedString64().ToString();
        var msg = reader.ReadFixedString512().ToString();

        if (type == MessageType.Chat)
        {
            if (chatMessagePrefab != null && chatContent != null)
            {
                GameObject entry = Instantiate(chatMessagePrefab, chatContent);
                entry.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"[{sender}]: {msg}";
            }

        }
    }
}