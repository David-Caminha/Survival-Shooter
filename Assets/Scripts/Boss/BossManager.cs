using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public Transform shotOrigin;
    public GameObject healthBar;
    public EnemyHealth enemyHealth;

    Animator anim;
    int bossPhase = 0;
    float phaseAttackTimer = 5;
    int attackCycle = 0;
    float timer;
    BulletManager bulletManager;

    void Awake()
    {
        // Setting up the references.
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        bulletManager = BulletManager.Instance;
        StartCoroutine(Attack());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        switch (bossPhase)
        {
            case 0:
                if (timer > 5)
                {
                    anim.SetTrigger("Attack");
                    timer = 0;
                }
                break;
            case 1:
                if (timer > 5)
                {
                    anim.SetTrigger("Attack");
                    timer = 0;
                }
                break;
            case 2:
                if (timer > 4)
                {
                    anim.SetTrigger("Attack");
                    timer = 0;
                }
                break;
            case 3:
                if (timer > 3)
                {
                    anim.SetTrigger("Attack");
                    timer = 0;
                }
                break;
        }
    }

    IEnumerator Attack()
    {
        while(enemyHealth.currentHealth > 0)
        {
            yield return new WaitForSeconds(phaseAttackTimer);
            if (enemyHealth.currentHealth <= 1000)
            {
                phaseAttackTimer = 4;
            }
            if (enemyHealth.currentHealth <= 500)
            {
                phaseAttackTimer = 3;
            }
            anim.SetTrigger("Attack");
        }
    }

    public void Shoot()
    {
        attackCycle = attackCycle % 5;
        switch (bossPhase)
        {
            case 0:
                if(attackCycle == 0 || attackCycle == 1 || attackCycle == 3)
                {
                    int rand = Random.Range(0, 45);
                    CircleAttack(rand);
                }
                else if(attackCycle == 2 || attackCycle == 4)
                {
                    ConeAttack();
                }
                break;
            case 1:
                if (attackCycle == 0 || attackCycle == 3)
                {
                    int rand = Random.Range(0, 45);
                    CircleAttack(rand);
                }
                else if (attackCycle == 1 || attackCycle == 4)
                {
                    int rand = Random.Range(0, 360);
                    ConeAttack(rand);
                    ConeAttack(rand + 180);
                }
                else
                {
                    int rand = Random.Range(0, 360);
                    SpiralAttack(rand);
                }
                break;
            case 2:
                if (attackCycle == 0 || attackCycle == 3)
                {
                    int rand = Random.Range(0, 45);
                    CircleAttack(rand, 40);
                }
                else if (attackCycle == 1 || attackCycle == 4)
                {
                    int rand = Random.Range(0, 360);
                    ConeAttack(rand);
                    ConeAttack(rand + 90);
                    ConeAttack(rand - 90);
                }
                else
                {
                    int rand = Random.Range(0, 360);
                    SpiralAttack(rand, 15);
                }
                break;
            case 3:
                if (attackCycle == 0)
                {
                    int rand = Random.Range(0, 45);
                    CircleAttack(rand, 30);
                }
                else if (attackCycle == 2)
                {
                    int rand = Random.Range(0, 360);
                    ConeAttack(rand);
                    ConeAttack(rand + 90);
                    ConeAttack(rand - 90);
                    ConeAttack(rand + 180);
                }
                else if (attackCycle == 1)
                {
                    int rand = Random.Range(0, 360);
                    SpiralAttack(rand, 10);
                }
                else if (attackCycle == 3)
                {
                    int rand = Random.Range(0, 360);
                    SpiralAttack(rand, -10);
                }
                else if (attackCycle == 4)
                {
                    int rand = Random.Range(0, 360);
                    SpiralAttack(rand, 10);
                    SpiralAttack(rand, -10);
                }
                break;
        }
        attackCycle++;
        anim.SetTrigger("StopAttack");
    }

    void CircleAttack(float startOffset = 0, float bulletAngle = 45)
    {
        Quaternion originalRotation = Quaternion.Euler(0, shotOrigin.rotation.eulerAngles.y + startOffset, 0);
        int num = (int) (360 / bulletAngle);
        for (int i = 0; i < num; i++)
        {
            Quaternion rot = Quaternion.Euler(0, originalRotation.eulerAngles.y + i * bulletAngle, 0);
            bulletManager.Shoot(shotOrigin.position, rot);
        }
    }

    void SpiralAttack(float startOffset = 0, float bulletAngle = 20)
    {
        StartCoroutine(ShootSpiral(startOffset, bulletAngle));
    }

    void ConeAttack(float angleOffset = 0)
    {
        Quaternion centerRotation;
        centerRotation = Quaternion.Euler(0, shotOrigin.rotation.eulerAngles.y + angleOffset, 0);
        bulletManager.Shoot(shotOrigin.position, centerRotation);

        Quaternion rot1 = Quaternion.Euler(0, centerRotation.eulerAngles.y + 10, 0);
        bulletManager.Shoot(shotOrigin.position, rot1);

        Quaternion rot2 = Quaternion.Euler(0, centerRotation.eulerAngles.y - 10, 0);
        bulletManager.Shoot(shotOrigin.position, rot2);

        Quaternion rot3 = Quaternion.Euler(0, centerRotation.eulerAngles.y + 20, 0);
        bulletManager.Shoot(shotOrigin.position, rot3);

        Quaternion rot4 = Quaternion.Euler(0, centerRotation.eulerAngles.y - 20, 0);
        bulletManager.Shoot(shotOrigin.position, rot4);
    }

    IEnumerator ShootSpiral(float startOffset, float bulletAngle)
    {
        Quaternion originalRotation = Quaternion.Euler(0, shotOrigin.rotation.eulerAngles.y + startOffset, 0);
        int num = (int) (360 / bulletAngle);
        for(int i = 0; i < num; i++)
        {
            Debug.Log(i * bulletAngle);
            Quaternion rot = Quaternion.Euler(0, originalRotation.eulerAngles.y + i * bulletAngle, 0);
            bulletManager.Shoot(shotOrigin.position, rot);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
