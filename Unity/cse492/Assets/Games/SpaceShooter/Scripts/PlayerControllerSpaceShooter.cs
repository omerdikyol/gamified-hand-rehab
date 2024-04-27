using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerSpaceShooter : MonoBehaviour
{
    public float speed = 30f;

    public float min_Y, max_Y;

    [SerializeField]
    private GameObject Player_Bullet;

    [SerializeField]
    private Transform attack_Point;

    public float attack_Timer = 1f;
    private float current_Attack_Timer;
    private bool attack_On;

    private AudioSource laserAudio;

    void Awake()
    {
        laserAudio = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        current_Attack_Timer = attack_Timer;
    }

    // Update is called once per frame
    void Update()
    {
        attack_Timer += Time.deltaTime;
        if (attack_Timer > current_Attack_Timer)
        {
            attack_On = true;
        }
    }

    public void MoveUp()
    {
        Vector3 temp = transform.position;
        temp.y += speed * Time.deltaTime;

        if(temp.y > max_Y)
            temp.y = max_Y;

        transform.position = temp;
    }

    public void MoveDown()
    {
        Vector3 temp = transform.position;
        temp.y -= speed * Time.deltaTime;

        if(temp.y < min_Y)
            temp.y = min_Y;
        
        transform.position = temp;
    }

    public void Shoot()
    {
        if(attack_On)
        {
            attack_On = false;
            attack_Timer = 0f;
            Instantiate(Player_Bullet, attack_Point.position, Quaternion.identity);

            //play sound FX
            laserAudio.Play();
        }
    }
}
