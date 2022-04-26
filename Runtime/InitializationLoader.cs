namespace GameLibrary.SceneManagement
{
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using UnityEngine.ResourceManagement.ResourceProviders;
	using UnityEngine.SceneManagement;

	/// <summary>
	/// This class is responsible for starting the game by loading the persistent managers scene
	/// and raising the event to load the Main Menu.
	/// </summary>

	public class InitializationLoader : MonoBehaviour
	{
		[SerializeField] private PersistentManagersSO _persistentManagersSceneSO = default;
		[SerializeField] private SceneSO _mainSceneSO = default;

		[Header("Broadcasting on")]
		[SerializeField] private AssetReference _relatedLoadChannel = default;

		protected void Start()
		{
			//Load the persistent managers scene
			_persistentManagersSceneSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
		}

		private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
		{
			// In a production environment, you should add exception handling to catch scenarios such as a null result.
			_relatedLoadChannel.LoadAssetAsync<SceneLoadEventChannelSO>().Completed += RequestLoadMainScene;
		}

		private void RequestLoadMainScene(AsyncOperationHandle<SceneLoadEventChannelSO> obj)
		{
			// In a production environment, you should add exception handling to catch scenarios such as a null result.

			obj.Result.RaiseEvent(_mainSceneSO);
			SceneManager.UnloadSceneAsync(0); //Initialization is the only scene in BuildSettings, thus it has index 0
		}
	}
}
