namespace GameLibrary.SceneManagement
{
	using GameLibrary.SOWorkflowCommon.Events;
	using UnityEngine;

	public class GameExiter : MonoBehaviour
	{
		[Header("Optional")]
		[SerializeField] private VoidEventChannelSO _onExitGameButton = default;

		void OnEnable()
		{
			if (_onExitGameButton != null)
			{
				_onExitGameButton.OnEventRaised += ExitGame;
			}
		}

		void OnDisable()
		{
			if (_onExitGameButton != null)
			{
				_onExitGameButton.OnEventRaised -= ExitGame;
			}
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
