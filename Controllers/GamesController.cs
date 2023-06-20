using mongodb_dotnet_example.Models;
using mongodb_dotnet_example.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MongoDB.Bson;

namespace mongodb_dotnet_example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly GamesService _gameService;

        public GamesController(GamesService gamesService)
        {
            _gameService = gamesService;
        }

        [HttpGet]
        public ActionResult<List<Game>> Get() =>
            _gameService.Get();

        [HttpGet("{id:length(24)}", Name = "GetGame")]
        public ActionResult<Game> Get(string id)
        {
            var game = _gameService.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        [HttpPost]
        public ActionResult<GameInput> Create(GameInput gameI)
        {
            _gameService.Create(gameI);
            return CreatedAtRoute("GetGame", new { id = ObjectId.GenerateNewId().ToString() }, gameI);
            //return Ok(data);
            //newww Push
        }

        [HttpPut("{id:length(24)}")]
        public ActionResult<Game> Update(string id, Game gameIn)
        {
            var game = _gameService.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            return _gameService.Update(id, gameIn);

            //return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var game = _gameService.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            _gameService.Delete(game.Id);

            return NoContent();
        }
    }
}