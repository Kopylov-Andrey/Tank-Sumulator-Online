using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class NetworkSessionManager : NetworkManager
{
    [SerializeField] private SphereArea[] spawnZonesRed;
    [SerializeField] private SphereArea[] spawnZonesBlue;

    public Vector3 RandomSpawnPointRed => spawnZonesRed[UnityEngine.Random.Range(0, spawnZonesRed.Length)].RandomInsise;

    public Vector3 RandomSpawnPointBlue => spawnZonesBlue[UnityEngine.Random.Range(0, spawnZonesBlue.Length)].RandomInsise;


    public static NetworkSessionManager Instance => singleton as NetworkSessionManager;

    public bool IsServer => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ServerOnly);
    public bool IsClient => (mode == NetworkManagerMode.Host || mode == NetworkManagerMode.ClientOnly);

}
