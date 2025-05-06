using UnityEngine;
using System;

namespace ArtNotes.PhysicalInteraction
{
	public abstract class Executor : MonoBehaviour
	{
		public abstract void Execute(float signal);
	}
}
