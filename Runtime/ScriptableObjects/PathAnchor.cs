namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon;
	using UnityEngine;

	[CreateAssetMenu(fileName = "PathAnchor", menuName = "GameLibrary/Scene Management/PathAnchor")]
	public class PathAnchor : DescriptionBaseSO
	{
		public bool isSet { get; private set; } = false; // Any script can check if the transform is null before using it, by just checking this bool

		private PathSO _Path;
		public PathSO Path
		{
			get { return _Path; }
			set
			{
				_Path = value;
				isSet = _Path != null;
			}
		}

		public void OnDisable()
		{
			_Path = null;
			isSet = false;
		}
	}
}
