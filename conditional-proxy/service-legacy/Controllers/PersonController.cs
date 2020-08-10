using System;
using Microsoft.AspNetCore.Mvc;

namespace service_legacy.Controllers
{
    [ApiController]
    [Route("person")]
    public class PersonController : ControllerBase
    {
        [HttpPost]
        public ActionResult<PersonDto> CreatePerson([FromBody] PersonDto person)
        {
            Console.WriteLine($"Legacy Service - Received: {person.Name}, {person.Age}");
            return Ok(person);
        }
    }
}