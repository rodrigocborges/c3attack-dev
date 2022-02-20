using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject spriteObject;
    [SerializeField] private float speed;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.75f; //0.13f
    private float nextFire = 0.0f;
    private PhotonView PV;
    private AudioSource audioSource;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePos;
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private bool isAlive = true;
    private float health = 3;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Axis()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void Shoot()
    {
        if(Input.GetMouseButton(0) && Time.time > nextFire)
        {
            PV.RPC("ShootRpc", RpcTarget.All);
        }
    }

    [PunRPC]
    void ShootRpc()
    {

        GameObject bullet = Instantiate(Resources.Load<GameObject>("Prefabs/Bullet"), firePoint.position, firePoint.rotation);
        
        audioSource.PlayOneShot(fireSound);
        nextFire = Time.time + fireRate;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            Axis();

            if(isAlive)
                Shoot();

            cinemachineVirtualCamera.Follow = transform;
        }
    }

    void FixedUpdate()
    {
        if (PV.IsMine)
        {
            //Evita problema de aumentar velocidade na diagonal
            if (movement.magnitude > 1)
                movement = movement.normalized;

            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);


            Vector2 lookDir = mousePos - rb.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    public bool IsAlive() { 
        return isAlive; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Zombie>() != null)
        {
            health -= 1;

            if(health <= 0)
            {
                PV.RPC("HideDeadPlayer", RpcTarget.All, PV.ViewID);
            }
            else
            {
                PV.RPC("LostLife", RpcTarget.All, PV.ViewID);
            }
        }
    }

    [PunRPC]
    public void HideDeadPlayer(int playerID)
    {
        isAlive = false;
        spriteObject.SetActive(false);
    }

    [PunRPC]
    public void LostLife(int playerID)
    {
        StartCoroutine(Respawn());
    }

    [PunRPC]
    IEnumerator Respawn()
    {
        spriteObject.SetActive(false);
        boxCollider.enabled = true;
        isAlive = false;
        yield return new WaitForSeconds(5);
        transform.position = new Vector3(10, 10, 0);
        spriteObject.SetActive(true);
        boxCollider.enabled = true;
        isAlive = true;
    }
}
