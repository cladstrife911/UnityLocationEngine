using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MyAnchorData
{
    public float timestamp;
    public int distance;
    public int power;
}

public class MyConnectScript : MonoBehaviour
{

    private int portToConnect = 4242;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private bool socketReady = false;

    public InputField serverAddressInputField;
    public Text text_log_view;
    public GameObject mySphere;


    public MeshRenderer myRenderer;

    // Start is called before the first frame update
    void Start()
    {
        myRenderer = mySphere.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    public void ClickOnConnect(string host)
    {
        Debug.Log("IP:" + serverAddressInputField.text);
        try
        {
            //need to tell unity to let me use the port im about to use
            //Security.PrefetchSocketPolicy(host, portToConnect);
            //socket = new TcpClient(host, portToConnect);
            socket = new TcpClient();
            socket.Connect(serverAddressInputField.text, portToConnect);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
            Debug.Log("Socket connected!");
        }
        catch (Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        string[] aData = data.Split('|');
        Debug.Log("Received from server: " + data);
        text_log_view.text += data + "\r\n";

        //MyAnchorData anchorData = JsonUtility.FromJson<MyAnchorData>(data);

        //string fileData = System.IO.File.ReadAllText(data);
        string[] lines = data.Split("\n"[0]);
        string[] lineData = (lines[0].Trim()).Split(";"[0]);

        MyAnchorData anchorData = new MyAnchorData();
        anchorData.distance = Int32.Parse(lineData[2]);
        anchorData.power = Int32.Parse(lineData[3]);

        Debug.Log("anchorData " + anchorData.distance +"cm, "+ anchorData.power +"dBm");
        float dist = anchorData.distance / 100.0f;
        mySphere.transform.localScale = new Vector3(dist, dist, dist);

        //myRenderer = mySphere.GetComponent<MeshRenderer>();
        Color color = myRenderer.material.color;
        color.a = (anchorData.power+109.0f)/109.0f;
        myRenderer.material.color = color;

#if MY_TEST
        switch (aData[0])
        {
            case "WhoAreYou":
                Send("Iam|" + clientName + "|" + password);
                break;
            case "Authenticated":
                SceneManager.LoadScene("ClientGameView");
                break;
            case "UnitSpawned":
                GameObject prefab = Resources.Load("Prefabs/Unit1") as GameObject;
                GameObject go = Instantiate(prefab);
                float parsedX = float.Parse(aData[3], culture);
                float parsedY = float.Parse(aData[4], culture);
                float parsedZ = float.Parse(aData[5], culture);
                go.GetComponent<NavMeshAgent>().Warp(new Vector3(parsedX, parsedY, parsedZ));
                Unit un = go.AddComponent<Unit>();
                unitsOnMap.Add(un);
                int parsed;
                Int32.TryParse(aData[2], out parsed);
                un.unitID = parsed;

                if (aData[1] == clientName)
                {
                    un.isPlayersUnit = true;
                }
                else
                {
                    un.isPlayersUnit = false;
                }
                break;
            case "UnitMoved":
                if (aData[1] == clientName)
                {
                    return;
                }
                else
                {
                    Int32.TryParse(aData[2], out parsed);
                    foreach (Unit unit in unitsOnMap)
                    {
                        if (unit.unitID == parsed)
                        {
                            parsedX = float.Parse(aData[3], culture);
                            parsedY = float.Parse(aData[4], culture);
                            parsedZ = float.Parse(aData[5], culture);
                            unit.MoveTo(new Vector3(parsedX, parsedY, parsedZ));
                        }
                    }
                }
                break;
            case "Synchronizing":
                int numberOfUnitsOnServersMap;
                Int32.TryParse(aData[1], out numberOfUnitsOnServersMap);
                int serverUnitID;
                int[] serverUnitIDs = new int[numberOfUnitsOnServersMap];
                for (int i = 0; i < numberOfUnitsOnServersMap; i++)
                {
                    Int32.TryParse(aData[2 + i * 4], out serverUnitID);
                    serverUnitIDs[i] = serverUnitID;
                    bool didFind = false;
                    foreach (Unit unit in unitsOnMap) //synchronize existing units
                    {
                        if (unit.unitID == serverUnitID)
                        {
                            parsedX = float.Parse(aData[3 + i * 4], culture);
                            parsedY = float.Parse(aData[4 + i * 4], culture);
                            parsedZ = float.Parse(aData[5 + i * 4], culture);
                            unit.MoveTo(new Vector3(parsedX, parsedY, parsedZ));
                            didFind = true;
                        }
                    }
                    if (!didFind) //add non-existing (at client) units
                    {
                        prefab = Resources.Load("Prefabs/Unit1") as GameObject;
                        go = Instantiate(prefab);
                        un = go.AddComponent<Unit>();
                        unitsOnMap.Add(un);
                        un.unitID = serverUnitID;
                        parsedX = float.Parse(aData[3 + i * 4], culture);
                        parsedY = float.Parse(aData[4 + i * 4], culture);
                        parsedZ = float.Parse(aData[5 + i * 4], culture);
                        go.GetComponent<NavMeshAgent>().Warp(new Vector3(parsedX, parsedY, parsedZ));
                    }

                }
                //remove units which are not on server's list (like disconnected ones)
                foreach (Unit unit in unitsOnMap)
                {
                    bool exists = false;
                    for (int i = 0; i < serverUnitIDs.Length; i++)
                    {
                        if (unit.unitID == serverUnitIDs[i])
                        {
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        Destroy(unit.gameObject);
                        unitsOnMap.Remove(unit);
                    }
                }
                break;
            default:
                Debug.Log("Unrecognizable command received");
                break;
        }
#endif
    }

}
