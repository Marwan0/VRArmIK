using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneRenderer : MonoBehaviour
{
	public Transform target;
	public Transform bone;

	void LateUpdate()
	{
		float distance = (transform.position - target.position).magnitude;
		Vector3 scale = bone.localScale;
		scale.z = distance;
		bone.localScale = scale;

		Vector3 pos = (transform.position + target.position) * 0.5f;
		bone.transform.position = pos;

		bone.LookAt(target.position);
	}
}
