using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Player player;
    public float recoil;
    public float reload;
    public Vector3 dir;

    private bool firing;

    private float cooldown = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (firing)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0)
            {
                GetComponentInChildren<ParticleSystem>().Play();
                cooldown += reload;
                player.gameObject.GetComponent<Rigidbody>().velocity += -Vector3.Normalize(dir) * recoil;
            }
        } 
        else
        {
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
        }
    }

    public void SetFiring(bool firing)
    {
        this.firing = firing;
    }

    public bool IsFiring()
    {
        return firing;
    }

    public void Reset()
    {
        cooldown = 0;
    }

}
