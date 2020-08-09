using UnityEngine;

namespace DREditor.Dialogues
{
	[System.Serializable]
	[CreateAssetMenu(menuName = "DREditor/CameraAnim/CameraAnim", fileName = "TrialCameraAnim")]
	public class TrialCameraAnim : ScriptableObject
	{
		public AnimationClip animClip;
	}
}
