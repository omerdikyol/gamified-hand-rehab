﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public float speed = 2f;
    public float rotateSpeed = 50f;
    public bool canShoot;
    public bool canRotate;
    public float bound_X = -20f;
    public Transform attack_Point;
    public GameObject bulletPrefab;


    private bool canMove = true;
    private Animator anim;
    private AudioSource explosionSound;

    // Start is called before the first frame update
    void Start()
    {
        if(canRotate)
        {
            if(Random.Range(0, 2) > 0)
            {
                rotateSpeed = Random.Range(rotateSpeed, rotateSpeed + 20f);
                rotateSpeed *= -1f;
            }
        }

        if(canShoot)
        {
            Invoke("Shooting", Random.Range(1f, 3f));
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        explosionSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        RoteateEnemy();
    }

    void Move()
    {
        if(canMove)
        {
            Vector3 temp = transform.position;
            temp.x -= speed * Time.deltaTime;
            transform.position = temp;

            if(temp.x < bound_X)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void RoteateEnemy()
    {
        if(canRotate)
        {
            transform.Rotate(new Vector3(0f, 0f, rotateSpeed * Time.deltaTime), Space.World);
        }
    }

    void Shooting()
    {
        GameObject bullet = Instantiate(bulletPrefab, attack_Point.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().is_EnemyBullet = true;

        if(canShoot)
        {
            Invoke("Shooting", Random.Range(1f, 3f));
        }
    }

    void TurnOffGameObject()
    {
        gameObject.SetActive(false);   
    }

    void OnTriggerEnter2D(Collider2D target){

        if(target.tag =="Bullet" && !target.GetComponent<Bullet>().is_EnemyBullet)
        {
            canMove = false;
            if(canShoot)
            {
                canShoot = false;
                CancelInvoke("Shooting");
            }

            Invoke("TurnOffGameObject", 2f);

            //play explosion sound
            explosionSound.Play();
            anim.Play("Destroyed");
        }
    }
}
