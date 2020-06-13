using UnityEngine;

namespace DREditor.DialogueEditor
{
	[System.Serializable]
	[CreateAssetMenu(menuName = "DREditor/CameraAnim/CameraAnim", fileName = "TrialCameraAnim")]
	public class TrialCameraAnim : ScriptableObject
	{
		public AnimationClip animClip;
	}
}
