﻿using UnityEngine;
using DREditor.EventObjects;
using DREditor.Dialogues;

namespace DREditor.Camera
{
	public class SmoothMouseLook : MonoBehaviour
	{
		Vector2 _mouseAbsolute;
		Vector2 _smoothMouse;

		public Vector2 clampInDegrees = new Vector2(360, 180);
		public bool lockCursor;
		public Vector2 sensitivity = new Vector2(2, 2);
		public Vector2 smoothing = new Vector2(3, 3);
		public Vector2 targetDirection;
		public Vector2 targetCharacterDirection;

		public BoolWithEvent InDialogue;

		DRInput _controls;

		// Assign this if there's a parent object controlling motion, such as a Character Controller.
		// Yaw rotation will affect this object instead of the camera if set.
		public GameObject characterBody;

		private void Awake()
		{
			// Check if using the new Input System by Unity.
		#if ENABLE_INPUT_SYSTEM
			_controls = new DRInput();
		#endif
		}

		void Start()
		{
			// Set target direction to the camera's initial orientation.
			targetDirection = transform.localRotation.eulerAngles;

			// Set target direction for the character body to its inital state.
			if (characterBody)
				targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
		}

		private void OnEnable()
		{
		#if ENABLE_INPUT_SYSTEM
			_controls.Enable();
		#endif
		}

		private void OnDisable()
		{
		#if ENABLE_INPUT_SYSTEM
			_controls.Disable();
		#endif
		}

		public Quaternion GetRotation()
		{
			// Ensure the cursor is always locked when set
			if (lockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}

			// Allow the script to clamp based on a desired target value.
			var targetOrientation = Quaternion.Euler(targetDirection);
			var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

			// Get raw mouse input for a cleaner reading on more sensitive mice.
		#if ENABLE_INPUT_SYSTEM
			var mouseDelta = _controls._3DExplore.Look.ReadValue<Vector2>();
		#elif ENABLE_LEGACY_INPUT_MANAGER
			var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		#endif

			var newSensiv = sensitivity;
            if (DialogueHandler.InDialog)
            {
                newSensiv *= 0;
            }

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(newSensiv.x * smoothing.x, newSensiv.y * smoothing.y));

			// Interpolate mouse movement over time to apply smoothing delta.
			_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
			_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

			// Find the absolute mouse movement value from point zero.
			_mouseAbsolute += _smoothMouse;

			// Clamp and apply the local x value first, so as not to be affected by world transforms.
			if (clampInDegrees.x < 360)
				_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

			// Then clamp and apply the global y value.
			if (clampInDegrees.y < 360)
				_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);


			if (characterBody)
			{
				var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
				characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
			}
			else
			{
				var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
				transform.localRotation *= yRotation;
			}


			return Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;
		}
		public Quaternion DRGetRotation()
		{
			// Ensure the cursor is always locked when set
			if (lockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}

			// Allow the script to clamp based on a desired target value.
			var targetOrientation = Quaternion.Euler(targetDirection);
			var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

			// Get raw mouse input for a cleaner reading on more sensitive mice.
		#if ENABLE_INPUT_SYSTEM
			var mouseDelta = _controls._3DExplore.Look.ReadValue<Vector2>();
		#elif ENABLE_LEGACY_INPUT_MANAGER
			var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		#endif

			var newSensiv = sensitivity;
			if (InDialogue.Value)
			{
				newSensiv /= 20;
			}

			// Scale input against the sensitivity setting and multiply that against the smoothing value.
			mouseDelta = Vector2.Scale(mouseDelta, new Vector2(newSensiv.x * smoothing.x, newSensiv.y * smoothing.y));

			// Interpolate mouse movement over time to apply smoothing delta.
			_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
			_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

			// Find the absolute mouse movement value from point zero.
			_mouseAbsolute += _smoothMouse;

			// Clamp and apply the local x value first, so as not to be affected by world transforms.
			if (clampInDegrees.x < 360)
				_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

			// Then clamp and apply the global y value.
			if (clampInDegrees.y < 360)
				_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);


			if (characterBody)
			{
				var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
				characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
			}
			else
			{
				var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
				transform.localRotation *= yRotation;
			}

			_mouseAbsolute.y = Mathf.Lerp(_mouseAbsolute.y, 0f, 0.2f);

			return Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;
		}
	}
}