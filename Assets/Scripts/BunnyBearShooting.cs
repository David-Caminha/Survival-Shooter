using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyBearShooting : MonoBehaviour {

    GameObject player;
    int counter;
    BulletManager bulletManager;

    public bool shoots = false;
    public bool isBear = false;
    public Transform shotOrigin;
    public int countsBetweenShots = 3;

    void Start()
    {
        counter = 3;
        player = GameObject.FindGameObjectWithTag("Player");
        bulletManager = BulletManager.Instance;
        Debug.Log(bulletManager);
    }

    void Shoot()
    {
        if(shoots)
        {
            counter++;
            if(counter >= countsBetweenShots && Vector3.Distance(transform.position, player.transform.position) < 15)
            {
                counter = 0;
                TakeShot();
            }
        }
    }

    public void TakeShot()
    {
        shotOrigin.LookAt(player.transform);
        shotOrigin.rotation = Quaternion.Euler(0, shotOrigin.transform.rotation.eulerAngles.y, 0);
        bulletManager.Shoot(shotOrigin.position, shotOrigin.rotation);
        if (isBear)
        {
            shotOrigin.transform.rotation = Quaternion.Euler(0, shotOrigin.transform.rotation.eulerAngles.y + 15, 0);
            bulletManager.Shoot(shotOrigin.position, shotOrigin.rotation);

            shotOrigin.transform.rotation = Quaternion.Euler(0, shotOrigin.transform.rotation.eulerAngles.y - 30, 0);
            bulletManager.Shoot(shotOrigin.position, shotOrigin.rotation);
        }
    }
}
