namespace GameLibrary.SceneManagement
{
	using UnityEngine;
	using GameLibrary.SOWorkflowCommon.Events;

	public class LoadingScreenManager : MonoBehaviour
	{
		[Header("Loading screen Event")]
		//The loading screen event we are listening to
		[SerializeField] private BoolEventChannelSO _toggleLoadingScreen = default;

		[Header("Loading screen ")]
		[SerializeField] private GameObject _loadingInterface;

		protected void OnEnable()
		{
			if (_toggleLoadingScreen != null)
			{
				_toggleLoadingScreen.OnEventRaised += ToggleLoadingScreen;
			}
		}

		protected void OnDisable()
		{
			if (_toggleLoadingScreen != null)
			{
				_toggleLoadingScreen.OnEventRaised -= ToggleLoadingScreen;
			}
		}

		private void ToggleLoadingScreen(bool state)
		{
			_loadingInterface.SetActive(state);
		}

	}
}
