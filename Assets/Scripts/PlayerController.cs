using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
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

            Shoot();

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
}
