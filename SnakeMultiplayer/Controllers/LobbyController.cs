using Microsoft.AspNetCore.Http;
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
        private static string InvalidStringErrorMessage = @"Please use only letters, numbers and spaces only between words. ";
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
        public IActionResult CreateLobby([FromServices] GameServerService gameServer, string id = "", string playerName = "")
        {
            ViewData["playerName"] = playerName;
            ViewData["lobbyId"] = id;

            string errorMessage = IsValid(id, playerName);
            if (!errorMessage.Equals(string.Empty))
            {
                ViewData["ErrorMessage"] = errorMessage;
                return View();
            }

            bool success = gameServer.TryCreateLobby(id, playerName, gameServer);
            if (success)
            {
                SetCookie("PlayerName", playerName);
                SetCookie("LobbyId", id);
                ViewData["IsHost"] = true;
                return View("Views/Lobby/Index.cshtml");
            }

            ViewData["ErrorMessage"] = $"Lobby with {id} already exists. Please enter different name";
            //return View("Views/Lobby/CreateLobby.cshtml");
            return View();
        }

        [HttpGet]
        public IActionResult JoinLobby(string id = "")
        {
            ViewData["lobbyId"] = id;
            return View();
        }

        //[HttpPost("/JoinLobby/{playerName}/{id}")]
        [HttpPost]
        public IActionResult JoinLobby([FromServices] GameServerService gameServer, string id = "", string playerName= "")
        {
            ViewData["playerName"] = playerName;
            ViewData["lobbyId"] = id;

            string errorMessage = IsValid(id, playerName);
            if (!errorMessage.Equals(string.Empty))
            {
                ViewData["ErrorMessage"] = errorMessage;
                return View();
            }

            errorMessage = gameServer.CanJoin(id, playerName);
            if (errorMessage.Equals(string.Empty))
            {
                SetCookie("PlayerName", playerName);
                SetCookie("LobbyId", id);
                return View("Views/Lobby/Index.cshtml");
            }
            else
            {
                ViewData["ErrorMessage"] = errorMessage;
                return View();
            }
        }

        private static string IsValid(string lobbyName, string playerName)
        {
            if (String.IsNullOrEmpty(playerName))
            {
                return "Please enter your player name";
            }
            else if (String.IsNullOrEmpty(lobbyName))
            {
                return "Please enter lobby name";
            }
            else if (!GameServerService.ValidStringRegex.IsMatch(playerName))
            {
                return "Player name is incorrect.\n" + InvalidStringErrorMessage;
            }
            else if (!GameServerService.ValidStringRegex.IsMatch(lobbyName))
            {
                return "Lobby name is incorrect.\n" + InvalidStringErrorMessage;
            }
            else
            {
            return string.Empty;
            }
        }

        private void SetCookie(string name, string value)
        {
            var options = new CookieOptions()
            {
                IsEssential = true,
            };
            Response.Cookies.Append(name, value, options);
        }
    }
}
