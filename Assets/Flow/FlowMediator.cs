using strange.extensions.mediation.impl;
using UnityEngine;
namespace ca.HenrySoftware.Flow
{
	public class FlowMediator : Mediator
	{
		[Inject]
		public FlowView FlowView { get; set; }
		[Inject]
		public NextSignal NextSignal { get; set; }
		[Inject]
		public PrevSignal PrevSignal { get; set; }
		[Inject]
		public FlowSignal FlowSignal { get; set; }
		[Inject]
		public FlowToSignal FlowToSignal { get; set; }
		[Inject]
		public FlowSnapSignal FlowSnapSignal { get; set; }
		[Inject]
		public FlowPanSignal FlowPanSignal { get; set; }
		[Inject]
		public InertiaSignal InertiaSignal { get; set; }
		[Inject]
		public InertiaStopSignal InertiaStopSignal { get; set; }
		public override void OnRegister()
		{
			NextSignal.AddListener(HandleNext);
			PrevSignal.AddListener(HandlePrev);
			FlowSignal.AddListener(HandleFlow);
			FlowToSignal.AddListener(HandleFlowTo);
			FlowSnapSignal.AddListener(HandleFlowSnap);
			FlowPanSignal.AddListener(HandleFlowPan);
			InertiaSignal.AddListener(HandleInertia);
			InertiaStopSignal.AddListener(HandleInertiaStop);
		}
		public override void OnRemove()
		{
			NextSignal.RemoveListener(HandleNext);
			PrevSignal.RemoveListener(HandlePrev);
			FlowSignal.RemoveListener(HandleFlow);
			FlowToSignal.RemoveListener(HandleFlowTo);
			FlowSnapSignal.RemoveListener(HandleFlowSnap);
			FlowPanSignal.RemoveListener(HandleFlowPan);
			InertiaSignal.RemoveListener(HandleInertia);
			InertiaStopSignal.RemoveListener(HandleInertiaStop);
		}
		private void HandleNext()
		{
			FlowView.Next();
		}
		private void HandlePrev()
		{
			FlowView.Prev();
		}
		private void HandleFlow()
		{
			FlowView.Flow();
		}
		private void HandleFlowTo(GameObject value)
		{
			FlowView.Flow(value);
		}
		private void HandleFlowSnap(int value)
		{
			FlowView.Flow(value);
		}
		private void HandleFlowPan(float value)
		{
			FlowView.Flow(value);
		}
		private void HandleInertia(float value)
		{
			FlowView.Inertia(value);
		}
		private void HandleInertiaStop()
		{
			FlowView.InertiaStop();
		}
	}
}
