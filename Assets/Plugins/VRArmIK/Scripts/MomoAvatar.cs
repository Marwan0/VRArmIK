using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRArmIK;

public class MomoAvatar : MonoBehaviour
{
	public Transform lHand, rHand, lElbow, rElbow, lShoulder, rShoulder, neck, head;

	public void updatePositions(MomoOutputData output)
	{
		lElbow.position = output.lelbow;
		rElbow.position = output.relbow;
		lShoulder.position = output.lshoulder;
		rShoulder.position = output.rshoulder;
		neck.position = output.hals;
	}

	public void updatePositions(AvatarVRTrackingReferences vrInput)
	{
		rHand.position = vrInput.leftHand.transform.position;
		rHand.rotation = vrInput.leftHand.transform.rotation;

		lHand.position = vrInput.rightHand.transform.position;
		lHand.rotation = vrInput.rightHand.transform.rotation;

		head.position = vrInput.head.transform.position;
		head.rotation = vrInput.head.transform.rotation;
	}
}
