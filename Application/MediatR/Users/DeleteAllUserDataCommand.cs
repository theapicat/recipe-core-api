using MediatR;

namespace Application.MediatR.Users;

// Slett alt brukerskapt data for én bruker. Trigges av at recipe-auth-api har slettet kontoen (se
// Application/Messaging/Consumers) - ingen HTTP-rute kaller denne direkte, og ingen eier-sjekk trengs
// siden UserId kommer fra en betrodd hendelse, ikke fra en klient.
public record DeleteAllUserDataCommand(Guid UserId) : IRequest;
