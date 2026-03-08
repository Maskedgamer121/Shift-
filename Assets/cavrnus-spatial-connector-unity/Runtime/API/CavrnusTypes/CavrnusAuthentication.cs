using Cavrnus.Comm.Comm.RestApi;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents a successful authentication with the Cavrnus Server. Contains the JWT token used in server requests to authenticate the user.
	/// </summary>
	public class CavrnusAuthentication
	{
		public readonly string Token;
		
		internal readonly RestUserCommunication RestUserComm;
		internal readonly RestApiEndpoint Endpoint;
		
		internal CavrnusAuthentication(RestUserCommunication ruc, RestApiEndpoint endpoint, string token)
		{
			RestUserComm = ruc;
			Endpoint = endpoint;
			Token = token;
		}
	}
}