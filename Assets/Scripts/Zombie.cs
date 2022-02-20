using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField] private GameObject blood;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private float speed;
    private Vector2 currentPlayerPos;
    private Rigidbody2D rb;

    private Dictionary<int, float> playersInfo = new Dictionary<int, float>();
    private AudioSource audioSource;
    private PhotonView PV;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        PV = GetComponent<PhotonView>();

        if(Random.value >= 0.7f)
        {
            PV.RPC("PlaySoundRpc", RpcTarget.All);
        }
            
    }

    [PunRPC]
    void PlaySoundRpc() {
        audioSource.clip = sounds[Random.Range(0, sounds.Length)];
        audioSource.Play();
    }

    [PunRPC]
    void PlaySplashSound()
    {
        AudioSource.PlayClipAtPoint(splashSound, transform.position, 1);
    }

    [PunRPC]
    void SpawnBloodEffect()
    {
        Instantiate(blood, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
    }

    void Update()
    {
        foreach(PlayerController playerController in FindObjectsOfType<PlayerController>())
        {
            if (!playerController.IsAlive())
                continue;

            if(playerController.GetComponent<PhotonView>() != null)
            {
                PhotonView photonViewPlayer = playerController.GetComponent<PhotonView>();
                if (!playersInfo.ContainsKey(photonViewPlayer.ViewID))
                {
                    playersInfo.Add(photonViewPlayer.ViewID, Vector2.Distance(transform.position, playerController.transform.position));
                }
                else
                {
                    playersInfo[photonViewPlayer.ViewID] = Vector2.Distance(transform.position, playerController.transform.position);
                }
            }
        }

        if (playersInfo == null || !playersInfo.Any())
            return;

        int playerIdWithSmallerDistance = playersInfo.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
        
        foreach(PhotonView pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.ViewID == playerIdWithSmallerDistance)
                currentPlayerPos = pv.transform.position;
        }
    }
    void FixedUpdate()
    {
        rb.position = Vector2.MoveTowards(transform.position, currentPlayerPos, speed * Time.fixedDeltaTime);

        Vector2 lookDir = currentPlayerPos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }
}
