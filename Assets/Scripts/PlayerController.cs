using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float moveSensitivity = 1f;
    private float verticalRotationStore;
    private Vector2 mouseInput;

    public bool inverLook;

    public float moveSpeed = 5f, runSpeed = 8f;
    private Vector3 moveDir, movement;
    private float activeMoveSpeed;
    public CharacterController charCon;

    private Camera cam;

    public float jumpForce = 12f, gracityMod = 2.5f;

    public Transform groundCheckPoint;
    public bool isGrounded;
    public LayerMask groundLayers;

    public GameObject bulletImpact;
    //public float timeBetweenShots = .1f;
    private float shotCounter;
    public float muzzleDispalyTime;
    private float muzzleCounter;

    public float maxHeat = 10f, /* heatPerShot = 1f, */ coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;

        SwitchGun();

        Transform newTrans = SpawnManager.instance.GetSpawnPoint();
        transform.position = newTrans.position;
        transform.rotation = newTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * moveSensitivity;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotationStore += mouseInput.y;
        verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);

        if (inverLook)
        {
            viewPoint.rotation = Quaternion.Euler(verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }


        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        float yVel = movement.y;

        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;

        if (charCon.isGrounded)
        {
            movement.y = 0f;
        }

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .5f, groundLayers);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gracityMod;

        charCon.Move(movement * Time.deltaTime);

        //Shooting
        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if(muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);
            }
        }
       

        if (!overHeated)
        {

            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }
            heatCounter -= coolRate * Time.deltaTime;
        } else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if(heatCounter <= 0)
            {
                overHeated = false;
                UIController.instance.overheatedMessage.gameObject.SetActive(false);
            }
        }

        if(heatCounter < 0 )
        {
            heatCounter = 0;
        }
        UIController.instance.weaponTempSlider.value = heatCounter;

        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;

            if(selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
            SwitchGun();
        } else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;

            if (selectedGun < allGuns.Length)
            {
                selectedGun = allGuns.Length -1;
                
            }
            SwitchGun();
        }

        for(int i = 0; i < allGuns.Length; i++)
        {
            if(Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;
                SwitchGun();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("We hit " + hit.collider.gameObject.name);
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            Destroy(bulletImpactObject, 10f);
        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if(heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;

            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDispalyTime;
    }

    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    void SwitchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }
}
