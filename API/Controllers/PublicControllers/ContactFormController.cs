using Application.MediatR.Public.ContactForm;
using Domain.Public;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.PublicControllers;

[AllowAnonymous]
public class ContactFormController(IMediator mediator) : PublicController
{
   
    [HttpPost]
    [Route("contact-form")]
    public async Task<IActionResult> ReceiveContactFormAsync(ContactFormRequest request)
    {
        var command = new SendContactFormCommand(
            request.Name,
            request.Email,
            request.Subject,
            request.Message,
            request.SubmittedAt
        );
        
        var success = await mediator.Send(command);

        if (!success)
            return BadRequest("Ugyldig epost eller forespørsel");
        
        
        return Ok("Takk for din henvendelse! Meldingen er til behandling.");
    }
}