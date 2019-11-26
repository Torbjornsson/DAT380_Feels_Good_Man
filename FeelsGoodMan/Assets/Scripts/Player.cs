using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    private const double gravity_constant = 0.05; //actually 6.67430e-11
    public float sprintAcceleration, jumpThrust, arrowDistance;
    public Weapon weapon;
    public int lives;
    private Vector3 mousePos, playerPos, dir;
    private float angle;
    public double mass = 10;
    private MassObject[] massObjects;
    private TouchMode touchMode;
    private Rigidbody rb;
    public GameObject arrow;

    private const float air_resistance = 0.1f;
    private float surface_resistance = 0.1f;
    private float timer = 1.0f;
    private bool paused = false;

    private enum TouchMode
    {
        none, platform, instant_death
    }


    void Start()
    {
        touchMode = TouchMode.none;
        GameObject.Find("Text").GetComponent<UnityEngine.UI.Text>().text = "Lives: " + lives.ToString();
        GameObject.Find("RespawnPoint").transform.position = transform.position;
        massObjects = GameObject.FindObjectsOfType<MassObject>();

        if (rb == null) 
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (paused) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                paused = false;
                timer = 1.0f;
                touchMode = TouchMode.none;
                Death();
                rb.WakeUp();
            } else {
                return;
            }
        }

        mousePos= Input.mousePosition;
        mousePos= Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = 0;
        playerPos = gameObject.transform.position;

        dir = Vector3.Normalize(mousePos- playerPos);
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        weapon.transform.position = playerPos + dir;
        weapon.transform.rotation = Quaternion.Euler(new Vector3(0,0,angle-90));
        weapon.dir = dir;

        if (arrow) {
            dir = GameObject.Find("Goal").transform.position - playerPos;
            arrow.transform.position = playerPos + Vector3.Normalize(dir) * arrowDistance;
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(new Vector3(0,0,angle + 90));
        }

        ApplyAirResistance();
        switch (touchMode)
        {
            case TouchMode.platform:
                ApplySurfaceResistance();
                HandlePlatformMovement();
                break;
            
            case TouchMode.none: 
                ApplyGravity(); 
                break;

            case TouchMode.instant_death:
                paused = true;
                rb.Sleep();
                break;
        }

        weapon.SetFiring(Input.GetMouseButton(0));
        if (Input.GetKey("r"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }

    private void ApplyAirResistance()
    {
        rb.velocity -= rb.velocity * Time.deltaTime * surface_resistance;
    }

    private void ApplySurfaceResistance()
    {
        rb.velocity -= rb.velocity * Time.deltaTime * air_resistance;
    }

    private void HandlePlatformMovement()
    {

        if (Input.GetKey("d"))
        {
            rb.velocity += Vector3.right * sprintAcceleration * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            rb.velocity += Vector3.left * sprintAcceleration * Time.deltaTime;
        }
        if (Input.GetKey("w"))
        {
            rb.velocity += Vector3.up * jumpThrust;
        }
    }

    private void ApplyGravity()
    {
        if (massObjects != null && massObjects.Length > 0)
            foreach (MassObject obj in massObjects)
            {
                Vector3 v = obj.transform.position - gameObject.transform.position;
                double d = v.magnitude;
                double g = gravity_constant * mass * obj.mass / (d * d);
                rb.AddForce(v.normalized * (float)g);
            }
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case "Platform":
                surface_resistance = collision.gameObject.GetComponent<PlatformResistance>().surface_resistance;
                touchMode = TouchMode.platform;
                break;
            case "Obstacle":
                Debug.Log("Hit an obstacle!");
                touchMode = TouchMode.instant_death;
                break;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        touchMode = TouchMode.none;    
    }

    void Death()
    {
        lives--;
        if (lives > 0)
        {
            transform.position = GameObject.Find("RespawnPoint").transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            weapon.Reset();
            GameObject.Find("Text").GetComponent<UnityEngine.UI.Text>().text = "Lives: " + lives.ToString();
        }
        else
        {
        Debug.Log("Quitting!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
