using System;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents an input device for purposes of Voice and Video systems. See <see cref="CavrnusFunctionLibrary.FetchAudioInputs"/>, <see cref="CavrnusFunctionLibrary.UpdateAudioInput"/>.
	/// </summary>
	public class CavrnusInputDevice
	{
		public string Name { get; set; }
		public readonly string Id;

		internal CavrnusInputDevice(string name, string id)
		{
			Name = name;
			Id = id;
		}
		
		public override bool Equals(object obj)
		{
			return obj is CavrnusInputDevice device &&
				   Name == device.Name &&
				   Id == device.Id;
		}
		
		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Id);
		}
	}
}