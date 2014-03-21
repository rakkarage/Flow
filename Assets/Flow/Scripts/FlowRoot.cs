using strange.extensions.context.impl;
namespace ca.HenrySoftware.Flow
{
	public class FlowRoot : ContextView
	{
		private void Start()
		{
			context = new FlowContext(this);
		}
	}
}
