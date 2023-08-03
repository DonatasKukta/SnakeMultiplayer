namespace Domain
open System.Collections.Concurrent

type Speed =
    | NoSpeed = 0
    | Fast = 1
    | Normal = 2
    | Slow = 3

type Direction =
    | Up = 1
    | Right = 2
    | Down = 3
    | Left = 4

type Cell =
    | Empty = 0
    | Food = 1
    | Snake = 2

type Color =
    | GreenYellow
    | DodgerBlue
    | Orange
    | MediumPurple

type LobbyStates =
    | Idle
    | Initialized
    | InGame
    | Closed

type Settings = { cellCount: int; speed: Speed }

type Coordinate = { X: int; Y: int }
type Food = Coordinate

type Snake =
    { name: string
      body: list<Coordinate>
      color: Color
      previousDirection: Direction option }

type Arena =
    { settings: Settings
      board: Cell [,]
      players: Map<string, Snake>
      food: Food
      maxPlayers: int
      hostPlayer: string }

type arenaId = string
type playerId = string

type PendingDirections = ConcurrentDictionary<arenaId * playerId, Direction>
