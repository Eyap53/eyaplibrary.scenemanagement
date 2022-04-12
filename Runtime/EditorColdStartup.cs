namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using UnityEngine.ResourceManagement.ResourceProviders;
	using UnityEngine.SceneManagement;

	/// <summary>
	/// Allows a "cold start" in the editor, when pressing Play and not passing from the Initialisation scene.
	/// </summary> 
	public class EditorColdStartup : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] private SceneSO _thisSceneSO = default;
		[SerializeField] private SceneSO _persistentManagersSO = default;
		[SerializeField] private AssetReference _coldStartupChannel = default;
		[SerializeField] private VoidEventChannelSO _onSceneReadyChannelSO = default;
		[SerializeField] private PathStorageSO _pathStorageSO = default;

		private bool isColdStart = false;

		private void Awake()
		{
			if (!SceneManager.GetSceneByName(_persistentManagersSO.sceneReference.editorAsset.name).isLoaded)
			{
				isColdStart = true;

				if (_pathStorageSO != null)
				{
					//Reset the path taken, so the character will spawn in this location's default spawn point
					_pathStorageSO.lastPathTaken = null;
				}
			}
		}

		private void Start()
		{
			if (isColdStart)
			{
				_persistentManagersSO.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
			}
		}

		private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
		{
			_coldStartupChannel.LoadAssetAsync<SceneLoadEventChannelSO>().Completed += OnNotifyChannelLoaded;
		}

		private void OnNotifyChannelLoaded(AsyncOperationHandle<SceneLoadEventChannelSO> obj)
		{
			if (_thisSceneSO != null)
			{
				obj.Result.RaiseEvent(_thisSceneSO);
			}
			else
			{
				//Raise a fake scene ready event, so the player is spawned
				_onSceneReadyChannelSO.RaiseEvent();
				//When this happens, the player won't be able to move between scenes because the SceneLoader has no conception of which scene we are in
			}
		}
#endif
	}
}
