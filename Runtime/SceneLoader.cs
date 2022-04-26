namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using UnityEngine.ResourceManagement.ResourceProviders;
	using UnityEngine.SceneManagement;

	/// <summary>
	/// This class manages the scene loading and unloading.
	/// </summary>
	public class SceneLoader : MonoBehaviour
	{
		[SerializeField] protected GameplayManagersSO _gameplayScene = default;

		[Header("Events/Listening to")]
		[SerializeField] protected SceneLoadEventChannelSO _loadLocation = default;
		[SerializeField] protected SceneLoadEventChannelSO _loadMenu = default;
#if UNITY_EDITOR
		[SerializeField] protected SceneLoadEventChannelSO _coldStartupLocation = default;
#endif

		[Header("Events/Broadcasting on")]
		[SerializeField] protected BoolEventChannelSO _toggleLoadingScreen = default;
		[SerializeField] protected VoidEventChannelSO _onSceneReady = default;

		protected AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
		protected AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

		//Parameters coming from scene loading requests
		protected SceneSO _sceneToLoad;
		protected SceneSO _currentlyLoadedScene;
		protected bool _showLoadingScreen;

		protected SceneInstance _gameplayManagerSceneInstance = new SceneInstance();
		protected bool _isLoading = false; //To prevent a new loading request while already loading a new scene

		protected virtual void OnEnable()
		{
			_loadLocation.OnLoadingRequested += LoadLocation;
			_loadMenu.OnLoadingRequested += LoadMenu;
#if UNITY_EDITOR
			if (_coldStartupLocation != null)
			{
				_coldStartupLocation.OnLoadingRequested += LocationColdStartup;
			}
#endif
		}

		protected virtual void OnDisable()
		{
			_loadLocation.OnLoadingRequested -= LoadLocation;
			_loadMenu.OnLoadingRequested -= LoadMenu;
#if UNITY_EDITOR
			if (_coldStartupLocation != null)
			{
				_coldStartupLocation.OnLoadingRequested -= LocationColdStartup;
			}
#endif
		}

#if UNITY_EDITOR
		/// <summary>
		/// This special loading function is only used in the editor, when the developer presses Play in a Location scene, without passing by Initialisation.
		/// </summary>
		protected virtual void LocationColdStartup(SceneSO currentlyOpenedLocation, bool showLoadingScreen)
		{
			_currentlyLoadedScene = currentlyOpenedLocation;

			if (_currentlyLoadedScene is LocationSO)
			{
				//Gameplay managers is loaded synchronously
				_gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
				_gameplayManagerLoadingOpHandle.WaitForCompletion();
				_gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

				StartGameplay();
			}
		}
#endif

		/// <summary>
		/// This function loads the location scenes passed as array parameter
		/// </summary>
		protected virtual void LoadLocation(SceneSO locationToLoad, bool showLoadingScreen)
		{
			//Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
			if (_isLoading)
				return;
			if (_currentlyLoadedScene == locationToLoad)
				return;

			_sceneToLoad = locationToLoad;
			_showLoadingScreen = showLoadingScreen;
			_isLoading = true;

			//In case we are coming from the main menu, we need to load the Gameplay manager scene first
			if (_gameplayManagerSceneInstance.Scene == null
				|| !_gameplayManagerSceneInstance.Scene.isLoaded)
			{
				_gameplayManagerLoadingOpHandle = _gameplayScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
                _gameplayManagerLoadingOpHandle.Completed += OnGameplayManagersLoaded;
			}
			else
			{
				UnloadPreviousScene();
			}
		}

		protected void OnGameplayManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
		{
			_gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

			UnloadPreviousScene();
		}

		/// <summary>
		/// Prepares to load the main menu scene, first removing the Gameplay scene in case the game is coming back from gameplay to menus.
		/// </summary>
		protected virtual void LoadMenu(SceneSO menuToLoad, bool showLoadingScreen)
		{
			//Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
			if (_isLoading)
				return;
			if (_currentlyLoadedScene == menuToLoad)
				return;

			_sceneToLoad = menuToLoad;
			_showLoadingScreen = showLoadingScreen;
			_isLoading = true;

			//In case we are coming from a Location back to the main menu, we need to get rid of the persistent Gameplay manager scene
			if (_gameplayManagerSceneInstance.Scene != null
				&& _gameplayManagerSceneInstance.Scene.isLoaded)
				Addressables.UnloadSceneAsync(_gameplayManagerLoadingOpHandle, true);

			UnloadPreviousScene();
		}

		/// <summary>
		/// In both Location and Menu loading, this function takes care of removing previously loaded scenes.
		/// </summary>
		protected virtual void UnloadPreviousScene()
		{
			if (_currentlyLoadedScene != null) //would be null if the game was started in Initialisation
			{
				if (_currentlyLoadedScene.sceneReference.OperationHandle.IsValid())
				{
					//Unload the scene through its AssetReference, i.e. through the Addressable system
					_currentlyLoadedScene.sceneReference.UnLoadScene();
				}
#if UNITY_EDITOR
				else
				{
					//Only used when, after a "cold start", the player moves to a new scene
					//Since the AsyncOperationHandle has not been used (the scene was already open in the editor),
					//the scene needs to be unloaded using regular SceneManager instead of as an Addressable
					SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneReference.editorAsset.name);
				}
#endif
			}

			LoadNewScene();
		}

		/// <summary>
		/// Kicks off the asynchronous loading of a scene, either menu or Location.
		/// </summary>
		protected virtual void LoadNewScene()
		{
			if (_showLoadingScreen)
			{
				_toggleLoadingScreen.RaiseEvent(true);
			}

			_loadingOperationHandle = _sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
			_loadingOperationHandle.Completed += OnNewSceneLoaded;
		}

		protected void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
		{
			//Save loaded scenes (to be unloaded at next load request)
			_currentlyLoadedScene = _sceneToLoad;

			Scene s = obj.Result.Scene;
			SceneManager.SetActiveScene(s);
			LightProbes.TetrahedralizeAsync();

			_isLoading = false;

			if (_showLoadingScreen)
			{
				_toggleLoadingScreen.RaiseEvent(false);
			}

			StartGameplay();
		}

		protected virtual void StartGameplay()
		{
			_onSceneReady.RaiseEvent(); // Spawn system will spawn the player in a gameplay scene
		}
	}
}
