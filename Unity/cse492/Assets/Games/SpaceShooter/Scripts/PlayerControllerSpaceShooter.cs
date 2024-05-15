using System.Collections;
using UnityEngine;

public class PlayerControllerSpaceShooter : MonoBehaviour
{
    public float speed = 1f;
    public float min_Y, max_Y;

    [SerializeField] private GameObject Player_Bullet;

    [SerializeField] private Transform attack_Point;

    public float attack_Timer = 1f;
    private float current_Attack_Timer;
    private bool attack_On;

    private AudioSource laserAudio;

    private Coroutine currentMoveCoroutine;
    [SerializeField] private bool isMoving = false;

    void Awake()
    {
        laserAudio = GetComponent<AudioSource>();
    }

    void Start()
    {
        current_Attack_Timer = attack_Timer;
    }

    void Update()
    {
        attack_Timer += Time.deltaTime;
        if (attack_Timer > current_Attack_Timer)
        {
            attack_On = true;
        }
    }

    public void StartMoving(int direction)
    {
        if (isMoving)
        {
            if (transform.position.y >= max_Y && direction > 0 || transform.position.y <= min_Y && direction < 0)
            {
                StopMoving();
                return;
            }
        }
        
        isMoving = true;
        currentMoveCoroutine = StartCoroutine(MoveContinuously(direction));
    }

    public void StopMoving()
    {
        if (!isMoving) return;

        isMoving = false;
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            // Stop the current momentum
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
    }

    private IEnumerator MoveContinuously(int direction)
    {
        while (isMoving)
        {
            Vector3 temp = transform.position;
            temp.y += direction * speed * Time.deltaTime;

            if (temp.y > max_Y)
                temp.y = max_Y;
            else if (temp.y < min_Y)
                temp.y = min_Y;

            transform.position = temp;
            yield return null;
        }
    }

    public void Shoot()
    {
        if (attack_On)
        {
            attack_On = false;
            attack_Timer = 0f;
            Instantiate(Player_Bullet, attack_Point.position, Quaternion.identity);

            // play sound FX
            laserAudio.Play();
        }
    }
}
