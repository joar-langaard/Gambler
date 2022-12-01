﻿namespace Gambler.PoC.Services
{
    using Gambler.PoC.Data;
    using Gambler.PoC.Business.Entities;
    using Gambler.PoC.Models;

    public interface IGamblerService
    {
        Gambler Register(string nickname);
        Score Bet(Guid id, int value);
        Score Lottery(Guid id);
        Score Score(Guid id);
        IEnumerable<Score> GetTop10Gamblers();
    }

    public class GamblerService : IGamblerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _maxBetsPerDay = 500;

        public GamblerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Score Bet(Guid id, int value)
        {
            var response = new Score();

            var entity = _unitOfWork.Gamblers
                .Find(g => g.UniquieIdentity == id)
                .FirstOrDefault();

            if (entity == null)
                throw new Exception("Unknown gambler");

            if (value > entity.Score)
                throw new Exception("You cannot bet more than you have...");

            if(entity.LatestBet.Date == DateTime.Now.Date && entity.NumberOfBets >= _maxBetsPerDay)
                throw new Exception("You have reached your treshold for number of bets pr. day...");

            // Implement really smart algorithm with 50%-ish win chance unless betting cool numbers
            var factor = 1.0;

            if (value == 69 || value == 420 || value == 666 || value == 1337 || value == 42069 || value == 69420)
                factor = 1.2;

            if (Random.Shared.Next(0, 99) < (50 * factor))
            {
                // Win
                entity.Score += value;

                response.Nickname = entity.Nickname;
                response.Points = entity.Score;
                response.Message = string.Format("Gambler won {0}!", value);
            }
            else
            {
                // Loss
                entity.Score -= value;

                response.Nickname = entity.Nickname;
                response.Points = entity.Score;
                response.Message = string.Format("Gambler lost {0}!", value);
            }

            if(entity.LatestBet.Date != DateTime.Now.Date)
                entity.NumberOfBets = 1;
            else
                entity.NumberOfBets += 1;

            entity.LatestBet = DateTime.Now;
            entity.Highscore = entity.Score > entity.Highscore ? entity.Score : entity.Highscore;

            _unitOfWork.Complete();

            return response;
        }

        public Score Lottery(Guid id)
        {
            var response = new Score();
            
            var entity = _unitOfWork.Gamblers
                .Find(g => g.UniquieIdentity == id)
                .FirstOrDefault();

            if (entity == null)
                throw new Exception("Unknown gambler");

            // Implement really smart algorithm with a very low win chance
            if (Random.Shared.Next(0, 10000) <= 10)
            {
                // Win
                entity.Score += 100000;

                response.Nickname = entity.Nickname;
                response.Points = entity.Score;
                response.Message = string.Format("Gambler won the lottery!");

            }
            else
            {
                // Loose
                entity.Score -= 100;

                response.Nickname = entity.Nickname;
                response.Points = entity.Score;
                response.Message = string.Format("Sorry, no lottery for you! We took 100 points from your account");
            }

            _unitOfWork.Complete();

            return response;
        }

        public IEnumerable<Score> GetTop10Gamblers()
        {
            return _unitOfWork.Gamblers.GetTop10Gamblers()
                .Select(g => new Score() 
                {   Nickname = g.Nickname, 
                    Points = g.Score 
                });
        }

        public Gambler Register(string nickname)
        {
            var entity = new Gambler();

            entity.Nickname = nickname;
            
            // default values
            entity.Score = 1000;
            entity.Created = DateTime.Now;

            _unitOfWork.Gamblers.Add(entity);
            _unitOfWork.Complete();

            return entity;
        }

        public Score Score(Guid id)
        {
            var entity = _unitOfWork.Gamblers
                .Find(g => g.UniquieIdentity == id)
                .FirstOrDefault();

            if (entity == null)
                throw new Exception("Unknown gambler");

            var response = new Score()
            {
                Nickname = entity.Nickname,
                Points = entity.Score,
                Message = "Current score for user"
            };

            return response;
        }
    }
}
