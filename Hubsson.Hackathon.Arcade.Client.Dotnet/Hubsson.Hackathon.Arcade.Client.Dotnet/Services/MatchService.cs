﻿using System;
using Hubsson.Hackathon.Arcade.Client.Dotnet.Contracts;
using ClientGameState = Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.ClientGameState;

namespace Hubsson.Hackathon.Arcade.Client.Dotnet.Services
{
    public class MatchService
    {
        private MatchRepository _matchRepository;
        private ArcadeSettings _arcadeSettings;
        private readonly ILogger<MatchService> _logger;

        public MatchService(ArcadeSettings settings, ILogger<MatchService> logger)
        {
            _matchRepository = new MatchRepository();
            _arcadeSettings = settings;
            _logger = logger;
        }
        
        public void Init()
        {
            // On Game Init
            throw new NotImplementedException();
        }

        public Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.Action Update(ClientGameState gameState)
        {
            _logger.LogInformation($"{gameState}");
            _logger.LogError($"CurrentDirection: {_matchRepository.currentDirection}");
            var action = new Domain.Action();
            Coordinate coords = getMyCoordinates(gameState);
            if (coords.x == 1 && _matchRepository.currentDirection == Domain.Direction.Left)
            {
                // Bal széle
                if (_matchRepository.currentDirection == Domain.Direction.Down || coords.y > gameState.height / 2)
                {
                    _logger.LogError("Turning up");
                    _matchRepository.currentDirection = Domain.Direction.Up;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Up,
                        iteration = gameState.iteration,
                    };
                }
                else
                {
                    _logger.LogError("Turning down");
                    _matchRepository.currentDirection = Domain.Direction.Down;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Down,
                        iteration = gameState.iteration,
                    };
                }
            }
            if (coords.x == gameState.width - 1 && _matchRepository.currentDirection == Domain.Direction.Right)
            {
                // jobb széle
                if (_matchRepository.currentDirection == Domain.Direction.Down || coords.y > gameState.height / 2)
                {
                    _logger.LogError("Turning up");
                    _matchRepository.currentDirection = Domain.Direction.Up;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Up,
                        iteration = gameState.iteration,
                    };
                }
                else
                {
                    _logger.LogError("Turning down");
                    _matchRepository.currentDirection = Domain.Direction.Down;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Down,
                        iteration = gameState.iteration,
                    };
                }
            }
            if (coords.y == 1)
            {
                _logger.LogError($"CurrentDirection: {_matchRepository.currentDirection}");
                if (_matchRepository.currentDirection == Domain.Direction.Up)
                {

                    // teteje
                    if (_matchRepository.currentDirection == Domain.Direction.Right || coords.x > gameState.width / 2)
                    {
                        _logger.LogError("Turning left");
                        _matchRepository.currentDirection = Domain.Direction.Left;
                        return new Domain.Action
                        {
                            direction = Domain.Direction.Left,
                            iteration = gameState.iteration,
                        };
                    }
                    else
                    {
                        _logger.LogError("Turning right");
                        _matchRepository.currentDirection = Domain.Direction.Right;
                        return new Domain.Action
                        {
                            direction = Domain.Direction.Right,
                            iteration = gameState.iteration,
                        };
                    }
                }
            }
            if (coords.y == gameState.height - 1 && _matchRepository.currentDirection == Domain.Direction.Down)
            {
                // alja
                if (_matchRepository.currentDirection == Domain.Direction.Left || coords.x > gameState.width / 2)
                {
                    _logger.LogError("Turning left");
                    _matchRepository.currentDirection = Domain.Direction.Left;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Left,
                        iteration = gameState.iteration,
                    };
                }
                else
                {
                    _logger.LogError("Turning right");
                    _matchRepository.currentDirection = Domain.Direction.Right;
                    return new Domain.Action
                    {
                        direction = Domain.Direction.Right,
                        iteration = gameState.iteration,
                    };
                }
            }

            _matchRepository.currentDirection = GetDirection(_arcadeSettings.TeamId, gameState);    

            _logger.LogError("Turning Down (default)");
            _matchRepository.currentDirection = Domain.Direction.Down;
            return new Domain.Action
            {
                direction = Domain.Direction.Down,
                iteration = gameState.iteration,
            };
        }

        private Coordinate getMyCoordinates(ClientGameState gameState)
        {
            return gameState.players.FirstOrDefault(player => player.playerId == _arcadeSettings.TeamId).coordinates.Last();
        }

        private Domain.Direction GetDirection(string playerId, ClientGameState gameState)
        {
            var direction = Domain.Direction.Down;
            var player = gameState.players.FirstOrDefault(player => player.playerId == playerId);
            try
            {
                if (player?.coordinates[player.coordinates.Length - 1].x > player?.coordinates[player.coordinates.Length - 2].x)
                {
                    direction = Domain.Direction.Right;
                }
                else if (player?.coordinates[player.coordinates.Length - 1].x < player?.coordinates[player.coordinates.Length - 2].x)
                {
                    direction = Domain.Direction.Left;
                }
                else if (player?.coordinates[player.coordinates.Length - 1].y > player?.coordinates[player.coordinates.Length - 2].y)
                {
                    direction = Domain.Direction.Down;
                }
                else if (player?.coordinates[player.coordinates.Length - 1].y < player?.coordinates[player.coordinates.Length - 2].y)
                {
                    direction = Domain.Direction.Up;
                }
            }
            catch (IndexOutOfRangeException)
            {
                _logger.LogError("currentDirrection is empty, default: Direction.Down");
            }
            return direction;
           
        }

        private class MatchRepository
        {
            public Domain.Direction currentDirection;
            // Write your data fields here what you would like to store between the match rounds
        }
    }
}