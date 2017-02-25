using System;
using UnityEngine;


namespace UnityStandardAssets.Characters.FirstPerson {
  
    public class RigidbodyFirstPersonController : MonoBehaviour {
     
            public float ForwardSpeed = 8.0f;   // Speed when walking forward

            public float JumpForce = 30f;

            public float CurrentTargetSpeed = 8f;

            public void UpdateDesiredTargetSpeed(Vector2 input) {
                if (input == Vector2.zero) return;

                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                CurrentTargetSpeed = ForwardSpeed;

            }


        


        
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
        


        public Camera cam;
        public MouseLook mouseLook = new MouseLook();


        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;


        public Vector3 Velocity {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded {
            get { return m_IsGrounded; }
        }

        public bool Jumping {
            get { return m_Jumping; }
        }


        private void Start() {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);
        }


        private void Update() {
            RotateView();

            if (Input.GetButtonDown("Jump") && !m_Jump) {
                m_Jump = true;
            }
        }


        private void FixedUpdate() {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && m_IsGrounded) {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (CurrentTargetSpeed * CurrentTargetSpeed)) {
                    m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
                }
            }

            if (m_IsGrounded) {
                m_RigidBody.drag = 5f;

                if (m_Jump) {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f) {
                    m_RigidBody.Sleep();
                }
            }
            else {
                m_RigidBody.drag = 0f;
               
            }
            m_Jump = false;
        }

      

        private Vector2 GetInput() {

            Vector2 input = new Vector2 {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView() {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded) {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck() {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * 1.0f, Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping) {
                m_Jumping = false;
            }
        }
    }
}
