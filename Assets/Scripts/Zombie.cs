using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zombie : MonoBehaviour, IPunObservable
{
    [SerializeField] private LayerMask lookLayers;
    [SerializeField] private GameObject blood;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioClip[] sounds;
    [SerializeField] private float speed;
    private Rigidbody2D rb;

    private AudioSource audioSource;
    private PhotonView PV;

    private float shortestPlayerDistance = Mathf.Infinity;
    private PlayerController currentPlayer = null;
    private bool followingPlayer = true;
    private Vector2 currentPlayerPos;
    private RaycastHit2D hit;
    private Vector2 lookDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        PV = GetComponent<PhotonView>();

        InvokeRepeating("PlaySoundEffects", 0f, 6f);
        InvokeRepeating("FindTarget", 0f, 5f);
    }

    void PlaySoundEffects()
    {
        if (Random.value >= 0.7f)
        {
            PV.RPC("PlaySoundRpc", RpcTarget.All);
        }
    }

    [PunRPC]
    void PlaySoundRpc()
    {
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
        GameObject splash = Instantiate(blood, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
        StartCoroutine(DestroySpawnBloodEffect(splash));
    }

    IEnumerator DestroySpawnBloodEffect(GameObject splash)
    {
        print("DestroySpawnBloodEffect");
        yield return new WaitForSeconds(5f);
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(splash);
        }
        else
        {
            Destroy(splash);
        }
    }


    void Update()
    {
        currentPlayerPos = currentPlayer != null ? (Vector2)currentPlayer.transform.position : Vector2.zero;
    }

    void FixedUpdate()
    {

        const float rayDistance = 2;
        hit = Physics2D.Raycast(rb.position, transform.up, rayDistance, lookLayers);

        if (hit.collider != null)
        {
            followingPlayer = false;
            rb.SetRotation(90);
        }
        else
        {
            followingPlayer = true;
        }

        if (followingPlayer)
        {
            rb.position = Vector2.MoveTowards(transform.position, currentPlayerPos, speed * Time.fixedDeltaTime);
            lookDir = currentPlayerPos - rb.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }


        Debug.DrawRay(rb.position, transform.up, Color.red);
        print("Following the player: " + followingPlayer);

    }

    private void FindTarget()
    {
        IEnumerable<PlayerController> players = FindObjectsOfType<PlayerController>().Where(x => x.IsAlive());

        foreach (PlayerController player in players)
        {
            Transform playerTransform = player.transform;
            float playerDistance = Vector2.Distance(rb.position, playerTransform.position);
            if (playerDistance < shortestPlayerDistance)
            {
                shortestPlayerDistance = playerDistance;
                currentPlayer = player;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(followingPlayer);
            stream.SendNext(currentPlayerPos);
            stream.SendNext(currentPlayer);
            stream.SendNext(shortestPlayerDistance);
        }
        else if (stream.IsReading)
        {
            followingPlayer = (bool)stream.ReceiveNext();
            currentPlayerPos = (Vector2)stream.ReceiveNext();
            currentPlayer = (PlayerController)stream.ReceiveNext();
            shortestPlayerDistance = (float)stream.ReceiveNext();
        }
    }
}
