using strange.extensions.command.impl;
using strange.extensions.signal.impl;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class StartSignal : Signal { }
	public class FlowStartCommand : Command
	{
		public override void Execute()
		{
			Debug.Log("<color=blue>FlowStartCommand</color>");
		}
	}
	public class NextSignal : Signal { }
	public class PrevSignal : Signal { }
	public class FlowSignal : Signal { }
	public class FlowToSignal : Signal<GameObject> { }
	public class FlowSnapSignal : Signal<int> { }
	public class FlowPanSignal : Signal<float> { }
	public class InertiaSignal : Signal<float> { }
	public class InertiaStopSignal : Signal { }
}
