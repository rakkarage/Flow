using strange.extensions.mediation.impl;
using TouchScript.Gestures;
using TouchScript.Gestures.Simple;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class PanView : View
	{
		public float Threshold = 0.1f;
		[Inject]
		public FlowSignal FlowSignal { get; set; }
		[Inject]
		public FlowPanSignal FlowPanSignal { get; set; }
		[Inject]
		public InertiaSignal InertiaSignal { get; set; }
		public void OnEnable()
		{
			GetComponent<SimplePanGesture>().StateChanged += HandleSimplePanStateChanged;
		}
		public void OnDisable()
		{
			GetComponent<SimplePanGesture>().StateChanged -= HandleSimplePanStateChanged;
		}
		private void HandleSimplePanStateChanged(object sender, TouchScript.Events.GestureStateChangeEventArgs e)
		{
			SimplePanGesture target = sender as SimplePanGesture;
			switch (e.State)
			{
				case Gesture.GestureState.Began:
				case Gesture.GestureState.Changed:
					if (target.LocalDeltaPosition != Vector3.zero)
						FlowPanSignal.Dispatch(target.LocalDeltaPosition.x);
					break;
				case Gesture.GestureState.Ended:
					float velocity = (target.LocalTransformCenter.x - target.PreviousLocalTransformCenter.x) * 0.5f;
					if (Mathf.Abs(velocity) > Threshold)
						InertiaSignal.Dispatch(velocity);
					else
						FlowSignal.Dispatch();
					break;
			}
		}
	}
}
