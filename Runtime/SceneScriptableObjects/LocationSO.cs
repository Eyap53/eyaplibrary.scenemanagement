namespace GameLibrary.SceneManagement
{
	using UnityEngine;
	using UnityEngine.Localization;

	/// <summary>
	/// This class contains Settings specific to Locations only
	/// </summary>

	[CreateAssetMenu(fileName = "NewLocation", menuName = "GameLibrary/Scene Management/Scene Data/Location")]
	public class LocationSO : SceneSO
	{
		public LocalizedString locationName;
	}
}
