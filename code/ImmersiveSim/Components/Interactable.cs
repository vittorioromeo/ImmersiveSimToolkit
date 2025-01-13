#nullable enable

namespace ImmersiveSim.Components;

public class Interactable : Component
{
    [Property] public string DisplayName { get; set; } = "Unnamed";
}
