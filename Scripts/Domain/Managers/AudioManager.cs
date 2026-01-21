using Godot;

namespace Snake.Scripts.Domain.Managers;

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

	private void PlayMusic(AudioStreamPlayer music)
	{
		if (_currentMusic == music && _currentMusic.IsPlaying()) return;
		if (_currentMusic != null && _currentMusic.IsPlaying()) _currentMusic.Stop();
		_currentMusic = music;
		_currentMusic.Play();
	}

	public void PlayBaseMusic() => PlayMusic(_baseMusic);
	private void PlayGameOverMusic() => PlayMusic(_gameOverMusic);
	public void PlayLobbyMusic() => PlayMusic(_lobbyMusic);
	public void PlaySpaceMusic() => PlayMusic(_spaceMusic);
	public void PlayComboMusic() => PlayMusic(_comboMusic);
	
	public void PlayProgressSfx() => _progressSfx.Play();
	public void PlayBadFoodSfx() => _badFoodSfx.Play();
	private void PlayDimRingSfx() => _dimRingSfx.Play();
	private void PlayBrightRingSfx() => _brightRingSfx.Play();
	public void PlaySpecialEggSfx(bool isInCombo)
	{
		if (isInCombo) PlayBrightRingSfx();
		else PlayDimRingSfx();
	}

	private void PlayGameOverSfx() => _gameOverSfx.Play();
	public void PlayHurtSfx() => _hurtSfx.Play();
	public void PlayPowerDownSfx() => _powerDownSfx.Play();
	public void PlayPowerUpSfx() => _powerUpSfx.Play();
	public void PlayHighChimeSfx() => _highChimeSfx.Play();
	private void PlayBlipSfx() => _blipSfx.Play();

	public void GameOver()
	{
		_comboMusic.Stop();
		_baseMusic.Stop();
		PlayGameOverMusic();
		PlayGameOverSfx();
	}

	public void NewGame()
	{
		_spaceMusic.Stop();
		PlayLobbyMusic();
	}
}
