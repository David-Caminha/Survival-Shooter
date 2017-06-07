using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElephantShooting : MonoBehaviour
{
    GameObject player;
    int counter;
    BulletManager bulletManager;

    public bool shoots = false;
    public Transform shotOrigin;
    public int countsBetweenShots = 3;

    void Start()
    {
        counter = 0;
        player = GameObject.FindGameObjectWithTag("Player");
        bulletManager = BulletManager.Instance;
    }

    void Shoot()
    {
        if (shoots)
        {
            counter++;
            if (counter >= countsBetweenShots && Vector3.Distance(transform.position, player.transform.position) < 15)
            {
                counter = 0;
                TakeShot();
            }
        }
    }

    public void TakeShot()
    {
        for (int i = 0; i < 8; i++)
        {
            shotOrigin.Rotate(0, i * 45, 0);
            bulletManager.Shoot(shotOrigin.position, shotOrigin.rotation);
        }
    }
}
