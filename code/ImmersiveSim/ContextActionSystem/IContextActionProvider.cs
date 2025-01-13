#nullable enable

namespace ImmersiveSim.ContextActionSystem;

public interface IContextActionProvider
{
	public ContextAction[] GetContextActions( Player player );
	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player );
}
