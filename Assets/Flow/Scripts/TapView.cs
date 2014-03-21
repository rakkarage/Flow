using strange.extensions.mediation.impl;
using TouchScript.Gestures;
namespace ca.HenrySoftware.Flow
{
	public class TapView : View
	{
		public float Scale = 1.1f;
		public float TimeTween = 0.333f;
		[Inject]
		public FlowToSignal FlowToSignal { get; set; }
		public void OnEnable()
		{
			GetComponent<TapGesture>().StateChanged += HandleTap;
		}
		public void OnDisable()
		{
			GetComponent<TapGesture>().StateChanged -= HandleTap;
		}
		private void HandleTap(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
		{
			if (e.State == Gesture.GestureState.Recognized)
			{
				FlowToSignal.Dispatch(gameObject);
			}
		}
	}
}
