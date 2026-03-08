using System;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents a video input source for purposes of Voice and Video systems. See <see cref="CavrnusFunctionLibrary.FetchVideoInputs"/>, <see cref="CavrnusFunctionLibrary.UpdateVideoInput"/>.
	/// </summary>
	public class CavrnusVideoInputDevice
	{
		public string Name;
		public readonly string Id;

		internal CavrnusVideoInputDevice(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public override bool Equals(object obj)
		{
			return obj is CavrnusVideoInputDevice device &&
				   Name == device.Name &&
				   Id == device.Id;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}
	}
}