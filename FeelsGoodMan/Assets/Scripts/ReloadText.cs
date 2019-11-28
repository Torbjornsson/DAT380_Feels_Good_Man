using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadText : MonoBehaviour
{
    GameObject weapon;
    private float cooldown;
    // Start is called before the first frame update
    void Start()
    {
        weapon = GameObject.Find("Weapon");
    }

    // Update is called once per frame
    void Update()
    {
        cooldown = weapon.GetComponent<Weapon>().cooldown;
        if (cooldown > 0)
            GetComponent<UnityEngine.UI.Text>().text = cooldown.ToString();
        else
            GetComponent<UnityEngine.UI.Text>().text = "";
    }
}
