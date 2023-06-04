using Hubsson.Hackathon.Arcade.Client.Dotnet.Contracts;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Domain;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Settings;
using ClientGameState = Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.ClientGameState;
namespace Hubsson.Hackathon.Arcade.Client.Dotnet.Services
{
    public class MatchService
    {
        private MatchRepository _matchRepository;
        private ArcadeSettings _arcadeSettings;
        private BotSettings _botSettings;
        private readonly ILogger<MatchService> _logger;
        public MatchService(ArcadeSettings arcadeSettings, BotSettings botSettings, ILogger<MatchService> logger)
        {
            _matchRepository = new MatchRepository();
            _arcadeSettings = arcadeSettings;
            _botSettings = botSettings;
            _logger = logger;
        }
        public void Init()
        {
            _matchRepository.direction = (Direction)new Random().Next(1, 5);
        }
        public Domain.Action Update(ClientGameState gameState)
        {
            Coordinate head = gameState.players.First(player => player.playerId == _arcadeSettings.TeamId).coordinates.Last();
            Dictionary<Direction, int> freeSpaces = new()
            {
                { Direction.Up, 0 },
                { Direction.Down, 0 },
                { Direction.Left, 0 },
                { Direction.Right, 0 }
            };

            Dictionary<Direction, int> straightFreeSpaces = new()
            {
                { Direction.Up, 0 },
                { Direction.Down, 0 },
                { Direction.Left, 0 },
                { Direction.Right, 0 }
            };


            Direction oppositeDirection = GetOppositeDirection(_matchRepository.direction.Value);
            if (freeSpaces.ContainsKey(oppositeDirection))
            {
                freeSpaces.Remove(oppositeDirection);
            }
            

            foreach (var direction in freeSpaces.Keys.ToList())
            {
                Coordinate newHead = Move(head, direction);
                if (IsValidMove(newHead, gameState))
                {
                    if (IsDeadEnd(newHead, gameState))
                    {
                        freeSpaces[direction] = int.MinValue;
                        continue;
                    }
                    freeSpaces[direction] = CountFreeSpacesInArea(newHead, gameState, _botSettings.CheckArea);
                    straightFreeSpaces[direction] = CountFreeSpacesInLine(newHead, gameState, direction);
                }
                else
                {
                    freeSpaces[direction] = int.MinValue;
                }
            }
            _matchRepository.direction = freeSpaces.Aggregate((l, r) => l.Value > r.Value || (l.Value == r.Value && straightFreeSpaces[l.Key] > straightFreeSpaces[r.Key]) ? l : r).Key;
            return new Domain.Action { direction = _matchRepository.direction.Value, iteration = gameState.iteration };
        }

        private bool IsDeadEnd(Coordinate coordinate, ClientGameState state)
        {
            int freeSpaces = 0;

            foreach (var direction in new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
            {
                Coordinate newCoordinate = Move(coordinate, direction);

                if (IsValidMove(newCoordinate, state))
                {
                    freeSpaces++;

                    foreach (var secondDirection in new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                    {
                        Coordinate secondNewCoordinate = Move(newCoordinate, secondDirection);

                        if (IsValidMove(secondNewCoordinate, state))
                        {
                            freeSpaces++;
                        }
                    }
                }
            }

            return freeSpaces <= 2;
        }
        private int CountFreeSpacesInLine(Coordinate coordinate, ClientGameState state, Direction direction)
        {
            int freeSpaces = 0;
            Coordinate newCoordinate = Move(coordinate, direction);
            while (IsValidMove(newCoordinate, state))
            {
                freeSpaces++;
                newCoordinate = Move(newCoordinate, direction);
            }
            return freeSpaces;
        }
        private Coordinate Move(Coordinate coordinate, Direction direction)
        {
            switch (direction)
            {
                case Direction.Down: return new Coordinate { x = coordinate.x, y = coordinate.y + 1 };
                case Direction.Up: return new Coordinate { x = coordinate.x, y = coordinate.y - 1 };
                case Direction.Left: return new Coordinate { x = coordinate.x - 1, y = coordinate.y };
                case Direction.Right: return new Coordinate { x = coordinate.x + 1, y = coordinate.y };
                default: throw new ArgumentException($"Invalid direction: {direction}");
            }
        }
        private bool IsValidMove(Coordinate coordinate, ClientGameState state)
        {
            if (coordinate.x < 0 || coordinate.x > state.width - 1 || coordinate.y < 0 || coordinate.y > state.height - 1)
            {
                return false;
            }
            foreach (var player in state.players)
            {
                if (player.coordinates.Any(c => c.x == coordinate.x && c.y == coordinate.y))
                {
                    return false;
                }
            }
            return true;
        }

        private int CountFreeSpacesInArea(Coordinate coordinate, ClientGameState state, int radius)
        {
            if (radius < 1)
                return 0;

            int freeSpaces = 0;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Coordinate newCoordinate = new Coordinate { x = coordinate.x + dx, y = coordinate.y + dy };
                    if (IsValidMove(newCoordinate, state))
                    {
                        freeSpaces++;
                    }
                }
            }
            return freeSpaces;
        }
        private Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: throw new ArgumentException($"Invalid direction: {direction}");
            }
        }

        private class MatchRepository
        {
            public Direction? direction;
        }
    }
}
