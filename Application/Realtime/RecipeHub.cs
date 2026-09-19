using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application.Realtime;

/// <summary>
/// Plassholder for sanntidsoppdateringer til frontend via SignalR/WebSockets.
/// Ingen hub-metoder eller push-varsler er lagt til ennå.
/// </summary>
[Authorize]
public class RecipeHub : Hub
{
}
