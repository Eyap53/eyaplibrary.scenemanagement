namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using UnityEngine.ResourceManagement.ResourceProviders;
	using UnityEngine.SceneManagement;

	public class SceneLoadRequester : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField] private SceneSO _sceneToLoad;
		[SerializeField] private bool _useLoadingScene;

		[Header("Events")]
		[SerializeField] private SceneLoadEventChannelSO _sceneLoadEventChannelSO;

		[Header("Optional")]
		[Tooltip("You can use an event to trigger the request, or you can also reference the button directly.")]
		[SerializeField] private VoidEventChannelSO _onRequestButtonChannelSO = default;

		protected void OnEnable()
		{
			if (_onRequestButtonChannelSO != null)
			{
				_onRequestButtonChannelSO.OnEventRaised += RequestSceneLoad;
			}
		}

		protected void OnDisable()
		{
			if (_onRequestButtonChannelSO != null)
			{
				_onRequestButtonChannelSO.OnEventRaised -= RequestSceneLoad;
			}
		}

		public void RequestSceneLoad()
		{
			_sceneLoadEventChannelSO.RaiseEvent(_sceneToLoad, _useLoadingScene);
		}
	}
}
