using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Player : NetworkBehaviour
{

    private static int TeamIdCounter;

    public static Player Local
    {
        get
        {
            var x = NetworkClient.localPlayer;

            if (x != null)
                return x.GetComponent<Player>();

            return null;    
        }
    }


    [SerializeField] private Vehicle vehiclePrefab;

    public Vehicle ActiveVehicle { get; set; }


    [Header("Player")]
    [SyncVar(hook = nameof(OnNicknameCanged) )]
    public string Nickname;


    [SyncVar]
    [SerializeField] private int teamId;
    public int TeamId => teamId;




    private void OnNicknameCanged(string old, string newVal)
    {
        gameObject.name = "Player_" + newVal; // on Client
    }

    [Command] // on Server
    public void CmdSetName(string name)
    {
        Nickname = name;    
        gameObject.name = "Player_" + name;
    }


    [Command]
    public void CmdSetTeamID(int teamId)
    {
        this.teamId = teamId;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        teamId = TeamIdCounter % 2;
        TeamIdCounter++;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (hasAuthority == true)
        {
            CmdSetName(NetworkSessionManager.Instance.GetComponent<NetworkManagerHUD>().PlayerNickname);
        }
    }

    private void Update()
    {

       
        if (isServer == true)
        {
          
            if (Input.GetKeyDown(KeyCode.F9))
            {
               
                foreach (var p in FindObjectsOfType<Player>())
                {
                    if (p.ActiveVehicle != null)
                    {
                        NetworkServer.UnSpawn(p.ActiveVehicle.gameObject);
                        Destroy(p.ActiveVehicle.gameObject);    
                        p.ActiveVehicle = null; 
                    }
                }

                foreach (var p in FindObjectsOfType<Player>())
                {
                    p.SvSpavnClientVehicle();
                }
            }
        }

        if (hasAuthority == true)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

       
    }








    [Server]
    public void SvSpavnClientVehicle()
    {
        if (ActiveVehicle != null) return;
        

        GameObject playerVehicle = Instantiate(vehiclePrefab.gameObject, transform.position, Quaternion.identity);

        playerVehicle.transform.position = teamId % 2 == 0 ?
            NetworkSessionManager.Instance.RandomSpawnPointRed :
            NetworkSessionManager.Instance.RandomSpawnPointBlue;

        NetworkServer.Spawn(playerVehicle, netIdentity.connectionToClient);
   
        ActiveVehicle = playerVehicle.GetComponent<Vehicle>();

        ActiveVehicle.Owner = netIdentity;

        RpcSetVehicle(ActiveVehicle.netIdentity);
    
    }


    [ClientRpc]
    private void RpcSetVehicle(NetworkIdentity vehicle)
    {
        if (vehicle == null) return; 
        

        ActiveVehicle = vehicle.GetComponent<Vehicle>();

        if (ActiveVehicle != null && ActiveVehicle.hasAuthority && VehicleCamera.Instance != null)
        {
            VehicleCamera.Instance.SetTarget(ActiveVehicle);
        }
    }
}
