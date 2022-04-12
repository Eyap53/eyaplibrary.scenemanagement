namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;

	public class GameExiter : MonoBehaviour
	{
		[SerializeField] private VoidEventChannelSO _onExitGameButton = default;

		void OnEnable()
		{
			_onExitGameButton.OnEventRaised += ExitGame;
		}

		void OnDisable()
		{
			_onExitGameButton.OnEventRaised -= ExitGame;
		}

		public void ExitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
			Application.Quit();
		}
	}
}
