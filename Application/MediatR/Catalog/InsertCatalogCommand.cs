using Application.Results;
using Domain;
using MediatR;

namespace Application.MediatR.Catalog;

// Returnerer id-en til den opprettede raden. For Guid-nøkler tildeles den av serveren (uansett hva klienten sendte);
// for andre nøkler (f.eks. Matvaretabellens tekstkoder) er den oppgitt av kalleren.
public record InsertCatalogCommand<T, TKey>(T Entity) : IRequest<Result<TKey>> where T : IHasId<TKey>;
