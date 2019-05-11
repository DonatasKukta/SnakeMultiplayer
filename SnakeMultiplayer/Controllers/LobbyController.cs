using Microsoft.AspNetCore.Mvc;
using SnakeMultiplayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeMultiplayer.Controllers
{
    //[Route("Lobby")]
    public class LobbyController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("Views/Lobby/CreateLobby.cshtml");
        }
        [HttpGet]
        public IActionResult CreateLobby()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateLobby(string id, string playerName)
        {
            return View();
        }

        [HttpGet]
        public IActionResult JoinLobby()
        {
            return View();
        }

        [HttpGet("{id}")]
        public IActionResult JoinLobby(string id)
        {
            @ViewData["lobbyId"] = id;
            return View();
        }

        //[HttpPost("/JoinLobby/{playerName}/{id}")]
        [HttpPost]
        public IActionResult JoinLobby(string id, string playerName)
        {
            return View();
        }

    }
}
