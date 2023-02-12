//Point n' Click Camera Script for DREditor by SeleniumSoul
using UnityEngine;
using System.Collections;
using DREditor.Dialogues;

namespace DRDistrust.Camera
{
	[AddComponentMenu("DREditor/Cameras/DRPNCCamera")]
	public class DRPNCCamera : MonoBehaviour
	{
		private DRInput _controls;

		//Public parameters
		public bool Controllable = false;
		public bool Inverted = false;
		public Vector3 CenterPosition = new Vector3(0f, 1.8f, 0f);
		public float MaxHAngle = 30f;
		public float MaxVAngle = 20f;
		public float CamSpeed = 3f;
		public float InitialHAngle = 0f;
		public float InitialVAngle = 0f;
		public float CamDistance = 8f;

		//Focus on Object
		public GameObject TargetObject;
		public Vector3 TargetAimRotation;
		public float TargetCamDistance = 5f;

		//Private variables for calculation
		//private float CamHAngle, CamVAngle;
		private Vector2 InputAngle;
		private Vector2 CamAngle;
		private Vector3 TargetFocus;
		private Quaternion InitAngle;
		private Quaternion FinalAimRotation;
		private Vector3 FinalCamPosition;

		private void Awake()
		{
			_controls = new DRInput();

			_controls.PNCExplore.Move.performed += ctx =>
			{
				if (!DialogueHandler.InDialog && !DialogueHandler.InMenu && Controllable)
					InputAngle = ctx.ReadValue<Vector2>();
			};
			_controls.PNCExplore.Move.canceled += ctx =>
			{
				InputAngle = Vector2.zero;
			};
		}

		void Start()
		{
			InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);
		}

		private void OnEnable()
		{
			_controls.Enable();
		}

		private void OnDisable()
		{
			_controls.Disable();
		}

		void Update()
		{
			//Please comment Line below if your desired inital angles is already applied.
			//InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);

			if (TargetObject == null)
			{
				TargetFocus = CenterPosition;

				CamAngle.x += Mathf.Clamp(InputAngle.x, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
				CamAngle.y += Mathf.Clamp(InputAngle.y, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
				CamAngle.x = Mathf.Clamp(CamAngle.x, -MaxHAngle, MaxHAngle);
				CamAngle.y = Mathf.Clamp(CamAngle.y, -MaxVAngle, MaxVAngle);

				FinalAimRotation = Quaternion.Euler(CamAngle.y + InitialVAngle, -CamAngle.x + InitialHAngle, 0);
				FinalCamPosition = (TargetFocus - new Vector3(0, CamAngle.y / 30f)) - (transform.forward * CamDistance);
				transform.SetPositionAndRotation(FinalCamPosition, FinalAimRotation);
			}
			else
			{
				Debug.Log("3DVNCamera: Currently Focusing!");
				TargetFocus = TargetObject.transform.position;
				FinalAimRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(TargetAimRotation), Time.deltaTime * 30f);
				FinalCamPosition = Vector3.Lerp(transform.position, TargetFocus - (transform.forward * TargetCamDistance), Time.deltaTime * 30f);
				transform.SetPositionAndRotation(FinalCamPosition, FinalAimRotation);
			}
		}

		public void FocusCamera(string TarObj)
		{
			Controllable = false;
			TargetObject = GameObject.Find(TarObj);
			TargetAimRotation = Vector3.zero;
			TargetCamDistance = 8f;
			Debug.Log("3DVNCamera: Focusing on " + TargetObject.name + "using a string.");
		}

		public void FocusCamera(GameObject target, Vector3 rotation, float distance)
		{
			Controllable = false;
			TargetObject = target;
			TargetAimRotation = rotation;
			TargetCamDistance = distance;
			Debug.Log("3DVNCamera: Focusing on a spot manually.");
		}

		public void ControlSwitch(bool option)
		{
			Controllable = option;
		}

		public IEnumerator CameraAnimStart(GameObject camera, Vector3 endposition, Vector3 endrotation, float speed)
		{
			float _StartTime = 0;
			while (_StartTime < speed)
			{
				camera.transform.localPosition = Vector3.Slerp(camera.transform.localPosition, endposition, (_StartTime / (speed*2f)));
				camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, Quaternion.Euler(endrotation), (_StartTime / (speed*2f)));
				_StartTime += Time.deltaTime;
				yield return new WaitForSeconds(0.01f);
			}
			Controllable = true;
		}
	}
}