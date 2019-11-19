using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public float thrust, distance;
    public GameObject weapon;
    private Vector3 mouse, player, dir;
    private float angle;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mouse = Input.mousePosition;
        mouse = Camera.main.ScreenToWorldPoint(mouse);
        mouse.z = 0;
        player = gameObject.transform.position;

        dir = Vector3.Normalize(mouse - player);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        weapon.transform.position = player + dir;
        weapon.transform.rotation = Quaternion.Euler(new Vector3(0,0,angle-90));

        if (Input.GetKey("d"))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.right * thrust);
        }
        if (Input.GetKey("a"))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left * thrust);
        }
        if (Input.GetKey("s"))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * thrust);
        }
        if (Input.GetKey("w"))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * thrust);
        }
        if (Input.GetMouseButtonDown(0))
        {
            gameObject.GetComponent<Rigidbody>().AddForce(-Vector3.Normalize(dir) * thrust * 100);
        }
    }
}
