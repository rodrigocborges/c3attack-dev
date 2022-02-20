using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    private int maxEnemiesPerWave = 10;
    private float timeToSpawn = 1f;
    private float currentTimeToSpawn = 0;
    private int currentEnemies = 0;
    private int currentEnemiesSpawned = 0;
    private PhotonView PV;
    private int wave = 1;

    private bool startSpawn = true;

    private TMP_Text waveText;
    private TMP_Text zombiesCountText;

    void Start()
    {
        PV = GetComponent<PhotonView>();
        waveText = GameObject.Find("WaveText").GetComponent<TMP_Text>();
        zombiesCountText = GameObject.Find("ZombiesCountText").GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentEnemies = FindObjectOfType<Zombie>() != null ? FindObjectsOfType<Zombie>().Length : 0;

            if (startSpawn)
            {
                currentTimeToSpawn += Time.deltaTime;
                if (currentTimeToSpawn >= timeToSpawn && currentEnemies < maxEnemiesPerWave)
                {
                    PhotonNetwork.Instantiate(Path.Combine("Prefabs", "Zombie"), new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0), Quaternion.identity);
                    ++currentEnemiesSpawned;
                    currentTimeToSpawn = 0;
                }

                if (currentEnemiesSpawned >= maxEnemiesPerWave)
                {
                    startSpawn = false;
                }
            }
            else
            {
                if(currentEnemies == 0)
                {
                    ++wave;
                    currentEnemiesSpawned = 0;
                    maxEnemiesPerWave += 4;
                    currentTimeToSpawn = 0;
                    startSpawn = true;
                }
            }

            PV.RPC("WaveStats", RpcTarget.All, wave, currentEnemiesSpawned, currentEnemies, maxEnemiesPerWave);

        }
    }

    [PunRPC]
    void WaveStats(int localWave, int localCurrentEnemiesSpawned, int localCurrentEnemies, int localMaxEnemiesPerWave)
    {
        waveText.text = "WAVE " + localWave;
        zombiesCountText.text = string.Format("{0}/{1} ({2})", localCurrentEnemiesSpawned, localMaxEnemiesPerWave, localCurrentEnemies);
    }
}
