using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public Animator canvasAnim;
    public Button exitGameBtn;
    public Text endingText;

    public Transform shotOrigin;
    public Slider healthBar;
    public EnemyHealth enemyHealth;
    public GameObject[] enemyGroups;

    Animator anim;
    public int bossPhase = 0;
    public float phaseAttackTimer = 2;
    public int attackCycle = 0;
    BossBulletManager bulletManager;
    public int groupIndex = 0;
    bool gameOver = false;

    void Awake()
    {
        // Setting up the references.
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        bulletManager = BossBulletManager.Instance;
        StartCoroutine(Attack());
        StartCoroutine(GroupManager());
    }

    void Update()
    {
        if (enemyHealth.currentHealth <= 0 && !gameOver)
        {
            gameOver = true;
            EndGame();
        }
        healthBar.value = enemyHealth.currentHealth;
    }

    IEnumerator Attack()
    {
        while(enemyHealth.currentHealth > 0)
        {
            yield return new WaitForSeconds(phaseAttackTimer);
            if (enemyHealth.currentHealth <= 1500)
            {
                bossPhase = 1;
            }
            if (enemyHealth.currentHealth <= 1000)
            {
                phaseAttackTimer = 1.5f;
                bossPhase = 2;
            }
            if (enemyHealth.currentHealth <= 500)
            {
                phaseAttackTimer = 1;
                bossPhase = 3;
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
        StopCoroutine("ShootSpiral");
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
            Quaternion rot = Quaternion.Euler(0, originalRotation.eulerAngles.y + i * bulletAngle, 0);
            bulletManager.Shoot(shotOrigin.position, rot);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator GroupManager()
    {
        while(true)
        {
            Debug.Log("groupmanager");
            switch (groupIndex)
            {
                case 0:
                    if (enemyHealth.currentHealth <= 1750)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[0].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[0].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex = 1;
                    }
                    break;
                case 1:
                    if (enemyHealth.currentHealth <= 1300)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[1].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[1].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex = 2;
                    }
                    break;
                case 2:
                    if (enemyHealth.currentHealth <= 1000)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[2].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[2].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex++;
                    }
                    break;
                case 3:
                    if (enemyHealth.currentHealth <= 800)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[3].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[3].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex++;
                    }
                    break;
                case 4:
                    if (enemyHealth.currentHealth <= 600)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[4].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[4].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex++;
                    }
                    break;
                case 5:
                    if (enemyHealth.currentHealth <= 500)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[5].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[5].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex++;
                    }
                    break;
                case 6:
                    if (enemyHealth.currentHealth <= 400)
                    {
                        EnemyMovement[] moveScripts = enemyGroups[6].GetComponentsInChildren<EnemyMovement>();
                        Animator[] anims = enemyGroups[6].GetComponentsInChildren<Animator>();
                        for (int i = 0; i < moveScripts.Length; i++)
                        {
                            anims[i].gameObject.tag = "Enemy";
                            moveScripts[i].enabled = true;
                            anims[i].SetTrigger("Moving");
                        }
                        groupIndex++;
                    }
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }


    void EndGame()
    {
        endingText.text = "GOOD JOB YOU WON! Now if only you had a way to go back home...";
        exitGameBtn.gameObject.SetActive(false);
        canvasAnim.SetTrigger("GameOver");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Animator>().SetTrigger("Dead");
        }
        GameObject[] spectators = GameObject.FindGameObjectsWithTag("Spectator");
        for (int i = 0; i < spectators.Length; i++)
        {
            spectators[i].GetComponent<Animator>().SetTrigger("Dead");
        }
        Invoke("MainMenu", 10f);
    }

    void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
