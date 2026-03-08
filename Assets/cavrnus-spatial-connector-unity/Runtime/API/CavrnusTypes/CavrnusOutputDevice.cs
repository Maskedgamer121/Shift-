using System;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents an output device for purposes of Voice and Video systems. Only applicable for Audio.
	/// </summary>
	public class CavrnusOutputDevice
	{
		public string Name;
		public readonly string Id;

		internal CavrnusOutputDevice(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public override bool Equals(object obj)
		{
			return obj is CavrnusOutputDevice device &&
				   Name == device.Name &&
				   Id == device.Id;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}
	}
}