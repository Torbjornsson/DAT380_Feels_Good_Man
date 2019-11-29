using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    // G is actually 6.67430e-11, but with 1 we can use small masses and distances for gravity objects.
    private const double gravity_constant = 1.0; 
    public float sprintAcceleration, jumpThrust, arrowDistance;
    public Weapon weapon;
    public int lives;
    private Vector3 mousePos, playerPos, dir;
    private float angle;
    private MassObject[] massObjects;
    private TouchMode touchMode;
    private Rigidbody rb;
    public GameObject arrow;

    private const float air_resistance = 0.1f;
    private float kinetic_friction_coefficient = 0.1f;
    private float static_friction_coefficient = 0.1f;
    private float timer = 1.0f;
    private bool paused = false;

    private const double espilon = 0.0001;

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
        //Since the size and shape of the object remains unchanged
        //And the air density and drag coefficient are constant
        //We model these with an air_resistance constant.
        //Otherwise: Drag force = 1/2 * density * v^2 * drag coefficient * cross section area
        rb.velocity -= rb.velocity * rb.velocity.magnitude * Time.deltaTime * air_resistance;
    }

    private void ApplySurfaceResistance()
    {
        //Normal force: N = m*g
        //Kinetic friction: Kinetic friction force = kinetic friction coefficient * normal force

        //If moving, apply kinetic friction
        //Should be multiplied by normal force, but we don't have sloping platforms at the moment, so we don't need it.
        if (rb.velocity.magnitude > 0) rb.velocity -= rb.velocity.normalized * Time.deltaTime * kinetic_friction_coefficient;

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

                //F=a*m <=> a = F/m. In this case a = G*m/(d^2)
                //How to deal with d approaching 0? Could use max function with small constant for a lower cap.
                //Also, what distance unit are we even using? Formula works regardless, but the constant changes.
                double a_g = gravity_constant * obj.mass / (d * d);
                rb.velocity += v.normalized * (float) a_g * Time.deltaTime;
            }
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case "Platform":
                kinetic_friction_coefficient = collision.gameObject.GetComponent<PlatformResistance>().kinetic_friction_coefficient;
                static_friction_coefficient = collision.gameObject.GetComponent<PlatformResistance>().static_friction_coefficient;
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
