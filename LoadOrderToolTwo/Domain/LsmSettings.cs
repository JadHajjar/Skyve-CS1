namespace LoadOrderToolTwo.Domain;

public class LsmSettings
{
	public bool LoadEnabled { get; set; } = true;
	public bool LoadUsed { get; set; } = true;
	public string? SkipFile { get; set; }
}