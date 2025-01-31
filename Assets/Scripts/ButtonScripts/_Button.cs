﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

public abstract class Button : MonoBehaviour {

	public KMSelectable btn;
	public MeshRenderer flashingRenderer;
	private KMSelectable _parentMod;
	private MeshCollider _collider;
	private Mesh _mesh;
	private Color _initCol;
	public Dir direction { get; set; }

	protected const RegexOptions regexFlags = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
	public abstract Predicate<string> rule { get; }
	public abstract string name { get; }

	void Awake() {
		_parentMod = GetComponentInParent<ModuleManeuversScript>().GetComponent<KMSelectable>();
		btn.Parent = _parentMod;
		_collider = (MeshCollider)btn.SelectableColliders[0];
		_mesh = _collider.sharedMesh;
		_initCol = flashingRenderer.material.color;
	}

	public void SetChildStatus(bool enable)
    {
		if (enable)
		{
			_collider.sharedMesh = _mesh;
		}
		else
			_collider.sharedMesh = null;
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public void SetRendererFlash(bool state)
    {
		flashingRenderer.material.color = state ? Color.white : _initCol;
    }
	public IEnumerator RotateToCenter()
    {
		float delta = 0;
		Vector3 start = transform.localEulerAngles;
		while (delta < 1)
        {
			delta += Time.deltaTime / 0.5f;
			yield return null;
			transform.localEulerAngles = new Vector3(start.x, Mathf.Lerp(start.y > 180 ? start.y - 360 : start.y, 0, delta), start.z);
        }
    }
}
