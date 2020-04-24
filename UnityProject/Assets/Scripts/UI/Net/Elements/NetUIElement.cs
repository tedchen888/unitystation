using System;
using UnityEngine;
/// Base class for networked UI element
[Serializable]
public abstract class NetUIElement : MonoBehaviour
{
	protected bool externalChange;

	/// Unique tab that contains this element
	public NetTab MasterTab {
		get {
			if ( !masterTab ) {
				masterTab = GetComponentsInParent<NetTab>(true)[0];
			}
			return masterTab;
		}
	}
	private NetTab masterTab;

	public ElementValue ElementValue => new ElementValue{Id = name, Value = Value};

	public virtual ElementMode InteractionMode => ElementMode.Normal;

	/// Server-only method for updating element (i.e. changing label text) from server GUI code
	public virtual string SetValue {
		set {
			Value = value;
			UpdatePeepers();
		}
	}

	/// Initialize method before element list is collected. For editor-set values
	public virtual void Init() {}

	public virtual string Value {
		get {
			return "-1";
		}
		set {
		}
	}

	/// Always point to this method in OnValueChanged
	/// <a href="https://camo.githubusercontent.com/e3bbac26b36a01c9df8fbb6a6858bb4a82ba3036/68747470733a2f2f63646e2e646973636f72646170702e636f6d2f6174746163686d656e74732f3339333738373838303431353239373534332f3435333632313031363433343833353435362f7467745f636c69656e742e676966">See GIF</a>
	public void ExecuteClient() {
		//Don't send if triggered by external change
		if ( !externalChange ) {
			TabInteractMessage.Send(MasterTab.Provider, MasterTab.Type, name, Value);
		}
	}

	/// Send update to observers.
	protected void UpdatePeepers() {
		if ( gameObject.activeInHierarchy )
		{
			UpdatePeepersLogic();
		} else
		{
//			Logger.LogTraceFormat( "'{0}': didn't update peepers because gameObject is inactive (another page?)", Category.NetUI, name );
			MasterTab.ValidatePeepers();
		}
	}


	/// Override if you want custom "send update to peepers" logic
	/// i.e. to include more values than just the current one
	protected virtual void UpdatePeepersLogic()
	{
		TabUpdateMessage.SendToPeepers( MasterTab.Provider, MasterTab.Type, TabAction.Update, new[] {ElementValue} );
	}

	public abstract void ExecuteServer(ConnectedPlayer subject);

	public override string ToString() {
		return ElementValue.ToString();
	}

	public const char DELIMITER = '~';

	/// <summary>
	/// Special logic to execute after all tab elements are initialized
	/// </summary>
	public virtual void AfterInit() { }
}

public enum ElementMode {
	/// Changeable by both client and server
	Normal,
	/// Only server can change value
	ServerWrite,
	/// Only client can change value, and server doesn't store it
	ClientWrite
}