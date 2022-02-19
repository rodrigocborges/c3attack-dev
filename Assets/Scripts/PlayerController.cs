using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private GameObject spriteObject;
    [SerializeField] private float speed;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private Transform firePoint;
    private float fireRate = 0.75f; //0.13f
    private float nextFire = 0.0f;
    private PhotonView PV;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePos;
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    
    private Image heart;
    private float heart_count;
    private bool is_alive = true;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        heart_count = 1.0f;
        heart = GameObject.Find("Healthbar").GetComponent<Image>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
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
            
            if (is_alive) 
            {
                Shoot();
            }
            
            cinemachineVirtualCamera.Follow = transform;
        }
    }

    void FixedUpdate()
    {
        if (PV.IsMine)
        {
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

            Vector2 lookDir = mousePos - rb.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<Zombie>() != null)
        {
            if (PV.IsMine)
            {
                heart_count = heart_count - 0.340f;
                heart.fillAmount = heart_count;
                if (heart_count <= 0) 
                {
                    PV.RPC("HideDeadPlayer", RpcTarget.All, PV.ViewID);
                }
                else 
                {
                    PV.RPC("LostLife", RpcTarget.All, PV.ViewID);
                }
            }
        }
    }

    [PunRPC]
    public void HideDeadPlayer(int playerId)
    {
        is_alive = false;
        spriteObject.SetActive(false);
    }

    [PunRPC]
    public void LostLife(int playerId)
    {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        spriteObject.SetActive(false);
        is_alive = false;
        boxCollider.enabled = false;        
        yield return new WaitForSeconds(5);
        transform.position = new Vector3(-5, 5, 0);
        spriteObject.SetActive(true);
        boxCollider.enabled = true;
        is_alive = true;
    }

}
