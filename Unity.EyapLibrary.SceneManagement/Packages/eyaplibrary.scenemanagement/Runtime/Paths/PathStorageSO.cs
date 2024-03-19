namespace GameLibrary.SceneManagement.Paths
{
	using GameLibrary.SOWorkflowCommon;
	using UnityEngine;

	/// <summary>
	/// This one-of-a-kind-SO stores, during gameplay, the path that was used last (i.e. the one that was taken to get to the current scene).
	/// Only one PathStorageSO is needed.   
	/// </summary>
	[CreateAssetMenu(fileName = "PathStorage", menuName = "GameLibrary/Scene Management/Paths/Path Storage")]
	public class PathStorageSO : DescriptionBaseSO
	{
		[Tooltip("Last path taken by the player. Usually null when starting the app.")]
		public PathSO lastPathTaken;
	}
}
