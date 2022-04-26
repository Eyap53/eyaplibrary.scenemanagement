namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon;
	using UnityEngine.AddressableAssets;

	/// <summary>
	/// This class is a base class which contains what is common to all game scenes (Locations, Menus, Managers)
	/// </summary>
	public abstract class SceneSO : DescriptionBaseSO
	{
		public AssetReference sceneReference; //Used at runtime to load the scene from the right AssetBundle
	}
}
