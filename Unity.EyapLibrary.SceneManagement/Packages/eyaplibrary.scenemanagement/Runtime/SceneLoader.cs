namespace EyapLibrary.SceneManagement
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.SceneManagement;
#if USE_NETCODE
	using Unity.Netcode;
	using Scene = UnityEngine.SceneManagement.Scene;
#endif

	/// <summary>
	/// Script responsible to load the scene in the background.
	/// It is called from the SceneSwitcher monobehaviour mainly.
	/// </summary>
	public class SceneLoader
	{
		/// <summary>
		/// Contains the currently loading scenes.
		/// </summary>
		public static HashSet<string> LoadingScenes { get; private set; } = new HashSet<string>();

#if USE_NETCODE
		public static List<string> _netcodeLoadedScenes = new List<string>();
#endif


		/// <summary>
		/// Loads the additional scene if it is not already loaded.
		/// </summary>
		/// <exception cref="ArgumentException"> If no scene with that name exists.</exception>
		public static bool LoadScene(string sceneName) => LoadScene(sceneName, null);
		public static bool LoadScene(string sceneName, Action sceneLoadedCallback)
		{
			// Checks
			if (SceneManager.GetSceneByName(sceneName) == null)
			{
				throw new ArgumentException("SceneLoader: No scene with that name.");
			}
			if (LoadingScenes.Contains(sceneName))
			{
				Debug.LogWarning("SceneLoader: Scene currently loading");
				return false;
			}

			// Implementation
			LoadingScenes.Add(sceneName);
#if USE_NETCODE
			if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
			{
				void sceneEventHandler(SceneEvent sceneEvent)
				{
					Debug.Log("SceneLoader: Scene event handler called.");
					sceneEvent.AsyncOperation.completed += (AsyncOperation a) => OnSceneLoaded(sceneName);
					if (sceneLoadedCallback != null)
					{
						sceneEvent.AsyncOperation.completed += (AsyncOperation a) => sceneLoadedCallback();
					}
				}

				NetworkManager.Singleton.SceneManager.OnSceneEvent += sceneEventHandler;
				SceneEventProgressStatus sceneEventProgressStatus = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
				_netcodeLoadedScenes.Add(sceneName);
				NetworkManager.Singleton.SceneManager.OnSceneEvent -= sceneEventHandler;
			}
			else
			{
				AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				async.completed += (AsyncOperation a) => OnSceneLoaded(sceneName);
				if (sceneLoadedCallback != null)
				{
					async.completed += (AsyncOperation a) => sceneLoadedCallback();
				}
			}
#else
			AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			async.completed += (AsyncOperation a) => OnSceneLoaded(sceneName);
			if (sceneLoadedCallback != null)
			{
				async.completed += (AsyncOperation a) => sceneLoadedCallback();
			}
#endif
			return true;
		}

		/// <summary>
		/// Remove a scene from the currently loading scenes.
		/// </summary>
		/// <param name="sceneName">The name of the scene to remove.</param>
		/// <exception cref="ArgumentException">If no scene with that name exists.</exception>
		/// <exception cref="InvalidOperationException">If the scene is not currently loading.</exception>
		/// <remarks>
		/// This function is called automatically when the scene is loaded.
		/// </remarks>
		protected static void OnSceneLoaded(string sceneName)
		{
			if (SceneManager.GetSceneByName(sceneName) == null)
			{
				throw new ArgumentException("SceneLoader: No scene with that name.");
			}
			if (!LoadingScenes.Contains(sceneName))
			{
				throw new InvalidOperationException("SceneLoader: Scene not currently loading.");
			}

			LoadingScenes.Remove(sceneName);
		}

		/// <summary>
		/// Unloads the scene if it is loaded.
		/// </summary>
		/// <exception cref="ArgumentException">If no scene with that name exists.</exception>
		/// <exception cref="InvalidOperationException">If the scene is currently loading.</exception>
		public static bool UnloadScene(string sceneName) => UnloadScene(sceneName, null);
		public static bool UnloadScene(string sceneName, Action sceneUnloadedCallback)
		{
			// Checks
			if (SceneManager.GetSceneByName(sceneName) == null)
			{
				throw new ArgumentException("SceneLoader: No scene with that name.");
			}
			if (LoadingScenes.Contains(sceneName))
			{
				throw new InvalidOperationException("SceneLoader: Scene currently loading.");
			}
			Scene scene = SceneManager.GetSceneByName(sceneName);
			if (!scene.isLoaded)
			{
				Debug.LogWarning("SceneLoader: Scene not loaded.");
				return false;
			}

			// Implementation
			Debug.Log(string.Format("SceneLoader: Unloading scene {0}.", sceneName));

#if USE_NETCODE
			// Debug.Log($"SceneLoader: _netcodeLoadedScenes : {_netcodeLoadedScenes}, {_netcodeLoadedScenes.Contains(sceneName)}");
			if (NetworkManager.Singleton != null && _netcodeLoadedScenes.Contains(sceneName))
			{
				void sceneEventHandler(SceneEvent sceneEvent)
				{
					Debug.Log("SceneLoader: Scene event handler called.");
					sceneEvent.AsyncOperation.completed += (AsyncOperation a) => OnSceneUnloaded(sceneName);
					if (sceneUnloadedCallback != null)
					{
						sceneEvent.AsyncOperation.completed += (AsyncOperation a) => sceneUnloadedCallback();
					}
				}

				NetworkManager.Singleton.SceneManager.OnSceneEvent += sceneEventHandler;
				SceneEventProgressStatus sceneEventProgressStatus = NetworkManager.Singleton.SceneManager.UnloadScene(scene);
				NetworkManager.Singleton.SceneManager.OnSceneEvent -= sceneEventHandler;
			}
			else
			{
				// Debug.LogWarning("SceneLoader: Server not started.");
				AsyncOperation async = SceneManager.UnloadSceneAsync(sceneName);
				if (async == null)
				{
					Debug.LogWarning("SceneLoader: Scene not unloaded.");
					return false;
				}
				async.completed += (AsyncOperation a) => OnSceneUnloaded(sceneName);
				if (sceneUnloadedCallback != null)
				{
					async.completed += (AsyncOperation a) => sceneUnloadedCallback();
				}
			}
#else
			AsyncOperation async = SceneManager.UnloadSceneAsync(sceneName);
			if (async == null)
			{
				Debug.LogWarning("SceneLoader: Scene not unloaded.");
				return false;
			}
			async.completed += (AsyncOperation a) => OnSceneUnloaded(sceneName);
			if (sceneUnloadedCallback != null)
			{
				async.completed += (AsyncOperation a) => sceneUnloadedCallback();
			}
#endif
			return true;
		}

		protected static void OnSceneUnloaded(string sceneName)
		{
			Debug.Log(string.Format("SceneLoader: Scene {0} unloaded.", sceneName));
		}
	}
}
