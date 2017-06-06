using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    List<GameObject> freeShots;
    List<GameObject> onSceneShots;

    public GameObject shotPrefab;

    private static BulletManager instance;
    public static BulletManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        instance = this;
        freeShots = new List<GameObject>();
        onSceneShots = new List<GameObject>();
    }

    public void Shoot(Vector3 position, Quaternion rotation)
    {
        if (freeShots.Count == 0)
        {
            GameObject shot = Instantiate(shotPrefab, position, rotation, transform);
            onSceneShots.Add(shot);
        }
        else
        {
            GameObject shot = freeShots[0];
            onSceneShots.Add(shot);
            freeShots.RemoveAt(0);
            shot.SetActive(true);
            shot.transform.position = position;
            shot.transform.rotation = rotation;
        }
    }

    public void StopBullet(GameObject bullet)
    {
        freeShots.Add(bullet);
        onSceneShots.Remove(bullet);
        bullet.SetActive(false);
    }
}
