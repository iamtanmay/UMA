using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;
using System.Collections;

public class SetupAndUserInput : MonoBehaviour
{
    public LayerMask groundLayer; // get this layer from smbs to determine which layer is ground
    public float sensitivityX = 1, sensitivityY = 1;
    public float Horizontal { get; private set; }
    public float MouseX { get; private set; }
    public float MouseY { get; private set; }
    public float Vertical { get; private set; }
    public bool InputEnabled = true;
    public bool movementInputEnabled = true;
    public float LastInputAt { get; private set; }      // To store the time of the last input detection.
    public bool CoverDown
    {
        get
        {
            return m_CoverDown;
        }
        private set { m_CoverDown = value; }
    }
    public bool CameraResetDown
    {
        get
        {
            return m_CameraResetDown;
        }
        private set { m_CameraResetDown = value; }
    }
    public bool Fire1Down
    {
        get
        {
            return m_Fire1Down;
        }
        private set { m_Fire1Down = value; }
    }
    public bool JumpDown                                // If the fire1 button is just been pressed.
    {
        get
        {
            return m_JumpDown;
        }
        private set { m_JumpDown = value; }
    }
    public bool DrawDown                               // If the draw Gun is just been pressed.
    {
        get
        {
            return m_DrawDown;
        }
        private set { m_DrawDown = value; }
    }
    public bool CrouchDown                             // If the crouch button is just been pressed.
    {
        get
        {
            return m_CrouchDown;
        }
        private set { m_CrouchDown = value; }
    }
    public bool FirePress
    {
        get
        {
            return m_FirePress;
        }
        private set { m_FirePress = value; }
    }
    public bool Fire2Press
    {
        get
        {
            return m_Fire2Press;
        }
        set { m_Fire2Press = value; }
    }
    public bool ReloadDown
    {
        get
        {
            return m_ReloadDown;
        }
        private set { m_ReloadDown = value; }
    }
    public bool MenuDown
    {
        get
        {
            return m_MenuDown;
        }
        private set { m_MenuDown = value; }
    }
    public bool UseDown
    {
        get
        {
            return m_UseDown;
        }
        private set { m_UseDown = value; }
    }
#if !MOBILE_INPUT
    public bool Item1Down
    {
        get
        {
            return m_Item1;
        }
        private set { m_Item1 = value; }
    }
    public bool Item2Down
    {
        get
        {
            return m_Item2;
        }
        private set { m_Item2 = value; }
    }
    public bool Item3Down
    {
        get
        {
            return m_Item3;
        }
        private set { m_Item3 = value; }
    }
    public bool Item4Down
    {
        get
        {
            return m_Item4;
        }
        private set { m_Item4 = value; }
    }
    public bool Item5Down
    {
        get
        {
            return m_Item5;
        }
        private set { m_Item5 = value; }
    }
    public bool Item6Down
    {
        get
        {
            return m_Item6;
        }
        private set { m_Item6 = value; }
    }
#endif
    public bool FlashLightDown
    {
        get
        {
            return m_FlashLightDown;
        }
        private set { m_FlashLightDown = value; }
    }
    public bool WalkToggleDown
    {
        get
        {
            return m_WalkToggleDown;
        }
        private set { m_WalkToggleDown = value; }
    }
    public bool ChangeGunPartsDown
    {
        get
        {
            return m_ChangeGunPartsDown;
        }
        private set { m_ChangeGunPartsDown = value; }
    }
    public bool MenuRightDown
    {
        get
        {
            return m_MenuRightDown;
        }
        set { m_MenuRightDown = value; }
    }
    public bool MenuLeftDown
    {
        get
        {
            return m_MenuLeftDown;
        }
        set { m_MenuLeftDown = value; }
    }
    public bool MenuDownDown
    {
        get
        {
            return m_MenuDownDown;
        }
        set { m_MenuDownDown = value; }
    }
    public bool MenuUpDown
    {
        get
        {
            return m_MenuUpDown;
        }
        set { m_MenuUpDown = value; }
    }
    public bool SecondaryFireDown
    {
        get
        {
            return m_SecondaryFire;
        }
        private set { m_SecondaryFire = value; }
    }
    public bool DropDown
    {
        get
        {
            return m_DropDown;
        }
        private set { m_DropDown = value; }
    }
    public bool FirstPersonLookDown
    {
        get
        {
            return m_FirstPersonLookDown;
        }
        private set { m_FirstPersonLookDown = value; }
    }
    public bool SprintPress
    {
        get
        {
            return m_SprintPress;
        }
        private set { m_SprintPress = value; }
    }

    public Transform cameraRig;         // Reference to the camera rig in the scene so that SMBs have access to it.
    public Transform mainCamera;        // Reference to the camera itself.


    private Animator m_Animator;        // Reference to the animator to initialise all the SMBs.
    public bool m_JumpDown;
    public bool m_DrawDown;
    public bool m_FirePress;
    public bool m_Fire2Press;
    public bool m_CrouchDown;
    public bool m_ReloadDown;
    public bool m_MenuDown;
    public bool m_CoverDown;
    public bool m_CameraResetDown;
    public bool m_FirstPersonLookDown;
    public bool m_UseDown;
    public bool m_DropDown;
    public bool m_SprintPress;
#if !MOBILE_INPUT
    public bool m_Item1;
    public bool m_Item2;
    public bool m_Item3;
    public bool m_Item4;    // not used
    public bool m_Item5;    // not used
    public bool m_Item6;    // not used
#endif

    public bool m_ChangeGunPartsDown;
    public bool m_FlashLightDown;
    public bool m_WalkToggleDown;
    public bool m_MenuLeftDown;
    public bool m_MenuRightDown;
    public bool m_MenuUpDown;
    public bool m_MenuDownDown;
    public bool m_SecondaryFire;
    public bool m_Fire1Down;

    public bool m_ResetButtons;       // Used to reset the JumpDown property if a frame has passed.

    public Transform moveReference;

#if MOBILE_INPUT
    private bool canGetXY = false;
    private int mouseTouchId;
    private PlayerAtts plAtts;
    public float minSwipeDistY = 45;
    public float minSwipeDistX = 45;
    private Vector2 startPos;
#endif

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (!mainCamera)
            mainCamera = Camera.main.transform;
        moveReference = mainCamera.parent.FindChild("MoveReference");
#if MOBILE_INPUT
        plAtts = GetComponent<PlayerAtts>();
#endif
    }


    private void OnEnable()
    {
        // Find all the SMBs on the animator that inherit from CustomSMB.
        CustomSMB[] allSMBs = m_Animator.GetBehaviours<CustomSMB>();
        for (int i = 0; i < allSMBs.Length; i++)
        {
            // For each SMB set it's userInput reference to this instance and run the initialisation function.
            allSMBs[i].userInput = this;
            allSMBs[i].Init(m_Animator);
        }
        LastInputAt = Time.time;
    }

    private void Update()
    {
        if (InputEnabled)
        {
#if !MOBILE_INPUT // normal controls
            Horizontal = Input.GetAxis("Horizontal");
            Vertical = Input.GetAxis("Vertical");
            MouseX = Input.GetAxis("Mouse X") * sensitivityX;
            MouseY = Input.GetAxis("Mouse Y") * sensitivityY;
#else  // mobile controls
            // swipes - menu left, right, up, down
            //#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                float restime = .3f;
                Touch touch = Input.touches[0];
                switch (touch.phase)
                {
                        
                    case TouchPhase.Began:
                        startPos = touch.position;
                        break;
                    case TouchPhase.Ended:
                        float swipeDistV = (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;
                        if (swipeDistV > minSwipeDistY)
                        {
                            float swipeValue = Mathf.Sign(touch.position.y - startPos.y);
                            if (swipeValue > 0)
                            {
                                MenuUpDown = true;
                                StartCoroutine(ResetSwipes(restime));
                            }
                                
                            else if (swipeValue < 0)
                            {
                                MenuDownDown = true;
                                StartCoroutine(ResetSwipes(restime));
                            }
                                
                        }
                        float swipeDistH = (new Vector3(touch.position.x, 0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
                        if (swipeDistH > minSwipeDistX)
                        {
                            float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
                            if (swipeValue > 0)
                            {
                                MenuRightDown = true;
                                StartCoroutine(ResetSwipes(restime));
                            }

                            else if (swipeValue < 0)
                            {
                                MenuLeftDown = true;
                                StartCoroutine(ResetSwipes(restime));
                            }
                                
                        }
                        break;
                }
            }

            // ready button
            if (CrossPlatformInputManager.GetButtonDown("Ready") && plAtts.cGun)
            {
                Fire2Press = !Fire2Press;
            }

            // sprint button
            if (Input.touchCount > 0 && CrossPlatformInputManager.GetButtonDown("Sprint") )
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        mouseTouchId = Input.GetTouch(i).fingerId;
                        break;
                    }
                }
            }
            if (CrossPlatformInputManager.GetButton("Sprint"))
            {
                canGetXY = true;
                SprintPress = true;
            }
            else
            {
                canGetXY = false;
                SprintPress = false;
            }
                

            // fire button & mouse movement logic for mobile
            // determine touch index
            if (Input.touchCount > 0 && (CrossPlatformInputManager.GetButtonDown("Fire") || CrossPlatformInputManager.GetButtonDown("RightSide")))
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        mouseTouchId = Input.GetTouch(i).fingerId;
                        break;
                    }
                }
            }

            // determine touch area & see if mouse movement should occur
            if (CrossPlatformInputManager.GetButton("Fire"))
            {
                FirePress = true;
                canGetXY = true;
            }
            else if( !CrossPlatformInputManager.GetButton("Fire") && !CrossPlatformInputManager.GetButton("RightSide") && !SprintPress )
            {
                FirePress = false;
                canGetXY = false;
            }
            else if (CrossPlatformInputManager.GetButton("RightSide") && !CrossPlatformInputManager.GetButton("Fire"))
            {
                canGetXY = true;
            }

            if (canGetXY)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).fingerId == mouseTouchId)
                    {
                        MouseX = Input.GetTouch(i).deltaPosition.x * sensitivityX;
                        MouseY = Input.GetTouch(i).deltaPosition.y * sensitivityY;
                        break;
                    }
                }
            }
            else
            {
                MouseX = 0;
                MouseY = 0;
            }

            Horizontal = CrossPlatformInputManager.GetAxis("HorizontalL");
            Vertical = CrossPlatformInputManager.GetAxis("VerticalL");
#endif
        }
        else
        {
            // Otherwise reset all the input.
            Horizontal = 0;
            Vertical = 0;
        }

        // If there is some input, not the time.
        if (Horizontal != 0 || Vertical != 0)
            LastInputAt = Time.time;

        // If a FixedUpdate has happened since a button was set, then reset it.
        if (m_ResetButtons)
        {
            m_ResetButtons = false;

            DrawDown = false;
            FirstPersonLookDown = false;
            CameraResetDown = false;
            JumpDown = false;
            CrouchDown = false;
#if !MOBILE_INPUT // mobile input manages this manually
            FirePress = false;
            Fire2Press = false;
            Item1Down = false;
            Item2Down = false;
            Item3Down = false;
            Item4Down = false;
            Item5Down = false;
            Item6Down = false;
            Fire1Down = false;
            SprintPress = false;
            MenuRightDown = false;
            MenuLeftDown = false;
            MenuUpDown = false;
            MenuDownDown = false;
#endif
            ReloadDown = false;
            CoverDown = false;
            MenuDown = false;
            UseDown = false;
            FlashLightDown = false;
            WalkToggleDown = false;
            ChangeGunPartsDown = false;
            DropDown = false;

            SecondaryFireDown = false;
        }

        if (InputEnabled)
        {
#if !MOBILE_INPUT // normal control buttons
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                MenuDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                CameraResetDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                FirstPersonLookDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetButtonDown("Jump"))
            {
                JumpDown = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Draw")*/ Input.GetKey(KeyCode.F))
            {
                DrawDown = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Crouch")*/ Input.GetKeyDown(KeyCode.LeftControl))
            {
                CrouchDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                DropDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetButton("Fire1"))
            {
                FirePress = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButton("Fire2")*/Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Keypad0))
            {
                Fire2Press = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Reload")*/ Input.GetKeyDown(KeyCode.R))
            {
                ReloadDown = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item1")*/ Input.GetKeyDown(KeyCode.Alpha1))
            {
                Item1Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item2")*/ Input.GetKeyDown(KeyCode.Alpha2))
            {
                Item2Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item3")*/ Input.GetKeyDown(KeyCode.Alpha3))
            {
                Item3Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item4")*/ Input.GetKeyDown(KeyCode.Alpha4))
            {
                Item4Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item5")*/ Input.GetKeyDown(KeyCode.Alpha5))
            {
                Item5Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Item6")*/ Input.GetKeyDown(KeyCode.Alpha6))
            {
                Item6Down = true;
                LastInputAt = Time.time;
            }
            if (/*Input.GetButtonDown("Cover")*/ Input.GetKeyDown(KeyCode.Q))
            {
                CoverDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                ChangeGunPartsDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                FlashLightDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                WalkToggleDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MenuRightDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MenuLeftDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MenuUpDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MenuDownDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                SecondaryFireDown = true;
                LastInputAt = Time.time;
            }
            if (Input.GetButtonDown("Fire1"))
            {
                Fire1Down = true;
                LastInputAt = Time.time;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                SprintPress = true;
                LastInputAt = Time.time;
            }
#else // mobile control buttons
            if (CrossPlatformInputManager.GetButtonDown("Use"))
            {
                UseDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                JumpDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Draw"))
            {
                DrawDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Crouch"))
            {
                CrouchDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Drop"))
            {
                DropDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("Reload"))
            {
                ReloadDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("Cover"))
            {
                CoverDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Modify"))
            {
                ChangeGunPartsDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Flashlight"))
            {
                FlashLightDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("WalkRun"))
            {
                WalkToggleDown = true;
                LastInputAt = Time.time;
            }

            if (CrossPlatformInputManager.GetButtonDown("SecondaryFire"))
            {
                SecondaryFireDown = true;
                LastInputAt = Time.time;
            }
            if (CrossPlatformInputManager.GetButtonDown("Fire"))
            {
                Fire1Down = true;
                LastInputAt = Time.time;
            }
#endif

            if (!movementInputEnabled)
            {
                Horizontal = 0;
                Vertical = 0;
            }
        }
    }

    public IEnumerator ResetSwipes(float resTime)
    {
        yield return new WaitForSeconds(resTime);
        MenuUpDown = false;
        MenuDownDown = false;
        MenuRightDown = false;
        MenuLeftDown = false;
    }


    private void FixedUpdate()
    {
        // Whenever a FixedUpdate happens, reset All Buttons property.
        m_ResetButtons = true;
    }

    public void CalculateRefs(ref Vector3 targetMoveDirection, ref float targetAngle)
    {
        Vector3 stickDirection = new Vector3(Horizontal, 0, Vertical);
        //Vector3 camDirection = mainCamera.forward;  // general purpose camera
        Vector3 camDirection = moveReference.forward;
        camDirection.y = 0;

        Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, camDirection);
        Quaternion refShift2 = new Quaternion(transform.rotation.x, transform.rotation.y * -1f, transform.rotation.z, transform.rotation.w);

        targetMoveDirection = refShift1 * stickDirection;
        Vector3 axisSign = Vector3.Cross(targetMoveDirection, transform.forward);
        targetAngle = Vector3.Angle(transform.forward, targetMoveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        targetMoveDirection = refShift2 * targetMoveDirection;

    }




}
