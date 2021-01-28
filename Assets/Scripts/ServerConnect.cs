using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using BestHTTP.SocketIO;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Handles socket.io connection with picture server 
/// Current web address to communicate with server: https://agile-badlands-39994.herokuapp.com/
/// Socket connection URI denoted on line 89
/// </summary>
public class ServerConnect : MonoBehaviour
{
    // Singleton 
    public static ServerConnect S;

    public string socketURI; 
    public bool useLocalHost;
    public bool useOliviasTestServer;
    public bool attempted = false;
    public SocketOptions options = null;
    public SocketManager socketManager = null;

    public AudioClip m_messageRecievedAudio; 
    
    void Start()
    {
        S = this; 

        CreateSocketRef();

        if (socketManager == null)
        {
            Debug.Log("Socket Manager Unable to find Reference");
            return;
        }

        // Socket messages that we are listening for 
        socketManager.Socket.On("connect", OnConnect);
        socketManager.Socket.On("connecting", OnConnecting);
        socketManager.Socket.On("reconnect_attempt", onReconAttempt);
        socketManager.Socket.On("error", onError);
        socketManager.Socket.On("reconnect_failed", onReconFailed);
        //socketManager.Socket.On("event", onSocketEvent);
        socketManager.Socket.On("disconnect", onSocketDisconnect);
        socketManager.Socket.On("connect_error", onConnectError);
        socketManager.Socket.On("connect_timeout", onConnectTimeout); 
        socketManager.Socket.On(SocketIOEventTypes.Error, OnError);
        socketManager.Socket.On("picture", getPicture);
        socketManager.Socket.On("command", onCommand);

        //StartCoroutine(RunSwitchWWW());

        /*HTTPRequest request = new HTTPRequest(new Uri("http://db45ecb7.ngrok.io/socket.io/?EIO=4&transport=polling"));
        request.Send();*/
    }

    public void FixedUpdate()
    {
        /*
        if(!attempted)
        {
            attempted = true;
            socketManager.Open();
        }
        */
    }
    /*IEnumerator RunSwitchWWW()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://db45ecb7.ngrok.io/socket.io/?EIO=4&transport=polling"))
        {
            Debug.Log("Well we are at least trying");
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log("HELP");

            }
            else if (www.isHttpError)
            {
                Debug.Log("HELPP");
                Debug.Log(www.error);
            }
            else
            {
                // We are connected to the server 

                // Use line below only if the JSON comes in with brackets around it 
                //json = RemoveBrackets(www.downloadHandler.text);  
                Debug.Log("You connected");
                Debug.Log(www.responseCode);

            }
            // Debug.Log("None of the above?");
        }
    }*/

    ///////////////////// Public functions that emit socket messages /////////////////////
    public void sendPicture(Texture2D tx)
    {
        if (socketManager == null)
            return;
        Debug.Log("Sending Message"); 
        socketManager.GetSocket().Emit("pictureFromHolo", tx.EncodeToPNG()); 
    }

    public void sos()
    {
        if (socketManager == null)
            return;
        socketManager.GetSocket().Emit("SOS"); 
    }

    ///////////////////// Handlers for recieved socket messages from voice command override /////////////////////
    void onCommand(Socket socket, Packet packet, params object[] args)
    {
        string command = (string)args[0];
        Debug.Log("got command: " + command); 

        switch (command)
        {
            ///////////////////// Main Menus /////////////////////
            case "main":
                Debug.Log("Main Menu Overrided");
                VoiceManager.S.MainMenu(); 
                break;
            case "settings":
                VoiceManager.S.Settings();
                break;
            case "brightness":
                VoiceManager.S.Brightness();
                break;
            case "volume":
                VoiceManager.S.Volume();
                break;
            case "biometrics":
                VoiceManager.S.Biometrics();
                break;
            case "help":
                VoiceManager.S.Help();
                break;
            case "diagrams":
                VoiceManager.S.DiagramList();
                break;
            case "procedures":
                VoiceManager.S.ProcedureList();
                break;
            case "music":
                VoiceManager.S.musicMenu();
                break;
            case "tasks":
                VoiceManager.S.TaskList();
                break;

            ///////////////////// Menu Navigation /////////////////////
            case "reset":
                VoiceManager.S.ResetScene();
                break;
            case "close":
                VoiceManager.S.Close();
                break;
            case "complete":
                VoiceManager.S.Next();
                break;
            case "back":
                VoiceManager.S.Back();
                break;
            case "zoom_in":
                VoiceManager.S.zoomIn();
                break;
            case "zoom_out":
                VoiceManager.S.zoomOut();
                break;

            ///////////////////// Translation /////////////////////
            case "show":
                VoiceManager.S.ShowPath();
                break;
            case "hide":
                VoiceManager.S.HidePath();
                break;
            case "record":
                VoiceManager.S.StartTranslation();
                break;
            case "end":
                VoiceManager.S.StopTranslation();
                break;

            ///////////////////// Special Functions /////////////////////
            case "increase":
                VoiceManager.S.Increase();
                break;
            case "decrease":
                VoiceManager.S.Decrease();
                break;
            case "capture":
                VoiceManager.S.TakePhoto();
                break;
            case "toggle":
                VoiceManager.S.Toggle();
                break;

            ///////////////////// Music /////////////////////
            case "hello":
                VoiceManager.S.PlayAdele();
                break;
            case "africa":
                VoiceManager.S.PlayAfrica();
                break;
            case "skyfall":
                VoiceManager.S.PlaySkyfall();
                break;
            case "oddity":
                VoiceManager.S.PlaySpaceOddity();
                break;
            case "thunderstruck":
                VoiceManager.S.PlayThunderstruck();
                break;
            case "eclipse":
                VoiceManager.S.PlayEclipse();
                break;
            case "rocketman":
                VoiceManager.S.PlayRocketMan();
                break;
            case "stop_music":
                VoiceManager.S.StopMusic();
                break;
        }
    }

    void getPicture(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("Picture recieved from web server"); 
        // Convert picture to correct format 
        Dictionary<String, object> fromSocket = (Dictionary<String, object>)args[0];
        String b64String = (String)fromSocket["image"];
        b64String = b64String.Remove(0, 22);  // removes the header 
        byte[] b64Bytes = System.Convert.FromBase64String(b64String);
        Texture2D tx = new Texture2D(1, 1);
        tx.LoadImage(b64Bytes);
        
        // Display picture and text 
        VuforiaCameraCapture.S.SetImage(tx);
        VuforiaCameraCapture.S.SetText(fromSocket["sendtext"].ToString());

        // Play sound 
        VoiceManager vm = (VoiceManager)GameObject.FindObjectOfType(typeof(VoiceManager));
        vm.m_Source.clip = m_messageRecievedAudio;
        vm.m_Source.Play(); 
    }

    void OnConnect(Socket socket, Packet packet, params object[] args)
    {
        //Debug.Log("Connected to Socket.IO server");
    }

    void OnConnecting(Socket socket, Packet packet, params object[] args)
    {
       // Debug.Log("Connecting to Socket.IO server");
    }

    void onReconAttempt(Socket socket, Packet packet, params object[] args)
    {
        //Debug.Log("Attempting to reconnect to Socket.IO server");
    }

    void onError(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("ERROR SOCKET HELP " + args[0]);
    }

    void onReconFailed(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("Reconnect failed");
    }

    void onSocketDisconnect(Socket socket, Packet packet, params object[] args)
    {
       // Debug.Log("Socket disconnect ");
    }

    void onConnectError(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("Connect error");
    }

    void onConnectTimeout(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("Connect timeout");
    }

    void OnError(Socket socket, Packet packet, params object[] args)
    {
        Error error = args[0] as Error;

        switch (error.Code) { case SocketIOErrors.User: Debug.Log("Exception in an event handler!"); break; case SocketIOErrors.Internal: Debug.Log("Internal error!"); break; default: Debug.Log("Server error!"); break; }

        Debug.Log(error.ToString());
    }
    /////////////////////////////// Socket.io connection utilities /////////////////////////////////
    void OnApplicationQuit()
    {
        LeaveRoomFromServer();
        DisconnectMySocket();
    }

    public void CreateSocketRef()
    {
        if (socketManager != null)
            return;
        TimeSpan miliSecForReconnect = TimeSpan.FromMilliseconds(10000);

        options = new SocketOptions();
        options.ReconnectionAttempts = 5;

        options.AutoConnect = true;
        options.ReconnectionDelay = miliSecForReconnect;
        
        //options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.Polling; 
        options.Timeout = TimeSpan.FromMilliseconds(5000); // set this to much faster than default, should probably remove 

        //Server URI
        if (useLocalHost)
        {
            socketManager = new SocketManager(new Uri("http://localhost:3000/socket.io/"), options);
        }
        else if (useOliviasTestServer)
        {
            socketManager = new SocketManager(new Uri("http://54.175.254.200:3000/socket.io/"));
        }
        else
        {
            //socketManager = new SocketManager(new Uri("http://db45ecb7.ngrok.io:3000/socket.io/"), options);
            socketManager = new SocketManager(new Uri(socketURI), options);
        } 


        //Debug.Log("Connected to Socket server"); 
    }

    public void DisconnectMySocket()
    {
        //Debug.Log("Disconnected from Socket Server"); 
        if (socketManager == null)
            return;
        socketManager.GetSocket().Disconnect();
    }

    public void LeaveRoomFromServer()
    {
        if (socketManager == null)
            return;
        socketManager.GetSocket().Emit("leave", OnSendEmitDataToServerCallBack);
    }

    private void OnSendEmitDataToServerCallBack(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log("Send Packet Data : " + packet.ToString());
    }

    public void SetNamespaceForSocket()
    {
        //namespaceForCurrentPlayer = socketNamespace;
        //mySocket = socketManagerRef.GetSocket(“/ Room - 1);
    }

}

[System.Serializable]
public class FromServerData
{
    public String b64String;
    public String sendText;
}
