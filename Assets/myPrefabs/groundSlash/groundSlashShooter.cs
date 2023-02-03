using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class groundSlashShooter : MonoBehaviour
{
    public GameObject projectilePref = default;
    public Transform shootTrans = default;
    public float fireRate = 4;

    private Vector3 destination = default;
    private bool canShoot=true;
    private WaitForSeconds wfs;
    private groundSlash groundSlashScript;
    private void Update()
    {
        if (Input.GetButton("Fire1") && canShoot){
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        canShoot = false;
        StartCoroutine(shootCooldown());

        Ray ray = new Ray(shootTrans.position,shootTrans.forward);
        destination = ray.GetPoint(1000);

        InstantiateProjectile();
    }

    private void InstantiateProjectile()
    {
        GameObject projectObj = Instantiate(projectilePref, shootTrans.position, Quaternion.identity);
        groundSlashScript = projectObj.GetComponent<groundSlash>();
        RotateToDestination(projectObj,destination,true);
        projectObj.GetComponent<Rigidbody>().velocity = shootTrans.forward * groundSlashScript.speed;
    }

    private void RotateToDestination(GameObject obj, Vector3 destination, bool onlyY)
    {
        Vector3 direction = destination - obj.transform.position;
        Quaternion rot = Quaternion.LookRotation(direction);

        if (onlyY)
        {
            rot.x = 0;
            rot.z = 0;
        }

        obj.transform.localRotation = rot; //Quaternion.Lerp(obj.transform.rotation, rot, 1); //?????????
    }

    IEnumerator shootCooldown()
    {
        wfs = new WaitForSeconds(fireRate);
        yield return wfs;
        canShoot = true;
    }
}
