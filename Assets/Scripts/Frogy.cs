using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Frogy : MonoBehaviour
{
    Rigidbody rb;

    /*[SerializeField]
    Canvas frogyCanvas;
    [SerializeField]
    GameObject frogObject;*/

    [SerializeField]
    Animator frogAnimator;

    [SerializeField]
    AudioSource jumpAudio;

    float jumpTimer = 0.0f;
    [SerializeField]
    float maxJump = 1.0f;
    [SerializeField]
    float jumpStrength = 1.0f;

    [SerializeField]
    float turnSpeed = 10.0f;

    bool isRotatingLeft = false;
    bool isRotatingRight = false;
    bool doubleRotateSpeed = false;

    int flyCount = 0;

    List<GameObject> collectedFlies = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Start Jump Animation
            frogAnimator.SetTrigger("PrepareJump");
        }
        else if(Input.GetKey(KeyCode.Space))
        {
            // Increase Jump Time
            jumpTimer += Time.deltaTime;

            jumpTimer = Mathf.Clamp(jumpTimer, 0, maxJump);

            float jumpPercentage = jumpTimer / maxJump;
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            // Play leap animation
            frogAnimator.ResetTrigger("PrepareJump");
            frogAnimator.SetBool("IsJumping", true);

            if (jumpTimer / maxJump < 0.2f)
            {
                StartCoroutine(ExitJumpEarly());
            }

            // Jump
            rb.AddRelativeForce(new Vector3(0.0f, jumpTimer * jumpStrength, jumpTimer * jumpStrength), ForceMode.Impulse);
            jumpAudio.pitch = 1 + ((1 - jumpTimer / maxJump) * 0.5f) - Random.Range(0.05f, 0.1f);
            jumpAudio.Play();
            jumpTimer = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // Rotate left
            isRotatingLeft = true;
        }
        else if(Input.GetKeyUp(KeyCode.A))
        {
            isRotatingLeft = false;
        }

        if(Input.GetKey(KeyCode.D))
        {
            // Rotate right
            isRotatingRight = true;
        }
        else if(Input.GetKeyUp(KeyCode.D))
        {
            isRotatingRight = false;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            doubleRotateSpeed = true;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            doubleRotateSpeed = false;
        }
    }

    IEnumerator ExitJumpEarly()
    {
        yield return new WaitForSeconds(0.1f);
        frogAnimator.SetBool("IsJumping", false);
    }

    private void FixedUpdate()
    {
        if(isRotatingLeft)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, doubleRotateSpeed ? -turnSpeed * 2 : -turnSpeed, 0.0f) * Time.fixedDeltaTime);
            rb.MoveRotation(deltaRotation * rb.rotation);
        }
        else if(isRotatingRight)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0.0f, doubleRotateSpeed ? turnSpeed * 2 : turnSpeed, 0.0f) * Time.fixedDeltaTime);
            rb.MoveRotation(deltaRotation * rb.rotation);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        frogAnimator.SetBool("IsJumping", false);

        if (collision.collider.transform.CompareTag("Water"))
        {
            Debug.Log("Collided with water!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Fly"))
        {
            Debug.Log("Found Fly!");
            flyCount++;
            other.gameObject.SetActive(false);
            // Add to array of collected flies
            collectedFlies.Add(other.gameObject);
        }
        else if(other.CompareTag("Home"))
        {
            Debug.Log("Depositing Flies!");
            HomeManager home = other.GetComponent<HomeManager>();
            home.DepositFlies(flyCount);
            flyCount = 0;
            // Empty array of collected flies
            collectedFlies.Clear();
        }
    }
}
