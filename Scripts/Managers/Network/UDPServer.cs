using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public enum MessageType : byte
{
    Register = 0,
    Chat = 1,
    Command = 2
}

public class UDPServer : MonoBehaviour
{
    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private Dictionary<ulong, FixedString64Bytes> playerNames;

    public TextMeshProUGUI serverLog;
    public GameObject connectedPlayersPanel;
    public GameObject playerEntryPrefab;

    public ushort port = 9000;
    private List<string> serverLogMessages = new();
    private int maxLogEntries = 50;

    void Start()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = port;

        if (driver.Bind(endpoint) != 0)
            LogToServer("Failed to start server on port " + port);
        else
        {
            driver.Listen();
            LogToServer("Server listening on port " + port);
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        playerNames = new Dictionary<ulong, FixedString64Bytes>();

        InvokeRepeating(nameof(UpdateConnectedPlayersUI), 1f, 5f);
    }

    void OnDestroy()
    {
        if (driver.IsCreated) driver.Dispose();
        if (connections.IsCreated) connections.Dispose();
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        NetworkConnection c;
        while ((c = driver.Accept()) != default)
        {
            connections.Add(c);
            LogToServer("New client connected");
        }

        for (int i = 0; i < connections.Length; i++)
        {
            var conn = connections[i];

            if (!conn.IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(conn, out var reader)) != NetworkEvent.Type.Empty)
            {
                switch (cmd)
                {
                    case NetworkEvent.Type.Data:
                        HandleMessage(conn, reader);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        ulong key = GetConnectionHash(conn);
                        if (playerNames.TryGetValue(key, out var name))
                        {
                            LogToServer($"Client disconnected: {name}");
                            playerNames.Remove(key);
                        }
                        else
                            LogToServer("Client disconnected (unregistered)");

                        connections[i] = default;
                        UpdateConnectedPlayersUI();
                        break;
                }
            }
        }
    }

    void HandleMessage(NetworkConnection connection, DataStreamReader reader)
    {
        var type = (MessageType)reader.ReadByte();
        ulong connId = GetConnectionHash(connection);

        if (type == MessageType.Register)
        {
            var playerName = reader.ReadFixedString64();
            playerNames[connId] = playerName;
            LogToServer($"[Register] {playerName} joined");

            foreach (var conn in connections)
            {
                if (!conn.IsCreated) continue;
                driver.BeginSend(conn, out var writer);
                writer.WriteByte((byte)MessageType.Chat);
                writer.WriteFixedString64("Server");
                writer.WriteFixedString512($"Player {playerName} connected");
                driver.EndSend(writer);
            }

            UpdateConnectedPlayersUI();
        }
        else if (type == MessageType.Chat)
        {
            var message = reader.ReadFixedString512();
            var senderName = playerNames.TryGetValue(connId, out var name) ? name : new FixedString64Bytes("Anon");

            Debug.Log($"Received message from {senderName}: {message}");

            foreach (var conn in connections)
            {
                if (!conn.IsCreated) continue;
                driver.BeginSend(conn, out var writer);
                writer.WriteByte((byte)MessageType.Chat);
                writer.WriteFixedString64(senderName);
                writer.WriteFixedString512(message);
                driver.EndSend(writer);
            }
        }
        else if (type == MessageType.Command)
        {
            var command = reader.ReadFixedString512();
            ProcessCommand(connection, connId, command);
        }
    }

    void ProcessCommand(NetworkConnection connection, ulong connId, FixedString512Bytes commandText)
    {
        string cmd = commandText.ToString();

        if ((cmd.StartsWith("/name ") || cmd.StartsWith("/nombre ")) && cmd.Length > 6)
        {
            string newName;
            if (cmd.StartsWith("/name "))
                newName = cmd.Substring(6);
            else
                newName = cmd.Substring(8);

            string oldName = playerNames.TryGetValue(connId, out var old) ? old.ToString() : "Anon";
            playerNames[connId] = new FixedString64Bytes(newName);
            LogToServer($"[Command] {oldName} changed name to {newName}");

            driver.BeginSend(connection, out var writer);
            writer.WriteByte((byte)MessageType.Chat);
            writer.WriteFixedString64("Server");
            writer.WriteFixedString512($"Your name has been changed to {newName}");
            driver.EndSend(writer);

            foreach (var conn in connections)
            {
                if (!conn.IsCreated || conn == connection) continue;
                driver.BeginSend(conn, out var w);
                w.WriteByte((byte)MessageType.Chat);
                w.WriteFixedString64("Server");
                w.WriteFixedString512($"{oldName} changed their name to {newName}");
                driver.EndSend(w);
            }

            UpdateConnectedPlayersUI();
        }
        else if (cmd == "/help" || cmd == "/ayuda")
        {
            driver.BeginSend(connection, out var writer);
            writer.WriteByte((byte)MessageType.Chat);
            writer.WriteFixedString64("Server");
            writer.WriteFixedString512("Available commands:\n/name [new_name]: Change your name\n/help: Show this help");
            driver.EndSend(writer);
        }
        else
        {
            driver.BeginSend(connection, out var writer);
            writer.WriteByte((byte)MessageType.Chat);
            writer.WriteFixedString64("Server");
            writer.WriteFixedString512("Unknown command. Type /help for available commands.");
            driver.EndSend(writer);
        }
    }

    private ulong GetConnectionHash(NetworkConnection conn)
    {
        return (ulong)conn.GetHashCode();
    }

    private void LogToServer(string message)
    {
        string time = System.DateTime.Now.ToString("HH:mm:ss");
        string entry = $"[UDP Server] [{time}] {message}";

        Debug.Log(entry);
        serverLogMessages.Add(entry);
        if (serverLogMessages.Count > maxLogEntries)
            serverLogMessages.RemoveAt(0);

        if (serverLog != null)
            serverLog.text = string.Join("\n", serverLogMessages);
    }

    private void UpdateConnectedPlayersUI()
    {
        if (connectedPlayersPanel == null || playerEntryPrefab == null)
        {
            Debug.LogWarning("UI references not set in UDPServer");
            return;
        }

        foreach (Transform child in connectedPlayersPanel.transform)
            Destroy(child.gameObject);

        foreach (var player in playerNames.Values)
        {
            GameObject entry = Instantiate(playerEntryPrefab);
            entry.transform.SetParent(connectedPlayersPanel.transform, false);

            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = player.ToString();
        }
    }
}