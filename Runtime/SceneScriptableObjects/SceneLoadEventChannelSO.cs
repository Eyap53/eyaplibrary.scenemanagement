namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// This class is used for scene-loading events.
	/// Takes a GameSceneSO of the location or menu that needs to be loaded, and a bool to specify if a loading screen needs to display.
	/// </summary>
	[CreateAssetMenu(fileName = "SceneLoadEventChannelSO", menuName = "GameLibrary/Scene Management/Events/Scene Load Event Channel SO")]
	public class SceneLoadEventChannelSO : EventChannelBaseSO
	{
		public UnityAction<SceneSO, bool> OnLoadingRequested;

		public void RaiseEvent(SceneSO locationToLoad, bool showLoadingScreen = false)
		{
			if (OnLoadingRequested != null)
			{
				OnLoadingRequested.Invoke(locationToLoad, showLoadingScreen);
			}
			else
			{
				Debug.LogWarning("A Scene loading was requested, but nobody picked it up. " +
					"Check why there is no SceneLoader already present, " +
					"and make sure it's listening on this Load Event channel.");
			}
		}
	}
}
