/*
 *************************
 * EE5 - Smartgun Team 3 *
 *************************
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
// Write packetsender method that scans all the vests and the messages that have to be send
// It sends the messages
public class UDPServer : MonoBehaviour
{

    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    public string serverIP; //default local
    public int receivePort; // define > init
    public string dataString;
    public string messageSend;
    public GameObject prefabGatlinGun;
    public GameObject prefabPistol;
    public GameObject prefabVest;
    public int maxTimeInactive = 5; // Used to destroy a gameobject if its clients has been inactive for this amount of seconds
    public bool chargeAllMagazines;

    private string lastClientIP;
    private int lastClientPort;
    private Dispatcher dispatcher = Dispatcher.Instance;
    private Dictionary<IPAddress, GameObject> clients;
    private Dictionary<GameObject, IPEndPoint> vests;
    private Dictionary<IPAddress, DateTime> timestamps;
    private Vector3 positionOffset = new Vector3(0f, -0.23f, 0f); // used to display object underneath eachother and to check availablility of a position

    // start from unity3d
    public void Start()
    {
        // define serverIP
        serverIP = LocalIPAddress;

        // define port
        receivePort = 4210;

        // status
        print("Receiving on : " + serverIP + " : " + receivePort);

        // define client table
        clients = new Dictionary<IPAddress, GameObject>();
        timestamps = new Dictionary<IPAddress, DateTime>();
        vests = new Dictionary<GameObject, IPEndPoint>();

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    //Loop to update the seen if new gameobject are being created
    void Update()
    {
        dispatcher.InvokePending();
        //CheckActivityStatus(); // Removes weapons that haven't been active for a long time
        SendVestMessages();
    }

    // To prevent SocketExeption error: Address already in use
    void OnApplicationQuit()
    {
        client.Close();
    }

    // Displays info on the GUI
    void OnGUI()
    {
        Rect rectObj = new Rect(40, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPServer\n" 
                    + "Server IP : " + serverIP + " \n"
                    + "Server Port : " + receivePort + " \n"
                    + "\nLast client IP : " + lastClientIP + " \n"
                    + "Last client Port : " + lastClientPort + " \n"
                    + "\nMessage received in hex :\n" + dataString +  "\n"
                    + "Last message send : " + messageSend + " \n"
                , style);
    }

    // send thread
    private void SendData(IPEndPoint clientEndpoint, byte[] data)
    {
        client = new UdpClient();
        try
        {
            // Send message to remote client
            client.Send(data, data.Length, clientEndpoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient(receivePort);
        while (true)
        {
            try
            {
                // Bytes received
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref clientEndpoint);
                lastClientIP = clientEndpoint.Address.ToString();
                lastClientPort = clientEndpoint.Port;

                if (data.Length == 4) // if the packet received is not equal to 4 bytes (32 bits), the packet is discarded
                {
                    // The action of creating or updating the weapons happens in the main thread.
                    Action<IPEndPoint, byte[]> action = new Action<IPEndPoint, byte[]>(CreateWeapon); //The action is created  
                    dispatcher.Invoke(action, clientEndpoint, data); // The action is send to the dispatcher to be executed in the main thread (UDPServer.Update())

                    // Gives a hex representation of the data in String form
                    dataString = ByteArrayToString(data); // gives a hex representation of the data (handy for debugging purposes)
                }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // Gets local IP adress to be displayed on the GUI
    private static string LocalIPAddress
    {
        get
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    // Getter and setter for the clients dictionary (= Hashmap in C#)
    public Dictionary<IPAddress, GameObject> Clients { get => clients; set => clients = value; }

    // Converts the byte array into a string
    private static string ByteArrayToString(byte[] byteArray)
    {
        int[] conversionInt = Array.ConvertAll(byteArray, c => (int)c);
        string conversion = string.Join(",", conversionInt.Select(p => p.ToString()).ToArray());
        return conversion;
    }

    // Creates a scar gameobject if it does not exist and updates the object with the new data
    private void CreateWeapon(IPEndPoint clientEndpoint, byte[] data)
    {
        try
        {
            //If client connects for the first time a new GameObject is made and the sending IPadress is linked together in the dictionary
            if (!clients.TryGetValue(clientEndpoint.Address, out GameObject weapon))
            {
                GameObject prefab = prefabPistol;
                WeaponType weaponType = ElementInteraction.GetWeaponTypeOutOfData(data);
                switch (weaponType)
                {
                    case WeaponType.GatlinGun:
                        prefab = prefabGatlinGun;
                        break;

                    case WeaponType.Pistol:
                        prefab = prefabPistol;
                        break;

                    case WeaponType.Vest:
                        prefab = prefabVest;
                        AcknowledgeVest(clientEndpoint);
                        weapon = Instantiate(prefab, GetNewPosition(), Quaternion.identity);
                        weapon.GetComponent<VestInteraction>().SetWeaponType(weaponType);
                        clients.Add(clientEndpoint.Address, weapon);
                        vests.Add(weapon, clientEndpoint);
                        return;
                }
                // Create gameobject
                weapon = Instantiate(prefab, GetNewPosition(), Quaternion.identity);
                // Add IPaddress as key and new gameobject as value to the dictionary clients
                clients.Add(clientEndpoint.Address, weapon);
                // Set the weapon type field of the created weapon
                weapon.GetComponent<WeaponInteraction>().SetWeaponType(weaponType);
                // Set IP address in weapon component
                weapon.GetComponent<WeaponInteraction>().SetIPAddress(clientEndpoint.Address);
                // Modify fields of the new object by the data received by the client
                weapon.GetComponent<WeaponInteraction>().PacketTranslater(data);
                // Add timestamp of last weapon update to the dictionary timestamps
                DateTime lastUpdate = weapon.GetComponent<WeaponInteraction>().GetLastUpdate();
                timestamps.Add(clientEndpoint.Address, lastUpdate);
            }
            else
            {
                // Update weapon with new data
                weapon.GetComponent<WeaponInteraction>().PacketTranslater(data);
                // Update timestamp of last weapon updates
                DateTime lastUpdate = weapon.GetComponent<WeaponInteraction>().GetLastUpdate();
                timestamps[clientEndpoint.Address] = lastUpdate;
            }
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void CheckActivityStatus()
    {
        List<IPAddress> toRemove = new List<IPAddress>();
        foreach(KeyValuePair<IPAddress, DateTime> time in timestamps)
        {
            if ((DateTime.Now - time.Value).TotalSeconds > maxTimeInactive) // If there is no contact for more than 5 seconds
            {
                toRemove.Add(time.Key);
            }
        }
        foreach (IPAddress IPtoRemove in toRemove)
        {            
            if (clients.TryGetValue(IPtoRemove, out GameObject weapon))
            {
                clients.Remove(IPtoRemove);
                timestamps.Remove(IPtoRemove);
                Destroy(weapon);
            }
        }
    }

    private Vector3 GetNewPosition()
    {
        Vector3 newPosition = new Vector3(0, 0, 0);
        Boolean posAvailable = false;
        while (!posAvailable)
        {
            posAvailable |= clients.Count == 0; // if clients dictionary is empty then posAvailable = true
            foreach (KeyValuePair<IPAddress, GameObject> weapon in clients)
            {
                Vector3 position = weapon.Value.transform.position;
                if (position == newPosition)
                {
                    newPosition += positionOffset;
                    posAvailable = false;
                    break;
                }
                posAvailable = true;
            }
        }
        return newPosition;
    }

    private void AcknowledgeVest(IPEndPoint clientEndpoint)
    {
        string acknowledgement = "ack!";
        byte[] ack = Encoding.UTF8.GetBytes(acknowledgement);
        messageSend = acknowledgement;
        SendData(clientEndpoint, ack);
    }

    private void SendVestMessages()
    {
        foreach (KeyValuePair<GameObject, IPEndPoint> vest in vests)
        {
            byte[] message = vest.Key.GetComponent<VestInteraction>().GetMessageToSend();
            messageSend = ByteArrayToString(message);
            SendData(vest.Value, message);
            vest.Key.GetComponent<VestInteraction>().ResetMessageToSend(); // Reset the message after it is read;
        }
    }
}