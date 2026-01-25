using Godot;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Represents the audio manager responsible for handling background music and sound effects
/// in the application.
/// </summary>
public partial class AudioManager : GodotObject
{
	private Node _audioGroupRoot;
	private Node _sfxGroupRoot;
	private Node _musicGroupRoot;
	
	// Music
	private AudioStreamPlayer _baseMusic;
	private AudioStreamPlayer _gameOverMusic;
	private AudioStreamPlayer _lobbyMusic;
	private AudioStreamPlayer _spaceMusic;
	private AudioStreamPlayer _comboMusic;
	
	// SFX
	private AudioStreamPlayer _progressSfx;
	private AudioStreamPlayer _badFoodSfx;
	private AudioStreamPlayer _dimRingSfx;
	private AudioStreamPlayer _brightRingSfx;
	private AudioStreamPlayer _gameOverSfx;
	private AudioStreamPlayer _hurtSfx;
	private AudioStreamPlayer _powerDownSfx;
	private AudioStreamPlayer _powerUpSfx;
	private AudioStreamPlayer _highChimeSfx;
	private AudioStreamPlayer _blipSfx;

	private AudioStreamPlayer _currentMusic;

	/// <summary>
	/// Initializes the audio manager by configuring the root nodes for audio groups and
	/// assigning the corresponding AudioStreamPlayer instances for music and sound effects.
	/// </summary>
	/// <param name="root">The root node of the audio group hierarchy containing music and SFX nodes.</param>
	public void Initialize(Node root)
	{
		_audioGroupRoot = root;
		_musicGroupRoot = _audioGroupRoot.GetNode<Node>("Music");
		_sfxGroupRoot = _audioGroupRoot.GetNode<Node>("SFX");

		_baseMusic = _musicGroupRoot.GetNode<AudioStreamPlayer>("BaseGameplayMusic");
		_gameOverMusic = _musicGroupRoot.GetNode<AudioStreamPlayer>("GameOverMusic");
		_lobbyMusic = _musicGroupRoot.GetNode<AudioStreamPlayer>("LobbyMusic");
		_spaceMusic = _musicGroupRoot.GetNode<AudioStreamPlayer>("SpaceMusic");
		_comboMusic = _musicGroupRoot.GetNode<AudioStreamPlayer>("ComboMusic");

		_progressSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("ProgressSFX");
		_badFoodSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("BadFoodSFX");
		_dimRingSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("DimRingSFX");
		_brightRingSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("BrightRingSFX");
		_gameOverSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("GameOverSFX");
		_hurtSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("HurtSFX");
		_powerDownSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("PowerDownSFX");
		_powerUpSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("PowerUpSFX");
		_highChimeSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("HighChimeSFX");
		_blipSfx = _sfxGroupRoot.GetNode<AudioStreamPlayer>("BlipSFX");
	}

	/// <summary>
	/// Plays the specified music using the provided AudioStreamPlayer instance.
	/// Stops any currently playing music before starting the new one.
	/// </summary>
	/// <param name="music">The AudioStreamPlayer instance representing the music to be played.</param>
	private void PlayMusic(AudioStreamPlayer music)
	{
		if (_currentMusic == music && _currentMusic.IsPlaying()) return;
		_currentMusic?.Stop();
		_currentMusic = music;
		_currentMusic.Play();
	}

	/// <summary>
	/// Plays the 'Base Gameplay' music to indicate the start of the game.
	/// </summary>
	public void PlayBaseMusic() => PlayMusic(_baseMusic);
	/// <summary>
	/// Plays the 'Game Over' music to indicate the end of the game.
	/// </summary>
	private void PlayGameOverMusic() => PlayMusic(_gameOverMusic);
	/// <summary>
	/// Plays the 'Lobby' music to indicate the start of the game.
	/// </summary>
	public void PlayLobbyMusic() => PlayMusic(_lobbyMusic);
	/// <summary>
	/// Plays the 'Space' music to indicate the player's movement in the space.'
	/// </summary>
	public void PlaySpaceMusic() => PlayMusic(_spaceMusic);
	/// <summary>
	/// Plays the 'Combo' music to indicate a successful collection of a special egg.
	/// </summary>
	public void PlayComboMusic() => PlayMusic(_comboMusic);
	/// <summary>
	/// Plays the 'Progress' sound effect to indicate a positive in-game event.
	/// </summary>
	public void PlayProgressSfx() => _progressSfx.Play();
	/// <summary>
	/// Plays the 'Bad Food' sound effect to indicate a negative in-game event.
	/// </summary>
	public void PlayBadFoodSfx() => _badFoodSfx.Play();
	/// <summary>
	/// Plays the 'Dim Ring' sound effect to provide feedback for a failed collection of a special egg.
	/// </summary>
	private void PlayDimRingSfx() => _dimRingSfx.Play();
	/// <summary>
	/// Plays the 'Bright Ring' sound effect to provide feedback for a successful collection of a special egg.
	/// </summary>
	private void PlayBrightRingSfx() => _brightRingSfx.Play();

	/// <summary>
	/// Plays the sound effect associated with collecting a special egg.
	/// Depending on whether the player is in a combo state, it alternates
	/// between a bright ring sound effect (for combos) and a dim ring
	/// sound effect (for non-combo states), providing auditory feedback
	/// based on the current gameplay context.
	/// </summary>
	/// <param name="isInCombo">
	/// A boolean indicating whether the player is currently in a combo state.
	/// If true, the bright ring sound effect is played; if false, the dim
	/// ring sound effect is played.
	/// </param>
	public void PlaySpecialEggSfx(bool isInCombo)
	{
		if (isInCombo) PlayBrightRingSfx();
		else PlayDimRingSfx();
	}

	/// <summary>
	/// Plays the 'Game Over' sound effect to indicate the end of the game.
	/// </summary>
	private void PlayGameOverSfx() => _gameOverSfx.Play();

	/// <summary>
	/// Plays the 'Hurt' sound effect to indicate a negative in-game event
	/// </summary>
	public void PlayHurtSfx() => _hurtSfx.Play();

	/// <summary>
	/// Plays the 'Power Down' sound effect to indicate a negative in-game event
	/// </summary>
	public void PlayPowerDownSfx() => _powerDownSfx.Play();

	/// <summary>
	/// Plays the 'Power Up' sound effect to indicate a positive in-game event
	/// </summary>
	public void PlayPowerUpSfx() => _powerUpSfx.Play();

	/// <summary>
	/// Plays the 'High Chime' sound effect to indicate a positive in-game event.
	/// </summary>
	public void PlayHighChimeSfx() => _highChimeSfx.Play();

	/// <summary>
	/// Plays the 'Blip' sound effect to indicate a positive in-game event.
	/// </summary>
	private void PlayBlipSfx() => _blipSfx.Play();

	/// <summary>
	/// Plays the 'Game Over' sound effect and music to indicate the end of the game.
	/// </summary>
	public void GameOver()
	{
		PlayGameOverMusic();
		PlayGameOverSfx();
	}
}
