using System;
using Microsoft.AspNetCore.Mvc;

namespace new_service.Controllers
{
    [ApiController]
    [Route("person")]
    public class PersonController : ControllerBase
    {
        [HttpPost]
        public ActionResult<PersonDto> CreatePerson([FromBody] PersonDto person)
        {
            Console.WriteLine($"New Service - Received: {person.Name}, {person.Age}");
            return Ok(person);
        }
    }
}